using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EnvDTE;
using EnvDTE80;
namespace ParaEngine.NPLDebuggerPackage
{
    /// <summary>
    /// Interaction logic for LaunchControl.xaml
    /// </summary>
    public partial class LaunchControl : UserControl
    {
        public LaunchControl(DTE2 dte)
        {
            InitializeComponent();
            LaunchForm1 form = new LaunchForm1(dte);
            //form.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
            //form.AutoSize = true;
            //form.Size = new System.Drawing.Size(0, 0);
            //form.Location = new System.Drawing.Point(0, 0);
            this.windowsFormsHost.Child = form;
        }
    }
}
