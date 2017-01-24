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
using ParaEngine.Tools.Lua.CodeDom.Providers;
using ParaEngine.Tools.Services;
using Microsoft.Win32;
using Microsoft;

using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using LuaParser = ParaEngine.Tools.Lua.Parser.Parser;
using Source = ParaEngine.Tools.Lua.Parser.Source;
using LuaScanner = ParaEngine.Tools.Lua.Parser.LuaScanner;
using ParaEngine.NPLLanguageService;
using System.Text.RegularExpressions;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Collections.Specialized;
using System.Threading.Tasks;

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

        static bool bOneTimeInited = false;
        /// <summary>
        /// Initializes the Language Service and loads the documentation.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // Initialize providers
            snippetDeclarationProvider = new SnippetDeclarationProvider(this);
            keywordDeclarationProvider = new KeywordDeclarationProvider();
            xmlDeclarationProvider = new TableDeclarationProvider();

            // By Xizhi: we will also load ${SolutionDir}/Documentation/*.xml when opening a new solution file.  
            LoadXmlDocumentation();
            authoringScope = new DeclarationAuthoringScope(this);
            DTE = GetService(typeof(DTE)) as DTE2;

            if (!bOneTimeInited)
            {
                bOneTimeInited = true;
                OneTimeInitialize();
            }
        }

        public void OneTimeInitialize()
        {
            // By Xizhi: fixed a bug that custom colors are not displayed
            IVsFontAndColorCacheManager mgr = this.GetService(typeof(SVsFontAndColorCacheManager)) as IVsFontAndColorCacheManager;
            mgr.ClearAllCaches();
        }

        /// <summary>
        /// Loads the XML documentation.
        /// </summary>
        public void LoadXmlDocumentation(string documentationRootPath = null)
        {
            if (xmlDeclarationProvider == null)
            {
                xmlDeclarationProvider = new TableDeclarationProvider();
            }
            // Retrieve install directory
            if (documentationRootPath == null)
            {
                // documentationRootPath = ParaEngine.NPLLanguageService.NPLLanguageServicePackage.PackageRootPath;
                documentationRootPath = ObtainInstallationFolder() + "\\";
            }

            if (documentationRootPath != null)
            {
                try
                {
                    documentationRootPath += "Documentation";
                    int nFileCount = 0;
                    //Look for XML files and load them using the XML documentation loader
                    foreach (string path in Directory.GetFiles(documentationRootPath, "*.xml"))
                    {
                        nFileCount++;
                        xmlDocumentationLoader.LoadXml(path);
                    }
                    WriteOutput(String.Format("Load {0} NPL doc file(s) in folder: {1}", nFileCount, documentationRootPath));
                    xmlDocumentationLoader.AddDeclarations(xmlDeclarationProvider);
                }
                catch (Exception)
                {

                }
            }
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
                commandFilter.SetTextView(textView);
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
        /// write a line of text to NPL output panel
        /// </summary>
        /// <param name="text"></param>
        public void WriteOutput(String text)
        {
            CreateGetOutputPane("NPL").OutputString(text + "\n");
        }

        private OutputWindowPane CreateGetOutputPane(string title)
        {
            DTE2 dte = (DTE2)GetService(typeof(DTE));
            OutputWindowPanes panes = dte.ToolWindows.OutputWindow.OutputWindowPanes;
            try
            {
                // If the pane exists already, return it.
                return panes.Item(title);
            }
            catch (ArgumentException)
            {
                // Create a new pane.
                return panes.Add(title);
            }
        }

        public override TypeAndMemberDropdownBars CreateDropDownHelper(IVsTextView forView)
        {
            return new NPLTypeAndMemberDropdownBars(this);
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

            if (!string.IsNullOrEmpty(lastAddedLuaFile))
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

        public TableDeclarationProvider GetFileDeclarationProvider(string path)
        {
            if (luaFileDeclarationProviders.ContainsKey(path))
                return luaFileDeclarationProviders[path];
            else
                return null;
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
            return "Lua File (*.lua)\n*.lua\nNPL Page File (*.page)\n*.page\nNPL File (*.npl)\n*.npl";
        }

        /// <summary>
        /// Turn the key and value pairs into a multipart form
        /// </summary>
        private static string MakeMultipartForm(Dictionary<string, string> values, string boundary)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var pair in values)
            {
                sb.AppendFormat("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n", boundary, pair.Key, pair.Value);
            }

            sb.AppendFormat("--{0}--\r\n", boundary);

            return sb.ToString();
        }

        public bool IsKnownFileName()
        {
            if (DTE != null)
            {
                try
                {
                    String sFileName = DTE.ActiveDocument.FullName;
                    if (sFileName.EndsWith(".lua") || sFileName.EndsWith(".npl") || sFileName.EndsWith(".page"))
                        return true;
                }
                catch (Exception)
                {
                }
            }
            return false;
        }

        async public Task<int> SetBreakPointAtCurrentLine()
        {
            //Retrieve TextDocument from ProjectItem
            if (DTE != null)
            {
                try
                {
                    String sFileName = DTE.ActiveDocument.FullName;
                    var ts = DTE.ActiveDocument.Selection as TextSelection;
                    int lineNumber = ts.CurrentLine;
                    // System.Windows.MessageBox.Show("Set breakpoint here..." + sFileName + lineNumber.ToString());
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri("http://127.0.0.1:8099/");
                        var content = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("action", "addbreakpoint"),
                            new KeyValuePair<string, string>("filename", sFileName),
                            new KeyValuePair<string, string>("line", lineNumber.ToString()),
                        });
                        var result = client.PostAsync("/ajax/debugger", content).Result;
                        var task = result.Content.ReadAsStringAsync();
                        task.ContinueWith((t) => {
                            if (t.IsFaulted || t.IsCanceled)
                            {
                                if (System.Windows.MessageBox.Show("Please start your NPL process first \nand start NPL Code Wiki at: \n http://127.0.0.1:8099/ \nDo you want to see help page?", "NPL HTTP Debugger", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes)
                                {
                                    System.Diagnostics.Process.Start("https://github.com/LiXizhi/NPLRuntime/wiki/NPLCodeWiki");
                                }
                            }
                            else
                            {
                                // completed successfully
                                string url = "http://127.0.0.1:8099/debugger";
                                // url += string.Format("?filename={0}&line={1}", sFileName, lineNumber);
                                System.Diagnostics.Process.Start(url);
                            }
                        });
                    }
                }
                catch (Exception e)
                {
                    WriteOutput(e.Message);
                    if (System.Windows.MessageBox.Show("Please start your NPL process first \nand start NPL Code Wiki at: \n http://127.0.0.1:8099/ \nDo you want to see help page?", "NPL HTTP Debugger", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start("https://github.com/LiXizhi/NPLRuntime/wiki/NPLCodeWiki");
                    }
                }
            }
            return 0;
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
                String sFileName = DTE.ActiveDocument.ProjectItem.get_FileNames(1);
                codeModel = DTE.ActiveDocument.ProjectItem.FileCodeModel as LuaFileCodeModel;

                // LiXizhi: codeModel is always null, need to register a code model somewhere. 
                // we will dynamically create code model based on file extension even no File Code Model is found. 
                if (codeModel == null)
                {
                    LuaCodeDomProvider domProvider = new LuaCodeDomProvider(DTE.ActiveDocument.ProjectItem);
                    codeModel = LuaCodeModelFactory.CreateFileCodeModel(DTE.ActiveDocument.ProjectItem, domProvider, sFileName) as LuaFileCodeModel;
                }

                if (codeModel != null && !codeModel.ModelInitialized)
                {
                    string text = null;
                    try
                    {
                        //Retrieve TextDocument from ProjectItem
                        var td = ((TextDocument)DTE.ActiveDocument.ProjectItem.Document.Object("TextDocument"));
                        EditPoint ep = td.CreateEditPoint(td.StartPoint);
                        text = ep.GetText(td.EndPoint);
                    }
                    catch (Exception)
                    {
                        // open external file if file does not belong to project. 
                        System.IO.StreamReader fileReader = new System.IO.StreamReader(sFileName);
                        text = fileReader.ReadToEnd();
                        fileReader.Close();
                    }
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
        /// thread-safty: can only be called from main thread
        /// </summary>
        /// <param name="request"></param>
        /// <param name="span"></param>
        /// <returns></returns>
        private string FindWordInfo(ParseRequest request, out TextSpan span)
        {
            string sWord = null;
            TextSpan[] wordSpan = { new TextSpan() };
            int nColFrom = 0;
            int nColTo = 0;
            if (VSConstants.S_OK == request.View.GetWordExtent(request.Line, request.Col, (int)WORDEXTFLAGS.WORDEXT_FINDWORD, wordSpan))
            {
                nColFrom = wordSpan[0].iStartIndex;
                nColTo = wordSpan[0].iEndIndex;
                request.View.GetTextStream(request.Line, nColFrom, request.Line, nColTo, out sWord);
            }
            span = wordSpan[0];
            return sWord;
        }

        //FIXME:a bug exist when getting the text of last line
        private string GetLineText(ParseRequest request)
        {
            string sLine = null;
            int nLineStart = 0;
            int nLineEnd = 0;
            int nLineCount = 0;
            int nLength = request.Text.Length;
            for (int i = 0; i < nLength; ++i)
            {
                char c = request.Text[i];
                if (c == '\n')
                {
                    nLineStart = nLineEnd == 0 ? 0 : nLineEnd + 1;
                    nLineEnd = i;
                    if (nLineCount++ == request.Line)
                    {
                        sLine = request.Text.Substring(nLineStart, nLineEnd - nLineStart);
                        if (sLine.Length > 0 && sLine[sLine.Length - 1] == '\r')
                            sLine = sLine.Substring(0, sLine.Length - 1);
                        break;
                    }
                }
            }
            return sLine;
        }

        private void BuildQuickInfoString(TableDeclarationProvider declarations, string sWord, StringBuilder info, string sPrefix = null)
        {
            if (declarations != null)
            {
                foreach (var method in declarations.FindMethods(sWord))
                {
                    info.AppendFormat("{0}{1}",
                        info.Length == 0 ? "" : "\n-------------------\n",
                        method.Value.GetQuickInfo(!String.IsNullOrEmpty(sPrefix) ? sPrefix : (String.IsNullOrEmpty(method.Key) ? "" : method.Key + ".")));
                }
            }
        }

        static bool IsFunctionWordChar(char cChar)
        {
            return Char.IsLetterOrDigit(cChar) || cChar == '_';
        }

        private string GetWordFromRequest(ParseRequest request, out int nColFrom, out int nColTo)
        {
            string sLine = GetLineText(request);
            nColFrom = request.Col - 1;     //Zhiyuan, 2017-1-19, fixed a small bug
            nColTo = nColFrom;

            if (sLine != null && nColFrom >= 0 && nColFrom < sLine.Length)
            {
                char cChar = sLine[nColFrom];   

                if (IsFunctionWordChar(cChar))
                {
                    for (int i = nColFrom - 1; i >= 0; i--)
                    {
                        if (IsFunctionWordChar(sLine[i]))
                            nColFrom = i;
                        else
                            break;
                    }
                    for (int i = nColTo + 1; i < sLine.Length; i++)
                    {
                        if (IsFunctionWordChar(sLine[i]))
                            nColTo = i;
                        else
                            break;
                    }
                    string sWord = sLine.Substring(nColFrom, nColTo - nColFrom + 1);
                    if (sWord.Length > 0 && Char.IsLetter(sWord[0]))
                    {
                        return sWord;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// mouse over a text to display some info. 
        /// called from parser thread
        /// </summary>
        /// <param name="request"></param>
        private void FindQuickInfo(ParseRequest request)
        {
            if (request.Reason != ParseReason.QuickInfo)
                return;
            authoringScope.m_quickInfoText = "";
            int nColFrom, nColTo;
            string sWord = GetWordFromRequest(request, out nColFrom, out nColTo);
            if (sWord != null && sWord.Length > 0)
            {
                StringBuilder info = new StringBuilder();

                BuildQuickInfoString(GetFileDeclarationProvider(request.FileName), sWord, info, "this_file: ");

                foreach (var declareProvider in luaFileDeclarationProviders)
                {
                    if (request.FileName != declareProvider.Key)
                        BuildQuickInfoString(declareProvider.Value, sWord, info, Path.GetFileName(declareProvider.Key) + ": ");
                }

                BuildQuickInfoString(xmlDeclarationProvider, sWord, info);

                if (info.Length > 0)
                {
                    authoringScope.m_quickInfoText = info.ToString();
                    TextSpan span = new TextSpan();
                    span.iStartLine = request.Line;
                    span.iEndLine = request.Line;
                    span.iStartIndex = nColFrom;
                    span.iEndIndex = nColTo;
                    authoringScope.m_quickInfoSpan = span;
                }
            }
        }

        private string FindDocFileInDir(string filename, string solutionDir)
        {
            string fullpath = solutionDir + "\\" + filename;
            if (File.Exists(fullpath))
                return fullpath;
            else
            {
                fullpath = solutionDir + "\\src\\" + filename;
                if (File.Exists(fullpath))
                    return fullpath;
                else
                {
                    fullpath = solutionDir + "\\Documentation\\" + filename;
                    if (File.Exists(fullpath))
                        return fullpath;
                }
            }
            return null;
        }

        /// <summary>
        /// we will search in current solution directory, and `./src` and `./Documentation` directory. 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string GetAbsolutionFilePath(string filename)
        {
            if (!File.Exists(filename) && !filename.Contains(":"))
            {
                DTE2 dte = (DTE2)GetService(typeof(DTE));
                string solutionDir = System.IO.Path.GetDirectoryName(DTE.Solution.FullName);

                string fullname = FindDocFileInDir(filename, solutionDir);
                if (fullname == null)
                {
                    foreach (Project proj in dte.Solution.Projects)
                    {
                        string projDir = System.IO.Path.GetDirectoryName(proj.FullName);
                        if (projDir != null && projDir != solutionDir)
                        {
                            fullname = FindDocFileInDir(filename, projDir);
                            if (fullname != null)
                            {
                                return fullname;
                            }
                        }
                    }
                }
            }
            return filename;
        }
        private bool BuildGotoDefinitionUri(TableDeclarationProvider declarations, DeclarationAuthoringScope authoringScope, string sWord)
        {
            if (declarations != null)
            {
                foreach (var method in declarations.FindDeclarations(sWord, true))
                {
                    var func = method.Value;
                    if (func.FilenameDefinedIn != null)
                    {
                        authoringScope.m_goto_filename = GetAbsolutionFilePath(func.FilenameDefinedIn);
                        authoringScope.m_goto_textspan.iEndLine = authoringScope.m_goto_textspan.iStartLine = func.TextspanDefinedIn.sLin;
                        // authoringScope.m_goto_textspan.iEndLine = func.TextspanDefinedIn.eLin;
                        authoringScope.m_goto_textspan.iEndIndex = authoringScope.m_goto_textspan.iStartIndex = func.TextspanDefinedIn.sCol;
                        // authoringScope.m_goto_textspan.iEndIndex = func.TextspanDefinedIn.eCol;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// in parser thread, find the goto uri and location.
        /// </summary>
        /// <param name="request"></param>
        private bool FindGotoDefinition(ParseRequest request)
        {
            if (request.Reason != ParseReason.Goto)
                return false;
            authoringScope.m_goto_filename = null;
            authoringScope.m_goto_textspan = new TextSpan();

            int nColFrom, nColTo;
            string sWord = GetWordFromRequest(request, out nColFrom, out nColTo);
            if (sWord != null && sWord.Length > 0)
            {
                if (!BuildGotoDefinitionUri(GetFileDeclarationProvider(request.FileName), authoringScope, sWord))
                {
                    foreach (var declareProvider in luaFileDeclarationProviders)
                    {
                        if (request.FileName != declareProvider.Key)
                            if (BuildGotoDefinitionUri(declareProvider.Value, authoringScope, sWord))
                                return true;
                    }
                    if (BuildGotoDefinitionUri(xmlDeclarationProvider, authoringScope, sWord))
                        return true;
                }
                else
                    return true;
            }

            // look for NPL.load and implement open file
            string sLine = GetLineText(request);
            if (sLine != null)
            {
                if (sLine.Contains("NPL.load("))
                {
                    // we will goto the file specified in NPL.load
                    Regex reg = new Regex("NPL\\.load\\(\"([^\"]+)\"\\)");
                    Match m = reg.Match(sLine);
                    if (m.Success && m.Groups.Count >= 2)
                    {
                        string sFilename = m.Groups[1].Value;
                        if (sFilename.StartsWith("("))
                        {
                            int nIndex = sFilename.IndexOf(')');
                            if (nIndex > 0)
                            {
                                sFilename = sFilename.Substring(nIndex + 1);
                            }
                        }
                        sFilename = GetAbsolutionFilePath(sFilename);
                        if (sFilename != null)
                        {
                            authoringScope.m_goto_filename = sFilename;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Processes a Parse request.
        /// </summary>
        /// <param name="request">The request to process.</param>
        /// <returns>An AuthoringScope containing the declarations and other information.</returns>
        public override Microsoft.VisualStudio.Package.AuthoringScope ParseSource(ParseRequest request)
        {
            if(request.Reason == ParseReason.MethodTip)
                Trace.WriteLine("Parse Reason: " + request.Reason);
            authoringScope.Clear();

            if (request.ShouldParse())
            {
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
                if (request.Reason == ParseReason.QuickInfo)
                {
                    FindQuickInfo(request);
                }
                else if (request.Reason == ParseReason.Goto)
                {
                    FindGotoDefinition(request);
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

            // Parse the AST and add the declarations
            if (addLocals)
                declarationParser.AddChunk(parser.Chunk, request.Line, request.Col, request.FileName);
            else
                declarationParser.AddChunk(parser.Chunk, request.FileName);
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
            if (DTE == null) throw new InvalidOperationException("DTE is not available!");

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
                if (DTE.ActiveDocument.ProjectItem.FileCodeModel != null)
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
                    // Parse the AST and add the declarations
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
            LuaScanner scanner = new LuaScanner();
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

        public static string ObtainInstallationFolder()
        {
            Type packageType = typeof(NPLLanguageServicePackage);
            Uri uri = new Uri(packageType.Assembly.CodeBase);
            var assemblyFileInfo = new FileInfo(uri.LocalPath);
            return assemblyFileInfo.Directory.FullName;
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