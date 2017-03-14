﻿using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudioTools;

namespace NPLTools.Project
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideProjectFactory(typeof(NPLProjectFactory), null,
    "NPL Project Files (*.nplproj);*.nplproj", "nplproj", "nplproj",
    ".\\NullPath", LanguageVsTemplate = "NPL")]
    [ProvideProjectItem(typeof(NPLProjectFactory), "NPL Items", ".\\NullPath", 500)]
    [Guid(Guids.guidNPLProjectPkgString)]
    [ProvideObject(typeof(NPLPropertyPage))]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class NPLProjectPackage : CommonProjectPackage
    {
        /// <summary>
        /// NPLProjectPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "341ba1ac-a1ce-4ab3-b281-ae5dc002f09a";

        public string nplExePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="NPLProjectPackage"/> class.
        /// </summary>
        public NPLProjectPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }
        
        #region Package Members

        //public override string ProductUserContext
        //{
        //    get { return ""; }
        //}

        public override CommonEditorFactory CreateEditorFactory()
        {
            return null;
        }

        public override ProjectFactory CreateProjectFactory()
        {
            return new NPLProjectFactory(this);
        }

        public override CommonEditorFactory CreateEditorFactoryPromptForEncoding()
        {
            return null;
        }

        public override uint GetIconIdForAboutBox()
        {
            return 400;
        }

        public override uint GetIconIdForSplashScreen()
        {
            return 300;
        }

        public override string GetProductDescription()
        {
            return "npl";
        }

        public override string GetProductName()
        {
            return "npl";
        }

        public override string GetProductVersion()
        {
            return "1.0";
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.RegisterProjectFactory(new NPLProjectFactory(this));
            CommandRun.Initialize(this);
        }

        #endregion
    }
}
