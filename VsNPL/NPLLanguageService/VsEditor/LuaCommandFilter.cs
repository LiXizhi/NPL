using System;
using System.Diagnostics;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.OLE.Interop;
using ParaEngine.Tools.Lua.CodeDom;
using ParaEngine.Tools.Lua.Refactoring.UndoManager;
using ParaEngine.NPLLanguageService;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using MSXML;

namespace ParaEngine.Tools.Lua.VsEditor
{
	/// <summary>
	/// The IOleCommandTarget interface enables objects and their containers 
	/// to dispatch commands to each other.
	/// </summary>
	public class LuaCommandFilter : IOleCommandTarget, IVsExpansionClient
    {
		private readonly LanguageService languageService;
		private static readonly object syncLock = new object();
		private static LuaCommandFilter commandFilter;

        public IVsTextView m_vsTextView;
        public IVsExpansionManager m_exManager;
        public IVsExpansionSession m_exSession = null;


        public void SetTextView(IVsTextView textViewAdapter)
        {
            m_vsTextView = textViewAdapter;
            //get the text manager from the service provider
            IVsTextManager2 textManager = (IVsTextManager2)languageService.GetService(typeof(SVsTextManager));
            textManager.GetExpansionManager(out m_exManager);
            // m_exSession = null;
        }
        #region Expansion Client

        private bool InsertAnyExpansion(string shortcut, string title, string path)
        {
            //first get the location of the caret, and set up a TextSpan
            int endColumn, startLine;
            //get the column number from  the IVsTextView, not the ITextView
            m_vsTextView.GetCaretPos(out startLine, out endColumn);

            TextSpan addSpan = new TextSpan();
            addSpan.iStartIndex = endColumn;
            addSpan.iEndIndex = endColumn;
            addSpan.iStartLine = startLine;
            addSpan.iEndLine = startLine;
            
            if (shortcut != null) //get the expansion from the shortcut
            {
                //reset the TextSpan to the width of the shortcut, 
                //because we're going to replace the shortcut with the expansion
                addSpan.iStartIndex = addSpan.iEndIndex - shortcut.Length;

                m_exManager.GetExpansionByShortcut(
                    this,
                    Guids.LuaLanguageService,
                    shortcut,
                    m_vsTextView,
                    new TextSpan[] { addSpan },
                    0,
                    out path,
                    out title);

            }
            if (title != null && path != null)
            {
                IVsTextLines textLines;
                m_vsTextView.GetBuffer(out textLines);
                IVsExpansion bufferExpansion = (IVsExpansion)textLines;

                if (bufferExpansion != null)
                {
                    int hr = bufferExpansion.InsertNamedExpansion(
                        title,
                        path,
                        addSpan,
                        this,
                        Guids.LuaLanguageService,
                        0,
                       out m_exSession);
                    if (VSConstants.S_OK == hr)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public int OnItemChosen(string pszTitle, string pszPath)
        {
            InsertAnyExpansion(null, pszTitle, pszPath);
            return VSConstants.S_OK;
        }

        public int EndExpansion()
        {
            m_exSession = null;
            return VSConstants.S_OK;
        }

        public int FormatSpan(IVsTextLines pBuffer, TextSpan[] ts)
        {
            return VSConstants.S_OK;
        }

        public int GetExpansionFunction(IXMLDOMNode xmlFunctionNode, string bstrFieldName, out IVsExpansionFunction pFunc)
        {
            pFunc = null;
            return VSConstants.S_OK;
        }

        public int IsValidKind(IVsTextLines pBuffer, TextSpan[] ts, string bstrKind, out int pfIsValidKind)
        {
            pfIsValidKind = 1;
            return VSConstants.S_OK;
        }

        public int IsValidType(IVsTextLines pBuffer, TextSpan[] ts, string[] rgTypes, int iCountTypes, out int pfIsValidType)
        {
            pfIsValidType = 1;
            return VSConstants.S_OK;
        }

        public int OnAfterInsertion(IVsExpansionSession pSession)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeInsertion(IVsExpansionSession pSession)
        {
            return VSConstants.S_OK;
        }

        public int PositionCaretForEditing(IVsTextLines pBuffer, TextSpan[] ts)
        {
            return VSConstants.S_OK;
        }

        #endregion
        /// <summary>
        /// Initializes a new instance of the <see cref="LuaCommandFilter"/> class.
        /// </summary>
        /// <param name="languageService">The Lua LanguageService.</param>
        public LuaCommandFilter(LanguageService languageService)
		{
			this.languageService = languageService;
		}

		/// <summary>
		/// Creates LuaCommandFilter singleton instance.
		/// </summary>
		/// <param name="languageService">The language service.</param>
		/// <returns></returns>
		public static LuaCommandFilter GetCommandFilter(LanguageService languageService)
		{
			lock (syncLock)
			{
				if (commandFilter == null)
					commandFilter = new LuaCommandFilter(languageService);
				return commandFilter;
			}
		}

		/// <summary>
		/// Gets or sets CommandFilter from editor.
		/// </summary>
		public IOleCommandTarget VsCommandFilter { get; set; }

		#region IOleCommandTarget Members

		/// <summary>
		/// Queries the object for the status of one or more commands generated by user interface events.
		/// </summary>
		/// <param name="pguidCmdGroup">Unique identifier of the command group; can be NULL to specify the standard group. All the commands that are passed in the prgCmds array must belong to the group specified by pguidCmdGroup.</param>
		/// <param name="cCmds">The number of commands in the prgCmds array.</param>
		/// <param name="prgCmds">A caller-allocated array of OLECMD structures that indicate the commands for which the caller needs status information. This method fills the cmdf member of each structure with values taken from the OLECMDF enumeration.</param>
		/// <param name="pCmdText">Pointer to an OLECMDTEXT structure in which to return name and/or status information of a single command. Can be NULL to indicate that the caller does not need this information.</param>
		/// <returns>This method supports the standard return values E_FAIL and E_UNEXPECTED, as well as the following: S_OK, E_POINTER, OLECMDERR_E_UNKNOWNGROUP</returns>
		public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
		{
            // add insert snippets context menu
            if (!VsShellUtilities.IsInAutomationFunction(languageService.Site))
            {
                if (pguidCmdGroup == VSConstants.VSStd2K && cCmds > 0)
                {
                    // make the Insert Snippet command appear on the context menu 
                    if ((uint)prgCmds[0].cmdID == (uint)VSConstants.VSStd2KCmdID.INSERTSNIPPET)
                    {
                        prgCmds[0].cmdf = (int)Microsoft.VisualStudio.OLE.Interop.Constants.MSOCMDF_ENABLED | (int)Microsoft.VisualStudio.OLE.Interop.Constants.MSOCMDF_SUPPORTED;
                        return VSConstants.S_OK;
                    }
                }
            }

            return VsCommandFilter.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
		}

		/// <summary>
		/// Executes a specified command or displays help for a command.
		/// </summary>
		/// <param name="pguidCmdGroupRef">Pointer to unique identifier of the command group; can be NULL to specify the standard group.</param>
		/// <param name="nCmdID">The command to be executed. This command must belong to the group specified with pguidCmdGroup.</param>
		/// <param name="nCmdexecopt">Values taken from the OLECMDEXECOPT enumeration, which describe how the object should execute the command.</param>
		/// <param name="pvaIn">Pointer to a VARIANTARG structure containing input arguments. Can be NULL.</param>
		/// <param name="pvaOut">Pointer to a VARIANTARG structure to receive command output. Can be NULL.</param>
		/// <returns>This method supports the standard return values E_FAIL and E_UNEXPECTED, as well as the following:
		///            S_OK
		///                The command was executed successfully.
		///            OLECMDERR_E_UNKNOWNGROUP
		///                The pguidCmdGroup parameter is not NULL but does not specify a recognized command group.
		///            OLECMDERR_E_NOTSUPPORTED
		///                The nCmdID parameter is not a valid command in the group identified by pguidCmdGroup.
		///            OLECMDERR_E_DISABLED
		///                The command identified by nCmdID is currently disabled and cannot be executed.
		///            OLECMDERR_E_NOHELP
		///                The caller has asked for help on the command identified by nCmdID, but no help is available.
		///            OLECMDERR_E_CANCELED
		///                The user canceled the execution of the command.</returns>
		public int Exec(ref Guid pguidCmdGroupRef, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
		{
			const int retval = VSConstants.S_OK;
			string commandId = VSIDECommands.GetCommandId(pguidCmdGroupRef, nCmdID);
            

            //the snippet picker code starts here
            if (nCmdID == (uint)VSConstants.VSStd2KCmdID.INSERTSNIPPET)
            {
                IVsTextManager2 textManager = (IVsTextManager2)languageService.GetService(typeof(SVsTextManager));

                textManager.GetExpansionManager(out m_exManager);

                m_exManager.InvokeInsertionUI(
                    m_vsTextView,
                    this,      //the expansion client
                    Guids.LuaLanguageService,
                    null,       //use all snippet types
                    0,          //number of types (0 for all)
                    0,          //ignored if iCountTypes == 0
                    null,       //use all snippet kinds
                    0,          //use all snippet kinds
                    0,          //ignored if iCountTypes == 0
                    "Lua", //the text to show in the prompt
                    string.Empty);  //only the ENTER key causes insert 

                return VSConstants.S_OK;
            }
            //the expansion insertion is handled in OnItemChosen
            //if the expansion session is still active, handle tab/backtab/return/cancel
            if (m_exSession != null)
            {
                if (nCmdID == (uint)VSConstants.VSStd2KCmdID.BACKTAB)
                {
                    m_exSession.GoToPreviousExpansionField();
                    return VSConstants.S_OK;
                }
                else if (nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB)
                {

                    m_exSession.GoToNextExpansionField(0); //false to support cycling through all the fields
                    return VSConstants.S_OK;
                }
                else if (nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN || nCmdID == (uint)VSConstants.VSStd2KCmdID.CANCEL)
                {
                    if (m_exSession.EndCurrentExpansion(0) == VSConstants.S_OK)
                    {
                        m_exSession = null;
                        return VSConstants.S_OK;
                    }
                }
            }
            //neither an expansion session nor a completion session is open, but we got a tab, so check whether the last word typed is a snippet shortcut 
            //if (m_exSession == null && nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB)
            //{
            //    //get the word that was just added 
            //    CaretPosition pos = m_vsTextView.Caret.Position;
            //    TextExtent word = languageService.NavigatorService.GetTextStructureNavigator(m_textView.TextBuffer).GetExtentOfWord(pos.BufferPosition - 1); //use the position 1 space back
            //    string textString = word.Span.GetText(); //the word that was just added
            //                                             //if it is a code snippet, insert it, otherwise carry on
            //    if (InsertAnyExpansion(textString, null, null))
            //        return VSConstants.S_OK;
            //}
            

            if (!string.IsNullOrEmpty(commandId))
			{
                // refactor and undo are not working anyway.
                if(false)
                {
                    //Refactor command
                    if (VSIDECommands.IsRightClick(pguidCmdGroupRef, nCmdID))
                    {
                        SetRefactorMenuBars();
                        return ExecVsHandler(ref pguidCmdGroupRef, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                    }

                    //Undo command
                    if (commandId == "cmdidUndo")
                    {
                        var luaUndoService = languageService.GetService(typeof(ILuaUndoService)) as ILuaUndoService;
                        if (luaUndoService != null)
                            luaUndoService.Undo();

                        return ExecVsHandler(ref pguidCmdGroupRef, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                    }
                }
			}

			return ExecVsHandler(ref pguidCmdGroupRef, nCmdID, nCmdexecopt, pvaIn, pvaOut);
		}

		#endregion

		/// <summary>
		/// Sets the refactor menu bars.
        /// Xizhi: this does not work even I set control to enabled. 
		/// </summary>
		private void SetRefactorMenuBars()
		{
			try
			{
				CommandBarControl renameCommand = null;
				var commandBarControl = GetCommandBarControl(Resources.RefactoringContextMenuName) as CommandBarPopup;

				if (commandBarControl != null)
					foreach (CommandBarControl subMenuItem in commandBarControl.Controls)
					{
						if (subMenuItem.Caption == Resources.RenameCommandName)
						{
							renameCommand = subMenuItem;
							break;
						}
					}
                if (renameCommand != null)
                {
                    bool bCanRefactor = IsRefactorableItemSelected();
                    if (bCanRefactor)
                        commandBarControl.Enabled = bCanRefactor;
                    renameCommand.Enabled = bCanRefactor;
                }
					
			}
			catch (Exception e)
			{
				Trace.WriteLine(e);
			}
		}

		/// <summary>
		/// Gets the command bar control.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		private CommandBarControl GetCommandBarControl(string name)
		{
			var dte = languageService.GetService(typeof (DTE)) as DTE2;
			var commandBars = (_CommandBars) dte.CommandBars;
			var commandBar = commandBars[Resources.CodeWindowCommandBarName];
			foreach (CommandBarControl ctrl in commandBar.Controls)
			{
				if (ctrl.Caption == name)
					return ctrl;
			}
			return null;
		}


		/// <summary>
		/// Determines whether [is refactorable item selected].
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if [is refactorable item selected]; otherwise, <c>false</c>.
		/// </returns>
		private bool IsRefactorableItemSelected()
		{
			LuaFileCodeModel codeModel = languageService.GetFileCodeModel();
			CodeElement element = codeModel.GetElementByEditPoint();
			return element != null;
		}

		/// <summary>
		/// Execs the vs handler.
		/// </summary>
		/// <param name="pguidCmdGroupRef">The pguid CMD group ref.</param>
		/// <param name="nCmdID">The n CMD ID.</param>
		/// <param name="nCmdexecopt">The n cmdexecopt.</param>
		/// <param name="pvaIn">The pva in.</param>
		/// <param name="pvaOut">The pva out.</param>
		/// <returns></returns>
		private int ExecVsHandler(ref Guid pguidCmdGroupRef, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
		{
			return VsCommandFilter.Exec(ref pguidCmdGroupRef, nCmdID, nCmdexecopt, pvaIn, pvaOut);
		}
        
    }
}