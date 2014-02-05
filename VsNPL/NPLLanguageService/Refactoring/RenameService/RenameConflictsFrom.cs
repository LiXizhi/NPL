using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using EnvDTE;
using ParaEngine.NPLLanguageService;

namespace ParaEngine.Tools.Lua.Refactoring.RenameService
{
    /// <summary>
    /// Show options for conflicted rename operation.
    /// </summary>
    public partial class RenameConflictsFrom : Form
    {
        private readonly IEnumerable<CodeElement> codeElements;
        private readonly Pen pen = new Pen(Color.DarkGray, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="RenameConflictsFrom"/> class.
        /// </summary>
        /// <param name="elements"></param>
        public RenameConflictsFrom(IEnumerable<CodeElement> elements)
        {
            codeElements = elements;
            InitializeComponent();
            Text = Resources.RenameConflictFoundCaption;
            lblDescription.Text = Resources.RenameConflictFoundMessage;
            var list = new ImageList();
            list.Images.Add(Resources.ErrorIcon);
            lstConflictView.SmallImageList = list;

            AddElements(elements);
        }


		/// <summary>
		/// Adds the elements.
		/// </summary>
		/// <param name="elements">The elements.</param>
        private void AddElements(IEnumerable<CodeElement> elements)
        {
            try
            {
                lstConflictView.BeginUpdate();
                foreach (CodeElement element in elements)
                {
                    string elementType = element.Kind == vsCMElement.vsCMElementFunction ? "method" : "variable";
					string desc = string.Format(Resources.RenameConflictDefinitionText, elementType,
                                                element.Name, Path.GetFileName(element.ProjectItem.get_FileNames(1)));
                    var listViewItem = new ListViewItem(desc, 0);
                    listViewItem.SubItems.Add(element.Name);
                    listViewItem.SubItems.Add(element.ProjectItem.get_FileNames(1));
					listViewItem.ToolTipText = Resources.RenameConflictsToolTipText;
                    lstConflictView.Items.Add(listViewItem);
                }
            }
            finally
            {
                lstConflictView.EndUpdate();
            }
        }

		/// <summary>
		/// Handles the Click event of the btnBack control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnBack_Click(object sender, EventArgs e)
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
		/// Handles the DoubleClick event of the lstConflictView control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void lstConflictView_DoubleClick(object sender, EventArgs e)
        {
            if (lstConflictView.SelectedItems != null && lstConflictView.SelectedItems.Count > 0)
                foreach (CodeElement codeElement in codeElements)
                {
                    if (codeElement.Name == lstConflictView.SelectedItems[0].SubItems[1].Text)
                    {
                        EditorSupport.GoToCodeElementHelper(codeElement.DTE, codeElement, false);
                        break;
                    }
                }
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