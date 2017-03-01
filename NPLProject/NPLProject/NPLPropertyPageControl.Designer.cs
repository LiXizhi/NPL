namespace NPLProject
{
    partial class NPLPropertyPageControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._nplExePath = new System.Windows.Forms.TextBox();
            this.nplExeArguments = new System.Windows.Forms.TextBox();
            this._scriptFile = new System.Windows.Forms.TextBox();
            this._scriptArguments = new System.Windows.Forms.TextBox();
            this._workingDir = new System.Windows.Forms.TextBox();
            this._nplExePathButton = new System.Windows.Forms.Button();
            this._nplExePathLabel = new System.Windows.Forms.Label();
            this._nplExeArgumentsLabel = new System.Windows.Forms.Label();
            this._sciptFileLabel = new System.Windows.Forms.Label();
            this._scriptArgumentsLabel = new System.Windows.Forms.Label();
            this._workingDirLabel = new System.Windows.Forms.Label();
            this._workingDirButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _nplExePath
            // 
            this._nplExePath.Location = new System.Drawing.Point(117, 31);
            this._nplExePath.Name = "_nplExePath";
            this._nplExePath.Size = new System.Drawing.Size(414, 20);
            this._nplExePath.TabIndex = 0;
            this._nplExePath.TextChanged += new System.EventHandler(this.NPLPathChanged);
            // 
            // nplExeArguments
            // 
            this.nplExeArguments.Location = new System.Drawing.Point(117, 81);
            this.nplExeArguments.Name = "nplExeArguments";
            this.nplExeArguments.Size = new System.Drawing.Size(464, 20);
            this.nplExeArguments.TabIndex = 1;
            // 
            // _scriptFile
            // 
            this._scriptFile.Location = new System.Drawing.Point(117, 130);
            this._scriptFile.Name = "_scriptFile";
            this._scriptFile.Size = new System.Drawing.Size(464, 20);
            this._scriptFile.TabIndex = 2;
            // 
            // _scriptArguments
            // 
            this._scriptArguments.Location = new System.Drawing.Point(117, 183);
            this._scriptArguments.Name = "_scriptArguments";
            this._scriptArguments.Size = new System.Drawing.Size(464, 20);
            this._scriptArguments.TabIndex = 3;
            // 
            // _workingDir
            // 
            this._workingDir.Location = new System.Drawing.Point(118, 241);
            this._workingDir.Name = "_workingDir";
            this._workingDir.Size = new System.Drawing.Size(413, 20);
            this._workingDir.TabIndex = 4;
            // 
            // _nplExePathButton
            // 
            this._nplExePathButton.AccessibleName = "";
            this._nplExePathButton.Location = new System.Drawing.Point(547, 29);
            this._nplExePathButton.Name = "_nplExePathButton";
            this._nplExePathButton.Size = new System.Drawing.Size(34, 23);
            this._nplExePathButton.TabIndex = 2;
            this._nplExePathButton.Text = "...";
            this._nplExePathButton.UseVisualStyleBackColor = true;
            this._nplExePathButton.Click += new System.EventHandler(this.NPLPathButtonClicked);
            // 
            // _nplExePathLabel
            // 
            this._nplExePathLabel.AutoSize = true;
            this._nplExePathLabel.Location = new System.Drawing.Point(25, 34);
            this._nplExePathLabel.Name = "_nplExePathLabel";
            this._nplExePathLabel.Size = new System.Drawing.Size(65, 13);
            this._nplExePathLabel.TabIndex = 5;
            this._nplExePathLabel.Text = "npl.exe path";
            // 
            // _nplExeArgumentsLabel
            // 
            this._nplExeArgumentsLabel.AutoSize = true;
            this._nplExeArgumentsLabel.Location = new System.Drawing.Point(25, 84);
            this._nplExeArgumentsLabel.Name = "_nplExeArgumentsLabel";
            this._nplExeArgumentsLabel.Size = new System.Drawing.Size(78, 13);
            this._nplExeArgumentsLabel.TabIndex = 6;
            this._nplExeArgumentsLabel.Text = "npl.exe options";
            // 
            // _sciptFileLabel
            // 
            this._sciptFileLabel.AutoSize = true;
            this._sciptFileLabel.Location = new System.Drawing.Point(25, 133);
            this._sciptFileLabel.Name = "_sciptFileLabel";
            this._sciptFileLabel.Size = new System.Drawing.Size(32, 13);
            this._sciptFileLabel.TabIndex = 7;
            this._sciptFileLabel.Text = "script";
            // 
            // _scriptArgumentsLabel
            // 
            this._scriptArgumentsLabel.AutoSize = true;
            this._scriptArgumentsLabel.Location = new System.Drawing.Point(25, 186);
            this._scriptArgumentsLabel.Name = "_scriptArgumentsLabel";
            this._scriptArgumentsLabel.Size = new System.Drawing.Size(84, 13);
            this._scriptArgumentsLabel.TabIndex = 8;
            this._scriptArgumentsLabel.Text = "script arguments";
            // 
            // _workingDirLabel
            // 
            this._workingDirLabel.AutoSize = true;
            this._workingDirLabel.Location = new System.Drawing.Point(25, 244);
            this._workingDirLabel.Name = "_workingDirLabel";
            this._workingDirLabel.Size = new System.Drawing.Size(87, 13);
            this._workingDirLabel.TabIndex = 9;
            this._workingDirLabel.Text = "working directory";
            // 
            // _workingDirButton
            // 
            this._workingDirButton.AccessibleName = "";
            this._workingDirButton.Location = new System.Drawing.Point(547, 239);
            this._workingDirButton.Name = "_workingDirButton";
            this._workingDirButton.Size = new System.Drawing.Size(34, 23);
            this._workingDirButton.TabIndex = 10;
            this._workingDirButton.Text = "...";
            this._workingDirButton.UseVisualStyleBackColor = true;
            // 
            // NPLPropertyPageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._workingDirButton);
            this.Controls.Add(this._workingDirLabel);
            this.Controls.Add(this._scriptArgumentsLabel);
            this.Controls.Add(this._sciptFileLabel);
            this.Controls.Add(this._nplExeArgumentsLabel);
            this.Controls.Add(this._nplExePathLabel);
            this.Controls.Add(this._nplExePathButton);
            this.Controls.Add(this._workingDir);
            this.Controls.Add(this._scriptArguments);
            this.Controls.Add(this._scriptFile);
            this.Controls.Add(this.nplExeArguments);
            this.Controls.Add(this._nplExePath);
            this.Name = "NPLPropertyPageControl";
            this.Size = new System.Drawing.Size(605, 308);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _nplExePath;
        private System.Windows.Forms.TextBox nplExeArguments;
        private System.Windows.Forms.TextBox _scriptFile;
        private System.Windows.Forms.TextBox _scriptArguments;
        private System.Windows.Forms.TextBox _workingDir;
        private System.Windows.Forms.Button _nplExePathButton;
        private System.Windows.Forms.Label _nplExePathLabel;
        private System.Windows.Forms.Label _nplExeArgumentsLabel;
        private System.Windows.Forms.Label _sciptFileLabel;
        private System.Windows.Forms.Label _scriptArgumentsLabel;
        private System.Windows.Forms.Label _workingDirLabel;
        private System.Windows.Forms.Button _workingDirButton;
    }
}
