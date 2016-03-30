namespace ParaEngine.NPLDebuggerPackage
{
    partial class LaunchForm
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
            this.components = new System.ComponentModel.Container();
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
            this.listViewProcs = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnRefreshProcList = new System.Windows.Forms.Button();
            this.btnRegisterDebugEngine = new System.Windows.Forms.Button();
            this.btnAttach = new System.Windows.Forms.Button();
            this.btnKillProc = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabAttachPage = new System.Windows.Forms.TabPage();
            this.tabLaunchPage = new System.Windows.Forms.TabPage();
            this.label7 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabAttachPage.SuspendLayout();
            this.tabLaunchPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 19);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(724, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "Launch or attach to a NPL/ParaEngine application. ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 81);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(143, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Project To Debug:";
            // 
            // cmbProjects
            // 
            this.cmbProjects.FormattingEnabled = true;
            this.cmbProjects.Location = new System.Drawing.Point(170, 77);
            this.cmbProjects.Margin = new System.Windows.Forms.Padding(4);
            this.cmbProjects.Name = "cmbProjects";
            this.cmbProjects.Size = new System.Drawing.Size(260, 23);
            this.cmbProjects.TabIndex = 2;
            this.cmbProjects.SelectionChangeCommitted += new System.EventHandler(this.cmbProjects_SelectionChangeCommitted);
            // 
            // btnLaunch
            // 
            this.btnLaunch.Location = new System.Drawing.Point(170, 223);
            this.btnLaunch.Margin = new System.Windows.Forms.Padding(4);
            this.btnLaunch.Name = "btnLaunch";
            this.btnLaunch.Size = new System.Drawing.Size(116, 31);
            this.btnLaunch.TabIndex = 11;
            this.btnLaunch.Text = "Launch";
            this.btnLaunch.UseVisualStyleBackColor = true;
            this.btnLaunch.Click += new System.EventHandler(this.btnLaunch_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(315, 223);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(116, 31);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 125);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(111, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "Command Line:";
            // 
            // textBoxCommandLine
            // 
            this.textBoxCommandLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCommandLine.Location = new System.Drawing.Point(170, 122);
            this.textBoxCommandLine.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxCommandLine.Name = "textBoxCommandLine";
            this.textBoxCommandLine.Size = new System.Drawing.Size(581, 25);
            this.textBoxCommandLine.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 193);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(151, 15);
            this.label4.TabIndex = 9;
            this.label4.Text = "Working Directory:";
            // 
            // textBoxWorkingDir
            // 
            this.textBoxWorkingDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxWorkingDir.Location = new System.Drawing.Point(170, 189);
            this.textBoxWorkingDir.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxWorkingDir.Name = "textBoxWorkingDir";
            this.textBoxWorkingDir.Size = new System.Drawing.Size(581, 25);
            this.textBoxWorkingDir.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 159);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(119, 15);
            this.label5.TabIndex = 7;
            this.label5.Text = "Cmd Arguments:";
            // 
            // textBoxCmdArguments
            // 
            this.textBoxCmdArguments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCmdArguments.Location = new System.Drawing.Point(170, 155);
            this.textBoxCmdArguments.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxCmdArguments.Name = "textBoxCmdArguments";
            this.textBoxCmdArguments.Size = new System.Drawing.Size(581, 25);
            this.textBoxCmdArguments.TabIndex = 8;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(439, 81);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(151, 15);
            this.label6.TabIndex = 3;
            this.label6.Text = "NPL State(Thread):";
            // 
            // cmbNPLStates
            // 
            this.cmbNPLStates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
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
            this.cmbNPLStates.Location = new System.Drawing.Point(598, 77);
            this.cmbNPLStates.Margin = new System.Windows.Forms.Padding(4);
            this.cmbNPLStates.Name = "cmbNPLStates";
            this.cmbNPLStates.Size = new System.Drawing.Size(153, 23);
            this.cmbNPLStates.TabIndex = 4;
            this.cmbNPLStates.Text = "main";
            this.cmbNPLStates.SelectionChangeCommitted += new System.EventHandler(this.cmbProjects_SelectionChangeCommitted);
            this.cmbNPLStates.TextChanged += new System.EventHandler(this.cmbNPLStates_TextChanged);
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
            this.listViewProcs.Location = new System.Drawing.Point(17, 7);
            this.listViewProcs.Margin = new System.Windows.Forms.Padding(4);
            this.listViewProcs.MultiSelect = false;
            this.listViewProcs.Name = "listViewProcs";
            this.listViewProcs.Size = new System.Drawing.Size(739, 356);
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
            this.btnRefreshProcList.Location = new System.Drawing.Point(17, 372);
            this.btnRefreshProcList.Margin = new System.Windows.Forms.Padding(4);
            this.btnRefreshProcList.Name = "btnRefreshProcList";
            this.btnRefreshProcList.Size = new System.Drawing.Size(113, 31);
            this.btnRefreshProcList.TabIndex = 1;
            this.btnRefreshProcList.Text = "Refresh";
            this.btnRefreshProcList.UseVisualStyleBackColor = true;
            this.btnRefreshProcList.Click += new System.EventHandler(this.btnRefreshProcList_Click);
            // 
            // btnRegisterDebugEngine
            // 
            this.btnRegisterDebugEngine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRegisterDebugEngine.Location = new System.Drawing.Point(352, 372);
            this.btnRegisterDebugEngine.Margin = new System.Windows.Forms.Padding(4);
            this.btnRegisterDebugEngine.Name = "btnRegisterDebugEngine";
            this.btnRegisterDebugEngine.Size = new System.Drawing.Size(115, 31);
            this.btnRegisterDebugEngine.TabIndex = 3;
            this.btnRegisterDebugEngine.Text = "Register";
            this.toolTip1.SetToolTip(this.btnRegisterDebugEngine, "One need to manually register the NPLEngine.dll once to complete installation");
            this.btnRegisterDebugEngine.UseVisualStyleBackColor = true;
            this.btnRegisterDebugEngine.Click += new System.EventHandler(this.btnRegisterDebugEngine_Click);
            // 
            // btnAttach
            // 
            this.btnAttach.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAttach.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnAttach.Location = new System.Drawing.Point(641, 372);
            this.btnAttach.Margin = new System.Windows.Forms.Padding(4);
            this.btnAttach.Name = "btnAttach";
            this.btnAttach.Size = new System.Drawing.Size(116, 31);
            this.btnAttach.TabIndex = 3;
            this.btnAttach.Text = "Attach";
            this.toolTip1.SetToolTip(this.btnAttach, "Click Attach button to debug the process");
            this.btnAttach.UseVisualStyleBackColor = true;
            this.btnAttach.Click += new System.EventHandler(this.btnAttach_Click);
            // 
            // btnKillProc
            // 
            this.btnKillProc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnKillProc.Location = new System.Drawing.Point(148, 372);
            this.btnKillProc.Margin = new System.Windows.Forms.Padding(4);
            this.btnKillProc.Name = "btnKillProc";
            this.btnKillProc.Size = new System.Drawing.Size(116, 31);
            this.btnKillProc.TabIndex = 2;
            this.btnKillProc.Text = "Kill";
            this.btnKillProc.UseVisualStyleBackColor = true;
            this.btnKillProc.Click += new System.EventHandler(this.btnKillProc_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.ToolTipTitle = "Attach to selected process";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabAttachPage);
            this.tabControl1.Controls.Add(this.tabLaunchPage);
            this.tabControl1.Location = new System.Drawing.Point(19, 58);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(780, 441);
            this.tabControl1.TabIndex = 14;
            // 
            // tabAttachPage
            // 
            this.tabAttachPage.Controls.Add(this.listViewProcs);
            this.tabAttachPage.Controls.Add(this.btnRefreshProcList);
            this.tabAttachPage.Controls.Add(this.btnKillProc);
            this.tabAttachPage.Controls.Add(this.btnRegisterDebugEngine);
            this.tabAttachPage.Controls.Add(this.btnAttach);
            this.tabAttachPage.Location = new System.Drawing.Point(4, 25);
            this.tabAttachPage.Name = "tabAttachPage";
            this.tabAttachPage.Padding = new System.Windows.Forms.Padding(3);
            this.tabAttachPage.Size = new System.Drawing.Size(772, 412);
            this.tabAttachPage.TabIndex = 1;
            this.tabAttachPage.Text = "Attach To Process";
            this.tabAttachPage.UseVisualStyleBackColor = true;
            // 
            // tabLaunchPage
            // 
            this.tabLaunchPage.Controls.Add(this.label6);
            this.tabLaunchPage.Controls.Add(this.label7);
            this.tabLaunchPage.Controls.Add(this.label2);
            this.tabLaunchPage.Controls.Add(this.textBoxCmdArguments);
            this.tabLaunchPage.Controls.Add(this.label3);
            this.tabLaunchPage.Controls.Add(this.textBoxWorkingDir);
            this.tabLaunchPage.Controls.Add(this.cmbProjects);
            this.tabLaunchPage.Controls.Add(this.textBoxCommandLine);
            this.tabLaunchPage.Controls.Add(this.cmbNPLStates);
            this.tabLaunchPage.Controls.Add(this.btnCancel);
            this.tabLaunchPage.Controls.Add(this.label4);
            this.tabLaunchPage.Controls.Add(this.label5);
            this.tabLaunchPage.Controls.Add(this.btnLaunch);
            this.tabLaunchPage.Location = new System.Drawing.Point(4, 25);
            this.tabLaunchPage.Name = "tabLaunchPage";
            this.tabLaunchPage.Padding = new System.Windows.Forms.Padding(3);
            this.tabLaunchPage.Size = new System.Drawing.Size(772, 412);
            this.tabLaunchPage.TabIndex = 0;
            this.tabLaunchPage.Text = "Launch";
            this.tabLaunchPage.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.Location = new System.Drawing.Point(19, 20);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(732, 40);
            this.label7.TabIndex = 0;
            this.label7.Text = "One can modify the project command line and working directory to let the path aut" +
    "omatically filled for you. ";
            // 
            // LaunchForm
            // 
            this.AcceptButton = this.btnAttach;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(809, 511);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "LaunchForm";
            this.Text = "NPL Debug Engine v2 Launcher";
            this.Load += new System.EventHandler(this.LaunchForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabAttachPage.ResumeLayout(false);
            this.tabLaunchPage.ResumeLayout(false);
            this.tabLaunchPage.PerformLayout();
            this.ResumeLayout(false);

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
        private System.Windows.Forms.ListView listViewProcs;
        private System.Windows.Forms.Button btnRefreshProcList;
        private System.Windows.Forms.Button btnAttach;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderID;
        private System.Windows.Forms.ColumnHeader columnHeaderPath;
        private System.Windows.Forms.Button btnKillProc;
        private System.Windows.Forms.Button btnRegisterDebugEngine;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabLaunchPage;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TabPage tabAttachPage;
    }
}