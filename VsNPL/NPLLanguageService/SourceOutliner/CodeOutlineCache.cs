/***************************************************************************

Copyright (c) 2006 Microsoft Corporation. All rights reserved.

***************************************************************************/

using System;
using System.Diagnostics;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using ParaEngine.Tools.Lua.CodeDom;
using ParaEngine.Tools.Lua.SourceOutliner.Controls;
using ParaEngine.NPLLanguageService;

namespace ParaEngine.Tools.Lua.SourceOutliner
{
	/// <summary>
	/// Class that caches and manages source files for outlining.
	/// </summary>
	[CLSCompliant(false)]
	public class CodeOutlineCache
	{
		private CodeOutlineFileManager fileManager;
		private readonly SourceOutlinerControl control;
		private readonly DTE dte;
		private CodeModelEvents codeModelEvents;
		private readonly LanguageService languageService;

		// The cached document.
		private Document _document;

		/// <summary>
		/// Initializes a new instance of the CodeOutlineCache class.
		/// </summary>
		/// <param name="control">The SourceOutlinerControl.</param>
		/// <param name="dte">A DTE object exposing the Visual Studio automation object model.</param>
		/// <param name="languageService">The Lua LanguageService.</param>
		public CodeOutlineCache(SourceOutlinerControl control, DTE dte, LanguageService languageService)
		{
			Debug.Assert(control != null);
			Debug.Assert(dte != null);

			this.languageService = languageService;
			this.control = control;
			this.dte = dte;

			// Initialize the events.
			AdviseCodeModelEvents();
		}

		private SourceOutlineToolWindow _toolWindow;

		/// <summary>
		/// Loads a document into display TreeViews, updates the cache, and 
		/// rebuilds the file manager that represents the document.
		/// </summary>
		/// <param name="d">The Document to load.</param>
		/// <param name="tw">The tool window associated with the cache.</param>
		/// <remarks>
		/// If the document is in the cache, it is reused.
		/// </remarks>
		public void AddDocumentToCache(Document d, SourceOutlineToolWindow tw)
		{
			Debug.Assert(d != null);

			_toolWindow = tw;

			if (d == _document)
			{
				return;
			}

			if (_document != null)
			{
				// Unregister events for the previous document.
				tw.UnRegisterTreeEvents(fileManager.TreeView, fileManager.FilterView);
				control.RemoveTreeFromControls(fileManager.TreeView);
				control.RemoveTreeFromControls(fileManager.FilterView);
			}

			_document = d;
			fileManager = new CodeOutlineFileManager(control, dte, d, tw, languageService);

			tw.RegisterTreeEvents(fileManager.TreeView, fileManager.FilterView);

			// Load the control.
			control.AddTreeToControls(fileManager.TreeView);
			control.AddTreeToControls(fileManager.FilterView);
			fileManager.State = CodeOutlineFileManager.OutlineFileManagerState.StartLoadingCodeModel;
			fileManager.HideTrees();

			control.HideTrees();
			control.TreeView = fileManager.TreeView;
			control.FilterView = fileManager.FilterView;

			// Re-display the last CodeElementType selected for this document.
			tw.SelectedType = fileManager.ElementFilter;

			// Re-display the last filter text entered for this document, but only if the file is loaded.
			if (fileManager.State == CodeOutlineFileManager.OutlineFileManagerState.DoneLoadingCodeModel)
			{
				fileManager.ReApplyText();
				tw.SelectedFilterText = fileManager.FilterText;
			}
			else
			{
				control.Reset();
			}
		}

		/// <summary>
		/// Gets the CodeOutlineFileManager object associated with the 
		/// source file currently being displayed and outlined.
		/// </summary>
		/// <returns>
		/// The file manager that represents the document,
		/// or null if no file is being displayed.
		/// </returns>
		public CodeOutlineFileManager CurrentFileManager
		{
			get { return fileManager; }
		}

		/// <summary>
		/// Registers for code model events.
		/// </summary>
		private void AdviseCodeModelEvents()
		{
			try
			{
				languageService.OnFileCodeModelChanged += OnFileCodeModelChanged;

				var dte2 = dte as DTE2;
				if (dte2 == null)
					throw new NullReferenceException("dte2 is NULL");

				var events2 = dte2.Events as Events2;
				if (events2 == null)
					throw new NullReferenceException("events2 is NULL");

				codeModelEvents = events2.get_CodeModelEvents(null);
				if (codeModelEvents != null)
				{
					codeModelEvents.ElementAdded += codeModelEvents_Added;
					codeModelEvents.ElementChanged += codeModelEvents_Changed;
					codeModelEvents.ElementDeleted += codeModelEvents_Deleted;
				}
			}
			catch (ArgumentException)
			{
				// Failed to get CodeModelEvents, this should not occur.
				SourceOutlineToolWindow.DisplayMessage(Resources.ErrorPrefix, "Failed to get Code Model events.");
				throw;
			}
		}

		/// <summary>
		/// Raised when a FileCodeModel object has been changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFileCodeModelChanged(object sender, FileCodeModelChangedEventArgs e)
		{
			try
			{
				if (CurrentFileManager == null)
					return;

				SourceOutlineToolWindow.DisplayMessage(Resources.StatusPrefix, "OnFileCodeModelChanged fired.");
				control.Invoke(new MethodInvoker(ForceReload));
			}
			catch (Exception ex)
			{
				SourceOutlineToolWindow.DisplayMessage(Resources.ErrorPrefix, "OnFileCodeModelChanged exception: " + ex);
			}
		}

		/// <summary>
		/// Rebuilds the document cache.
		/// </summary>
		private void ForceReload()
		{
			Document currentDocument = _document;
			_document = null;
			AddDocumentToCache(currentDocument, _toolWindow);
		}

		/// <summary>
		/// Raised when a CodeElement object has been created.
		/// </summary>
		/// <param name="newElement">The CodeElement object that was added.</param>
		private void codeModelEvents_Added(CodeElement newElement)
		{
			try
			{
				if (CurrentFileManager == null)
					return;

				if (!CurrentFileManager.FileIsOutlined)
				{
					ForceReload();
					return;
				}

				control.ShowWaitWhileReadyMessage();
				fileManager.OnCodeModelElementAdd(newElement);
				control.HideWaitWhileReadyMessage();
			}
			catch (Exception ex)
			{
				SourceOutlineToolWindow.DisplayMessage(Resources.ErrorPrefix, "codeModelEvents_Added exception: " + ex);
			}
		}

		/// <summary>
		/// Raised when a CodeElement object has been deleted.
		/// </summary>
		/// <param name="parent">The parent object for the CodeElement.</param>
		/// <param name="deletedElement">The CodeElement object that was deleted.</param>
		private void codeModelEvents_Deleted(object parent, CodeElement deletedElement)
		{
			try
			{
				if (CurrentFileManager == null)
					return;

				if (!CurrentFileManager.FileIsOutlined)
				{
					ForceReload();
					return;
				}

				fileManager.OnCodeModelElementDeleted(parent, deletedElement);
			}
			catch (Exception ex)
			{
				SourceOutlineToolWindow.DisplayMessage(Resources.ErrorPrefix, "codeModelEvents_Deleted exception: " + ex);
			}
		}

		/// <summary>
		/// Raised when a CodeElement object has been changed.
		/// </summary>
		/// <param name="modifiedElement">The CodeElement that was changed.</param>
		/// <param name="iChangeType">The type of change event that was fired.</param>
		private void codeModelEvents_Changed(CodeElement modifiedElement, vsCMChangeKind iChangeType)
		{
			try
			{
				if (CurrentFileManager == null)
					return;

				ForceReload();
			}
			catch (Exception ex)
			{
				SourceOutlineToolWindow.DisplayMessage(Resources.ErrorPrefix, "codeModelEvents_Changed exception: " + ex);
			}
		}
	}
}