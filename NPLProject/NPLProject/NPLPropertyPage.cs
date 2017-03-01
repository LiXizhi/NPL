using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using System.ComponentModel;

namespace NPLProject
{
    [ComVisible(true)]
    [Guid("F30D83A9-D13E-4FF2-A7BD-3685618EFA89")]
    public class NPLPropertyPage : SettingsPage
    {
        private string assemblyName;
        private OutputType outputType;
        private string defaultNamespace;

        public NPLPropertyPage()
        {
            this.Name = "General";
        }

        [Category("AssemblyName")]
        [DisplayName("AssemblyName")]
        [Description("The output file holding assembly metadata.")]
        public string AssemblyName
        {
            get { return this.assemblyName; }
        }
        [Category("Application")]
        [DisplayName("OutputType")]
        [Description("The type of application to build.")]
        public OutputType OutputType
        {
            get { return this.outputType; }
            set { this.outputType = value; this.IsDirty = true; }
        }
        [Category("Application")]
        [DisplayName("DefaultNamespace")]
        [Description("Specifies the default namespace for added items.")]
        public string DefaultNamespace
        {
            get { return this.defaultNamespace; }
            set { this.defaultNamespace = value; this.IsDirty = true; }
        }

        protected override void BindProperties()
        {
            this.assemblyName = this.ProjectMgr.GetProjectProperty(
                "AssemblyName", true);
            this.defaultNamespace = this.ProjectMgr.GetProjectProperty(
                "RootNamespace", false);

            string outputType = this.ProjectMgr.GetProjectProperty(
                "OutputType", false);
            this.outputType =
                (OutputType)Enum.Parse(typeof(OutputType), outputType);
        }

        protected override int ApplyChanges()
        {
            this.ProjectMgr.SetProjectProperty(
                "AssemblyName", this.assemblyName);
            this.ProjectMgr.SetProjectProperty(
                "OutputType", this.outputType.ToString());
            this.ProjectMgr.SetProjectProperty(
                "RootNamespace", this.defaultNamespace);
            this.IsDirty = false;

            return VSConstants.S_OK;
        }
    }
}