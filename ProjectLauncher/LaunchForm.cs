using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EnvDTE;
using EnvDTE80;

namespace ProjectLauncher
{
    public partial class LaunchForm : Form
    {
        DTE2 _dte;
        string _filePath;
        string _workingDir;
        string _Command;
        string _CommandArguments;
        string _SelectedProcess;
        public LaunchForm(DTE2 dte)
        {
            _dte = dte;
            InitializeComponent();
        }

        public string FilePath
        {
            get { return this._filePath; }
        }
        public string Command
        {
            get { return this._Command; }
        }
        public string CommandArguments
        {
            get { return this._CommandArguments; }
        }

        public string WorkingDir
        {
            get { return this._workingDir; }
        }

        public string SelectedProcess
        {
            get { return this._SelectedProcess; }
        }


        private void btnLaunch_Click(object sender, EventArgs e)
        {
            // for the type of projects the sample is interested in, the project output will be the primary output of the
            // active output group
            int selectedIndex = this.cmbProjects.SelectedIndex + 1;
            Project selectedProject = _dte.Solution.Projects.Item(selectedIndex);

            object[] objFileNames = selectedProject.ConfigurationManager.ActiveConfiguration.OutputGroups.Item(1).FileURLs as object[];
            string fileName = objFileNames[0] as string;
            System.Uri fileUri = new System.Uri(fileName);
            _filePath = fileUri.LocalPath;

            _Command = textBoxCommandLine.Text;
            _CommandArguments = textBoxCmdArguments.Text;
            _workingDir = textBoxWorkingDir.Text;
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void LaunchForm_Load(object sender, EventArgs e)
        {
            if (_dte.Solution.Projects.Count == 0)
            {
                // If the misc files project is the only project, display an error.
                MessageBox.Show("A project must be opened");
                this.Close();
                return;
            }

            // Add all the projects except the misc files project to the combobox.
            for (int i = 1; i <= _dte.Solution.Projects.Count; i++)
            {
                Project proj = _dte.Solution.Projects.Item(i);
                this.cmbProjects.Items.Add(proj.Name);
            }

            RefreshProcessList();
            
            // TODO: maybe we should select the active project. 
            this.cmbProjects.SelectedIndex = 0;
            cmbProjects_SelectionChangeCommitted(null, null);
        }

        private void cmbProjects_SelectionChangeCommitted(object sender, EventArgs e)
        {
            // for the type of projects the sample is interested in, the project output will be the primary output of the
            // active output group
            int selectedIndex = this.cmbProjects.SelectedIndex + 1;
            Project selectedProject = _dte.Solution.Projects.Item(selectedIndex);

            // selectedProject.ConfigurationManager.ActiveConfiguration;
            Configuration config = selectedProject.ConfigurationManager.ActiveConfiguration;
            Properties props = config.Properties;
            foreach (Property prop in props)
            {
                if (prop.Name == "WorkingDirectory")
                {
                    _workingDir = prop.Value as String;
                }
                else if (prop.Name == "CommandArguments")
                {
                    _CommandArguments = prop.Value as String;
                    if( _CommandArguments.IndexOf("debug=\"") <=0 )
                    {
                        if (cmbNPLStates.Text != "" && cmbNPLStates.Text != "none")
                        {
                            _CommandArguments += String.Format(" debug=\"{0}\"", cmbNPLStates.Text);
                        }
                    }
                }
                else if (prop.Name == "Command")
                {
                    _Command = prop.Value as String;
                }
            }
            textBoxCommandLine.Text = Command;
            textBoxCmdArguments.Text = CommandArguments;
            textBoxWorkingDir.Text = WorkingDir;
        }

        private void cmbNPLStates_TextChanged(object sender, EventArgs e)
        {
            cmbProjects_SelectionChangeCommitted(null, null);
        }

        private void RefreshProcessList()
        {
            // list all processes
            listViewProcs.BeginUpdate();
            listViewProcs.Items.Clear();
            int nIndex = 0;
            int nSelectedIndex = -1;
            foreach (Process lLocalProcess in _dte.Debugger.LocalProcesses)
            {
                string procName = System.IO.Path.GetFileName(lLocalProcess.Name);
                listViewProcs.Items.Add(new ListViewItem(new String[] { procName, lLocalProcess.ProcessID.ToString(), lLocalProcess.Name}));
                if (procName.IndexOf("ParaEngine") >= 0 || procName.IndexOf("paraengine") >= 0)
                {
                    nSelectedIndex = nIndex;
                }
                nIndex++;
            }
            listViewProcs.EndUpdate();
            if (nSelectedIndex >= 0)
            {
                listViewProcs.Items[nSelectedIndex].Selected = true;
                listViewProcs.Items[nSelectedIndex].EnsureVisible();
            }
        }

        private Process GetSelectedProcess()
        {
            if(listViewProcs.SelectedItems.Count == 1)
            {
                string procName = (listViewProcs.SelectedItems[0].SubItems[2]).Text;

                foreach (Process lLocalProcess in _dte.Debugger.LocalProcesses)
                {
                    if (procName == lLocalProcess.Name)
                    {
                        return lLocalProcess;
                    }
                }
            }
            return null;
        }

        private void btnRefreshProcList_Click(object sender, EventArgs e)
        {
            RefreshProcessList();
        }

        private void btnAttach_Click(object sender, EventArgs e)
        {
            Process proc = GetSelectedProcess();
            if(proc!=null)
            {
                _SelectedProcess = proc.Name;
                if(false)
                {
                    // method 1: let the connector call attach, but this will hang at delayhlp.cpp, which is really strange. This has something to do with DELAYLOAD of dlls
                    //this.DialogResult = DialogResult.Yes;
                }
                else
                {
                    // Tricky: Attach2 will cause the main thread to hang at delayhlp.cpp, which is pretty strange, press the attach button twice will solve the problem. This has something to do with DELAYLOAD of dlls
                    (proc as Process2).Attach2("NPL Debug Engine");
                }
                this.Close();
            }
        }

        private void btnKillProc_Click(object sender, EventArgs e)
        {
            Process proc = GetSelectedProcess();
            if (proc != null)
            {
                //here we're going to get a list of all running processes on the computer
                foreach (System.Diagnostics.Process clsProcess in System.Diagnostics.Process.GetProcesses()) 
                {
	                if (clsProcess.ProcessName.StartsWith(proc.Name))
	                {
		                clsProcess.Kill();
	                }
                }
            }
        }
    }
}
