namespace ParaEngine.Tools.Lua.Refactoring.RenameService
{
    partial class RenameSymbolForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (pen != null)
            {
                pen.Dispose();
            }
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.txtNewName = new System.Windows.Forms.TextBox();
            this.txtOldName = new System.Windows.Forms.TextBox();
            this.lblNewName = new System.Windows.Forms.Label();
            this.lblOldName = new System.Windows.Forms.Label();
            this.chPreview = new System.Windows.Forms.CheckBox();
            this.chOpenAllChanged = new System.Windows.Forms.CheckBox();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.panelButtons = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Location = new System.Drawing.Point(401, 22);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOK.Location = new System.Drawing.Point(311, 22);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // txtNewName
            // 
            this.txtNewName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNewName.Location = new System.Drawing.Point(12, 22);
            this.txtNewName.Name = "txtNewName";
            this.txtNewName.Size = new System.Drawing.Size(464, 20);
            this.txtNewName.TabIndex = 0;
            // 
            // txtOldName
            // 
            this.txtOldName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOldName.Enabled = false;
            this.txtOldName.Location = new System.Drawing.Point(12, 61);
            this.txtOldName.Name = "txtOldName";
            this.txtOldName.Size = new System.Drawing.Size(464, 20);
            this.txtOldName.TabIndex = 1;
            // 
            // lblNewName
            // 
            this.lblNewName.AutoSize = true;
            this.lblNewName.Location = new System.Drawing.Point(12, 5);
            this.lblNewName.Name = "lblNewName";
            this.lblNewName.Size = new System.Drawing.Size(62, 14);
            this.lblNewName.TabIndex = 56;
            this.lblNewName.Text = "New name:";
            // 
            // lblOldName
            // 
            this.lblOldName.AutoSize = true;
            this.lblOldName.Location = new System.Drawing.Point(12, 45);
            this.lblOldName.Name = "lblOldName";
            this.lblOldName.Size = new System.Drawing.Size(55, 14);
            this.lblOldName.TabIndex = 55;
            this.lblOldName.Text = "Old name:";
            // 
            // chPreview
            // 
            this.chPreview.AutoSize = true;
            this.chPreview.Enabled = false;
            this.chPreview.Location = new System.Drawing.Point(12, 12);
            this.chPreview.Name = "chPreview";
            this.chPreview.Size = new System.Drawing.Size(111, 18);
            this.chPreview.TabIndex = 6;
            this.chPreview.Text = "Preview changes";
            this.chPreview.UseVisualStyleBackColor = true;
            // 
            // chOpenAllChanged
            // 
            this.chOpenAllChanged.AutoSize = true;
            this.chOpenAllChanged.Checked = true;
            this.chOpenAllChanged.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chOpenAllChanged.Location = new System.Drawing.Point(12, 36);
            this.chOpenAllChanged.Name = "chOpenAllChanged";
            this.chOpenAllChanged.Size = new System.Drawing.Size(287, 18);
            this.chOpenAllChanged.TabIndex = 57;
            this.chOpenAllChanged.Text = "To enable Undo, open all files with changes for editing";
            this.chOpenAllChanged.UseVisualStyleBackColor = true;
            // 
            // errorProvider
            // 
            this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider.ContainerControl = this;
            // 
            // panelButtons
            // 
            this.panelButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.panelButtons.CausesValidation = false;
            this.panelButtons.Controls.Add(this.btnCancel);
            this.panelButtons.Controls.Add(this.chOpenAllChanged);
            this.panelButtons.Controls.Add(this.btnOK);
            this.panelButtons.Controls.Add(this.chPreview);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 96);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(488, 66);
            this.panelButtons.TabIndex = 58;
            this.panelButtons.Paint += new System.Windows.Forms.PaintEventHandler(this.panelButtons_Paint);
            // 
            // RenameSymbolForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(488, 162);
            this.Controls.Add(this.panelButtons);
            this.Controls.Add(this.lblOldName);
            this.Controls.Add(this.lblNewName);
            this.Controls.Add(this.txtOldName);
            this.Controls.Add(this.txtNewName);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "RenameSymbolForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Rename";
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.panelButtons.ResumeLayout(false);
            this.panelButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TextBox txtNewName;
        private System.Windows.Forms.TextBox txtOldName;
        private System.Windows.Forms.Label lblNewName;
        private System.Windows.Forms.Label lblOldName;
        private System.Windows.Forms.CheckBox chPreview;
        private System.Windows.Forms.CheckBox chOpenAllChanged;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.Panel panelButtons;
    }
}