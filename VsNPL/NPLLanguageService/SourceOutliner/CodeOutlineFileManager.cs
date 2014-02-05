/***************************************************************************

Copyright (c) 2006 Microsoft Corporation. All rights reserved.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.OLE.Interop;
using ParaEngine.Tools.Lua.CodeDom;
using ParaEngine.Tools.Lua.SourceOutliner.Controls;
using Configuration=ParaEngine.Tools.Lua.Parser.Configuration;
using ParaEngine.NPLLanguageService;

namespace ParaEngine.Tools.Lua.SourceOutliner
{
    /// <summary>
    /// Enumeration that lists the types of TreeViews used.
    /// </summary>
    public enum ViewType
    {
        TreeView,
        FlatView
    }

    /// <summary>
    /// Class that manages a source file.
    /// </summary>
    [CLSCompliant(false)]
    public class CodeOutlineFileManager
    {
        // List of code elements with a TreeNode wrapper.
        private CodeElementWrapperArray codeElementWrapperArray;

        // List of code element indexes, pointing to codeElementWrapperArray.
        // Sorted so they reference the list of code elements alphabetically.
        private CodeElementWrapperIndexTable codeElementWrapperIndexTable;

        private Dictionary<string, CodeElementWrapper> mapElementIDToTreeViewCodeElementWrapper;
        private Dictionary<string, CodeElementWrapper> mapElementIDToFilterViewCodeElementWrapper;
        private ResultsTable currentResults;
        private SearchCriteria searchCriteria;
        private string currentFilterText;

        // The TreeView is shown in list view mode if a filter is applied.
        private readonly TreeView codeFilterView;
        private readonly TreeView codeTreeView;

        private readonly SourceOutlinerControl control;
        private readonly Document currentDocument;
        private readonly DTE dte;
        private readonly SourceOutlineToolWindow sourceOutlineToolWindow;

        private const int RequiredDelayBeforeUpdate = 1000;
        private int transactionCounter;
        private readonly LanguageService languageService;
        private bool fileIsOutlined;
    	private ViewType viewType;

		private int yieldCount0;
		private int yieldCount1;

        /// <summary>
        /// Enumeration of states that occur while loading a source file.
        /// </summary>
        public enum OutlineFileManagerState
        {
            StartLoadingCodeModel,
            LoadingCodeModel,
            DoneLoadingCodeModel,
            WaitToStartOver
        }

        /// <summary>
        /// Gets or sets whether the file is fully outlined.
        /// </summary>
        /// <returns>true if the file is outlined, otherwise false.</returns>
        public bool FileIsOutlined
        {
            set { fileIsOutlined = value; }
            get { return fileIsOutlined; }
        }

    	/// <summary>
    	/// Gets or sets the current loading state.
    	/// </summary>
    	/// <returns>The loading state.</returns>
    	public OutlineFileManagerState State { get; set; }

    	/// <summary>
        /// Gets the TreeView that's appropriate to the current view.
        /// </summary>
        /// <returns>A TreeView object.</returns>
        private TreeView CurrentTreeView
        {
            get
            {
                switch (viewType)
                {
                    case ViewType.FlatView:
                        return (FilterView);

                    case ViewType.TreeView:
                        return (TreeView);
                }
                return (null);
            }
        }

        /// <summary>
        /// Gets or sets the current view type (tree or list).
        /// </summary>
        /// <returns>A value from the ViewType enumeration.</returns>
        private ViewType ViewType
        {
            get { return viewType; }
            set
            {
                viewType = value;

                // Toggle the control's view to match the property.
                if (viewType == ViewType.TreeView)
                {
                    control.ShowTree();
                }
                else
                {
                    control.ShowFilter();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the CodeOutlineFileManager class.
        /// </summary>
        /// <param name="control">The outline control object.</param>
        /// <param name="dte">A DTE object exposing the Visual Studio automation object model.</param>
        /// <param name="d">The source file Document.</param>
        /// <param name="toolWindow">The tool window for the package.</param>
        /// <param name="languageService">The Lua LanguageService.</param>
        public CodeOutlineFileManager(SourceOutlinerControl control, DTE dte,
                                      Document d, SourceOutlineToolWindow toolWindow, LanguageService languageService)
        {
            this.languageService = languageService;
            this.control = control;
            this.dte = dte;
            sourceOutlineToolWindow = toolWindow;
            viewType = ViewType.TreeView;
            searchCriteria = new SearchCriteria(CodeElementType.All);
            currentFilterText = "";
            currentDocument = d;

            codeTreeView = new TreeView();
            codeFilterView = new TreeView();

            // 
            // codeTreeView
            // 
            codeTreeView.Dock = DockStyle.Fill;
            codeTreeView.HideSelection = false;
            codeTreeView.Location = new Point(0, 45);
            codeTreeView.Name = "codeTreeView";
            codeTreeView.ShowNodeToolTips = true;
            codeTreeView.Size = new Size(352, 294);
            codeTreeView.TabIndex = 2;

            // 
            // codeFilterView
            // 
            codeFilterView.Dock = DockStyle.Fill;
            codeFilterView.HideSelection = false;
            codeFilterView.Location = new Point(0, 45);
            codeFilterView.Name = "codeFilterView";
            codeFilterView.ShowNodeToolTips = true;
            codeFilterView.Size = new Size(352, 294);
            codeFilterView.TabIndex = 3;
            codeFilterView.Visible = false;
            codeFilterView.ShowLines = false;
            codeFilterView.ShowRootLines = false;
            codeFilterView.FullRowSelect = true;
        }

        /// <summary>
        /// Marks both TreeViews as invisible.
        /// </summary>
        public void HideTrees()
        {
            codeTreeView.Visible = false;
            codeFilterView.Visible = false;
        }

        /// <summary>
        /// Gets the source file Document object.
        /// </summary>
        /// <returns>A Document object.</returns>
        public Document Document
        {
            get { return (currentDocument); }
        }

        /// <summary>
        /// Gets a value indicating whether the current TreeView has focus.
        /// </summary>
        /// <returns>true if the tree has focus, otherwise false.</returns>
        public bool TreeViewFocused
        {
            get { return (CurrentTreeView.Focused); }
        }

        /// <summary>
        /// Gets or sets the filter text string.
        /// </summary>
        /// <returns>The filter text string currently in effect.</returns>
        /// <remarks>
        /// When non-blank, limits the list of displayed elements to only those
        /// whose names start with the same letters as the filter text.
        /// </remarks>
        public string FilterText
        {
            get { return currentFilterText; }
            set
            {
                Debug.Assert(value != null);

                currentFilterText = value;

                ReApplyText();
            }
        }

        /// <summary>
        /// Re-applies the current filter text string.
        /// </summary>
        /// <remarks>
        /// Re-applying is necessary when the user switches from one source file to another.
        /// </remarks>
        public void ReApplyText()
        {
            Debug.Assert(currentResults != null);

            if (currentFilterText.Length == 0 && searchCriteria.ElementFilter == CodeElementType.All)
            {
                ViewType = ViewType.TreeView;
            }
            else
            {
                ViewType = ViewType.FlatView;
                currentResults.ApplyText(currentFilterText);
                LoadFlatView();
            }
        }

        /// <summary>
        /// Gets or sets the element type filter.
        /// </summary>
        /// <returns>The current element type filter.</returns>
        /// <remarks>
        /// When set to All and the filter text is blank, the hierarchical tree is displayed.
        /// When an element type is chosen, only code elements that match that type are displayed.
        /// The list may be further filtered by the filter text.
        /// </remarks>
        public CodeElementType ElementFilter
        {
            get { return (searchCriteria.ElementFilter); }
            set
            {
                if ((value == CodeElementType.All) && (currentFilterText.Length == 0))
                {
                    ViewType = ViewType.TreeView;
                    searchCriteria.ElementFilter = value;
                }
                else
                {
                    searchCriteria.ElementFilter = value;
                    currentResults = codeElementWrapperIndexTable.GenerateResultsTable(searchCriteria);
                    currentResults.ApplyText(currentFilterText);

                    ViewType = ViewType.FlatView;
                    LoadFlatView();
                }
            }
        }

        /// <summary>
        /// Gets the flat TreeView.
        /// </summary>
        /// <returns>The TreeView object that displays the flat view.</returns>
        public TreeView FilterView
        {
            get { return (codeFilterView); }
        }

        /// <summary>
        /// Gets the hierarchical TreeView.
        /// </summary>
        /// <returns>The TreeView object that displays the hierarchical view.</returns>
        public TreeView TreeView
        {
            get { return (codeTreeView); }
        }

        /// <summary>
        /// Populates the flat TreeView.
        /// </summary>
        private void LoadFlatView()
        {
            Debug.Assert(currentResults != null);
            Debug.Assert(viewType == ViewType.FlatView);

            currentResults.AddEntriesToTreeControl(FilterView);
        }

        /// <summary>
        /// Resets the state of the TreeView and the filters.
        /// </summary>
        public void Reset()
        {
            transactionCounter = 0;
            codeElementWrapperArray = new CodeElementWrapperArray();
            codeElementWrapperIndexTable = new CodeElementWrapperIndexTable(codeElementWrapperArray);
            mapElementIDToTreeViewCodeElementWrapper = null;
            mapElementIDToFilterViewCodeElementWrapper = null;
            currentResults = null;

            ResetFilters();
        }

        /// <summary>
        /// Clears the filters and displays the hierarchical TreeView.
        /// </summary>
        public void ResetFilters()
        {
            searchCriteria = new SearchCriteria(CodeElementType.All);

            if (currentDocument != null)
            {
                currentFilterText = "";
                currentResults = codeElementWrapperIndexTable.GenerateResultsTable(searchCriteria);
                ViewType = ViewType.TreeView;
                control.Reset();
            }
        }

        /// <summary>
        /// Clears the currently loaded TreeView and reloads the source outline.
        /// </summary>
        public void Load()
        {
            Debug.Assert(currentDocument != null);

            var package = sourceOutlineToolWindow.Package as NPLLanguageServicePackage;

            // Show the wait cursor while loading the file.
            package.SetWaitCursor();

            FilterView.Nodes.Clear();
            TreeView.Nodes.Clear();

            LoadCodeModel();
        }

        /// <summary>
        /// Clears the source outline.
        /// </summary>
        public void Clear()
        {
            Reset();
            FilterView.Nodes.Clear();
            TreeView.Nodes.Clear();
        }

        /// <summary>
        /// Returns the size of the index table.
        /// </summary>
        /// <returns>The number of indexes in the table.</returns>
        public int IndexSize()
        {
            if (codeElementWrapperIndexTable == null)
            {
                return 0;
            }
            return (codeElementWrapperIndexTable.Size());
        }

        #region Select TreeNode from Editor

        /// <summary>
        /// Highlights the first code element in the visible TreeView.
        /// </summary>
        public void SelectCodeElement()
        {
            if (control.VisibleTreeView.Nodes.Count > 0)
            {
                TreeNode node = control.VisibleTreeView.Nodes[0];
                control.VisibleTreeView.SelectedNode = node;

                // In theory, at this point GoToCodeElement should be used 
                // to select text in the text window.  However, there is an unpalatable
                // side-effect of doing this: if a filter is in effect (for example Methods),
                // and the user clicks on anything other than a method in the text window,
                // when SourceOutlinerToolWindow.FDoIdle() fires next it will re-select the nearest
                // method.  So, the side-effect is worse than not having the text auto-selected.
            }
        }

        /// <summary>
        /// Highlights the code element in the visible TreeView 
        /// that is closest to the parameter TextPoint.
        /// </summary>
        /// <param name="tp">The TextPoint cursor position in the source file.</param>
        public void SelectCodeElement(TextPoint tp)
        {
            var cew = GetClosestCodeElement(tp);
            if (cew != null)
            {
                CurrentTreeView.SelectedNode = cew;
            }
            else
            {
                SelectCodeElement();
            }
        }

        /// <summary>
        /// Finds the code element in the visible TreeView 
        /// that is closest to the parameter TextPoint.
        /// </summary>
        /// <param name="tp">The TextPoint cursor position in the source file.</param>
        /// <returns>A CodeElementWrapper object.</returns>
        private CodeElementWrapper GetClosestCodeElement(TextPoint tp)
        {
            switch (viewType)
            {
                case ViewType.FlatView:
                    return GetClosestCodeElementInFlatView(tp);

                case ViewType.TreeView:
                    return GetClosestCodeElementInTreeView(TreeView, tp);
            }
            return null;
        }

        /// <summary>
        /// Finds the code element in the flat view TreeView 
        /// that is closest to the parameter TextPoint.
        /// </summary>
        /// <param name="tp">The TextPoint cursor position in the source file.</param>
        /// <returns>A CodeElementWrapper object.</returns>
        private CodeElementWrapper GetClosestCodeElementInFlatView(TextPoint tp)
        {
            Debug.Assert(viewType == ViewType.FlatView);
            Debug.Assert(FilterView == CurrentTreeView);

            CodeElement ce = null;

            switch (searchCriteria.ElementFilter)
            {
                case CodeElementType.Class:
                    ce = tp.get_CodeElement(vsCMElement.vsCMElementClass);
                    break;
                case CodeElementType.Delegate:
                    ce = tp.get_CodeElement(vsCMElement.vsCMElementDelegate);
                    break;
                case CodeElementType.Method:
                    ce = tp.get_CodeElement(vsCMElement.vsCMElementFunction);
                    break;
                case CodeElementType.Property:
                    ce = tp.get_CodeElement(vsCMElement.vsCMElementProperty);
                    break;
                case CodeElementType.Variable:
                    ce = tp.get_CodeElement(vsCMElement.vsCMElementVariable);
                    break;
                case CodeElementType.Enumeration:
                    ce = tp.get_CodeElement(vsCMElement.vsCMElementEnum);
                    break;
                case CodeElementType.All:
                    return GetClosestCodeElementInTreeView(FilterView, tp);
            }

            // Find which nodes in the flat view have this code element.
            if (ce != null)
            {
                for (var i = 0; i <= FilterView.Nodes.Count - 1; i++)
                {
                    if (FilterView.Nodes[i].Text == CodeModelHelpers.GetDisplayNameFromCMElement(ce))
                    {
                        return (CodeElementWrapper) FilterView.Nodes[i];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the code element in the outlined TreeView 
        /// that is closest to the parameter TextPoint.
        /// </summary>
        /// <param name="tv">The source Treeview object.</param>
        /// <param name="tp">The TextPoint cursor position in the source file.</param>
        /// <returns>A CodeElementWrapper object.</returns>
        private CodeElementWrapper GetClosestCodeElementInTreeView(TreeView tv, TextPoint tp)
        {
            // Search all top nodes.
            return FindBestMatchInCollection(tv.Nodes, tp);
        }

        /// <summary>
        /// Determines if a code element is within a specific tree node.
        /// </summary>
        /// <param name="treeNode">The source CodeElementWrapper object.</param>
        /// <param name="tp">The TextPoint cursor position in the source file.</param>
        /// <returns>The CodeElementWrapper object that contains the text point.</returns>
        private CodeElementWrapper FindBestMatch(CodeElementWrapper treeNode, TextPoint tp)
        {
            CodeElementWrapper cewBestMatch = null;

            // Is the tp between the start and end point of this CodeElement.
            if ((treeNode.StartPoint.EqualTo(tp))
                || (treeNode.StartPoint.LessThan(tp) && treeNode.EndPoint.GreaterThan(tp)))
            {
                cewBestMatch = treeNode;
                var closestCEWInChildren = FindBestMatchInCollection(treeNode.Nodes, tp);
                if (closestCEWInChildren != null)
                {
                    cewBestMatch = closestCEWInChildren;
                }
            }

            return cewBestMatch;
        }

        /// <summary>
        /// Determines if a code element is within a specific TreeNodeCollection.
        /// </summary>
        /// <param name="nodes">The source TreeNodeCollection object.</param>
        /// <param name="tp">The TextPoint cursor position in the source file.</param>
        /// <returns>The CodeElementWrapper object that contains the text point.</returns>
        private CodeElementWrapper FindBestMatchInCollection(TreeNodeCollection nodes, TextPoint tp)
        {
            int nCount = nodes.Count;
            int nLow = 0;
            int nHigh = nCount - 1;

            while (nLow <= nHigh)
            {
                int nMid = (nHigh + nLow)/2;

                var cew = nodes[nMid] as CodeElementWrapper;
                try
                {
                    if ((cew == null) || (cew.CodeElement == null)
                        || (cew.CodeElement.StartPoint == null) || (cew.CodeElement.EndPoint == null))
                    {
                        return null;
                    }
                }
                catch
                {
                    // cew.CodeElement.EndPoint may throw an exception if 
                    // the element is currently being edited.
                    return null;
                }

                if (cew.CodeElement.StartPoint.EqualTo(tp)
                    || cew.CodeElement.EndPoint.EqualTo(tp))
                {
                    return cew;
                }
                else if (cew.CodeElement.StartPoint.LessThan(tp)
                         && cew.CodeElement.EndPoint.GreaterThan(tp))
                {
                    return FindBestMatch(cew, tp);
                }
                else if (cew.CodeElement.StartPoint.GreaterThan(tp))
                {
                    nHigh = nMid - 1;
                }
                else
                {
                    nLow = nMid + 1;
                }
            }

            return null;
        }

        #endregion Select TreeNode from Editor

        #region Code Model

        /// <summary>
        /// Initiates loading of the source outline.
        /// </summary>
        private void LoadCodeModel()
        {
            codeElementWrapperArray = new CodeElementWrapperArray();
            mapElementIDToTreeViewCodeElementWrapper = new Dictionary<string, CodeElementWrapper>();
            mapElementIDToFilterViewCodeElementWrapper = new Dictionary<string, CodeElementWrapper>();

            State = OutlineFileManagerState.LoadingCodeModel;
            _currentCodeElement = 1;
        }

        private int _currentCodeElement;

        /// <summary>
        /// Continues loading the source outline.
        /// </summary>
        private void ContinueLoadingCodeModel()
        {
            try
            {
                // Read the code model only if the document and project item opened are not null.
                if (currentDocument == null || currentDocument.ProjectItem == null)
                {
                    State = OutlineFileManagerState.DoneLoadingCodeModel;
                    return;
                }
            }
            catch (ArgumentException)
            {
                State = OutlineFileManagerState.DoneLoadingCodeModel;
                return;
            }


            LuaFileCodeModel fileCodeModel = languageService.GetFileCodeModel(currentDocument.ProjectItem);
            if (fileCodeModel == null)
            {
                State = OutlineFileManagerState.DoneLoadingCodeModel;
                return;
            }

            // Only C# and VB are currently supported.
            if (fileCodeModel.Language != Configuration.Name)
            {
                TreeView.Hide();
                State = OutlineFileManagerState.DoneLoadingCodeModel;
                return;
            }

            CodeElements fileCodeElements = fileCodeModel.LuaCodeElements;
            int nFileCodeElements = fileCodeElements.Count;

            bool isFinished = false;
            TreeView.BeginUpdate();
            while (_currentCodeElement <= nFileCodeElements)
            {
                isFinished =
                    ReadCodeModelElementsRecursive(fileCodeElements.Item(_currentCodeElement), TreeView.Nodes, true);
                if (!isFinished)
                {
                    break;
                }
                _currentCodeElement++;
            }
            TreeView.EndUpdate();

            if ((_currentCodeElement > nFileCodeElements && isFinished)
                || (nFileCodeElements == 0))
            {
                State = OutlineFileManagerState.DoneLoadingCodeModel;
                fileIsOutlined = true;
            }
        }

        /// <summary>
        /// Adds a new element to the tree views.
        /// </summary>
        /// <param name="codeElementWrapper">The new element as a CodeElementWrapper object.</param>
        private void AddNodeToInternalStructures(CodeElementWrapper codeElementWrapper)
        {
            try
            {
                // Add the element to the tree view map.
                mapElementIDToTreeViewCodeElementWrapper.Add(codeElementWrapper.UniqueElementId, codeElementWrapper);
            }
            catch (ArgumentException e)
            {
                SourceOutlineToolWindow.DisplayMessage(Resources.ErrorPrefix,
                                                       "Adding duplicate to mapElementIDToTreeViewCodeElementWrapper: " +
                                                       e.Message);
            }

            // Add to filtered (flat) tree.
            var codeElementWrapperFilterView = new CodeElementWrapper(codeElementWrapper.CodeElement);
            FilterView.Nodes.Add(codeElementWrapperFilterView);

            try
            {
                // Add the element to the filter view map.
                mapElementIDToFilterViewCodeElementWrapper.Add(codeElementWrapper.UniqueElementId,
                                                                codeElementWrapperFilterView);
            }
            catch (ArgumentException e)
            {
                SourceOutlineToolWindow.DisplayMessage(Resources.ErrorPrefix,
                                                       "Adding duplicate to mapElementIDToFilterViewCodeElementWrapper: " +
                                                       e.Message);
            }

            // Add the node to the array.
            var codeElementWrapperClone = new CodeElementWrapper(codeElementWrapper.CodeElement);
            codeElementWrapperArray.AddCodeElementWrapper(codeElementWrapperClone);
        }

        /// <summary>
        /// Iterates through the code model starting at a specified element
        /// and adds the discovered elements to a tree.
        /// </summary>
        /// <param name="codeElement">The starting CodeElement.</param>
        /// <param name="nodes">The TreeNodeCollection to add the element to.</param>
        /// <param name="interruptible">true if this method can be interrupted (is background), otherwise false.</param>
        /// <returns>true if the method completed, otherwise false.</returns>
        private bool ReadCodeModelElementsRecursive(CodeElement codeElement, TreeNodeCollection nodes,
                                                    bool interruptible)
        {
            if (interruptible)
            {
                yieldCount0++;
                if (yieldCount0 >= 5)
                {
                    yieldCount0 = 0;
                    var pkg = sourceOutlineToolWindow.Package as NPLLanguageServicePackage;
                    if (pkg != null)
                    {
                        IOleComponentManager cm = pkg.ComponentManager;
                        if (cm != null)
                        {
                            if (cm.FContinueIdle() == 0)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            if (codeElement == null)
            {
                return true;
            }

            // Test whether this is a code element of interest.
            if (!CodeModelHelpers.IsInterestingKind(codeElement.Kind))
            {
                return true;
            }

            if (codeElement.InfoLocation != vsCMInfoLocation.vsCMInfoLocationProject)
            {
                return true;
            }

            CodeElementWrapper treenode;
            string uniqueID = CodeModelHelpers.GetUniqueElementId(codeElement);

            // See if this node has finished processing.
            mapElementIDToTreeViewCodeElementWrapper.TryGetValue(uniqueID, out treenode);
            if (treenode != null)
            {
                if (treenode.FinishedLoading)
                    return true;
            }
            else
            {
                treenode = new CodeElementWrapper(codeElement);
                treenode.Expand();
                nodes.Add(treenode);
                AddNodeToInternalStructures(treenode);
            }

            treenode.FinishedLoading = ReadCodeModelChildrenElementsRecursive(treenode, interruptible);

            return treenode.FinishedLoading;
        }

        /// <summary>
        /// Iterates through the children of a code model element.
        /// </summary>
        /// <param name="codeElementWrapper">The CodeElementWrapper to process.</param>
        /// <param name="interruptible">true if this method can be interrupted (is background), otherwise false.</param>
        /// <returns>true if the method completed, otherwise false.</returns>
        private bool ReadCodeModelChildrenElementsRecursive(CodeElementWrapper codeElementWrapper, bool interruptible)
        {
            if (interruptible)
            {
                yieldCount1++;
                if (yieldCount1 >= 5)
                {
                    yieldCount1 = 0;
                    var pkg = sourceOutlineToolWindow.Package as NPLLanguageServicePackage;
                    if (pkg != null)
                    {
                        IOleComponentManager cm = pkg.ComponentManager;
                        if (cm != null)
                        {
                            if (cm.FContinueIdle() == 0)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            var members = CodeModelHelpers.GetMembersOf(codeElementWrapper.CodeElement);
            if (members != null)
            {
                foreach (CodeElement codeElementMember in members)
                {
                    string uniqueID = CodeModelHelpers.GetUniqueElementId(codeElementMember);
                    CodeElementWrapper node;
                    mapElementIDToTreeViewCodeElementWrapper.TryGetValue(uniqueID, out node);
                    if (node != null)
                    {
                        if (node.FinishedLoading)
                            continue;
                    }

                    // Prevent a recursive call if the code element is not of interest.
                    if (CodeModelHelpers.IsInterestingKind(codeElementMember.Kind))
                    {
                        var childrenRead = ReadCodeModelElementsRecursive(codeElementMember, codeElementWrapper.Nodes, interruptible);
                        if (!childrenRead)
                        {
                            // Children failed to be read, so leave.
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        #endregion Code Model

        #region Code Model Events

        /// <summary>
        /// Reloads the source file.
        /// </summary>
        /// <param name="element">The targeted CodeElement.</param>
        /// <remarks>
        /// This function exists for future enhancement.
        /// Currently, Source Outliner reloads the entire file when an element changes.
        /// This is done because there are difficulties with tracking the unique
        /// ID of some elements; see the comments in GetUniqueElementId. 
        /// In the future, if all languages correctly support unique identifiers,
        /// this method could start with the element parameter, find its parent 
        /// CodeElement in the hierarchical tree, and reload only the parent.
        /// Only if the parent cannot be found should it reload the entire source file.
        /// </remarks>
        public void FindBestMatchCodeElementToRefreshFrom(CodeElement element)
        {
            Load();
        }

        /// <summary>
        /// Occurs when a new code element is added to the text window,
        /// and adds the new element to the appropriate place in the outline.
        /// </summary>
        /// <param name="newElement">The new code element.</param>
        public void OnCodeModelElementAdd(CodeElement newElement)
        {
            if ((newElement == null) || !CodeModelHelpers.IsInterestingKind(newElement.Kind))
            {
                return;
            }

            try
            {
                int line = newElement.StartPoint.Line;
            }
            catch
            {
                // An exception can be thrown here when an element is being edited, so ignore it.
                return;
            }

            // Update the tree.
            if (newElement.Kind == vsCMElement.vsCMElementParameter)
            {
                FindBestMatchCodeElementToRefreshFrom(newElement);
                return;
            }

            // Get the start point from the wrapper object and not from the CodeElement directly.
            var temp = new CodeElementWrapper(newElement);
            TextPoint tp = temp.StartPoint;
            CodeElementWrapper cew = GetClosestCodeElementInTreeView(TreeView, tp);

            if (cew == null)
            {
                // Nothing found, this must be the first element drawn.
                ReadCodeModelElementsRecursive(newElement, TreeView.Nodes, false);
            }
            else
            {
                var newNode = new CodeElementWrapper(newElement);

                // Note that the add could result from a paste, and the editor only informs 
                // regarding the outer element that was added if the language is C# or VB;
                // the C++ language raises an event for each element.
                AddNodeToInternalStructures(newNode);
                ReadCodeModelChildrenElementsRecursive(newNode, false);

                newNode.ExpandAll();

                int index = 0;

                // Insert this element in the correct place.
                foreach (CodeElementWrapper n in cew.Nodes)
                {
                	if (n.Location > newNode.Location)
                    {
                        // Insert prior to n.
                        cew.Nodes.Insert(index, newNode);
                        newNode = null;
                        break;
                    }

                	index++;
                }

            	// If it was not inserted, append it.

                if (newNode != null)
                {
                    cew.Nodes.Add(newNode);
                }
            }

            // Update the filter view.
            codeElementWrapperIndexTable = new CodeElementWrapperIndexTable(codeElementWrapperArray);
            currentResults = codeElementWrapperIndexTable.GenerateResultsTable(searchCriteria);
            FilterText = currentFilterText;
        }

        /// <summary>
        /// Occurs when a code element is changed in the text window,
        /// and finds the element's parent in the outline and reloads it.
        /// </summary>
        /// <param name="modifiedElement">The changed code element.</param>
        /// <param name="iChangeType">The type of change.</param>
        public void OnCodeModelElementChanged(CodeElement modifiedElement, vsCMChangeKind iChangeType)
        {
            if (modifiedElement == null || !CodeModelHelpers.IsInterestingKind(modifiedElement.Kind))
            {
                return;
            }

            Debug.Assert(dte.ActiveDocument == currentDocument);

            FindBestMatchCodeElementToRefreshFrom(modifiedElement);
        }

        /// <summary>
        /// Occurs when a code element is removed from the text window,
        /// and deletes the element from the outline.
        /// </summary>
        /// <param name="parent">The parent object for the CodeElement.</param>
        /// <param name="deletedElement">The deleted CodeElement.</param>
        public void OnCodeModelElementDeleted(object parent, CodeElement deletedElement)
        {
            if (dte.ActiveDocument == null)
            {
                return;
            }

            if ((parent == null) || !CodeModelHelpers.IsInterestingKind(deletedElement.Kind))
            {
                return;
            }

            Debug.Assert(dte.ActiveDocument == currentDocument);

            try
            {
                var codeElementParent = parent as CodeElement;
                if (codeElementParent == null)
                {
                    Load();
                    return;
                }

                var codeElementWrapperParent = FindElementInTree(codeElementParent);
                if (codeElementWrapperParent == null)
                {
                    Load();
                    return;
                }

                List<CodeElementWrapper> listCodeElementWrapperToDelete = null;
                FindChildrenByName(codeElementWrapperParent, deletedElement, out listCodeElementWrapperToDelete);
                if ((listCodeElementWrapperToDelete == null)
                    || (listCodeElementWrapperToDelete.Count == 0))
                {
                    Load();
                    return;
                }

                if (listCodeElementWrapperToDelete.Count == 1)
                {
                    try
                    {
                        TreeView.BeginUpdate();
                        FilterView.BeginUpdate();
                        BeginTransaction();

                        var codeElementWrapperToDelete = listCodeElementWrapperToDelete[0];

                        // Remove element from internal data structures.
                        RemoveElementRecursive(codeElementWrapperToDelete, codeElementWrapperParent);
                    }
                    finally
                    {
                        EndTransaction();
                        FilterView.EndUpdate();
                        TreeView.EndUpdate();
                    }
                }
                else
                {
                    // If there are multiple elements with the same name and no exact
                    // match was found, do a full refresh of the parent node.
                    RefreshCodeModelForElement(ref codeElementWrapperParent, codeElementParent);
                }
            }
            catch (Exception)
            {
                // On error, reload everything.
                Load();
                throw;
            }
        }

        #endregion Code Model Events

        #region Tree Helpers

        /// <summary>
        /// Locates a specific element in the tree by ID.
        /// </summary>
        /// <param name="codeElement">The CodeElement object to locate.</param>
        /// <returns>The CodeElementWrapper object containing the element.</returns>
        private CodeElementWrapper FindElementInTree(CodeElement codeElement)
        {
            bool doSearch = false;
            CodeElementWrapper codeElementWrapper = null;
            string strElementID = CodeModelHelpers.GetUniqueElementId(codeElement);
            if (strElementID == null)
            {
                doSearch = true;
            }
            else
            {
                try
                {
                    // Try using fast lookup first.
                    codeElementWrapper = mapElementIDToTreeViewCodeElementWrapper[strElementID];
                }
                catch (KeyNotFoundException)
                {
                    doSearch = true;
                }
            }
            if (doSearch)
            {
                // If not found in the map, use sequential search.
                foreach (CodeElementWrapper node in TreeView.Nodes)
                {
                    codeElementWrapper = FindElementInTreeRecursive(codeElement, node);
                    if (codeElementWrapper != null)
                    {
                        break;
                    }
                }
            }

            return codeElementWrapper;
        }

        /// <summary>
        /// Locates a specific element in the flat view by ID.
        /// </summary>
		/// <param name="codeElementWrapper">The CodeElementWrapper object to locate.</param>
        /// <returns>The CodeElementWrapper object containing the element.</returns>
        private CodeElementWrapper FindElementInFilterView(CodeElementWrapper codeElementWrapper)
        {
            CodeElementWrapper codeElementWrapperOut = null;
            string strElementID = codeElementWrapper.UniqueElementId;

            // Use sequential search.
            foreach (CodeElementWrapper node in FilterView.Nodes)
            {
                if (strElementID == node.UniqueElementId)
                {
                    codeElementWrapperOut = node;
                    break;
                }
            }

            return codeElementWrapperOut;
        }

        /// <summary>
        /// Locates a specific element in a tree node by ID, recursively.
        /// </summary>
        /// <param name="codeElement">The CodeElement object to locate.</param>
        /// <param name="codeElementWrapperNode">The CodeElementWrapper node to search.</param>
        /// <returns>The CodeElementWrapper object containing the element.</returns>
        private static CodeElementWrapper FindElementInTreeRecursive(CodeElement codeElement,
                                                              CodeElementWrapper codeElementWrapperNode)
        {
            string strCodeElementID = CodeModelHelpers.GetUniqueElementId(codeElement);

            if (strCodeElementID == codeElementWrapperNode.UniqueElementId)
            {
                return codeElementWrapperNode;
            }

            // If this is not it, check the children.
            CodeElementWrapper codeElementWrapper = null;
            foreach (CodeElementWrapper node in codeElementWrapperNode.Nodes)
            {
                codeElementWrapper = FindElementInTreeRecursive(codeElement, node);
                if (codeElementWrapper != null)
                {
                    break;
                }
            }

            return codeElementWrapper;
        }

        /// <summary>
        /// Locates an element by name within a set of nodes.
        /// </summary>
        /// <param name="codeElementWrapperParent">The CodeElementWrapper object to search.</param>
        /// <param name="codeElement">The CodeElement object to locate.</param>
        /// <param name="listCodeElementWrapperOut">The List of elements found.</param>
        private static void FindChildrenByName(TreeNode codeElementWrapperParent,
                                               CodeElement codeElement,
                                               out List<CodeElementWrapper> listCodeElementWrapperOut)
        {
            listCodeElementWrapperOut = new List<CodeElementWrapper>();
            string strElementName = codeElement.Name;

            int matches = 0;
            foreach (CodeElementWrapper node in codeElementWrapperParent.Nodes)
            {
                if (node.ElementName == strElementName)
                {
                    listCodeElementWrapperOut.Add(node);
                    matches++;
                }
            }
        }

        /// <summary>
        /// Locates an element in the wrapper array.
        /// </summary>
        /// <param name="codeElementWrapper">The CodeElementWrapper object to locate.</param>
        /// <returns>The CodeElementWrapper node that was located in the array.</returns>
        private CodeElementWrapper FindElementInWrapperArray(CodeElementWrapper codeElementWrapper)
        {
            CodeElementWrapper codeElementWrapperOut = null;

            try
            {
                codeElementWrapperOut = codeElementWrapperArray.FindCodeElementWrapper(codeElementWrapper);
            }
            catch (Exception)
            {
                // If not found in the map, use a sequential search.
                string strElementID = codeElementWrapper.UniqueElementId;
                foreach (CodeElementWrapper node in codeElementWrapperArray)
                {
                    if (strElementID == node.UniqueElementId)
                    {
                        codeElementWrapperOut = node;
                        break;
                    }
                }
            }

            return codeElementWrapperOut;
        }

        /// <summary>
        /// Refreshes a specific tree node with a specific element.
        /// </summary>
        /// <param name="codeElementWrapper">The CodeElementWrapper to refresh.</param>
        /// <param name="codeElement">The CodeElement to refresh with.</param>
        private void RefreshCodeModelForElement(ref CodeElementWrapper codeElementWrapper,
                                                CodeElement codeElement)
        {
            TreeNode treeNodeFilterView = null;
            TreeNode treeNodeTreeView = null;

            try
            {
                treeNodeFilterView = FilterView.SelectedNode;
                treeNodeTreeView = TreeView.SelectedNode;

                TreeView.BeginUpdate();
                FilterView.BeginUpdate();

                BeginTransaction();

                // Update the element itself.
                codeElementWrapper.CodeElement = codeElement;

                // Remove elements from the parent in reverse order.
                for (int i = codeElementWrapper.Nodes.Count - 1; i >= 0; i--)
                {
                    var node = codeElementWrapper.Nodes[i] as CodeElementWrapper;
                    RemoveElementRecursive(node, codeElementWrapper);
                }

                codeElementWrapper.Nodes.Clear();

                ReadCodeModelChildrenElementsRecursive(codeElementWrapper, false);

                // Update the copy of the codeElementWrapper in the wrapper array.
                CodeElementWrapper codeElementWrapperClone = FindElementInWrapperArray(codeElementWrapper);
                if (codeElementWrapperClone != null)
                {
                    RemoveElementFromWrapperArray(codeElementWrapperClone);
                }
                codeElementWrapperClone = new CodeElementWrapper(codeElementWrapper.CodeElement);
                codeElementWrapperArray.AddCodeElementWrapper(codeElementWrapperClone);
            }
            finally
            {
                EndTransaction();

                TreeView.ExpandAll();

                FilterView.EndUpdate();
                TreeView.EndUpdate();

                FilterView.SelectedNode = treeNodeFilterView;
                TreeView.SelectedNode = treeNodeTreeView;
            }
        }

        /// <summary>
        /// Removes a node and all of its children.
        /// </summary>
        /// <param name="codeElementWrapperToDelete">The CodeElementWrapper to delete.</param>
        /// <param name="codeElementWrapperParent">The CodeElementWrapper parent of the object to delete.</param>
        private void RemoveElementRecursive(CodeElementWrapper codeElementWrapperToDelete,
                                            CodeElementWrapper codeElementWrapperParent)
        {
        	// Remove the children before the node, deleting upwards from last to first.
            for (int i = codeElementWrapperToDelete.Nodes.Count - 1; i >= 0; i--)
            {
                var node = codeElementWrapperToDelete.Nodes[i] as CodeElementWrapper;
                RemoveElementRecursive(node, codeElementWrapperToDelete);
            }

            // Remove the element from the outline view.
            RemoveElementFromTree(codeElementWrapperToDelete, codeElementWrapperParent);

            // Remove the element from the filter view.
            CodeElementWrapper codeElementWrapper = FindElementInFilterView(codeElementWrapperToDelete);
            if (codeElementWrapper != null)
            {
                RemoveElementFromFilterView(codeElementWrapper);
            }

            // Remove the element from the array.
            codeElementWrapper = FindElementInWrapperArray(codeElementWrapperToDelete);
            if (codeElementWrapper != null)
            {
                RemoveElementFromWrapperArray(codeElementWrapper);
            }
        }

        /// <summary>
        /// Removes an element from the outline tree view by ID.
        /// </summary>
        /// <param name="codeElementWrapperToDelete">The CodeElementWrapper to delete.</param>
        /// <param name="codeElementWrapperParent">The CodeElementWrapper parent of the object to delete.</param>
        private void RemoveElementFromTree(CodeElementWrapper codeElementWrapperToDelete,
                                           CodeElementWrapper codeElementWrapperParent)
        {
            // Remove the element from the tree view.
            codeElementWrapperParent.Nodes.Remove(codeElementWrapperToDelete);

            // Remove the element from the map.
            mapElementIDToTreeViewCodeElementWrapper.Remove(codeElementWrapperToDelete.UniqueElementId);
        }

        /// <summary>
        /// Removes an element from the flat view by ID.
        /// </summary>
        /// <param name="codeElementWrapperToDelete">The CodeElementWrapper to delete.</param>
        private void RemoveElementFromFilterView(CodeElementWrapper codeElementWrapperToDelete)
        {
            // Remove the element from the filter view.
            FilterView.Nodes.Remove(codeElementWrapperToDelete);

            // Remove the element from the map.
            mapElementIDToFilterViewCodeElementWrapper.Remove(codeElementWrapperToDelete.UniqueElementId);
        }

        /// <summary>
        /// Removes an element from the wrapper array.
        /// </summary>
        /// <param name="codeElementWrapperToDelete">The CodeElementWrapper to remove.</param>
        private void RemoveElementFromWrapperArray(CodeElementWrapper codeElementWrapperToDelete)
        {
            // Remove the element from the array.
            codeElementWrapperArray.Remove(codeElementWrapperToDelete);
        }

        #endregion Tree Helpers

        #region Transactions

        /// <summary>
        /// Increments the transaction counter.
        /// </summary>
        private void BeginTransaction()
        {
            transactionCounter++;
        }

        /// <summary>
        /// Decrements the transaction counter.
        /// </summary>
        /// <remarks>
        /// The counter determines if nested operations are running,
        /// and when it reaches zero, the filter is rebuilt to reflect the operations.
        /// </remarks>
        private void EndTransaction()
        {
            if (transactionCounter == 0)
            {
                Debug.Assert(false, "Invalid call to EndTransaction");

                return;
            }

            transactionCounter--;
            if (transactionCounter == 0)
            {
                // Recompute current results.
                codeElementWrapperIndexTable = new CodeElementWrapperIndexTable(codeElementWrapperArray);
                currentResults = codeElementWrapperIndexTable.GenerateResultsTable(searchCriteria);
                FilterText = currentFilterText;
            }
        }

        #endregion Transactions

        /// <summary>
        /// Continues loading the source outline.
        /// </summary>
        internal void ContinueLoading()
        {
            ContinueLoadingCodeModel();
        }

        /// <summary>
        /// Completes loading the source outline.
        /// </summary>
        internal void FinishLoading()
        {
            codeElementWrapperIndexTable = new CodeElementWrapperIndexTable(codeElementWrapperArray);
            currentResults = codeElementWrapperIndexTable.GenerateResultsTable(searchCriteria);

            if ((control.filterStringTextBox.Text != "<Filter>") && (control.filterStringTextBox.Text != ""))
            {
                control.filterStringTextBox.Text = "<Filter>";
                if (viewType == ViewType.TreeView)
                {
                    TreeView.ExpandAll();
                }
            }
            else
            {
                switch (viewType)
                {
                    case ViewType.FlatView:
                        LoadFlatView();
                        break;

                    case ViewType.TreeView:
                        TreeView.ExpandAll();
                        break;
                }
            }
            State = OutlineFileManagerState.WaitToStartOver;
        }
    }
}