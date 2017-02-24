using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace NPLProject
{
    [Guid(Guids.guidSimpleProjectFactoryString)]
    class NPLProjectFactory : ProjectFactory
    {
        private NPLProjectPackage package;

        public NPLProjectFactory(NPLProjectPackage package)
            : base(package)
        {
            this.package = package;
        }

        protected override ProjectNode CreateProject()
        {
            NPLProjectNode project = new NPLProjectNode(this.package);

            project.SetSite((IOleServiceProvider)((IServiceProvider)this.package).GetService(typeof(IOleServiceProvider)));
            return project;
        }
    }
}
