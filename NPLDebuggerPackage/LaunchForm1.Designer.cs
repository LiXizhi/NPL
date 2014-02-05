namespace ParaEngine.NPLDebuggerPackage
{
    partial class LaunchForm1
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbProjects = new System.Windows.Forms.ComboBox();
            this.btnLaunch = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxCommandLine = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxWorkingDir = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxCmdArguments = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbNPLStates = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.listViewProcs = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnRefreshProcList = new System.Windows.Forms.Button();
            this.btnAttach = new System.Windows.Forms.Button();
            this.btnKillProc = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(543, 46);
            this.label1.TabIndex = 0;
            this.label1.Text = "Launch a ParaEngine application to debug from below. One can modify the project c" +
                "ommand line and working directory to let the path automatically filled for you. " +
                "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "Project To Debug:";
            // 
            // cmbProjects
            // 
            this.cmbProjects.FormattingEnabled = true;
            this.cmbProjects.Location = new System.Drawing.Point(125, 75);
            this.cmbProjects.Name = "cmbProjects";
            this.cmbProjects.Size = new System.Drawing.Size(196, 20);
            this.cmbProjects.TabIndex = 2;
            this.cmbProjects.SelectionChangeCommitted += new System.EventHandler(this.cmbProjects_SelectionChangeCommitted);
            // 
            // btnLaunch
            // 
            this.btnLaunch.Location = new System.Drawing.Point(125, 192);
            this.btnLaunch.Name = "btnLaunch";
            this.btnLaunch.Size = new System.Drawing.Size(87, 25);
            this.btnLaunch.TabIndex = 11;
            this.btnLaunch.Text = "Launch";
            this.btnLaunch.UseVisualStyleBackColor = true;
            this.btnLaunch.Click += new System.EventHandler(this.btnLaunch_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(234, 192);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(87, 25);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 114);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "Command Line:";
            // 
            // textBoxCommandLine
            // 
            this.textBoxCommandLine.Location = new System.Drawing.Point(125, 111);
            this.textBoxCommandLine.Name = "textBoxCommandLine";
            this.textBoxCommandLine.Size = new System.Drawing.Size(430, 21);
            this.textBoxCommandLine.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 168);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(113, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "Working Directory:";
            // 
            // textBoxWorkingDir
            // 
            this.textBoxWorkingDir.Location = new System.Drawing.Point(125, 165);
            this.textBoxWorkingDir.Name = "textBoxWorkingDir";
            this.textBoxWorkingDir.Size = new System.Drawing.Size(430, 21);
            this.textBoxWorkingDir.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 141);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 12);
            this.label5.TabIndex = 7;
            this.label5.Text = "Cmd Arguments:";
            // 
            // textBoxCmdArguments
            // 
            this.textBoxCmdArguments.Location = new System.Drawing.Point(125, 138);
            this.textBoxCmdArguments.Name = "textBoxCmdArguments";
            this.textBoxCmdArguments.Size = new System.Drawing.Size(430, 21);
            this.textBoxCmdArguments.TabIndex = 8;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(327, 78);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(113, 12);
            this.label6.TabIndex = 3;
            this.label6.Text = "NPL State(Thread):";
            // 
            // cmbNPLStates
            // 
            this.cmbNPLStates.FormattingEnabled = true;
            this.cmbNPLStates.Items.AddRange(new object[] {
            "main",
            "none",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "r"});
            this.cmbNPLStates.Location = new System.Drawing.Point(446, 75);
            this.cmbNPLStates.Name = "cmbNPLStates";
            this.cmbNPLStates.Size = new System.Drawing.Size(109, 20);
            this.cmbNPLStates.TabIndex = 4;
            this.cmbNPLStates.Text = "main";
            this.cmbNPLStates.SelectionChangeCommitted += new System.EventHandler(this.cmbProjects_SelectionChangeCommitted);
            this.cmbNPLStates.TextChanged += new System.EventHandler(this.cmbNPLStates_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.listViewProcs);
            this.groupBox1.Controls.Add(this.btnRefreshProcList);
            this.groupBox1.Controls.Add(this.btnAttach);
            this.groupBox1.Controls.Add(this.btnKillProc);
            this.groupBox1.Location = new System.Drawing.Point(13, 249);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(542, 210);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Attach To Existing ParaEngine Process";
            // 
            // listViewProcs
            // 
            this.listViewProcs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewProcs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderID,
            this.columnHeaderPath});
            this.listViewProcs.FullRowSelect = true;
            this.listViewProcs.HideSelection = false;
            this.listViewProcs.Location = new System.Drawing.Point(16, 20);
            this.listViewProcs.MultiSelect = false;
            this.listViewProcs.Name = "listViewProcs";
            this.listViewProcs.Size = new System.Drawing.Size(510, 150);
            this.listViewProcs.TabIndex = 0;
            this.listViewProcs.UseCompatibleStateImageBehavior = false;
            this.listViewProcs.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 171;
            // 
            // columnHeaderID
            // 
            this.columnHeaderID.Text = "Proc ID";
            // 
            // columnHeaderPath
            // 
            this.columnHeaderPath.Text = "Path";
            this.columnHeaderPath.Width = 273;
            // 
            // btnRefreshProcList
            // 
            this.btnRefreshProcList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRefreshProcList.Location = new System.Drawing.Point(16, 176);
            this.btnRefreshProcList.Name = "btnRefreshProcList";
            this.btnRefreshProcList.Size = new System.Drawing.Size(166, 25);
            this.btnRefreshProcList.TabIndex = 1;
            this.btnRefreshProcList.Text = "Refresh Process List";
            this.btnRefreshProcList.UseVisualStyleBackColor = true;
            this.btnRefreshProcList.Click += new System.EventHandler(this.btnRefreshProcList_Click);
            // 
            // btnAttach
            // 
            this.btnAttach.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAttach.Location = new System.Drawing.Point(439, 176);
            this.btnAttach.Name = "btnAttach";
            this.btnAttach.Size = new System.Drawing.Size(87, 25);
            this.btnAttach.TabIndex = 3;
            this.btnAttach.Text = "Attach";
            this.btnAttach.UseVisualStyleBackColor = true;
            this.btnAttach.Click += new System.EventHandler(this.btnAttach_Click);
            // 
            // btnKillProc
            // 
            this.btnKillProc.Location = new System.Drawing.Point(197, 176);
            this.btnKillProc.Name = "btnKillProc";
            this.btnKillProc.Size = new System.Drawing.Size(84, 25);
            this.btnKillProc.TabIndex = 2;
            this.btnKillProc.Text = "Kill";
            this.btnKillProc.UseVisualStyleBackColor = true;
            this.btnKillProc.Click += new System.EventHandler(this.btnKillProc_Click);
            // 
            // LaunchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textBoxCmdArguments);
            this.Controls.Add(this.textBoxWorkingDir);
            this.Controls.Add(this.textBoxCommandLine);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnLaunch);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmbNPLStates);
            this.Controls.Add(this.cmbProjects);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "LaunchForm";
            this.Size = new System.Drawing.Size(567, 471);
            this.Load += new System.EventHandler(this.LaunchForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbProjects;
        private System.Windows.Forms.Button btnLaunch;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxCommandLine;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxWorkingDir;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxCmdArguments;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbNPLStates;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListView listViewProcs;
        private System.Windows.Forms.Button btnRefreshProcList;
        private System.Windows.Forms.Button btnAttach;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderID;
        private System.Windows.Forms.ColumnHeader columnHeaderPath;
        private System.Windows.Forms.Button btnKillProc;
    }
}