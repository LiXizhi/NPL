using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NPLTools.Project
{
    public partial class NPLPropertyPageControl : UserControl
    {
        public NPLPropertyPage _page;
        private const string _exeFilter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";
        public NPLPropertyPageControl(NPLPropertyPage page)
        {
            InitializeComponent();
            _page = page;
        }

        public string nplExePath
        {
            get { return _nplExePath.Text; }
            set { _nplExePath.Text = value; }
        }

        public string nplExeOptions
        {
            get { return _nplExeOptions.Text; }
            set { _nplExeOptions.Text = value; }
        }

        public string scriptFile
        {
            get { return _scriptFile.Text; }
            set { _scriptFile.Text = value; }
        }

        public string scriptArguments
        {
            get { return _scriptArguments.Text; }
            set { _scriptArguments.Text = value; }
        }

        public string workingDir
        {
            get { return _workingDir.Text; }
            set { _workingDir.Text = value; }
        }

        private void NPLPathChanged(object sender, EventArgs e)
        {
            _page.IsDirty = true;
        }

        private void NPLPathButtonClicked(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.Filter = _exeFilter;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _nplExePath.Text = dialog.FileName;
                _nplExePath.ForeColor = SystemColors.ControlText;
            }
        }

        private void NPLOptionsChanged(object sender, EventArgs e)
        {
            _page.IsDirty = true;
        }

        private void ScriptFileChanged(object sender, EventArgs e)
        {
            _page.IsDirty = true;
        }

        private void ScriptArgumentsChanged(object sender, EventArgs e)
        {
            _page.IsDirty = true;
        }

        private void WorkingDirChanged(object sender, EventArgs e)
        {
            _page.IsDirty = true;
        }
    }
}
