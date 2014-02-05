/***************************************************************************

Copyright (c) 2006 Microsoft Corporation. All rights reserved.

***************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;
using ParaEngine.NPLLanguageService;

namespace ParaEngine.Tools.Lua.SourceOutliner.Controls
{
    /// <summary>
    /// Control that displays a source outline.
    /// </summary>
    public partial class SourceOutlinerControl : UserControl
    {
        private readonly ImageList treeViewImages = new ImageList();
        private Boolean treeViewWasVisible;

        /// <summary>
        /// Initializes a new instance of the SourceOutlinerControl class.
        /// </summary>
        public SourceOutlinerControl()
        {
            InitializeComponent();

            // Create the image list for the icons and load it up
            // with the icon strip in the resources.
            treeViewImages.ImageSize = new Size(16, 16);
            Image img = Resources.TreeViewIcons;
            treeViewImages.Images.AddStrip(img);
            treeViewImages.TransparentColor = Color.Magenta;

            var colorTable = new ProfessionalColorTable {UseSystemColors = true};
        	filterToolStrip.Renderer = new ToolStripProfessionalRenderer(colorTable);
        }

        /// <summary>
        /// Gets the TreeView that is currently being displayed.
        /// </summary>
        /// <returns>A TreeView object.</returns>
        public TreeView VisibleTreeView
        {
            get
            {
            	if (codeTreeView.Visible)
            		return (codeTreeView);

            	return (codeFilterView);
            }
        }

        /// <summary>
        /// Gets or sets the TreeView object for the unfiltered (hierarchical) view.
        /// </summary>
        /// <returns>A TreeView object.</returns>
        public TreeView TreeView
        {
            get { return (codeTreeView); }
            set { codeTreeView = value; }
        }

        /// <summary>
        /// Gets or sets the TreeView object for the filtered view.
        /// </summary>
        /// <returns>A TreeView object.</returns>
        public TreeView FilterView
        {
            get { return (codeFilterView); }
            set { codeFilterView = value; }
        }

        /// <summary>
        /// Makes both the filtered and unfiltered TreeViews invisible.
        /// </summary>
        public void HideTrees()
        {
            codeTreeView.Visible = false;
            codeFilterView.Visible = false;
        }

        /// <summary>
        /// Adds a TreeView object to the list of controls.
        /// </summary>
        /// <param name="tv">The TreeView object to be added.</param>
        public void AddTreeToControls(TreeView tv)
        {
            RemoveTreeFromControls(tv);
            tv.Dock = DockStyle.Fill;
            tv.Location = new Point(0, 45);
            tv.Size = new Size(352, 294);
            tv.ImageList = treeViewImages;
            tv.CollapseAll();
            Controls.Add(tv);
            Controls.SetChildIndex(tv, 0);
        }

        /// <summary>
        /// Deletes a TreeView object from the list of controls.
        /// </summary>
        /// <param name="tv">The TreeView object to be deleted.</param>
        public void RemoveTreeFromControls(TreeView tv)
        {
            Controls.Remove(tv);
        }

        /// <summary>
        /// Makes the unfiltered TreeView visible, and makes the filtered TreeView
        /// and the 'Please Wait' message invisible.
        /// </summary>
        public void ShowTree()
        {
            codeTreeView.Visible = true;
            codeFilterView.Visible = false;
            richTextBoxWait.Visible = false;
        }

        /// <summary>
        /// Makes the filtered TreeView visible, and makes the unfiltered TreeView 
        /// and the 'Please Wait' message invisible.
        /// </summary>
        public void ShowFilter()
        {
            codeTreeView.Visible = false;
            codeFilterView.Visible = true;
            richTextBoxWait.Visible = false;
        }

        /// <summary>
        /// Makes the filtered and unfiltered TreeViews invisible, 
        /// and makes the 'Please Wait' message visible. 
        /// </summary>
        public void ShowWaitWhileReadyMessage()
        {
            treeViewWasVisible = codeTreeView.Visible;

            codeTreeView.Visible = false;
            codeFilterView.Visible = false;
            richTextBoxWait.Visible = true;
            Refresh();
        }

        /// <summary>
        /// Restores the visibility of the filtered and unfiltered TreeViews,
        /// and makes the 'Please Wait' message invisible.
        /// </summary>
        public void HideWaitWhileReadyMessage()
        {
            codeTreeView.Visible = treeViewWasVisible;
            codeFilterView.Visible = !treeViewWasVisible;
            richTextBoxWait.Visible = false;
            Refresh();
        }

        /// <summary>
        /// Shows the appropriate TreeView if the source file
        ///  has been loaded, otherwise does nothing.
        /// </summary>
        public void Reset()
        {
            if (!richTextBoxWait.Visible)
            {
                if (filterToolStripCombo.SelectedIndex == 0)
                    ShowTree();
                else
                    ShowFilter();
            }
        }
    }
}