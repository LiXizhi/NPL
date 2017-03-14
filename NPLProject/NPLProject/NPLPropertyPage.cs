using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.OLE.Interop;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using System.Drawing;
using Microsoft.VisualStudioTools.Project;
using System.Windows.Forms;

namespace NPLTools.Project
{
    [Guid("BE2402BF-92AC-4467-9455-E9615D8F569F")]
    public class NPLPropertyPage : CommonPropertyPage
    {
        private readonly NPLPropertyPageControl _control;
        public NPLPropertyPage()
        {
            _control = new NPLPropertyPageControl(this);
        }

        public override Control Control
        {
            get
            {
                return _control;
            }
        }

        public override string Name
        {
            get
            {
                return "NPL Settings";
            }
        }

        public override void Apply()
        {
            Project.SetProjectProperty(NPLProjectConstants.NPLExePath, _control.nplExePath);
            Project.SetProjectProperty(NPLProjectConstants.NPLOptions, _control.nplExeOptions);
            Project.SetProjectProperty(NPLProjectConstants.StartupFile, _control.scriptFile);
            Project.SetProjectProperty(NPLProjectConstants.Arguments, _control.scriptArguments);
            Project.SetProjectProperty(NPLProjectConstants.WorkingDirectory, _control.workingDir);
            IsDirty = false;
        }

        public override void LoadSettings()
        {
            Loading = true;
            try
            {
                _control.nplExePath = Project.GetUnevaluatedProperty(NPLProjectConstants.NPLExePath);
                _control.nplExeOptions = Project.GetUnevaluatedProperty(NPLProjectConstants.NPLOptions);
                _control.scriptFile = Project.GetUnevaluatedProperty(NPLProjectConstants.StartupFile);
                _control.scriptArguments = Project.GetUnevaluatedProperty(NPLProjectConstants.Arguments);
                _control.workingDir = Project.GetUnevaluatedProperty(NPLProjectConstants.WorkingDirectory);
            }finally
            {
                Loading = false;
            }
            
        }
    }
}
