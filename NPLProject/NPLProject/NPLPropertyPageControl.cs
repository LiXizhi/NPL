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
        public NPLPropertyPageControl(NPLPropertyPage page)
        {
            InitializeComponent();
        }

        public string nplExePath
        {
            get { return _nplExePath.Text; }
        }

        private void NPLPathChanged(object sender, EventArgs e)
        {
            this._nplExePath.Text = "";
        }

        private void NPLPathButtonClicked(object sender, EventArgs e)
        {

        }
    }
}
