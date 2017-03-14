using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio;
using System.Diagnostics;

namespace NPLTools.Project
{
    class NPLProjectLauncher : IProjectLauncher
    {
        private NPLProjectNode _project;
        public NPLProjectLauncher(NPLProjectNode project)
        {
            _project = project;
        }

        public int LaunchFile(string file, bool debug)
        {
            throw new NotImplementedException();
        }

        public int LaunchProject(bool debug)
        {

            return Start(debug);
        }

        private int Start(bool debug)
        {
            string nplExePath = GetExePath();
            string startupFile = GetStartupFile();
            var psi = new ProcessStartInfo();
            psi.UseShellExecute = false;
            psi.FileName = nplExePath;
            psi.Arguments = startupFile;
            psi.WorkingDirectory = @"D:\NPLTest";
            var process = Process.Start(psi);
            return VSConstants.S_OK;
        }

        private string GetExePath()
        {
            return _project.GetProjectProperty(NPLProjectConstants.NPLExePath);
        }

        private string GetStartupFile()
        {
            return _project.GetProjectProperty(NPLProjectConstants.StartupFile);
        }
    }
}
