﻿/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
/*
using Microsoft.LuaTools.Analysis;
using Microsoft.LuaTools.Intellisense;
using Microsoft.LuaTools.Interpreter;
using Microsoft.LuaTools.Interpreters;
*/
using Microsoft.VisualStudioTools.Navigation;
//using Microsoft.LuaTools.Repl;
using Microsoft.VisualStudio;
//using Microsoft.VisualStudio.Repl;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using NativeMethods = Microsoft.VisualStudioTools.Project.NativeMethods;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using Task = System.Threading.Tasks.Task;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;
using VsMenus = Microsoft.VisualStudioTools.Project.VsMenus;

using Microsoft.VisualStudio.OLE.Interop;

namespace NPLTools.Project
{
    [Guid(Guids.ProjectNodeGuid)]
    internal class NPLProjectNode : CommonProjectNode, ILuaProject
    {
        // For files that are analyzed because they were directly or indirectly referenced in the search path, store the information
        // about the directory from the search path that referenced them in IProjectEntry.Properties[_searchPathEntryKey], so that
        // they can be located and removed when that directory is removed from the path.
        private static readonly object _searchPathEntryKey = new { Name = "SearchPathEntry" };
        /*
                private object _designerContext;
                private VsProjectAnalyzer _analyzer;
        */
        private readonly HashSet<string> _warningFiles = new HashSet<string>();
        private readonly HashSet<string> _errorFiles = new HashSet<string>();
        /*
                private LuaDebugPropertyPage _debugPropPage;
                private CommonSearchPathContainerNode _searchPathContainer;
                private InterpretersContainerNode _interpretersContainer;
                private MSBuildProjectInterpreterFactoryProvider _interpreters;

                internal List<CustomCommand> _customCommands;

                private string _customCommandsDisplayLabel;
                private HashSet<IReplWindow> _associatedReplWindows;
        */
        public NPLProjectNode(CommonProjectPackage package)
            //            : base(package, Utilities.GetImageList(typeof(LuaProjectNode).Assembly.GetManifestResourceStream(LuaConstants.ProjectImageList))) {
            : base(package, Utilities.GetImageList(new FileStream("../../Resources/Solution.png", FileMode.Open)))//Images.png
        {
            /*
                        Type projectNodePropsType = typeof(LuaProjectNodeProperties);
                        AddCATIDMapping(projectNodePropsType, projectNodePropsType.GUID);
            */
        }

        protected override void NewBuildProject(Microsoft.Build.Evaluation.Project project)
        {
            base.NewBuildProject(project);
            /*
                        if (_interpreters != null) {
                            _interpreters.ActiveInterpreterChanged -= ActiveInterpreterChanged;
                            _interpreters.InterpreterFactoriesChanged -= InterpreterFactoriesChanged;
                            _interpreters.Dispose();
                            _interpreters = null;
                        }

                        // Remove old custom commands
                        if (_customCommands != null) {
                            foreach (var c in _customCommands) {
                                c.Dispose();
                            }
                        }
                        _customCommands = null;

                        // Project has been cleared, so nothing elso to do here
                        if (project == null) {
                            return;
                        }

                        // Hook up the interpreter factory provider
                        var interpreterService = LuaToolsPackage.ComponentModel.GetService<IInterpreterOptionsService>();
                        _interpreters = new MSBuildProjectInterpreterFactoryProvider(interpreterService, project);
                        try {
                            _interpreters.DiscoverInterpreters();
                        } catch (InvalidDataException ex) {
                            OutputWindowRedirector.GetGeneral(Site).WriteErrorLine(ex.Message);
                        }
                        _interpreters.ActiveInterpreterChanged += ActiveInterpreterChanged;
                        _interpreters.InterpreterFactoriesChanged += InterpreterFactoriesChanged;

                        // Add any custom commands
                        _customCommands = CustomCommand.GetCommands(project, this).ToList();
                        _customCommandsDisplayLabel = CustomCommand.GetCommandsDisplayLabel(project, this);
            */
        }
        /*
                private void InterpreterFactoriesChanged(object sender, EventArgs e) {
                    RefreshInterpreters();
                }

                internal MSBuildProjectInterpreterFactoryProvider Interpreters {
                    get {
                        return _interpreters;
                    }
                }
        */
        protected override Stream ProjectIconsImageStripStream
        {
            get
            {
                return new FileStream("../../Resources/imagelis.bmp", FileMode.Open);
            }
        }
        /*
                internal int GetIconIndex(LuaProjectImageName name) {
                    return ImageOffset + (int)name;
                }

                internal IntPtr GetIconHandleByName(LuaProjectImageName name) {
                    return ImageHandler.GetIconHandle(GetIconIndex(name));
                }

                private static string GetSearchPathEntry(IProjectEntry entry) {
                    object result;
                    entry.Properties.TryGetValue(_searchPathEntryKey, out result);
                    return (string)result;
                }

                private static void SetSearchPathEntry(IProjectEntry entry, string value) {
                    entry.Properties[_searchPathEntryKey] = value;
                }

                public override CommonFileNode CreateCodeFileNode(ProjectElement item) {
                    return new LuaFileNode(this, item);
                }

                public override CommonFileNode CreateNonCodeFileNode(ProjectElement item) {
                    return new LuaNonCodeFileNode(this, item);
                }

                protected override ReferenceContainerNode CreateReferenceContainerNode() {
                    return new LuaReferenceContainerNode(this);
                }

                protected override void LinkFileAdded(string filename) {
                    if (LuaToolsPackage.Instance.DebuggingOptionsPage.UpdateSearchPathsWhenAddingLinkedFiles) {
                        // update our search paths.
                        string dirToAdd = Path.GetDirectoryName(filename);
                        while (!String.IsNullOrEmpty(dirToAdd) && File.Exists(Path.Combine(dirToAdd, "__init__.py"))) {
                            dirToAdd = Path.GetDirectoryName(dirToAdd);
                        }

                        AddSearchPathEntry(CommonUtils.EnsureEndSeparator(dirToAdd));
                    }

                    base.LinkFileAdded(filename);
                }
        */
        protected override Guid[] GetConfigurationIndependentPropertyPages()
        {
            return new[] {
                GetGeneralPropertyPageType().GUID/*,
                typeof(LuaDebugPropertyPage).GUID,
                typeof(PublishPropertyPage).GUID*/
            };
        }

        /// <summary>
        /// Evaluates if a file is a current language code file based on is extension
        /// </summary>
        /// <param name="strFileName">The filename to be evaluated</param>
        /// <returns>true if is a code file</returns>
        public override bool IsCodeFile(string strFileName)
        {
            return IsLuaFile(strFileName);
        }

        public override string[] CodeFileExtensions
        {
            get
            {
                return new[] { LuaConstants.FileExtension, ".npl"};
            }
        }

        internal static bool IsLuaFile(string strFileName)
        {
            var ext = Path.GetExtension(strFileName);

            return String.Equals(ext, LuaConstants.FileExtension, StringComparison.OrdinalIgnoreCase) ||
                String.Equals(ext, ".npl", StringComparison.OrdinalIgnoreCase);
        }

        public override Type GetProjectFactoryType()
        {
            return typeof(NPLProjectFactory);
        }

        public override string GetProjectName()
        {
            return "NPLProject";
        }

        public override string GetFormatList()
        {
            return LuaConstants.ProjectFileFilter;
        }

        public override Type GetGeneralPropertyPageType()
        {
            return typeof(NPLPropertyPage);
        }

        public override Type GetEditorFactoryType()
        {
            //            return typeof(LuaEditorFactory);
            return typeof(IVsEditorFactory);
        }

        public override Type GetLibraryManagerType()
        {
            return typeof(ILibraryManager);
        }
        /*
                protected override NodeProperties CreatePropertiesObject() {
                    return new LuaProjectNodeProperties(this);
                }

                public override CommonProjectConfig MakeConfiguration(string activeConfigName) {
                    return new LuaProjectConfig(this, activeConfigName);
                }

                protected internal override FolderNode CreateFolderNode(ProjectElement element) {
                    return new LuaFolderNode(this, element);
                }

                public override FileNode CreateFileNode(ProjectElement item) {
                    var newNode = base.CreateFileNode(item);
                    string include = item.GetMetadata(ProjectFileConstants.Include);

                    if (XamlDesignerSupport.DesignerContextType != null &&
                        newNode is CommonFileNode &&
                        !string.IsNullOrEmpty(include) &&
                        Path.GetExtension(include).Equals(".xaml", StringComparison.OrdinalIgnoreCase)) {
                        //Create a DesignerContext for the XAML designer for this file
                        newNode.OleServiceProvider.AddService(XamlDesignerSupport.DesignerContextType, ((CommonFileNode)newNode).ServiceCreator, false);
                    }

                    return newNode;
                }

                protected override bool FilterItemTypeToBeAddedToHierarchy(string itemType) {
                    if (MSBuildProjectInterpreterFactoryProvider.InterpreterReferenceItem.Equals(itemType, StringComparison.Ordinal) ||
                        MSBuildProjectInterpreterFactoryProvider.InterpreterItem.Equals(itemType, StringComparison.Ordinal)) {
                        return true;
                    }
                    return base.FilterItemTypeToBeAddedToHierarchy(itemType);
                }

                public override void Load(string filename, string location, string name, uint flags, ref Guid iidProject, out int canceled) {
                    base.Load(filename, location, name, flags, ref iidProject, out canceled);

                    if (XamlDesignerSupport.DesignerContextType != null) {
                        //If this is a WPFFlavor-ed project, then add a project-level DesignerContext service to provide
                        //event handler generation (EventBindingProvider) for the XAML designer.
                        OleServiceProvider.AddService(XamlDesignerSupport.DesignerContextType, new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);
                    }
                }

                protected override object CreateServices(Type serviceType) {
                    if (XamlDesignerSupport.DesignerContextType == serviceType) {
                        return DesignerContext;
                    }

                    var res = base.CreateServices(serviceType);
                    return res;
                }

                protected override void Reload() {
                    _searchPathContainer = new CommonSearchPathContainerNode(this);
                    this.AddChild(_searchPathContainer);
                    RefreshCurrentWorkingDirectory();
                    RefreshSearchPaths();
                    _interpretersContainer = new InterpretersContainerNode(this);
                    this.AddChild(_interpretersContainer);
                    RefreshInterpreters();

                    OnProjectPropertyChanged += LuaProjectNode_OnProjectPropertyChanged;
                    base.Reload();

                    string id;
                    if (_interpreters.IsActiveInterpreterGlobalDefault &&
                        !string.IsNullOrEmpty(id = GetProjectProperty(MSBuildProjectInterpreterFactoryProvider.InterpreterIdProperty))) {
                        // The interpreter in the project file has no reference, so
                        // find and add it.
                        var interpreterService = LuaToolsPackage.ComponentModel.GetService<IInterpreterOptionsService>();
                        if (interpreterService != null) {
                            var existing = interpreterService.FindInterpreter(
                                id,
                                GetProjectProperty(MSBuildProjectInterpreterFactoryProvider.InterpreterVersionProperty));
                            if (existing != null && QueryEditProjectFile(false)) {
                                _interpreters.AddInterpreter(existing);
                                Debug.Assert(_interpreters.ActiveInterpreter == existing);
                            }
                        }
                    }

                    LuaToolsPackage.Instance.CheckSurveyNews(false);
                }

                private void RefreshCurrentWorkingDirectory() {
                    try {
                        IsRefreshing = true;
                        string projHome = ProjectHome;
                        string workDir = GetWorkingDirectory();

                        //Refresh CWD node
                        bool needCWD = !CommonUtils.IsSameDirectory(projHome, workDir);
                        var cwdNode = FindImmediateChild<CurrentWorkingDirectoryNode>(_searchPathContainer);
                        if (needCWD) {
                            if (cwdNode == null) {
                                //No cwd node yet
                                _searchPathContainer.AddChild(new CurrentWorkingDirectoryNode(this, workDir));
                            } else if (!CommonUtils.IsSameDirectory(cwdNode.Url, workDir)) {
                                //CWD has changed, recreate the node
                                cwdNode.Remove(false);
                                _searchPathContainer.AddChild(new CurrentWorkingDirectoryNode(this, workDir));
                            }
                        } else {
                            //No need to show CWD, remove if exists
                            if (cwdNode != null) {
                                cwdNode.Remove(false);
                            }
                        }
                    } finally {
                        IsRefreshing = false;
                    }
                }

                private void RefreshSearchPaths() {
                    try {
                        IsRefreshing = true;

                        string projHome = ProjectHome;
                        string workDir = GetWorkingDirectory();
                        IList<string> searchPath = ParseSearchPath();

                        //Refresh regular search path nodes

                        //We need to update search path nodes according to the search path property.
                        //It's quite expensive to remove all and build all nodes from scratch, 
                        //so we are going to perform some smarter update.
                        //We are looping over paths in the search path and if a corresponding node
                        //exists, we only update its index (sort order), creating new node otherwise.
                        //At the end all nodes that haven't been updated have to be removed - they are
                        //not in the search path anymore.
                        var searchPathNodes = new List<CommonSearchPathNode>();
                        this.FindNodesOfType<CommonSearchPathNode>(searchPathNodes);
                        bool[] updatedNodes = new bool[searchPathNodes.Count];
                        int index;
                        for (int i = 0; i < searchPath.Count; i++) {
                            string path = searchPath[i];
                            //ParseSearchPath() must resolve all paths
                            Debug.Assert(Path.IsPathRooted(path));
                            var node = FindSearchPathNodeByPath(searchPathNodes, path, out index);
                            if (node != null) {
                                //existing path, update index (sort order)
                                node.Index = i;
                                updatedNodes[index] = true;
                            } else {
                                //new path - create new node
                                _searchPathContainer.AddChild(new CommonSearchPathNode(this, path, i));
                            }
                        }

                        //Refresh nodes and remove non-updated ones
                        for (int i = 0; i < searchPathNodes.Count; i++) {
                            if (!updatedNodes[i]) {
                                searchPathNodes[i].Remove();
                            }
                        }
                    } finally {
                        IsRefreshing = false;
                    }
                }

                private void RefreshInterpreters() {
                    if (IsClosed) {
                        return;
                    }

                    if (_uiSync.InvokeRequired) {
                        _uiSync.BeginInvoke((Action)RefreshInterpreters, null);
                        return;
                    }

                    var node = _interpretersContainer;
                    if (node == null) {
                        return;
                    }

                    var remaining = node.AllChildren.OfType<InterpretersNode>().ToDictionary(n => n._factory);

                    var interpreters = Interpreters;
                    if (interpreters != null) {
                        foreach (var fact in interpreters.GetInterpreterFactories()) {
                            if (!remaining.Remove(fact)) {
                                node.AddChild(new InterpretersNode(
                                    this,
                                    Interpreters.GetProjectItem(fact),
                                    fact,
                                    isInterpreterReference: !interpreters.IsProjectSpecific(fact),
                                    canDelete:
                                        interpreters.IsProjectSpecific(fact) &&
                                        Directory.Exists(fact.Configuration.PrefixPath)
                                ));
                            }
                        }
                    }

                    foreach (var child in remaining.Values) {
                        node.RemoveChild(child);
                    }

                    bool wasExpanded = node.GetIsExpanded();
                    var expandAfter = node.AllChildren.Where(n => n.GetIsExpanded()).ToArray();
                    OnInvalidateItems(node);
                    if (wasExpanded) {
                        node.ExpandItem(EXPANDFLAGS.EXPF_ExpandFolder);
                    }
                    foreach (var child in expandAfter) {
                        child.ExpandItem(EXPANDFLAGS.EXPF_ExpandFolder);
                    }
                    BoldActiveEnvironment();
                }

                private void BoldActiveEnvironment() {
                    var node = _interpretersContainer;
                    if (node != null) {
                        foreach (var child in node.AllChildren.OfType<InterpretersNode>()) {
                            BoldItem(child, child._factory == Interpreters.ActiveInterpreter);
                        }
                    }
                }
        */
        /// <summary>
        /// Returns first immediate child node (non-recursive) of a given type.
        /// </summary>
        private static T FindImmediateChild<T>(HierarchyNode parent)
            where T : HierarchyNode
        {
            for (HierarchyNode n = parent.FirstChild; n != null; n = n.NextSibling)
            {
                if (n is T)
                {
                    return (T)n;
                }
            }
            return null;
        }
        /*
                /// <summary>
                /// Finds Search Path node by a given search path and returns it along with the node's index. 
                /// </summary>
                private CommonSearchPathNode FindSearchPathNodeByPath(IList<CommonSearchPathNode> nodes, string path, out int index) {
                    index = 0;
                    for (int j = 0; j < nodes.Count; j++) {
                        if (CommonUtils.IsSameDirectory(nodes[j].Url, path)) {
                            index = j;
                            return nodes[j];
                        }
                    }
                    return null;
                }

                private void LuaProjectNode_OnProjectPropertyChanged(object sender, ProjectPropertyChangedArgs e) {
                    switch (e.PropertyName) {
                        case CommonConstants.StartupFile:
                            var genProp = GeneralPropertyPageControl;
                            if (genProp != null) {
                                genProp.StartupFile = e.NewValue;
                            }
                            break;
                        case CommonConstants.WorkingDirectory:
                            RefreshCurrentWorkingDirectory();
                            genProp = GeneralPropertyPageControl;
                            if (genProp != null) {
                                genProp.WorkingDirectory = e.NewValue;
                            }
                            break;
                        case CommonConstants.SearchPath:
                            RefreshSearchPaths();

                            // we need to remove old files from the analyzer and add the new files
                            HashSet<string> oldDirs = new HashSet<string>(ParseSearchPath(e.OldValue), StringComparer.OrdinalIgnoreCase);
                            HashSet<string> newDirs = new HashSet<string>(ParseSearchPath(e.NewValue), StringComparer.OrdinalIgnoreCase);

                            // figure out all the possible directory names we could be removing...
                            foreach (var fileProject in _analyzer.LoadedFiles) {
                                string file = fileProject.Key;
                                IProjectEntry projectEntry = fileProject.Value;
                                string searchPathEntry = GetSearchPathEntry(fileProject.Value);
                                if (searchPathEntry != null && !newDirs.Contains(searchPathEntry)) {
                                    _analyzer.UnloadFile(projectEntry);
                                }
                            }

                            // find the values only in the old list, and let the analyzer know it shouldn't be watching those dirs
                            oldDirs.ExceptWith(newDirs);
                            foreach (var dir in oldDirs) {
                                _analyzer.StopAnalyzingDirectory(dir);
                            }

                            AnalyzeSearchPaths(newDirs);
                            break;
                    }

                    var debugProp = DebugPropertyPage;
                    if (debugProp != null) {
                        ((LuaDebugPropertyPageControl)debugProp.Control).ReloadSetting(e.PropertyName);
                    }
                }

                private void AnalyzeSearchPaths(IEnumerable<string> newDirs) {
                    // now add all of the missing files, any dups will automatically not be re-analyzed
                    foreach (var dir in newDirs) {
                        if (File.Exists(dir)) {
                            // If it's a file and not a directory, parse it as a .zip
                            // file in accordance with PEP 273.
                            _analyzer.AnalyzeZipArchive(dir, onFileAnalyzed: entry => SetSearchPathEntry(entry, dir));
                        } else if (Directory.Exists(dir)) {
                            _analyzer.AnalyzeDirectory(dir, onFileAnalyzed: entry => SetSearchPathEntry(entry, dir));
                        }
                    }
                }

                private LuaGeneralPropertyPageControl GeneralPropertyPageControl {
                    get {
                        if (PropertyPage != null && PropertyPage.Control != null) {
                            return (LuaGeneralPropertyPageControl)PropertyPage.Control;
                        }

                        return null;
                    }
                }

                internal LuaDebugPropertyPage DebugPropertyPage {
                    get {
                        return _debugPropPage;
                    }
                    set {
                        _debugPropPage = value;
                    }
                }

                internal object DesignerContext {
                    get {
                        if (_designerContext == null) {
                            _designerContext = XamlDesignerSupport.CreateDesignerContext();
                        }
                        return _designerContext;
                    }
                }

                public LuaAnalyzer GetProjectAnalyzer() {
                    return GetAnalyzer().Project;
                }

                VsProjectAnalyzer ILuaProject.GetProjectAnalyzer() {
                    return GetAnalyzer();
                }
        */
        public event EventHandler ProjectAnalyzerChanged;

        public override IProjectLauncher GetLauncher()
        {
            return new NPLProjectLauncher(this);
        }
        /*
                internal void AddAssociatedReplWindow(IReplWindow window) {
                    Debug.Assert(window != null);
                    if (window == null) {
                        return;
                    }

                    if (_associatedReplWindows == null) {
                        _associatedReplWindows = new HashSet<IReplWindow>();
                    }
                    _associatedReplWindows.Add(window);
                }
        */
        protected override void Dispose(bool disposing)
        {
            /*
                        if (disposing) {
                            var windows = _associatedReplWindows;
                            _associatedReplWindows = null;
                            if (windows != null) {
                                foreach (var window in windows) {
                                    // Close backends when the project closes so we don't
                                    // leave Lua processes hanging around.
                                    var pyEvaluator = window.Evaluator as BaseLuaReplEvaluator;
                                    if (pyEvaluator != null) {
                                        pyEvaluator.Close();
                                    }

                                    // Close project-specific REPL windows when the project
                                    // closes.
                                    var pane = window as ToolWindowPane;
                                    var frame = pane != null ? pane.Frame as IVsWindowFrame : null;
                                    if (frame != null) {
                                        frame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
                                    }
                                }
                            }

                            if (_analyzer != null) {
                                UnHookErrorsAndWarnings(_analyzer);
                                if (WarningFiles.Count > 0 || ErrorFiles.Count > 0) {
                                    foreach (var file in WarningFiles.Concat(ErrorFiles)) {
                                        var node = FindNodeByFullPath(file) as LuaFileNode;
                                        if (node != null) {
                                            _analyzer.RemoveErrors(node.GetAnalysis(), suppressUpdate: false);
                                        }
                                    }
                                }

                                if (_analyzer.RemoveUser()) {
                                    _analyzer.Dispose();
                                }
                                _analyzer = null;
                            }

                            if (_interpreters != null) {
                                _interpreters.Dispose();
                                _interpreters = null;
                            }
                            if (_customCommands != null) {
                                foreach (var c in _customCommands) {
                                    c.Dispose();
                                }
                                _customCommands = null;
                            }
                        }

                        base.Dispose(disposing);
            */
        }
        /*
                public int SetInterpreterFactory(ILuaInterpreterFactory factory) {
                    if (Interpreters != null && factory != Interpreters.ActiveInterpreter) {
                        //Make sure we can edit the project file
                        if (!ProjectMgr.QueryEditProjectFile(false)) {
                            return VSConstants.OLE_E_PROMPTSAVECANCELLED;
                        }

                        Interpreters.ActiveInterpreter = factory;
                    }
                    return VSConstants.S_OK;
                }

                public ILuaInterpreter GetInterpreter() {
                    return GetAnalyzer().Interpreter;
                }

                public VsProjectAnalyzer GetAnalyzer() {
                    if (_analyzer == null) {
                        _analyzer = CreateAnalyzer();
                        AnalyzeSearchPaths(ParseSearchPath());
                    }
                    return _analyzer;
                }

                private VsProjectAnalyzer CreateAnalyzer() {
                    // check to see if we should share our analyzer with another project in the same solution.  This enables
                    // refactoring, find all refs, and intellisense across projects.
                    var vsSolution = (IVsSolution)GetService(typeof(SVsSolution));
                    if (vsSolution != null) {
                        var guid = new Guid(LuaConstants.ProjectFactoryGuid);
                        IEnumHierarchies hierarchies;
                        ErrorHandler.ThrowOnFailure((vsSolution.GetProjectEnum((uint)(__VSENUMPROJFLAGS.EPF_MATCHTYPE | __VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION), ref guid, out hierarchies)));
                        IVsHierarchy[] hierarchy = new IVsHierarchy[1];
                        uint fetched;
                        var curFactory = GetInterpreterFactory();
                        while (ErrorHandler.Succeeded(hierarchies.Next(1, hierarchy, out fetched)) && fetched == 1) {
                            var proj = hierarchy[0].GetProject();
                            Debug.Assert(proj != null);
                            if (proj != null) {
                                var pyProj = proj.GetLuaProject();
                                Debug.Assert(pyProj != null);

                                if (pyProj != this &&
                                    pyProj._analyzer != null &&
                                    pyProj._analyzer.InterpreterFactory == curFactory) {
                                    // we have the same interpreter, we'll share analysis engines across projects.
                                    pyProj._analyzer.AddUser();
                                    HookErrorsAndWarnings(pyProj._analyzer);
                                    return pyProj._analyzer;
                                }
                            }
                        }
                    }

                    var model = LuaToolsPackage.ComponentModel;
                    var interpreterService = model.GetService<IInterpreterOptionsService>();
                    var factory = GetInterpreterFactory();
                    var res = new VsProjectAnalyzer(
                        factory.CreateInterpreter(),
                        factory,
                        interpreterService.Interpreters.ToArray(),
                        model.GetService<IErrorProviderFactory>(),
                        false);

                    HookErrorsAndWarnings(res);
                    return res;
                }

                private void HookErrorsAndWarnings(VsProjectAnalyzer res) {
                    res.ErrorAdded += OnErrorAdded;
                    res.ErrorRemoved += OnErrorRemoved;
                    res.WarningAdded += OnWarningAdded;
                    res.WarningRemoved += OnWarningRemoved;
                }

                private void UnHookErrorsAndWarnings(VsProjectAnalyzer res) {
                    res.ErrorAdded -= OnErrorAdded;
                    res.ErrorRemoved -= OnErrorRemoved;
                    res.WarningAdded -= OnWarningAdded;
                    res.WarningRemoved -= OnWarningRemoved;
                }

                private void OnErrorAdded(object sender, FileEventArgs args) {
                    if (_diskNodes.ContainsKey(args.Filename)) {
                        _errorFiles.Add(args.Filename);
                    }
                }

                private void OnErrorRemoved(object sender, FileEventArgs args) {
                    _errorFiles.Remove(args.Filename);
                }

                private void OnWarningAdded(object sender, FileEventArgs args) {
                    if (_diskNodes.ContainsKey(args.Filename)) {
                        _warningFiles.Add(args.Filename);
                    }
                }

                private void OnWarningRemoved(object sender, FileEventArgs args) {
                    _warningFiles.Remove(args.Filename);
                }
        */
        /// <summary>
        /// File names within the project which contain errors.
        /// </summary>
        public HashSet<string> ErrorFiles
        {
            get
            {
                return _errorFiles;
            }
        }

        /// <summary>
        /// File names within the project which contain warnings.
        /// </summary>
        public HashSet<string> WarningFiles
        {
            get
            {
                return _warningFiles;
            }
        }
        /*
                internal ILuaInterpreterFactory GetInterpreterFactory() {
                    var fact = _interpreters.ActiveInterpreter;

                    LuaToolsPackage.EnsureCompletionDb(fact);

                    return fact;
                }
        */
        /// <summary>
        /// Called when the active interpreter is changed.  A new interpreter
        /// will be created immediately unless another project in the solution
        /// can provide a matching analyzer.
        /// </summary>
        private void ActiveInterpreterChanged(object sender, EventArgs e)
        {
            if (IsClosed)
            {
                return;
            }

            if (_uiSync.InvokeRequired)
            {
                _uiSync.Invoke((EventHandler)ActiveInterpreterChanged, sender, e);
                return;
            }
            /*
                        RefreshInterpreters();

                        if (_analyzer != null) {
                            UnHookErrorsAndWarnings(_analyzer);
                        }
                        var analyzer = CreateAnalyzer();

                        Reanalyze(this, analyzer);
                        if (_analyzer != null) {
                            analyzer.SwitchAnalyzers(_analyzer);
                            if (_analyzer.RemoveUser()) {
                                _analyzer.Dispose();
                            }
                        }

                        _analyzer = analyzer;
                        var searchPath = ParseSearchPath();
                        if (searchPath != null && _analyzer != null) {
                            AnalyzeSearchPaths(searchPath);
                        }
            */
            var analyzerChanged = ProjectAnalyzerChanged;
            if (analyzerChanged != null)
            {
                analyzerChanged(this, EventArgs.Empty);
            }
        }
        /*
                private void Reanalyze(HierarchyNode node, VsProjectAnalyzer newAnalyzer) {
                    if (node != null) {
                        for (var child = node.FirstChild; child != null; child = child.NextSibling) {
                            if (child.IsNonMemberItem) {
                                continue;
                            }
                            if (child is FileNode) {
                                newAnalyzer.AnalyzeFile(child.Url);
                            }

                            Reanalyze(child, newAnalyzer);
                        }
                    }
                }

                public override ReferenceNode CreateReferenceNodeForFile(string filename) {
                    var interp = this.GetInterpreter() as ILuaInterpreterWithProjectReferences;
                    if (interp == null) {
                        return null;
                    }

                    var cancelSource = new CancellationTokenSource();
                    var task = interp.AddReferenceAsync(new ProjectReference(filename, ProjectReferenceKind.ExtensionModule), cancelSource.Token);

                    // try to complete synchronously w/o flashing the dialog...
                    if (!task.Wait(100)) {
                        var progress = new TaskProgressBar(task, cancelSource, "Waiting for analysis of extension module to complete...");
                        if (progress.ShowDialog() != true) {
                            // user cancelled.
                            return null;
                        }
                    }

                    var exception = task.Exception;
                    if (exception != null) {
                        string msg = GetErrorMessage(exception);

                        string fullMsg = String.Format("Cannot add reference to {0}:", filename);
                        if (msg != null) {
                            fullMsg = fullMsg + "\r\n\r\n" + msg;
                        }
                        MessageBox.Show(fullMsg);
                    } else {
                        return new LuaExtensionReferenceNode(this, filename);
                    }

                    return null;
                }

                private static string GetErrorMessage(AggregateException exception) {
                    string msg = null;
                    foreach (var inner in exception.InnerExceptions) {
                        if (inner is AggregateException) {
                            msg = GetErrorMessage((AggregateException)inner);
                        } else if (msg == null || inner is CannotAnalyzeExtensionException) {
                            msg = inner.Message;
                        }
                    }
                    return msg;
                }
        */
        class OutputDataReceiver
        {
            public readonly StringBuilder Received = new StringBuilder();

            public void OutputDataReceived(object sender, DataReceivedEventArgs e)
            {
                Received.Append(e.Data);
            }

        }

        protected override string AddReferenceExtensions
        {
            get
            {
                return "Lua Extension Modules (*.dll)\0*.dll\0All Files (*.*)\0*.*\0";
            }
        }

        internal override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (cmdGroup == VsMenus.guidStandardCommandSet2K)
            {
                switch ((VsCommands2K)cmd)
                {
                    case VsCommands2K.ECMD_PUBLISHSELECTION:
                    case VsCommands2K.ECMD_PUBLISHSLNCTX:
                        Publish(PublishProjectOptions.Default, true);
                        return VSConstants.S_OK;
                    case CommonConstants.OpenFolderInExplorerCmdId:
                        Process.Start(this.ProjectHome);
                        return VSConstants.S_OK;
                }
            }
            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }
        /*
                internal override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result) {
                    if (cmdGroup == GuidList.guidLuaToolsCmdSet) {
                        // Display the custom menu only on the Project node.
                        if (cmd == LuaConstants.CustomProjectCommandsMenu) {
                            if (_customCommands != null && _customCommands.Any()) {
                                result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                            } else {
                                result |= QueryStatusResult.INVISIBLE;
                            }

                            if (pCmdText != IntPtr.Zero && NativeMethods.OLECMDTEXT.GetFlags(pCmdText) == NativeMethods.OLECMDTEXT.OLECMDTEXTF.OLECMDTEXTF_NAME) {
                                NativeMethods.OLECMDTEXT.SetText(pCmdText, _customCommandsDisplayLabel);
                            }
                            return VSConstants.S_OK;
                        }
                    }
                    return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
                }

                private CustomCommand GetCustomCommand(uint cmdId) {
                    if ((int)cmdId >= LuaConstants.FirstCustomCmdId && (int)cmdId <= LuaConstants.LastCustomCmdId) {

                        int i = (int)cmdId - LuaConstants.FirstCustomCmdId;
                        if (_customCommands == null || i >= _customCommands.Count) {
                            return null;
                        }

                        return _customCommands[i];
                    } else {
                        return _customCommands.FirstOrDefault(c => c.AlternateCmdId == cmdId);
                    }
                }

                protected override QueryStatusResult QueryStatusSelectionOnNodes(IList<HierarchyNode> selectedNodes, Guid cmdGroup, uint cmd, IntPtr pCmdText) {
                    if (cmdGroup == GuidList.guidLuaToolsCmdSet) {
                        var command = GetCustomCommand(cmd);
                        if (command != null) {
                            // Update display text for the menu commands.
                            if ((int)cmd >= LuaConstants.FirstCustomCmdId && (int)cmd <= LuaConstants.LastCustomCmdId) {
                                if (pCmdText != IntPtr.Zero && NativeMethods.OLECMDTEXT.GetFlags(pCmdText) == NativeMethods.OLECMDTEXT.OLECMDTEXTF.OLECMDTEXTF_NAME) {
                                    NativeMethods.OLECMDTEXT.SetText(pCmdText, command.DisplayLabel);
                                }
                            }

                            var result = QueryStatusResult.SUPPORTED;
                            if (command.CanExecute) {
                                result |= QueryStatusResult.ENABLED;
                            }
                            return result;
                        }

                        if ((int)cmd >= LuaConstants.FirstCustomCmdId && (int)cmd <= LuaConstants.LastCustomCmdId) {
                            // All unspecified custom commands are hidden
                            return QueryStatusResult.INVISIBLE | QueryStatusResult.NOTSUPPORTED;
                        }
                    }

                    return base.QueryStatusSelectionOnNodes(selectedNodes, cmdGroup, cmd, pCmdText);
                }

                protected override int ExecCommandIndependentOfSelection(Guid cmdGroup, uint cmdId, uint cmdExecOpt, IntPtr vaIn, IntPtr vaOut, CommandOrigin commandOrigin, out bool handled) {
                    if (cmdGroup == GuidList.guidLuaToolsCmdSet) {
                        var command = GetCustomCommand(cmdId);

                        if (command != null) {
                            handled = true;
                            if (command.CanExecute) {
                                if (!Utilities.SaveDirtyFiles()) {
                                    return VSConstants.S_OK;
                                }

                                command.Execute(this).ContinueWith(t => {
                                    if (t.Exception != null) {
                                        MessageBox.Show(
                                            SR.GetString(
                                                SR.ErrorRunningCustomCommand,
                                                command.DisplayLabelWithoutAccessKeys,
                                                t.Exception.InnerException.Message
                                            ),
                                            SR.GetString(SR.LuaToolsForVisualStudio)
                                        );
                                    }
                                },
                                    CancellationToken.None,
                                    TaskContinuationOptions.None,
                                    TaskScheduler.FromCurrentSynchronizationContext()
                                );
                            }
                            return VSConstants.S_OK;
                        }
                    }

                    return base.ExecCommandIndependentOfSelection(cmdGroup, cmdId, cmdExecOpt, vaIn, vaOut, commandOrigin, out handled);
                }

                protected override bool DisableCmdInCurrentMode(Guid cmdGroup, uint cmd) {
                    if (cmdGroup == GuidList.guidLuaToolsCmdSet) {
                        if (IsCurrentStateASuppressCommandsMode()) {
                            switch ((int)cmd) {
                                case CommonConstants.AddSearchPathCommandId:
                                case CommonConstants.StartDebuggingCmdId:
                                case CommonConstants.StartWithoutDebuggingCmdId:
                                    return true;
                                case LuaConstants.ActivateEnvironment:
                                case LuaConstants.AddEnvironment:
                                case LuaConstants.AddExistingVirtualEnv:
                                case LuaConstants.AddVirtualEnv:
                                case LuaConstants.InstallLuaPackage:
                                case LuaConstants.AddSearchPathZipCommandId:
                                case LuaConstants.AddLuaPathToSearchPathCommandId:
                                    return true;
                                default:
                                    if (cmd >= LuaConstants.FirstCustomCmdId && cmd <= LuaConstants.LastCustomCmdId) {
                                        return true;
                                    }
                                    break;
                            }
                        }
                    }

                    return base.DisableCmdInCurrentMode(cmdGroup, cmd);
                }
        */
        #region ILuaProject Members

        string ILuaProject.ProjectName
        {
            get
            {
                return Caption;
            }
        }

        string ILuaProject.ProjectDirectory
        {
            get
            {
                return ProjectHome;
            }
        }

        string ILuaProject.GetProperty(string name)
        {
            return GetProjectProperty(name, true);
        }

        void ILuaProject.SetProperty(string name, string value)
        {
            SetProjectProperty(name, value);
        }

        string ILuaProject.GetStartupFile()
        {
            return base.GetStartupFile();
        }
        /*
                ILuaInterpreterFactory ILuaProject.GetInterpreterFactory() {
                    return this.GetInterpreterFactory();
                }
        */
        bool ILuaProject.Publish(PublishProjectOptions options)
        {
            return base.Publish(options, false);
        }

        string ILuaProject.GetUnevaluatedProperty(string name)
        {
            return base.GetUnevaluatedProperty(name);
        }

        #endregion
        /*
                internal int AddSearchPathZip() {
                    var fileName = LuaToolsPackage.Instance.BrowseForFileOpen(
                        IntPtr.Zero,
                        "Zip Archives (*.zip;*.egg)|*.zip;*.egg|All Files (*.*)|*.*",
                        ProjectHome
                    );
                    if (!string.IsNullOrEmpty(fileName)) {
                        AddSearchPathEntry(fileName);
                    }
                    return VSConstants.S_OK;
                }

                internal int AddLuaPathToSearchPath() {
                    var value = Environment.GetEnvironmentVariable(GetInterpreterFactory().Configuration.PathEnvironmentVariable);
                    if (string.IsNullOrEmpty(value)) {
                        return VSConstants.S_OK;
                    }

                    foreach (var bit in value.Split(';')) {
                        if (!string.IsNullOrEmpty(bit)) {
                            AddSearchPathEntry(bit);
                        }
                    }
                    return VSConstants.S_OK;
                }



                #region Virtual Env support

                internal void ShowAddInterpreter() {
                    var service = LuaToolsPackage.ComponentModel.GetService<IInterpreterOptionsService>();

                    var result = AddInterpreter.ShowDialog(this, service);
                    if (result == null) {
                        return;
                    }

                    var toRemove = new HashSet<ILuaInterpreterFactory>(_interpreters.GetInterpreterFactories());
                    var toAdd = new HashSet<ILuaInterpreterFactory>(result);
                    toRemove.ExceptWith(toAdd);
                    toAdd.ExceptWith(toRemove);

                    if (toAdd.Any() || toRemove.Any()) {
                        //Make sure we can edit the project file
                        if (!QueryEditProjectFile(false)) {
                            throw Marshal.GetExceptionForHR(VSConstants.OLE_E_PROMPTSAVECANCELLED);
                        }
                        foreach (var factory in toAdd) {
                            _interpreters.AddInterpreter(factory);
                        }
                        foreach (var factory in toRemove) {
                            _interpreters.RemoveInterpreterFactory(factory);
                        }
                    }
                }

                /// <summary>
                /// Executes Add Interpreter menu command.
                /// </summary>
                internal Task ShowAddVirtualEnvironment(bool browseForExisting) {
                    var service = LuaToolsPackage.ComponentModel.GetService<IInterpreterOptionsService>();
                    var scheduler = TaskScheduler.FromCurrentSynchronizationContext();

                    var data = AddVirtualEnvironment.ShowDialog(this, service, browseForExisting);

                    if (data == null) {
                        var tcs = new TaskCompletionSource<object>();
                        tcs.SetCanceled();
                        return tcs.Task;
                    }

                    var doCreate = data.WillCreateVirtualEnv;
                    var path = data.VirtualEnvPath;
                    var baseInterp = data.BaseInterpreter.Interpreter;

                    Task task;
                    if (doCreate && data.UseVEnv) {
                        task = VirtualEnv.CreateWithVEnv(baseInterp,
                            path,
                            OutputWindowRedirector.GetGeneral(Site));
                    } else if (doCreate) {
                        task = VirtualEnv.CreateAndInstallDependencies(baseInterp,
                            path,
                            OutputWindowRedirector.GetGeneral(Site));
                    } else {
                        var tcs = new TaskCompletionSource<object>();
                        tcs.SetResult(null);
                        task = tcs.Task;
                    }

                    return task.ContinueWith<InterpreterFactoryCreationOptions>(t => {
                        if (_interpreters.FindInterpreter(path) != null) {
                            return null;
                        }

                        var options = VirtualEnv.FindInterpreterOptions(path, service, baseInterp);
                        if (options == null) {
                            throw new InvalidOperationException("Unable to add virtual environment");
                        }
                        if (!doCreate) {
                            baseInterp = service.FindInterpreter(options.Id, options.LanguageVersion);
                        }
                        if (baseInterp != null) {
                            options.Description = string.Format("{0} ({1})", options.Description, baseInterp.Description);
                        }
                        return options;
                    }, TaskContinuationOptions.OnlyOnRanToCompletion).ContinueWith(t => {
                        if (t.Result != null) {
                            if (!QueryEditProjectFile(false)) {
                                throw Marshal.GetExceptionForHR(VSConstants.OLE_E_PROMPTSAVECANCELLED);
                            }

                            _interpreters.CreateInterpreterFactory(t.Result);
                        }
                    }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, scheduler);
                }


                /// <summary>
                /// Removes a given interpreter from the project, optionally deleting
                /// its prefix path from disk.
                /// </summary>
                internal async void RemoveInterpreter(ILuaInterpreterFactory factory, bool removeFromStorage = false) {
                    Utilities.ArgumentNotNull("factory", factory);

                    //Make sure we can edit the project file
                    if (!QueryEditProjectFile(false)) {
                        throw Marshal.GetExceptionForHR(VSConstants.OLE_E_PROMPTSAVECANCELLED);
                    }
                    _interpreters.RemoveInterpreterFactory(factory);

                    var path = factory.Configuration.PrefixPath;
                    if (removeFromStorage && Directory.Exists(path)) {
                        try {
                            await Task.Run(() => Directory.Delete(path, true));
                        } catch (Exception ex) {
                            ActivityLog.LogError(
                                SR.GetString(SR.LuaToolsForVisualStudio),
                                SR.GetString(SR.EnvironmentDeleteError, path) + ex.ToString()
                            );
                            MessageBox.Show(
                                SR.GetString(SR.EnvironmentDeleteError, path),
                                SR.GetString(SR.LuaToolsForVisualStudio)
                            );
                        }
                    }
                }

                #endregion

                public override Guid SharedCommandGuid {
                    get {
                        return GuidList.guidLuaToolsCmdSet;
                    }
                }

                protected internal override int ShowAllFiles() {
                    int hr = base.ShowAllFiles();
                    BoldActiveEnvironment();
                    return hr;
                }
        */
    }
}
