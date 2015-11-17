/***************************************************************************

Copyright (c) 2006 Microsoft Corporation. All rights reserved.

***************************************************************************/

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ParaEngine.Tools.Lua.SourceOutliner.Controls;
using ParaEngine.Tools.Services;
using ParaEngine.NPLLanguageService;
using EnvDTE80;
using Microsoft.VisualStudio;

namespace ParaEngine.Tools.Lua.SourceOutliner
{
	/// <summary>
	/// Class that implements the tool window exposed by this package and hosts a user control.
	/// </summary>
	/// <remarks>
	/// In Visual Studio, tool windows are composed of a frame (implemented by the shell) and a pane, 
	/// usually implemented by the package implementer.
	/// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
	/// implementation of the IVsWindowPane interface.
	/// </remarks>
	[CLSCompliant(false)]
	[Guid(GuidStrings.SourceOutlinerToolWindow)]
	public class SourceOutlineToolWindow : ToolWindowPane, IOleComponent
	{
		// This is the user control hosted by the tool window. It is exposed to the base class 
		// using the Window property. Note that, even if this class implements IDispose, 
		// Dispose is not called on this object because ToolWindowPane calls Dispose on 
		// the object returned by the Window property.
		private readonly SourceOutlinerControl control;

		private DTE dte;
		private Events events;
		private WindowEvents windowsEvents;
		private SolutionEvents solutionEvents;
		private bool isSlnClosing;
		private EditorSupport editSupport;
		private uint lastTickCount;
		private const uint delayBetweenIdleProcessing = 500;
		private const uint delayBetweenCodeElementSelection = 500;
		private uint lastTickCountBeforeUpdate;
		private int lineNum;
		private int colNum;
		private bool codeElementSelectedOnIdle;
		private bool swallowEnterKey;
		private bool swallowSelectedIndexChanged_toolStripComboBox;
		private bool swallowTextChanged_filterStringTextBox;

		// Declared Public for unit testing.
		public CodeOutlineCache codeCache;

		/// <summary>
		/// Initializes a new instance of the SourceOutlineToolWindow class.
		/// </summary>
		public SourceOutlineToolWindow() : base(null)
		{
			Caption = Resources.ToolWindowTitle;

			// Set the image that will appear on the tab of the window frame
			// when docked with another window.
			// The resource ID correspond to the one defined in the resx file,
			// while the Index is the offset in the bitmap strip. Each image in
			// the strip is 16x16.
			BitmapResourceID = 301;
			BitmapIndex = 1;

			control = new SourceOutlinerControl();

			// Populate the filter dropdown in the combo box with the 
			// list of possible code elements to be displayed.
			foreach (string name in Enum.GetNames(typeof (LuaCodeElementType)))
			{
				control.filterToolStripCombo.Items.Add(name);
			}
			control.filterToolStripCombo.SelectedIndex = 0;

			// Wire up the event handlers for the UI.
			control.filterToolStripCombo.SelectedIndexChanged += toolStripComboBox_SelectedIndexChanged;
			control.filterStringTextBox.TextChanged += filterStringTextBox_TextChanged;
			control.filterStringTextBox.KeyDown += filterStringTextBox_KeyDown;
			control.filterStringTextBox.KeyPress += filterStringTextBox_KeyPress;
			control.filterStringTextBox.MouseDown += filterStringTextBox_MouseDown;
			control.filterStringTextBox.Enter += filterStringTextBox_Enter;
			control.filterStringTextBox.Leave += filterStringTextBox_Leave;
			control.clearFilterButton.Click += clearFilterButton_Click;
		}

		/// <summary> 
		/// Cleans up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					var windowFrame = (IVsWindowFrame) Frame;

					if (windowFrame != null)
					{
						windowFrame.CloseFrame((uint) __FRAMECLOSE.FRAMECLOSE_SaveIfDirty);
					}

					if (control != null)
					{
						control.Dispose();
					}
				}
			}
			catch
			{
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		/// <summary>
		/// Gets the handle to the user control hosted in the Tool Window.
		/// </summary>
		/// <returns>An IWin32Window object.</returns>
		public override IWin32Window Window
		{
			get { return control; }
		}

		/// <summary>
		/// Gets or sets the Package property.
		/// </summary>
		/// <returns>A Package object.</returns>
		public new Package Package { get; set; }

		/// <summary>
		/// Sets the Visual Studio IDE object.
		/// </summary>
		/// <param name="dte">A DTE object exposing the Visual Studio automation object model.</param>
		public void InitializeDTE(DTE dte)
		{
			// Store the dte so that it can be used later.
			this.dte = dte;

			// Get the code model data.
			codeCache = new CodeOutlineCache(control, dte, (LanguageService) GetService(typeof (ILuaLanguageService)));
		}

		/// <summary>
		/// Registers handlers for the Activated and Closing events from the text window.
		/// </summary>
		public void AddWindowEvents()
		{
			events = dte.Events;
			windowsEvents = events.get_WindowEvents(null);
			windowsEvents.WindowActivated += windowsEvents_WindowActivated;
			windowsEvents.WindowClosing += windowsEvents_WindowClosing;
		}

		/// <summary>
		/// Registers handlers for certain solution events.
		/// </summary>
		public void AddSolutionEvents()
		{
			events = dte.Events;
			solutionEvents = events.SolutionEvents;
			solutionEvents.QueryCloseSolution += solnEvents_QueryCloseSolution;
			solutionEvents.Opened += solnEvents_Opened;
		}

		/// <summary>
		/// Occurs when the clearFilterButton button is clicked.
		/// </summary>
		/// <param name="sender">The source Button object for this event.</param>
		/// <param name="e">The EventArgs object that contains the event data.</param>
		private void clearFilterButton_Click(object sender, EventArgs e)
		{
			try
			{
				control.filterStringTextBox.Text = control.filterStringTextBox.Focused ? "" : "<Filter>";
				control.filterToolStripCombo.SelectedIndex = 0;

				// Reset the filters.
				if (codeCache.CurrentFileManager != null)
				{
					codeCache.CurrentFileManager.ResetFilters();
				}
				control.clearFilterButton.Enabled = false;
			}
			catch (Exception ex)
			{
				DisplayMessage(Resources.ErrorPrefix, "clearFilterButton_Click exception: " + ex);
			}
		}

		/// <summary>
		/// Displays the specified message string.
		/// </summary>
		/// <param name="prefix">An optional message prefix, such as Status: or Error:.</param>
		/// <param name="message">The message to write.</param>
		public static void DisplayMessage(string prefix, string message)
		{
			// Messages are current shown to the trace, not the user.
			// Change this to a MessageBox to display in the UI.
			string output = message.Trim();
			if (prefix.Length > 0)
			{
				output = prefix.Trim() + " " + output + Environment.NewLine;
			}
			Debug.WriteLine(output);
		}

		/// <summary>
		/// Gets or sets the filter text string showing in the textbox.
		/// </summary>
		/// <returns>A filter text string.</returns>
		public string SelectedFilterText
		{
			get { return control.filterStringTextBox.Text; }
			set
			{
				// Display the text and ignore the TextChanged event.
				if (control.filterStringTextBox.Text != value)
				{
					swallowTextChanged_filterStringTextBox = true;
					control.filterStringTextBox.Text = value;
				}
			}
		}

		/// <summary>
		/// Occurs when a key is pressed while the text box has focus. 
		/// </summary>
		/// <param name="sender">The source TextBox object for this event.</param>
		/// <param name="e">The KeyEventArgs object that contains the event data.</param>
		private void filterStringTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Down)
			{
				if ((control.VisibleTreeView.Nodes.Count > 0))
				{
					control.VisibleTreeView.SelectedNode = control.VisibleTreeView.Nodes[0];
					control.VisibleTreeView.Focus();
				}
			}

			if (e.KeyCode == Keys.Up)
			{
				if (control.VisibleTreeView.Nodes.Count > 0)
				{
					control.VisibleTreeView.SelectedNode =
						control.VisibleTreeView.Nodes[control.VisibleTreeView.Nodes.Count - 1];
					control.VisibleTreeView.Focus();
				}
			}

			if (e.KeyCode == Keys.Enter)
			{
				swallowEnterKey = true;
				NavigateToSelectedTreeNode();
			}
		}

		/// <summary>
		/// Occurs when the mouse pointer is over the text box control and a mouse button is pressed. 
		/// </summary>
		/// <param name="sender">The source TextBox object for this event.</param>
		/// <param name="e">The MouseEventArgs object that contains the event data.</param>
		private void filterStringTextBox_MouseDown(object sender, MouseEventArgs e)
		{
			if (control.filterStringTextBox.Text == "<Filter>")
			{
				swallowTextChanged_filterStringTextBox = true;
				control.filterStringTextBox.Text = "";
			}
		}

		/// <summary>
		/// Occurs when a key is pressed while the text box has focus. 
		/// </summary>
		/// <param name="sender">The source TextBox object for this event.</param>
		/// <param name="e">The KeyPressEventArgs object that contains the event data.</param>
		private void filterStringTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (swallowEnterKey)
			{
				e.Handled = true;
				swallowEnterKey = false;
			}
		}

		/// <summary>
		/// Occurs when the text box control is entered.
		/// </summary>
		/// <param name="sender">The source TextBox object for this event.</param>
		/// <param name="e">The EventArgs object that contains the event data.</param>
		private void filterStringTextBox_Enter(object sender, EventArgs e)
		{
			if (control.filterStringTextBox.Text == "<Filter>")
			{
				swallowTextChanged_filterStringTextBox = true;
				control.filterStringTextBox.Text = "";
			}
		}

		/// <summary>
		/// Occurs when the input focus leaves the text box control. 
		/// </summary>
		/// <param name="sender">The source TextBox object for this event.</param>
		/// <param name="e">The EventArgs object that contains the event data.</param>
		private void filterStringTextBox_Leave(object sender, EventArgs e)
		{
			TestFilterStringControlLeave();
		}

		/// <summary>
		/// Occurs when the Text property value changes. 
		/// </summary>
		/// <param name="sender">The source TextBox object for this event.</param>
		/// <param name="e">The EventArgs object that contains the event data.</param>
		private void filterStringTextBox_TextChanged(object sender, EventArgs e)
		{
			try
			{
				if (swallowTextChanged_filterStringTextBox)
				{
					swallowTextChanged_filterStringTextBox = false;
				}
				else
				{
					codeCache.CurrentFileManager.FilterText = control.filterStringTextBox.Text;
					codeCache.CurrentFileManager.SelectCodeElement();
				}
				CheckClearFilterButton();
			}
			catch (Exception ex)
			{
				DisplayMessage(Resources.ErrorPrefix, "filterStringTextBox_TextChanged exception: " + ex);
			}
		}

		/// <summary>
		/// Resets the text box, if needed.
		/// </summary>
		private void TestFilterStringControlLeave()
		{
			if (control.filterStringTextBox.Text.Length == 0)
			{
				swallowTextChanged_filterStringTextBox = true;
				control.filterStringTextBox.Text = "<Filter>";
			}
		}

		/// <summary>
		/// Enables or disables the Clear Filter button.
		/// </summary>
		private void CheckClearFilterButton()
		{
			if (((control.filterStringTextBox.Text.Length == 0) ||
			     (control.filterStringTextBox.Text == "<Filter>")) &&
			    (control.filterToolStripCombo.SelectedIndex == 0))
			{
				control.clearFilterButton.Enabled = false;
			}
			else
			{
				control.clearFilterButton.Enabled = true;
			}
		}

		/// <summary>
		/// Gets or sets the selected element type in the combo control.
		/// </summary>
		/// <returns>A value from the CodeElementType enumeration.</returns>
		public CodeElementType SelectedType
		{
			get
			{
				return
					(CodeElementType)
					Enum.Parse(typeof (CodeElementType), control.filterToolStripCombo.SelectedItem.ToString());
			}
			set
			{
				// Select the type in the combo and ignore the SelectedIndexChanged event.
				var currentType = (CodeElementType) Enum.Parse(typeof (CodeElementType),
				                                               control.filterToolStripCombo.SelectedItem.
				                                               	ToString());
				if (currentType != value)
				{
					string name = Enum.GetName(typeof (CodeElementType), value);
					int index = control.filterToolStripCombo.FindStringExact(name);
					if (index != -1)
					{
						swallowSelectedIndexChanged_toolStripComboBox = true;
						control.filterToolStripCombo.SelectedIndex = index;
					}
				}
			}
		}

		/// <summary>
		/// Occurs when the SelectedIndex property has changed.  
		/// </summary>
		/// <param name="sender">The source ComboBox object for this event.</param>
		/// <param name="e">The EventArgs object that contains the event data.</param>
		private void toolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				var selectedType = (CodeElementType) Enum.Parse(typeof (CodeElementType),
				                                                control.filterToolStripCombo.SelectedItem.
				                                                	ToString());

				if (swallowSelectedIndexChanged_toolStripComboBox)
				{
					swallowSelectedIndexChanged_toolStripComboBox = false;
				}
				else if (codeCache.CurrentFileManager != null)
				{
					codeCache.CurrentFileManager.ElementFilter = selectedType;
					codeCache.CurrentFileManager.SelectCodeElement();
					CheckClearFilterButton();
				}
			}
			catch (Exception ex)
			{
				DisplayMessage(Resources.ErrorPrefix,
				               "toolStripComboBox_SelectedIndexChanged exception: " + ex);
			}
		}

		/// <summary>
		/// Occurs when the text window is activated.
		/// </summary>
		/// <param name="gotFocus">The window that received the focus.</param>
		/// <param name="lostFocus">The window that lost the focus.</param>
		public void windowsEvents_WindowActivated(Window gotFocus, Window lostFocus)
		{
			try
			{
				Debug.Assert(gotFocus != null);
				if (gotFocus == null)
				{
					DisplayMessage(Resources.ErrorPrefix, "windowsEvents_WindowActivated has a null gotFocus parameter.");
					return;
				}

				if (lostFocus != null)
				{
					if (control.filterStringTextBox.Text.Length == 0)
					{
						swallowTextChanged_filterStringTextBox = true;
						control.filterStringTextBox.Text = "<Filter>";
					}
				}
				if (gotFocus != null)
				{
					if (control.filterStringTextBox.Focused)
					{
						if (control.filterStringTextBox.Text == "<Filter>")
						{
							swallowTextChanged_filterStringTextBox = true;
							control.filterStringTextBox.Text = "";
						}
					}
				}

				if (gotFocus.Type.Equals(vsWindowType.vsWindowTypeDocument) && !isSlnClosing)
				{
					// A document window got focus, so add its document to the cache.
					codeCache.AddDocumentToCache(gotFocus.Document, this);
				}
				else if (firstActivation
				         && (gotFocus.Object == this)
				         && (lostFocus != null)
				         && (lostFocus.Document != null))
				{
					// Got focus for the first time, this happens when the tool window is first opened.
					// If the window losing focus has a document, add that to the cache. 
					firstActivation = false;
					codeCache.AddDocumentToCache(lostFocus.Document, this);
				}
			}
			catch (Exception ex)
			{
				DisplayMessage(Resources.ErrorPrefix, "windowsEvents_WindowActivated exception: " + ex);
			}
		}

		private bool firstActivation = true;

		/// <summary>
		/// Occurs just before the text window closes.
		/// </summary>
		/// <param name="window">The window that is closing.</param>
		public void windowsEvents_WindowClosing(Window window)
		{
			Debug.Assert(window != null);
			if (window == null)
			{
				DisplayMessage(Resources.ErrorPrefix, "windowsEvents_WindowClosing has a null window parameter.");
				return;
			}

            try
            {
                if ((codeCache != null)
                && (codeCache.CurrentFileManager != null)
                && (codeCache.CurrentFileManager.Document == window.Document))
                {
                    codeCache.CurrentFileManager.State = CodeOutlineFileManager.OutlineFileManagerState.WaitToStartOver;
                    control.HideWaitWhileReadyMessage();
                    control.Enabled = false;
                    control.TreeView.Visible = false;
                    control.FilterView.Visible = false;
                }
            }
            catch(Exception)
            {
            }
		}

		/// <summary>
		/// Occurs immediately after opening a solution or project.
		/// </summary>
		private void solnEvents_Opened()
		{
			isSlnClosing = false;
            try
            {
                string solutionDir = System.IO.Path.GetDirectoryName(dte.Solution.FullName);
                if (solutionDir != null)
                {
                    WriteOutput("Open Solution: " + solutionDir);

                    var langService = (LanguageService)GetService(typeof(LanguageService));
                    if (langService != null)
                    {
                        // Load the documentation
                        langService.LoadXmlDocumentation(solutionDir + "\\");
                    }

                }
            }
            catch(Exception)
            {
            }
		}

        public void WriteOutput(String text)
        {
            var langService = (LanguageService)GetService(typeof(LanguageService));
            if (langService!=null)
            {
                langService.WriteOutput(text);
            }
        }

		/// <summary>
		/// Occurs when a solution is closing.
		/// </summary>
		/// <param name="fCancel">true to cancel the close, otherwise false.</param>
		private void solnEvents_QueryCloseSolution(ref bool fCancel)
		{
			isSlnClosing = true;
			control.HideTrees();
			swallowSelectedIndexChanged_toolStripComboBox = true;
			control.filterToolStripCombo.SelectedIndex = 0;
			swallowTextChanged_filterStringTextBox = true;
			control.filterStringTextBox.Text = "<Filter>";
			control.Enabled = false;
		}

		/// <summary>
		/// Occurs after the tree node is selected.   
		/// </summary>
		/// <param name="sender">The source TreeView object for this event.</param>
		/// <param name="e">The TreeViewEventArgs object that contains the event data.</param>
		private void codeTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			try
			{
				if ((e.Action == TreeViewAction.ByKeyboard) || (e.Action == TreeViewAction.ByMouse))
				{
					var codeElement = e.Node as CodeElementWrapper;
					editSupport = new EditorSupport();
					editSupport.GoToCodeElement(codeElement, dte);
				}
			}
			catch (Exception ex)
			{
				DisplayMessage(Resources.ErrorPrefix, "codeTreeView_AfterSelect exception: " + ex);
			}
		}

		/// <summary>
		/// Occurs when the tree view control is entered.  
		/// </summary>
		/// <param name="sender">The source TreeView object for this event.</param>
		/// <param name="e">The EventArgs object that contains the event data.</param>
		private void codeTreeView_Enter(object sender, EventArgs e)
		{
			try
			{
				// Make sure the selected element in the outline window is also selected in the text window.
				var codeElement = control.VisibleTreeView.SelectedNode as CodeElementWrapper;
				editSupport = new EditorSupport();
				editSupport.GoToCodeElement(codeElement, dte);
			}
			catch (Exception ex)
			{
				DisplayMessage(Resources.ErrorPrefix, "codeTreeView_Enter exception: " + ex);
			}
		}

		/// <summary>
		/// Occurs when a key is pressed while the tree view has focus. 
		/// </summary>
		/// <param name="sender">The source TreeView object for this event.</param>
		/// <param name="e">The KeyEventArgs object that contains the event data.</param>
		private void codeTreeView_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.KeyCode == Keys.Enter)
				{
					swallowEnterKey = true;
					NavigateToSelectedTreeNode();
				}
			}
			catch (Exception ex)
			{
				DisplayMessage(Resources.ErrorPrefix, "codeTreeView_KeyDown exception: " + ex);
			}
		}

		/// <summary>
		/// Occurs when a key is pressed while the tree view has focus. 
		/// </summary>
		/// <param name="sender">The source TreeView object for this event.</param>
		/// <param name="e">The KeyPressEventArgs object that contains the event data.</param>
		private void codeTreeView_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (swallowEnterKey)
			{
				e.Handled = true;
				swallowEnterKey = false;
			}
		}

		/// <summary>
		/// Occurs when the tree view control is double-clicked.
		/// </summary>
		/// <param name="sender">The source TreeView object for this event.</param>
		/// <param name="e">The EventArgs object that contains the event data.</param>
		private void codeTreeView_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				var codeElement = control.VisibleTreeView.SelectedNode as CodeElementWrapper;

				// This can happen if there were no matching code elements.
				if (codeElement == null)
				{
					return;
				}

				editSupport = new EditorSupport();
				editSupport.ActivateCodeWindow(codeElement, dte);
			}
			catch (Exception ex)
			{
				DisplayMessage(Resources.ErrorPrefix, "codeTreeView_DoubleClick exception: " + ex);
			}
		}

		/// <summary>
		/// Registers TreeView events.
		/// </summary>
		/// <param name="tree">The hierarchical tree.</param>
		/// <param name="filter">The filtered tree.</param>
		public void RegisterTreeEvents(TreeView tree, TreeView filter)
		{
			Debug.Assert(tree != null);
			if (tree == null)
			{
				DisplayMessage(Resources.ErrorPrefix, "RegisterTreeEvents has a null tree parameter.");
				return;
			}

			Debug.Assert(filter != null);
			if (filter == null)
			{
				DisplayMessage(Resources.ErrorPrefix, "RegisterTreeEvents has a null filter parameter.");
				return;
			}

			tree.AfterSelect += codeTreeView_AfterSelect;
			tree.KeyDown += codeTreeView_KeyDown;
			tree.KeyPress += codeTreeView_KeyPress;
			tree.DoubleClick += codeTreeView_DoubleClick;
			tree.Enter += codeTreeView_Enter;
			filter.AfterSelect += codeTreeView_AfterSelect;
			filter.KeyDown += codeTreeView_KeyDown;
			filter.KeyPress += codeTreeView_KeyPress;
			filter.DoubleClick += codeTreeView_DoubleClick;
			filter.Enter += codeTreeView_Enter;
		}

		/// <summary>
		/// Unregisters TreeView events.
		/// </summary>
		/// <param name="tree">The hierarchical tree.</param>
		/// <param name="filter">The filtered tree.</param>
		public void UnRegisterTreeEvents(TreeView tree, TreeView filter)
		{
			Debug.Assert(tree != null);
			if (tree == null)
			{
				DisplayMessage(Resources.ErrorPrefix, "UnRegisterTreeEvents has a null tree parameter.");
				return;
			}

			Debug.Assert(filter != null);
			if (filter == null)
			{
				DisplayMessage(Resources.ErrorPrefix, "UnRegisterTreeEvents has a null filter parameter.");
				return;
			}

			tree.AfterSelect -= codeTreeView_AfterSelect;
			tree.KeyDown -= codeTreeView_KeyDown;
			tree.KeyPress -= codeTreeView_KeyPress;
			tree.DoubleClick -= codeTreeView_DoubleClick;
			filter.AfterSelect -= codeTreeView_AfterSelect;
			filter.KeyDown -= codeTreeView_KeyDown;
			filter.KeyPress -= codeTreeView_KeyPress;
			filter.DoubleClick -= codeTreeView_DoubleClick;
		}

		/// <summary>
		/// Navigates from the selected node in the tree to its code 
		/// element in the editor window, and gives the editor focus.
		/// </summary>
		private void NavigateToSelectedTreeNode()
		{
			var codeElement = control.VisibleTreeView.SelectedNode as CodeElementWrapper;

			// This can happen if there were no matching code elements.
			if (codeElement == null)
			{
				return;
			}

			// Switch to the code window.
			editSupport = new EditorSupport();
			editSupport.ActivateCodeWindow(codeElement, dte);
		}

		/// <summary>
		/// Gets the current code cache.
		/// </summary>
		/// <returns>A CodeOutlineCache object.</returns>
		public CodeOutlineCache CodeCache
		{
			get { return codeCache; }
		}

		#region IOleComponent Members

		// Any component that needs idle time, the ability to process
		// messages before they are translated (for example, to call TranslateAccelerator 
		// or IsDialogMessage), notification about modal states, or the ability to push message 
		// loops must implement this interface and register with the Component Manager.

		/// <summary>
		/// Called during each iteration of a message loop that the component pushed.
		/// </summary>
		/// <param name="uReason">The reason for the call.</param>
		/// <param name="pvLoopData">The component's private data.</param>
		/// <param name="pMsgPeeked">The peeked message, or NULL if no message is in the queue.</param>
		/// <returns>
		/// TRUE (not zero) if the message loop should continue, otherwise FALSE (zero).
		/// If false is returned, the component manager terminates the loop without
		/// removing pMsgPeeked from the queue.
		/// </returns>
		/// <remarks>
		/// This method is called after peeking the next message in the queue (via PeekMessage)
		/// but before the message is removed from the queue.  This method may be additionally 
		/// called when the next message has already been removed from the queue, in which case
		/// pMsgPeeked is passed as NULL.
		/// </remarks>
		public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Called when the Visual Studio IDE goes idle to give 
		/// the component a chance to perform idle time tasks.  
		/// </summary>
		/// <param name="grfidlef">
		/// A group of bit flags taken from the enumeration of oleidlef values,
		/// indicating the type of idle tasks to perform.  
		/// </param>
		/// <returns>
		/// TRUE (not zero) if more time is needed to perform the idle time tasks, otherwise FALSE (zero).
		/// </returns>
		/// <remarks>
		/// The component may periodically call FContinueIdle and, if it returns
		/// false, the component should terminate its idle time processing and return.  
		/// If a component reaches a point where it has no idle tasks and does not need
		/// FDoIdle calls, it should remove its idle task registration via 
		/// FUpdateComponentRegistration.  If this method is called while the component
		/// is performing a tracking operation, the component should only perform idle time 
		/// tasks that it deems appropriate to perform during tracking.
		/// </remarks>
		public int FDoIdle(uint grfidlef)
		{
			var tickCount = (uint) Environment.TickCount;
			if (tickCount < lastTickCount)
			{
				// The tick count rolled over, so treat this as if the timeout has expired 
				// to keep from waiting until the count gets up to the required value again.
			}
			else
			{
				// Check to see when the last occurrence was.  Only search once per second.
				if ((tickCount - lastTickCount) < delayBetweenIdleProcessing)
				{
					return 0;
				}
			}

            // added by Xizhi, we will prevent DomCode parsing when outliner is not visible. 
            if (((IVsWindowFrame)this.Frame).IsVisible() != VSConstants.S_OK)
                return 0;

			try
			{
				if (codeCache.CurrentFileManager != null)
				{
					CodeOutlineFileManager.OutlineFileManagerState state = codeCache.CurrentFileManager.State;
					switch (state)
					{
						case CodeOutlineFileManager.OutlineFileManagerState.StartLoadingCodeModel:
							// Load completely anew.
							control.ShowWaitWhileReadyMessage();
							codeCache.CurrentFileManager.Load();
							return 0;

						case CodeOutlineFileManager.OutlineFileManagerState.LoadingCodeModel:
							// Continue loading after an interruption.
							codeCache.CurrentFileManager.ContinueLoading();
							return 0;

						case CodeOutlineFileManager.OutlineFileManagerState.DoneLoadingCodeModel:
							// Loading is complete.
							codeCache.CurrentFileManager.FinishLoading();
							codeCache.CurrentFileManager.TreeView.Refresh();
							codeCache.CurrentFileManager.FilterView.Refresh();
							control.Enabled = codeCache.CurrentFileManager.FileIsOutlined;
							if (control.Enabled)
							{
								var selectedType = (CodeElementType) Enum.Parse(typeof (CodeElementType),
								                                                control.
								                                                	filterToolStripCombo.
								                                                	SelectedItem.ToString());
								codeCache.CurrentFileManager.ElementFilter = selectedType;
							}

							control.HideWaitWhileReadyMessage();
							control.Reset();

							codeCache.CurrentFileManager.State =
								CodeOutlineFileManager.OutlineFileManagerState.WaitToStartOver;
							return 0;

						case CodeOutlineFileManager.OutlineFileManagerState.WaitToStartOver:
							break;
					}
				}

				// Get the current active TextPoint from the DTE.
				if ((dte.ActiveDocument == null)
				    || (codeCache == null)
				    || (codeCache.CurrentFileManager == null)
				    || (codeCache.CurrentFileManager.TreeViewFocused)
				    || !control.Enabled)
				{
					return 0;
				}

				var sel = (TextSelection) dte.ActiveDocument.Selection;
				if (sel == null)
					return 0;

				var tp = (TextPoint) sel.ActivePoint;

				if ((tp.Line == lineNum) && (tp.LineCharOffset == colNum))
				{
					if (!codeElementSelectedOnIdle
					    && ((tickCount - lastTickCountBeforeUpdate) > delayBetweenCodeElementSelection))
					{
						codeElementSelectedOnIdle = true;

						// Turn off pretty listing to fix the problem with line autocompletion  
						// being invoked when the code element position is determined.
						Properties properties = null;
						try
						{
							properties = dte.get_Properties("TextEditor", "Basic-Specific");
						}
						catch
						{
						}
						Property property = null;
						if (properties != null)
							foreach (Property p in properties)
							{
								if (p.Name == "PrettyListing")
								{
									property = p;
									break;
								}
							}
						var currentPrettyListing = true;
						if (property != null)
						{
							currentPrettyListing = (bool) property.Value;
							property.Value = false;
						}

						codeCache.CurrentFileManager.SelectCodeElement(tp);

						// Set pretty listing back to its previous value.
						if (property != null)
						{
							property.Value = currentPrettyListing;
						}

						lastTickCountBeforeUpdate = tickCount;
					}
				}
				else
				{
					codeElementSelectedOnIdle = false;
				}

				lineNum = tp.Line;
				colNum = tp.LineCharOffset;
			}
			catch (Exception ex)
			{
				DisplayMessage(Resources.ErrorPrefix, "FDoIdle exception: " + ex);
			}
			lastTickCount = tickCount;
			return 0;
		}

		/// <summary>
		/// Gives the component a chance to process the message before it is translated and dispatched.
		/// </summary>
		/// <param name="pMsg">The message to process.</param>
		/// <returns>TRUE (not zero) if the message is consumed, otherwise FALSE (zero).</returns>
		/// <remarks>
		/// The component can do TranslateAccelerator, do IsDialogMessage, modify pMsg, or take some other action.
		/// </remarks>
		public int FPreTranslateMessage(MSG[] pMsg)
		{
			return 0;
		}

		/// <summary>
		/// Called when component manager wishes to know if the 
		/// component is in a state where it can terminate.
		/// </summary>
		/// <param name="fPromptUser">
		/// TRUE (not zero) to prompt the user for permission to terminate, otherwise FALSE (zero).
		/// </param>
		/// <returns>TRUE (not zero) if okay to terminate, otherwise FALSE (zero).</returns>
		/// <remarks>
		/// If fPromptUser is false, the component should simply return true if 
		/// it can terminate or false otherwise.
		/// If fPromptUser is true, the component should return true if it can
		/// terminate without prompting the user; otherwise, it should prompt the
		/// user by either (a) asking the user if it can terminate and returning true
		/// or false appropriately, or (b) giving an indication as to why it cannot 
		/// terminate and returning false.
		/// </remarks>
		public int FQueryTerminate(int fPromptUser)
		{
			return 1;
		}

		/// <summary>
		/// A reserved method; must return TRUE (not zero).
		/// </summary>
		public int FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam)
		{
			return 1;
		}

		/// <summary>
		/// Retrieves a window associated with the component.
		/// </summary>
		/// <param name="dwReserved">Reserved for future use and should be zero.</param>
		/// <param name="dwWhich">An value from the olecWindow enumeration.</param>
		/// <returns>The desired window or NULL if no such window exists.</returns>
		public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved)
		{
			return Window.Handle;
		}

		/// <summary>
		/// Notifies the component when a new object is being activated.
		/// </summary>
		/// <param name="dwReserved">Reserved for future use.</param>
		/// <param name="fHostIsActivating">TRUE (not zero) if the host is the object being activated, otherwise FALSE (zero).</param>
		/// <param name="fSameComponent">TRUE (not zero) if pic is the same component as the callee of this method, otherwise FALSE (zero).</param>
		/// <param name="pchostinfo">An OLECHOSTINFO that contains information about the host.</param>
		/// <param name="pcrinfo">An OLECRINFO that contains information about pic.</param>
		/// <param name="pic">The IOleComponent object to activate.</param>
		/// <remarks>
		/// If pic is non-NULL, then it is the component that is being activated.
		/// In this case, fSameComponent is true if pic is the same component as
		/// the callee of this method, and pcrinfo is the information about the pic.
		/// If pic is NULL and fHostIsActivating is true, then the host is the
		/// object being activated, and pchostinfo is its host info.
		/// If pic is NULL and fHostIsActivating is false, then there is no current
		/// active object.
		/// If pic is being activated and pcrinfo->grf has the olecrfExclusiveBorderSpace 
		/// bit set, the component should hide its border space tools (toolbars, 
		/// status bars, etc.), and it should also do this if the host is activating and 
		/// pchostinfo->grfchostf has the olechostfExclusiveBorderSpace bit set.
		/// In either of these cases, the component should unhide its border space
		/// tools the next time it is activated.
		/// If pic is being activated and pcrinfo->grf has the olecrfExclusiveActivation
		/// bit is set, then pic is being activated in 'ExclusiveActive' mode.  The
		/// component should retrieve the top frame window that is hosting pic
		/// (via pic->HwndGetWindow(olecWindowFrameToplevel, 0)).  
		/// If this window is different from the component's own top frame window, 
		/// the component should disable its windows and do the things it would do
		/// when receiving an OnEnterState(olecstateModal, true) notification. 
		/// Otherwise, if the component is top-level, it should refuse to have its window 
		/// activated by appropriately processing WM_MOUSEACTIVATE.
		/// The component should remain in one of these states until the 
		/// ExclusiveActive mode ends, indicated by a future call to OnActivationChange 
		/// with the ExclusiveActivation bit not set or with a NULL pcrinfo.
		/// </remarks>
		public void OnActivationChange(IOleComponent pic, int fSameComponent, OLECRINFO[] pcrinfo, int fHostIsActivating,
		                               OLECHOSTINFO[] pchostinfo, uint dwReserved)
		{
		}

		/// <summary>
		/// Notifies the component when the host application gains or loses activation.
		/// </summary>
		/// <param name="dwOtherThreadID">The ID of the thread that owns the window.</param>
		/// <param name="fActive">TRUE (not zero) if the host application is being activated, otherwise FALSE (zero).</param>
		/// <remarks>
		/// If fActive is TRUE, the host application is being activated and
		/// dwOtherThreadID is the ID of the thread owning the window being deactivated.
		/// If fActive is false, the host application is being deactivated and 
		/// dwOtherThreadID is the ID of the thread owning the window being activated.
		/// This method is not called when both the window being activated
		/// and the window being deactivated belong to the host application.
		/// </remarks>
		public void OnAppActivate(int fActive, uint dwOtherThreadID)
		{
		}

		/// <summary>
		/// Notifies the component when the application enters or exits (as indicated by fEnter).
		/// </summary>
		/// <param name="fEnter">TRUE (not zero) for enter and FALSE (zero) for exit.</param>
		/// <param name="uStateID">The state identifier from the olecstate enumeration.</param>
		/// <remarks>
		/// If n calls are made with a true fEnter, the component should consider 
		/// the state to be in effect until n calls are made with a false fEnter.
		/// The component should be aware that it is possible for this method to
		/// be called with a false fEnter more times than it was called with a true
		/// fEnter.  For example, if the component is maintaining a state counter
		/// incremented when this method is called with a true fEnter and decremented
		/// when called with a false fEnter, then the counter should not be decremented
		/// for a false fEnter if it is already at zero.
		/// </remarks>
		public void OnEnterState(uint uStateID, int fEnter)
		{
		}

		/// <summary>
		/// Notifies the active component that it has lost its active status
		/// because the host or another component has become active.
		/// </summary>
		public void OnLoseActivation()
		{
		}

		/// <summary>
		/// Called when the component manager wishes to terminate the component's registration.
		/// </summary>
		/// <remarks>
		/// The component should revoke its registration with the component manager,
		/// release references to component manager and perform any necessary cleanup.
		/// </remarks>
		public void Terminate()
		{
		}

		#endregion IOleComponent Members
	}
}