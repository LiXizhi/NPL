//using ErrorHandler=Microsoft.VisualStudio.ErrorHandler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using ParaEngine.Tools.Lua.AST;
using ParaEngine.Tools.Lua.CodeDom;
using ParaEngine.Tools.Lua.Parser;
using ParaEngine.Tools.Lua.Refactoring;
using ParaEngine.Tools.Lua.Refactoring.RenameService;
using ParaEngine.Tools.Lua.SourceOutliner;
using ParaEngine.Tools.Lua.VsEditor;
using ParaEngine.Tools.Services;
using Microsoft.Win32;
using Microsoft;

using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using LuaParser = ParaEngine.Tools.Lua.Parser.Parser;
using Source = ParaEngine.Tools.Lua.Parser.Source;
using LuaScanner = ParaEngine.Tools.Lua.Lexer.Scanner;

namespace ParaEngine.Tools.Lua
{
	/// <summary>
	/// The language service provided for the Lua language.
	/// </summary>
	[Guid(GuidStrings.LuaLanguageService)]
	public sealed class LanguageService : BaseLanguageService, ILuaLanguageService
	{
		private const string documentationRelativePath = "Documentation";

		private static readonly char[] lastWordDelimiters = new[] { ' ', '\t', '\n', '(', '[', '{', '=' };
		private static readonly char[] memberSelectDelimiters = new[] { '.', ':' };

		private Dictionary<string, Chunk> luaChunks = new Dictionary<string, Chunk>();

		private readonly List<string> frameXmlFiles = new List<string>();
		private readonly List<string> luaFiles = new List<string>();

		private readonly XmlDocumentationLoader xmlDocumentationLoader = new XmlDocumentationLoader();

		private SnippetDeclarationProvider snippetDeclarationProvider;
		private KeywordDeclarationProvider keywordDeclarationProvider;
		private TableDeclarationProvider xmlDeclarationProvider;

		private readonly Dictionary<string, TableDeclarationProvider> frameXmlDeclarationProviders =
			new Dictionary<string, TableDeclarationProvider>();

		private readonly Dictionary<string, TableDeclarationProvider> luaFileDeclarationProviders =
			new Dictionary<string, TableDeclarationProvider>();

		private DeclarationAuthoringScope authoringScope;

		private event EventHandler<FileCodeModelChangedEventArgs> fileCodeModelChangedEvent;

		/// <summary>
		/// Occurs when [on file code model changed].
		/// </summary>
		public event EventHandler<FileCodeModelChangedEventArgs> OnFileCodeModelChanged
		{
			add { fileCodeModelChangedEvent += value; }
			remove { fileCodeModelChangedEvent -= value; }
		}

		//private ParseRequest previousRequest;
		private string lastAddedLuaFile;

		/// <summary>
		/// Gets or sets the DTE.
		/// </summary>
		/// <value>The DTE.</value>
		public DTE2 DTE { get; private set; }

		/// <summary>
		/// Initializes the Language Serice and loads the documentation.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			// Initialize providers
			snippetDeclarationProvider = new SnippetDeclarationProvider(this);
			keywordDeclarationProvider = new KeywordDeclarationProvider();
			xmlDeclarationProvider = new TableDeclarationProvider();

			// Load the documentation
			LoadXmlDocumentation();
			authoringScope = new DeclarationAuthoringScope(this);
			DTE = GetService(typeof(DTE)) as DTE2;
		}

		/// <summary>
		/// Loads the XML documentation.
		/// </summary>
		private void LoadXmlDocumentation()
		{
			// Retrieve install directory
			string documentationRootPath = null;

            //string installDirectory = GetInstallDirectory();
            //if (installDirectory != null)
            //{
            //     //Combine the install directory and the documentation's relative path into one
            //    string sPackagePath = @"Software\ParaEngine\ParaEngineSDK";
            //    using (RegistryKey setupKey = Registry.CurrentUser.OpenSubKey(
            //         sPackagePath))
            //    {
            //        if (setupKey != null)
            //        {
            //            string installPath = setupKey.GetValue("").ToString();
            //            if (!string.IsNullOrEmpty(installPath))
            //            {
            //                documentationRootPath = installPath + @"\script\VisualStudioNPL";
            //            }
            //        }
            //    }
            //    if (documentationRootPath == null)
            //        documentationRootPath = Path.Combine(installDirectory, documentationRelativePath);

            //    //Make sure we have a valid directory
            //    if (Directory.Exists(documentationRootPath))
            //    {
            //         //Look for XML files and load them using the XML documentation loader
            //        foreach (string path in Directory.GetFiles(documentationRootPath, "*.xml"))
            //            xmlDocumentationLoader.LoadXml(path);
            //    }
            //}

            documentationRootPath = ParaEngine.NPLLanguageService.NPLLanguageServicePackage.PackageRootPath;
            if (documentationRootPath != null)
            {
                //Look for XML files and load them using the XML documentation loader
                foreach (string path in Directory.GetFiles(documentationRootPath + "Documentation", "*.xml"))
                    xmlDocumentationLoader.LoadXml(path);
            }

			xmlDocumentationLoader.AddDeclarations(xmlDeclarationProvider);
		}

		/// <summary>
		/// Called when [active view changed].
		/// </summary>
		/// <param name="textView">The text view.</param>
		public override void OnActiveViewChanged(IVsTextView textView)
		{
			base.OnActiveViewChanged(textView);
			//Currently filters is not used.
			if (textView != null)
			{
			    IOleCommandTarget target;
			    LuaCommandFilter commandFilter = LuaCommandFilter.GetCommandFilter(this);
			    textView.RemoveCommandFilter(commandFilter);
			    textView.AddCommandFilter(commandFilter, out target);
			    commandFilter.VsCommandFilter = target;
			}
			else
			{
				//textView.RemoveCommandFilter()
			}
		}

		/// <summary>
		/// Called when changes generated by an auto-complete or code snippet expansion operation
		/// is committed to the buffer.
		/// </summary>
		/// <param name="flags">The flags.</param>
		/// <param name="ptsChanged">The PTS changed.</param>
		protected override void OnChangesCommitted(uint flags, TextSpan[] ptsChanged)
		{
			base.OnChangesCommitted(flags, ptsChanged);
			NotifyOnFileCodeModelChanged();
		}

		/// <summary>
		/// Called when a FileCodeModel object has been changed.
		/// </summary>
		internal void NotifyOnFileCodeModelChanged()
		{
			if (fileCodeModelChangedEvent != null)
			{
				fileCodeModelChangedEvent(this, new FileCodeModelChangedEventArgs(null));
			}
		}

		/// <summary>
		/// Indicates that rename allowed on selected item.
		/// </summary>
		public bool CanRefactorRename
		{
			get
			{
				return IsRefactorableItemSelected();
			}
		}


		/// <summary>
		/// Determines whether [is refactorable item selected].
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if [is refactorable item selected]; otherwise, <c>false</c>.
		/// </returns>
		private bool IsRefactorableItemSelected()
		{
			CodeElement codeElement = null;
			LuaFileCodeModel codeModel = GetFileCodeModel();
			if (codeModel != null)
			{
				codeElement = codeModel.GetElementByEditPoint();
			}
			return codeElement != null;
		}

		/// <summary>
		/// Start Refactor-Rename operation on selected element.
		/// </summary>
		public void RefactorRename()
		{
			var refactorAdpater = new RefactorRenameAdapter(Site);
			IRenameResult renameResult = refactorAdpater.Rename();
			if (renameResult != null && renameResult.HasChanges)
			{
				NotifyOnFileCodeModelChanged();
			}
		}

		/// <summary>
		/// Called when the user clicks the menu item that shows the tool window.
		/// </summary>
		public void ShowSourceOutlinerToolWindow()
		{
			try
			{
				var window = (ToolWindowPane)GetService(typeof(SourceOutlineToolWindow));
				var windowFrame = (IVsWindowFrame)window.Frame;
				ErrorHandler.ThrowOnFailure(windowFrame.Show());
			}
			catch (Exception ex)
			{
				Trace.WriteLine("ShowToolWindow exception: " + ex);
			}
		}

		/// <summary>
		/// Adds a FrameXML file to the list of files to be parsed.
		/// </summary>
		/// <param name="path">The path to the file.</param>
		public void AddFrameXmlFile(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			frameXmlFiles.Add(path);
			AddFrameXmlDeclarations(path);
		}

		/// <summary>
		/// Adds a Lua file to the list of files to be parsed.
		/// </summary>
		/// <param name="path">The path to the file.</param>
		public void AddLuaFile(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			luaFiles.Add(path);
			AddLuaDeclarations(path);
			
			if(!string.IsNullOrEmpty(lastAddedLuaFile))
			{
				AddLuaDeclarations(lastAddedLuaFile);
			}

			lastAddedLuaFile = path;
		}

		/// <summary>
		/// Removes a FrameXML file from the list of files to be parsed.
		/// </summary>
		/// <param name="path">The path to the file.</param>
		public void RemoveFrameXmlFile(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			if (frameXmlFiles.Contains(path))
			{
				frameXmlFiles.Remove(path);
				frameXmlDeclarationProviders.Remove(path);
			}
		}

		/// <summary>
		/// Removes a Lua file from the list of files to be parsed.
		/// </summary>
		/// <param name="path">The path to the file.</param>
		public void RemoveLuaFile(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			if (luaFiles.Contains(path))
			{
				luaFiles.Remove(path);
				luaFileDeclarationProviders.Remove(path);
			}

			if (string.Compare(path, lastAddedLuaFile, StringComparison.OrdinalIgnoreCase) == 0)
				lastAddedLuaFile = string.Empty;
		}

		/// <summary>
		/// Clears all files from the list of files to be parsed.
		/// </summary>
		public void Clear()
		{
			frameXmlFiles.Clear();
			luaFiles.Clear();
			frameXmlDeclarationProviders.Clear();
			luaFileDeclarationProviders.Clear();
		}


		/// <summary>
		/// Gets the format filter list for the language.
		/// </summary>
		/// <returns></returns>
		public override string GetFormatFilterList()
		{
			return "Lua File (*.lua)\n*.lua";
		}

		/// <summary>
		/// Creates the source for the buffer.
		/// </summary>
		/// <param name="buffer">The buffer.</param>
		/// <returns>An instance of the <see cref="LuaSource"/> class.</returns>
		public override Microsoft.VisualStudio.Package.Source CreateSource(IVsTextLines buffer)
		{
			return new LuaSource(this, buffer, GetColorizer(buffer));
		}


		/// <summary>
		/// Gets the FileCodeModel associated with current ProjectItem.
		/// </summary>
		/// <returns>LuaFileCodeModel</returns>
		public LuaFileCodeModel GetFileCodeModel()
		{
			LuaFileCodeModel codeModel = null;

			//Retrieve FileCodeModel from ProjectItem
			if (DTE != null)
			{
				codeModel = DTE.ActiveDocument.ProjectItem.FileCodeModel as LuaFileCodeModel;

                // LiXizhi: codeModel is always null, need to register a code model somewhere. 
				if (codeModel != null && !codeModel.ModelInitialized)
				{
					//Retrieve TextDocument from ProjectItem
					var td = ((TextDocument) DTE.ActiveDocument.ProjectItem.Document.Object("TextDocument"));
					EditPoint ep = td.CreateEditPoint(td.StartPoint);
					string text = ep.GetText(td.EndPoint);
					//Initialize FileCodeModel with parsed source code
					codeModel.Initialize(ParseSource(text));
				}
			}
			return codeModel;
		}

		/// <summary>
		/// Gets the FileCodeModel associated with specified ProjectItem.
		/// </summary>
		/// <param name="projectItem">Wow ProjectItem instance.</param>
		/// <returns>LuaFileCodeModel</returns>
		public LuaFileCodeModel GetFileCodeModel(ProjectItem projectItem)
		{
			if (projectItem == null)
				throw new ArgumentNullException("projectItem");

			if (!IsLuaFile(projectItem.Name)) return null;

			if (projectItem == DTE.ActiveDocument.ProjectItem)
				return GetFileCodeModel();

			//Retrieve FileCodeModel from ProjectItem
			var codeModel = projectItem.FileCodeModel as LuaFileCodeModel;
			string filePath = GetFilePath(projectItem);

			if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
			{
				if (codeModel != null && !codeModel.ModelInitialized)
				{
					//Retrieve source code from file
					string text = GetSourceFromFile(filePath);
					//Initialize FileCodeModel with parsed source code.
					codeModel.Initialize(ParseSource(text));
				}
			}
			return codeModel;
		}

		/// <summary>
		/// Gets source code from file.
		/// </summary>
		/// <param name="fileName">Lua Source file name.</param>
		/// <returns></returns>
		private static string GetSourceFromFile(string fileName)
		{
			if (String.IsNullOrEmpty(fileName))
				throw new ArgumentNullException("fileName");

			//Open source file and read all code lines into a stream
			using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}

		/// <summary>
		/// Parses the specified source code.
		/// </summary>
		/// <param name="source">Lua Source code.</param>
		/// <returns></returns>
		private static Chunk ParseSource(string source)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			//Create a parser for the request
			LuaParser parser = LanguageService.CreateParser((ParseRequest)null);

			// Set the source
			((LuaScanner)parser.scanner).SetSource(source, 0);

			// Trigger the parse (hidden region and errors will be added to the AuthoringSink)
			parser.Parse();

			return parser.Chunk;
		}

		/// <summary>
		/// Check Wow Lua file.
		/// </summary>
		/// <param name="fileName">Lua code file name.</param>
		/// <returns></returns>
		public static bool IsLuaFile(string fileName)
		{
			if (String.IsNullOrEmpty(fileName))
				return false;

			return (String.Compare(Path.GetExtension(fileName), ".lua", StringComparison.OrdinalIgnoreCase) == 0);
		}

		/// <summary>
		/// Gets the physical file path of ProjectItem.
		/// </summary>
		/// <param name="projectItem">Wow ProjectItem instance.</param>
		/// <returns></returns>
		private static string GetFilePath(ProjectItem projectItem)
		{
			if (projectItem == null)
				throw new ArgumentNullException("projectItem");

			if (projectItem.Properties == null)
				return string.Empty;

			object value = projectItem.Properties.Item("FullPath").Value;

			return (string)value;
		}

		/// <summary>
		/// Processes a Parse request.
		/// </summary>
		/// <param name="request">The request to process.</param>
		/// <returns>An AuthoringScope containing the declarations and other information.</returns>
		public override Microsoft.VisualStudio.Package.AuthoringScope ParseSource(ParseRequest request)
		{
			Trace.WriteLine(request.Reason);
			authoringScope.Clear();

            if (request.ShouldParse())
			{
				// Make sure we are processing hidden regions
				request.Sink.ProcessHiddenRegions = true;

				// Create a parser for the request, and execute parsing...
				bool successfulParse;
				LuaParser parser = TriggerParse(request, out successfulParse);
				InitializeFileCodeModel(parser.Chunk);

				if (successfulParse)
				{
					RefreshDeclarationsForRequest(request, parser, true);
				}

				if (request.NeedsDeclarations())
				{
					AddDeclarationProvidersToScope(request);

					if (request.NeedsQualifiedName())
					{
						authoringScope.SetQualifiedName(GetLastWord(request));
					}
				}
			}
            else 
            {
                if(request.Reason == ParseReason.QuickInfo)
                {
                    // TODO: mouse over a text to display some info. 
                }
            }

			// Return authoring scope
			return authoringScope;
		}

		private void RefreshDeclarationsForRequest(ParseRequest request, LuaParser parser, bool addLocals)
		{
			luaFileDeclarationProviders[request.FileName] = new TableDeclarationProvider();
			// Create an AST declaration parser to add the declarations from the parsed chunk
			var declarationParser = new AstDeclarationParser(luaFileDeclarationProviders[request.FileName]);

			// Parse the AST and add the declaarations
			if(addLocals)
				declarationParser.AddChunk(parser.Chunk, request.Line, request.Col);
			else
				declarationParser.AddChunk(parser.Chunk);
		}

		/// <summary>
		/// Triggers the parse.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="yyresult">if set to <c>true</c> [yyresult].</param>
		/// <returns></returns>
		private static LuaParser TriggerParse(ParseRequest request, out bool yyresult)
		{
			LuaParser parser = CreateParser(request);
			yyresult = parser.Parse();
			return parser;
		}

		/// <summary>
		/// Adds the declaration providers to scope.
		/// </summary>
		/// <param name="request">The request.</param>
		private void AddDeclarationProvidersToScope(ParseRequest request)
		{
			AddStaticDeclarationProvidersToScope(request);
			AddDynamicDeclarationProvidersToScope(request);
		}

		/// <summary>
		/// Adds the dynamic declaration providers to scope.
		/// </summary>
		/// <param name="request">The request.</param>
		private void AddDynamicDeclarationProvidersToScope(ParseRequest request)
		{
			AddTableProvidersFromDictionary(luaFileDeclarationProviders);
			AddTableProvidersFromDictionary(frameXmlDeclarationProviders);
		}

		/// <summary>
		/// Adds the table providers from dictionary.
		/// </summary>
		/// <param name="providers">The providers.</param>
		private void AddTableProvidersFromDictionary(IEnumerable<KeyValuePair<string, TableDeclarationProvider>> providers)
		{
			foreach (var provider in providers)
			{
				authoringScope.AddProvider(provider.Value);
			}
		}

		/// <summary>
		/// Adds the static declaration providers to scope.
		/// </summary>
		/// <param name="request">The request.</param>
		private void AddStaticDeclarationProvidersToScope(ParseRequest request)
		{
			if (request.Reason == ParseReason.CompleteWord)
			{
				authoringScope.AddProvider(keywordDeclarationProvider);
				authoringScope.AddProvider(snippetDeclarationProvider);
			}

			authoringScope.AddProvider(xmlDeclarationProvider);
		}

		/// <summary>
		/// Initialize LuaFileCodeModel from Chunk
		/// </summary>
		/// <param name="chunk">Lua Chunk from the parser.</param>
		private void InitializeFileCodeModel(Chunk chunk)
		{
			var DTE = GetService(typeof(DTE)) as DTE2;
			if(DTE == null) throw new InvalidOperationException("DTE is not available!");

			var codeModel = DTE.ActiveDocument.ProjectItem.FileCodeModel as LuaFileCodeModel;

            if (codeModel != null)
            {
                try
                {
                    codeModel.Initialize(chunk);
                    NotifyOnFileCodeModelChanged();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
            }
            else
            {
                if(DTE.ActiveDocument.ProjectItem.FileCodeModel!=null)
                {
                    string sLang = DTE.ActiveDocument.ProjectItem.FileCodeModel.Language;
                }
            }
		}

		/// <summary>
		/// Adds the lua declarations.
		/// </summary>
		/// <param name="luaFile">The lua file.</param>
		private void AddLuaDeclarations(string luaFile)
		{
			// Make sure file exists and skip the one that triggered the request
			if (File.Exists(luaFile))
			{
				LuaParser parser;

				// Try to get a source for the file
				Source fileSource = (Source)GetSource(luaFile);

				if (fileSource != null)
					parser = LanguageService.CreateParser(fileSource);
				else
					parser = LanguageService.CreateParser(File.ReadAllText(luaFile));

				// Trigger the parse
				bool yyresult = parser.Parse();

				if (yyresult)
				{
					luaFileDeclarationProviders[luaFile] = new TableDeclarationProvider();
					AstDeclarationParser declarationParser = new AstDeclarationParser(luaFileDeclarationProviders[luaFile]);
					// Parse the AST and add the declaarations
					declarationParser.AddChunk(parser.Chunk);
				}
			}
		}

		/// <summary>
		/// Adds the frame XML declarations.
		/// </summary>
		/// <param name="frameXmlFile">The frame XML file.</param>
		private void AddFrameXmlDeclarations(string frameXmlFile)
		{
			frameXmlDeclarationProviders[frameXmlFile] = new TableDeclarationProvider();

			FrameXmlDeclarationParser frameXmlDeclarationParser =
				new FrameXmlDeclarationParser(frameXmlDeclarationProviders[frameXmlFile]);
			ParseFrameXml(frameXmlDeclarationParser, frameXmlFile);
		}

		/// <summary>
		/// Parses the frame XML.
		/// </summary>
		/// <param name="frameXmlDeclarationParser">The frame XML declaration parser.</param>
		/// <param name="frameXmlFile">The frame XML file.</param>
		private void ParseFrameXml(FrameXmlDeclarationParser frameXmlDeclarationParser, string frameXmlFile)
		{
			// Get the running document table service
			var rdt = (IVsRunningDocumentTable)GetService(typeof(SVsRunningDocumentTable));

			IVsHierarchy hierarchy;
			IntPtr docData;
			uint itemid, dwCookie;

			// Try to retrieve current content through the running document table
			ErrorHandler.ThrowOnFailure(rdt.FindAndLockDocument(0, frameXmlFile, out hierarchy,
																					   out itemid, out docData,
																					   out dwCookie));

			// Check if we got a docdata
			if (docData != IntPtr.Zero)
			{
				// Query the docdata for IVsTextLines
				object dataObject = Marshal.GetObjectForIUnknown(docData);
				if (dataObject != null && dataObject is IVsTextLines)
				{
					var textLines = (IVsTextLines)dataObject;

					// Get the contents of the file
					string text = GetText(textLines);

					// Add the frame XML
					frameXmlDeclarationParser.AddFrameXmlText(text);
				}
			}
			else
			{
				// Could not get docdata, just add file
				frameXmlDeclarationParser.AddFrameXml(frameXmlFile);
			}
		}

		/// <summary>
		/// Gets the text.
		/// </summary>
		/// <param name="textLines">The text lines.</param>
		/// <returns></returns>
		private static string GetText(IVsTextLines textLines)
		{
			int line, index;
			string buffer;

			if (textLines.GetLastLineIndex(out line, out index) != VSConstants.S_OK)
				return String.Empty;
			if (textLines.GetLineText(0, 0, line, index, out buffer) != VSConstants.S_OK)
				return String.Empty;

			return buffer;
		}

		/// <summary>
		/// Creates the parser.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns></returns>
		private static LuaParser CreateParser(ParseRequest request)
		{
			// Create ErrorHandler for scanner
			var handler = new ParaEngine.Tools.Lua.Parser.ErrorHandler();

			// Create scanner and parser
            LuaScanner scanner = new ParaEngine.Tools.Lua.Lexer.Scanner();
			LuaParser parser = new LuaParser();

			// Set the error handler for the scanner
			scanner.Handler = handler;

			// Associate the scanner with the parser
			parser.scanner = scanner;

			// Initialize with the request (can be null)
			parser.Request = request;

			// If the parser is created for a request, automatically set the source from the request
			if (request != null)
				scanner.SetSource(request.Text, 0);

			// Return the parser
			return parser;
		}

		/// <summary>
		/// Creates the parser.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <returns></returns>
		private static LuaParser CreateParser(Source source)
		{
			// Create the parser without a request
			LuaParser parser = LanguageService.CreateParser((ParseRequest)null);

			// Set the source
			((LuaScanner)parser.scanner).SetSource(source.GetText(), 0);

			// Return the parser
			return parser;
		}

		/// <summary>
		/// Creates the parser.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns></returns>
		private static LuaParser CreateParser(string text)
		{
			// Create the parser without a request
			LuaParser parser = LanguageService.CreateParser((ParseRequest)null);

			// Set the source
			((LuaScanner)parser.scanner).SetSource(text, 0);

			// Return the parser
			return parser;
		}

		/// <summary>
		/// Gets the last word.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns></returns>
		private static string GetLastWord(ParseRequest request)
		{
			string line;
			request.View.GetTextStream(request.Line, 0, request.Line, request.TokenInfo.EndIndex, out line);

			if (line != null)
			{
				// Find the delimiter before the last word
				int lastDelimiterIndex = line.LastIndexOfAny(lastWordDelimiters);

				// Get the last word before the caret
				return lastDelimiterIndex != -1 ? line.Substring(lastDelimiterIndex + 1) : line;
			}

			return String.Empty;
		}

		/// <summary>
		/// Gets the install directory.
		/// </summary>
		/// <returns></returns>
		private string GetInstallDirectory()
		{
            string installPath = null;
            // added by LiXizhi, 2008.10.14
            // use the ParaEngine SDK path
#if DEBUG
            //return null;
            installPath = System.IO.Directory.GetCurrentDirectory();
            if (!string.IsNullOrEmpty(installPath))
            {
                int nIndex = installPath.LastIndexOf("\\");
                if (nIndex > 0)
                {
                    return installPath.Remove(nIndex);
                }
            }
            return installPath;
#else
            // use the module path, if previous path does not exist. 
            String sPackagePath = "Software\\Microsoft\\VisualStudio\\9.0\\Packages\\{";
            sPackagePath += GuidStrings.LuaLanguageServicePackage;
            sPackagePath += "}";
            using (RegistryKey setupKey = Registry.LocalMachine.OpenSubKey(
                 sPackagePath))
            {
                if (setupKey != null)
                {
                    installPath = setupKey.GetValue("CodeBase").ToString();
                    if (!string.IsNullOrEmpty(installPath))
                    {
                        int nIndex = installPath.LastIndexOf("\\");
                        if (nIndex > 0)
                        {
                            return installPath.Remove(nIndex);
                        }
                    }
                }
            }
#endif

            //// Get the IVsShell service
            //IVsShell shell = (IVsShell)GetService(typeof(IVsShell));

            //if (shell != null)
            //{
            //    // Retrieve the install directory from the shell
            //    object installDirectoryValue;
            //    shell.GetProperty((int)__VSSPROPID.VSSPROPID_InstallDirectory, out installDirectoryValue);

            //    if (installDirectoryValue != null)
            //        return (string)installDirectoryValue;
            //}

			return null;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, 
		/// releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			try
			{
				//Unsubscribe all attached event listeners
				fileCodeModelChangedEvent = null;
			}
			finally
			{
				base.Dispose();
			}
		}


        /// <summary>
        /// add by LiXizhi, since we need to set breakpoint on it. 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="line"></param>
        /// <param name="col"></param>
        /// <param name="pCodeSpan"></param>
        /// <returns></returns>
        public override int ValidateBreakpointLocation(IVsTextBuffer buffer, int line, int col, TextSpan[] pCodeSpan)
        {
            if (pCodeSpan != null)
            {
                // Make sure the span is set to at least the current
                // position by default.
                pCodeSpan[0].iStartLine = line;
                pCodeSpan[0].iStartIndex = col;
                pCodeSpan[0].iEndLine = line;
                pCodeSpan[0].iEndIndex = col;
            }
            return VSConstants.S_OK;
        }

        //public override int GetNameOfLocation(IVsTextBuffer buffer, int line, int col, out string name, out int lineOffset)
        //{
        //    name = "i";
        //    lineOffset = line;
        //    //return VSConstants.S_FALSE;
        //    return VSConstants.S_OK;
        //}
        //public override int ResolveName(string name, uint flags, out IVsEnumDebugName ppNames)
        //{
        //    ppNames = null;
        //    return VSConstants.E_NOTIMPL;
        //}
        //public override int GetProximityExpressions(IVsTextBuffer buffer, int line, int col, int cLines, out IVsEnumBSTR ppEnum)
        //{
        //    return VSConstants.S_OK;
        //}
	}

	/// <summary>
	/// 
	/// </summary>
	internal static class LanguageServiceExtensions
	{
		/// <summary>
		/// Shoulds the parse.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns></returns>
		public static bool ShouldParse(this ParseRequest request)
		{
			return NeedsChecking(request) || NeedsBraceMatching(request) ||
				   NeedsDeclarations(request);
		}

		/// <summary>
		/// Needses the checking.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns></returns>
		public static bool NeedsChecking(this ParseRequest request)
		{
			return (request.Reason == ParseReason.Check);
		}

		/// <summary>
		/// Needses the brace matching.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns></returns>
		public static bool NeedsBraceMatching(this ParseRequest request)
		{
			return (request.Reason == ParseReason.HighlightBraces ||
				request.Reason == ParseReason.MatchBraces);
		}

		/// <summary>
		/// Needses the name of the qualified.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns></returns>
		public static bool NeedsQualifiedName(this ParseRequest request)
		{
			return (request.Reason == ParseReason.MemberSelect ||
					request.Reason == ParseReason.MemberSelectAndHighlightBraces);
		}

		/// <summary>
		/// Needses the declarations.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns></returns>
		public static bool NeedsDeclarations(this ParseRequest request)
		{
			return NeedsQualifiedName(request) || (request.Reason == ParseReason.CompleteWord ||
					request.Reason == ParseReason.MethodTip);
		}
	}
}