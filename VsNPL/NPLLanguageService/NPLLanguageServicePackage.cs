using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;

using ParaEngine.Tools.Lua.Parser;
using ParaEngine.Tools.Lua.Refactoring;
using ParaEngine.Tools.Lua.Refactoring.UndoManager;
using ParaEngine.Tools.Lua.SourceOutliner;
using ParaEngine.Tools.Services;
using ParaEngine.Tools.Lua;

using Configuration = ParaEngine.Tools.Lua.Parser.Configuration;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace ParaEngine.NPLLanguageService
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]

    //This attribute registers a tool window exposed by this package.
    //[ProvideToolWindow(typeof(SourceOutlineToolWindow))]
    //[ProvideEditorExtension(typeof(EditorFactory), ".npllanguageservice", 50, 
    //          ProjectGuid = "{A2FE74E1-B743-11d0-AE1A-00A0C90FFFC3}", 
    //          TemplateDir = "Templates", 
    //          NameResourceID = 105,
    //          DefaultName = "NPLLanguageService")]
    // [ProvideKeyBindingTable(GuidList.guidNPLLanguageServiceEditorFactoryString, 102)]
    //[ProvideEditorLogicalView(typeof(EditorFactory), "{7651a703-06e5-11d1-8ebd-00a0c90f26ea}")]

    // Provide LanguageService as a Visual Studio service
#if DEBUG
    [ProvideService(typeof(ILuaLanguageService), ServiceName = "NPL Language Service (test)")]
#else
    [ProvideService(typeof(ILuaLanguageService), ServiceName = "NPL Language Service")]
#endif

    // Provide the language service for the .lua extension
    [ProvideLanguageExtension(typeof(LanguageService), Configuration.Extension)]
    // Provide the language service for the .page NPL web server page extension
    [ProvideLanguageExtension(typeof(LanguageService), ".page")]
    // Provide the language service for the .npl NPL file extension
    [ProvideLanguageExtension(typeof(LanguageService), ".npl")]
    // Provide and configure the language service features
    [ProvideLanguageService(typeof(LanguageService), Configuration.Name, 110,
        CodeSense = true,
        EnableCommenting = true,
        MatchBraces = true,
        ShowCompletion = true,
        ShowMatchingBrace = true,
        AutoOutlining = true,
        EnableAsyncCompletion = true,
        QuickInfo = true,
        EnableFormatSelection = true,
        CodeSenseDelay = 1000)]
    // This attribute registers a tool window exposed by this package.
    // It will initially be docked at the toolbox window location.
    [ProvideToolWindow(typeof(SourceOutlineToolWindow), Style = VsDockStyle.Tabbed)]

    //This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]

    // VSPackages can be set to autoload when a particular user interface (UI) context exists.For example, a VSPackage can be set to load whenever a solution exists. 
    [ProvideAutoLoad("F1536EF8-92EC-443C-9ED7-FDADF150DA82")] // = VSConstants.UICONTEXT_SolutionExists.ToString()
    // there is bug in vs 2015 for associating language service with code expansion
    // https://social.msdn.microsoft.com/Forums/vstudio/en-US/41f559c9-fa86-4108-b516-2d2fabd432a2/code-snippets-not-working-in-vs-2012?forum=vsx
    [ProvideLanguageCodeExpansion(typeof(LanguageService), Configuration.Name, 110, Configuration.Name,
            @"%InstallRoot%\NPL\Snippets\%LCID%\SnippetsIndex.xml",
            // the paths of the snippet files
            SearchPaths = @"%InstallRoot%\NPL\Snippets\%LCID%\Lua\;" +
                    @"%TestDocs%\Code Snippets\NPL\Lua\",
            ForceCreateDirs = @"%InstallRoot%\NPL\Snippets\%LCID%\Lua\;" +
                    @"%TestDocs%\Code Snippets\NPL\Lua\")]
    [Guid(GuidList.guidNPLLanguageServicePkgString)]
    public sealed class NPLLanguageServicePackage : BasePackage
    {
        private IOleComponentManager componentManager;
        private SourceOutlineToolWindow sourceOutlinerWindow;
        private ILuaUndoService undoService;
        public static NPLLanguageServicePackage m_currentPackage;
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public NPLLanguageServicePackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
            m_currentPackage = this;

        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            ////Create Editor Factory. Note that the base Package class will call Dispose on it.
            // base.RegisterEditorFactory(new EditorFactory(this));

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the tool window
                {
                    CommandID toolwndCommandID = new CommandID(GuidList.guidNPLLanguageServiceCmdSet, (int)PkgCmdIDList.cmdidMyNPLOutlineTool);
                    MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                    mcs.AddCommand(menuToolWin);
                }
                // Context menu
                {
                    // added by LiXizhi: help page: http://stackoverflow.com/questions/16842795/why-isnt-my-vspackages-context-menu-showing

                    // NPL Set breakpoint at current source location: 
                    CommandID contextmenuCommandID = new CommandID(GuidList.guidNPLLanguageServiceCmdSet, (int)PkgCmdIDList.cmdidNPLSetBreakPoint);
                    OleMenuCommand menuCmd = new OleMenuCommand(delegate (object sender, EventArgs e)
                    {
                        LanguageService service = (LanguageService)GetService(typeof(ILuaLanguageService));
                        if (service != null)
                            service.SetBreakPointAtCurrentLine();
                    }, contextmenuCommandID);
                    // http://stackoverflow.com/questions/21929372/dynamic-visibility-of-menu-item
                    menuCmd.BeforeQueryStatus += delegate (object sender, EventArgs e)
                    {
                        OleMenuCommand menuCommand = sender as OleMenuCommand;
                        if (menuCommand != null)
                        {
                            bool bVisible = false;

                            LanguageService service = (LanguageService)GetService(typeof(ILuaLanguageService));
                            if (service != null && service.DTE != null)
                            {
                                bVisible = service.IsKnownFileName();
                            }
                            menuCommand.Visible = bVisible;
                        }
                    };
                    mcs.AddCommand(menuCmd);
                }
            }

            componentManager = (IOleComponentManager)GetService(typeof(SOleComponentManager));
            // Register callback for Language Service interface that returns the language service itself
            IServiceContainer serviceContainer = this;
            serviceContainer.AddService(typeof(ILuaLanguageService), OnCreateLuaLanguageService, true);
            serviceContainer.AddService(typeof(IRefactoringService), OnCreateRefactoringService, true);

            // Initialize the DTE and the code outline file manager, and hook up events.
            InitializeSourceOutlinerToolWindow();
            serviceContainer.AddService(typeof(SourceOutlineToolWindow), sourceOutlinerWindow, true);

            CreateCustomServices(serviceContainer);
        }

        /// <summary>
        /// Creates the custom services.
        /// </summary>
        private void CreateCustomServices(IServiceContainer serviceContainer)
        {
            undoService = new RefactorUndoService(this);
            serviceContainer.AddService(typeof(ILuaUndoService), undoService, true);
        }

        /// <summary>
        /// Initializes the DTE and the code outline file manager, and hooks up events.
        /// </summary>
        private void InitializeSourceOutlinerToolWindow()
        {
            var dte = GetService(typeof(_DTE)) as DTE;
            sourceOutlinerWindow = (SourceOutlineToolWindow)FindToolWindow(typeof(SourceOutlineToolWindow), 0, true);
            sourceOutlinerWindow.Package = this;

            OLECRINFO[] crinfo = new OLECRINFO[1];
            crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
            crinfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime | (uint)_OLECRF.olecrfNeedPeriodicIdleTime
                               | (uint)_OLECRF.olecrfNeedAllActiveNotifs | (uint)_OLECRF.olecrfNeedSpecActiveNotifs;
            crinfo[0].grfcadvf = (uint)_OLECADVF.olecadvfModal | (uint)_OLECADVF.olecadvfRedrawOff |
                                 (uint)_OLECADVF.olecadvfWarningsOff;
            crinfo[0].uIdleTimeInterval = 500;

            // fix a bug that component id not called. 

            if (!HasComponent("sourceOutlinerWindow"))
            {
                uint componentID = 0;
                int hr = componentManager.FRegisterComponent(sourceOutlinerWindow, crinfo, out componentID);
                if (componentID != 0)
                    AddComponentToAutoReleasePool("sourceOutlinerWindow", componentID);
                if (!ErrorHandler.Succeeded(hr))
                {
                    Trace.WriteLine("Initialize->IOleComponent registration failed");
                }
            }


            sourceOutlinerWindow.InitializeDTE(dte);
            sourceOutlinerWindow.AddWindowEvents();
            sourceOutlinerWindow.AddSolutionEvents();
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(SourceOutlineToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());

        }

        /// <summary>
        /// Called when [create refactoring service].
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        private IRefactoringService OnCreateRefactoringService(IServiceContainer container, Type serviceType)
        {
            if (serviceType == typeof(IRefactoringService))
            {
                var service = container.GetService(typeof(LuaRefactoringService)) as IRefactoringService ??
                              new LuaRefactoringService(this);

                return service;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        private static ILuaLanguageService OnCreateLuaLanguageService(IServiceContainer container, Type serviceType)
        {
            if (serviceType == typeof(ILuaLanguageService))
                return container.GetService(typeof(LanguageService)) as ILuaLanguageService;
            return null;
        }

        /// <summary>
        /// Changes the cursor to the hourglass cursor. 
        /// </summary>
        /// <returns>A return code or S_OK.</returns>
        public int SetWaitCursor()
        {
            int hr = VSConstants.S_OK;

            var VsUiShell = GetService(typeof(SVsUIShell)) as IVsUIShell;
            if (VsUiShell != null)
            {
                // There is no check for return code because any failure of this call is ignored.
                hr = VsUiShell.SetWaitCursor();
            }

            return hr;
        }

        /// <summary>
        /// Gets the component manager for the package.
        /// </summary>
        /// <returns>An IOleComponentManager object.</returns>
        public IOleComponentManager ComponentManager
        {
            get { return componentManager; }
        }
        #endregion

    }
}
