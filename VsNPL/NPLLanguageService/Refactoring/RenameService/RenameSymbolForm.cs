using System;
using System.Drawing;
using System.Windows.Forms;

namespace ParaEngine.Tools.Lua.Refactoring.RenameService
{
    /// <summary>
    /// Show options for rename operation.
    /// </summary>
    public partial class RenameSymbolForm : Form
    {
        private readonly Pen pen = new Pen(Color.DarkGray, 1);

		/// <summary>
		/// Get/Set title of the Dialog.
		/// </summary>
		/// <value>The title.</value>
        public string Title
        {
            get { return Text; }
            set { Text = value; }
        }

		/// <summary>
		/// Get/Set new symbol name.
		/// </summary>
		/// <value>The new name.</value>
        public string NewName
        {
            get { return txtNewName.Text.Trim(); }
            set { txtNewName.Text = value; }
        }

		/// <summary>
		/// Get/Set documents behaviour.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [open all changed document]; otherwise, <c>false</c>.
		/// </value>
        public bool OpenAllChangedDocument
        {
            get { return chOpenAllChanged.Checked; }
            set { chOpenAllChanged.Checked = value; }
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="RenameSymbolForm"/> class.
		/// </summary>
		/// <param name="oldName">Old name of element.</param>
		/// <param name="title">Title of the Form.</param>
        public RenameSymbolForm(string oldName, string title)
        {
            InitializeComponent();

            btnOK.Enabled = false;
            txtOldName.Text = oldName.Trim();
            txtNewName.TextChanged += txtNewName_TextChanged;
            ActiveControl = txtNewName;
            txtNewName.SelectionStart = txtNewName.Text.Length;
            Title = title;
        }

		/// <summary>
		/// Handles the TextChanged event of the txtNewName control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void txtNewName_TextChanged(object sender, EventArgs e)
        {
            errorProvider.SetError(lblNewName, txtNewName.Text.Trim().Length > 0 ? String.Empty : "Name cannot be empty.");
			btnOK.Enabled = (txtNewName.Text.Trim() != txtOldName.Text.Trim() && !String.IsNullOrEmpty(txtNewName.Text));
        }

		/// <summary>
		/// Handles the Click event of the btnOK control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }


		/// <summary>
		/// Handles the Click event of the btnCancel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

		/// <summary>
		/// Handles the Paint event of the panelButtons control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        private void panelButtons_Paint(object sender, PaintEventArgs e)
        {
            var panel = (Panel)sender;
            e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        }


    }
}