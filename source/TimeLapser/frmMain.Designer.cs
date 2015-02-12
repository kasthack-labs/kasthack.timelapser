namespace TimeLapser
{
    partial class frmMain
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
                components.Dispose(); nicon.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.txtPath = new System.Windows.Forms.TextBox();
            this.lblPath = new System.Windows.Forms.Label();
            this.lblFormat = new System.Windows.Forms.Label();
            this.cmbFormat = new System.Windows.Forms.ComboBox();
            this.lblFreq = new System.Windows.Forms.Label();
            this.nudFreq = new System.Windows.Forms.NumericUpDown();
            this.btnGo = new System.Windows.Forms.Button();
            this.lblTime = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbScreen = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.budBitrate = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.nudFramerate = new System.Windows.Forms.NumericUpDown();
            this.btnbrs = new System.Windows.Forms.Button();
            this.fbdSave = new System.Windows.Forms.FolderBrowserDialog();
            this.chkSplit = new System.Windows.Forms.CheckBox();
            this.nudSplitInterval = new System.Windows.Forms.NumericUpDown();
            this.nicon = new System.Windows.Forms.NotifyIcon(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nudFreq)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.budBitrate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFramerate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSplitInterval)).BeginInit();
            this.SuspendLayout();
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(12, 30);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(241, 20);
            this.txtPath.TabIndex = 0;
            // 
            // lblPath
            // 
            this.lblPath.AutoSize = true;
            this.lblPath.Location = new System.Drawing.Point(12, 13);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(63, 13);
            this.lblPath.TabIndex = 1;
            this.lblPath.Text = "Output path";
            // 
            // lblFormat
            // 
            this.lblFormat.AutoSize = true;
            this.lblFormat.Location = new System.Drawing.Point(9, 97);
            this.lblFormat.Name = "lblFormat";
            this.lblFormat.Size = new System.Drawing.Size(39, 13);
            this.lblFormat.TabIndex = 2;
            this.lblFormat.Text = "Format";
            // 
            // cmbFormat
            // 
            this.cmbFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFormat.FormattingEnabled = true;
            this.cmbFormat.Location = new System.Drawing.Point(70, 94);
            this.cmbFormat.Name = "cmbFormat";
            this.cmbFormat.Size = new System.Drawing.Size(56, 21);
            this.cmbFormat.TabIndex = 1;
            // 
            // lblFreq
            // 
            this.lblFreq.AutoSize = true;
            this.lblFreq.Location = new System.Drawing.Point(9, 61);
            this.lblFreq.Name = "lblFreq";
            this.lblFreq.Size = new System.Drawing.Size(61, 13);
            this.lblFreq.TabIndex = 4;
            this.lblFreq.Text = "Interval(ms)";
            // 
            // nudFreq
            // 
            this.nudFreq.Location = new System.Drawing.Point(70, 61);
            this.nudFreq.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudFreq.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudFreq.Name = "nudFreq";
            this.nudFreq.Size = new System.Drawing.Size(56, 20);
            this.nudFreq.TabIndex = 3;
            this.nudFreq.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // btnGo
            // 
            this.btnGo.Location = new System.Drawing.Point(211, 213);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(67, 23);
            this.btnGo.TabIndex = 6;
            this.btnGo.Text = "Go";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(12, 213);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(46, 13);
            this.lblTime.TabIndex = 7;
            this.lblTime.Text = "Pending";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 133);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Screen";
            // 
            // cmbScreen
            // 
            this.cmbScreen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScreen.FormattingEnabled = true;
            this.cmbScreen.Location = new System.Drawing.Point(70, 133);
            this.cmbScreen.Name = "cmbScreen";
            this.cmbScreen.Size = new System.Drawing.Size(208, 21);
            this.cmbScreen.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(132, 97);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Bitrate(mbps)";
            // 
            // budBitrate
            // 
            this.budBitrate.Location = new System.Drawing.Point(211, 97);
            this.budBitrate.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.budBitrate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.budBitrate.Name = "budBitrate";
            this.budBitrate.Size = new System.Drawing.Size(67, 20);
            this.budBitrate.TabIndex = 2;
            this.budBitrate.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(132, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Framerate";
            // 
            // nudFramerate
            // 
            this.nudFramerate.Location = new System.Drawing.Point(211, 61);
            this.nudFramerate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudFramerate.Name = "nudFramerate";
            this.nudFramerate.Size = new System.Drawing.Size(67, 20);
            this.nudFramerate.TabIndex = 4;
            this.nudFramerate.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // btnbrs
            // 
            this.btnbrs.Location = new System.Drawing.Point(257, 30);
            this.btnbrs.Name = "btnbrs";
            this.btnbrs.Size = new System.Drawing.Size(21, 20);
            this.btnbrs.TabIndex = 13;
            this.btnbrs.Text = "...";
            this.btnbrs.UseVisualStyleBackColor = true;
            this.btnbrs.Click += new System.EventHandler(this.btnbrs_Click);
            // 
            // chkSplit
            // 
            this.chkSplit.AutoSize = true;
            this.chkSplit.Location = new System.Drawing.Point(12, 169);
            this.chkSplit.Name = "chkSplit";
            this.chkSplit.Size = new System.Drawing.Size(125, 17);
            this.chkSplit.TabIndex = 14;
            this.chkSplit.Text = "Split every N minutes";
            this.chkSplit.UseVisualStyleBackColor = true;
            this.chkSplit.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // nudSplitInterval
            // 
            this.nudSplitInterval.Enabled = false;
            this.nudSplitInterval.Location = new System.Drawing.Point(211, 169);
            this.nudSplitInterval.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudSplitInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudSplitInterval.Name = "nudSplitInterval";
            this.nudSplitInterval.Size = new System.Drawing.Size(67, 20);
            this.nudSplitInterval.TabIndex = 15;
            this.nudSplitInterval.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // nicon
            // 
            this.nicon.Icon = ((System.Drawing.Icon)(resources.GetObject("nicon.Icon")));
            this.nicon.Text = "Timelapser by kasthack";
            this.nicon.DoubleClick += new System.EventHandler(this.nicon_DoubleClick);
            // 
            // frmMain
            // 
            this.AcceptButton = this.btnGo;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 244);
            this.Controls.Add(this.nudSplitInterval);
            this.Controls.Add(this.chkSplit);
            this.Controls.Add(this.btnbrs);
            this.Controls.Add(this.nudFramerate);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.budBitrate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbScreen);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.btnGo);
            this.Controls.Add(this.nudFreq);
            this.Controls.Add(this.lblFreq);
            this.Controls.Add(this.cmbFormat);
            this.Controls.Add(this.lblFormat);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.txtPath);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.Text = "Timelapser by kasthack";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.SizeChanged += new System.EventHandler(this.frmMain_SizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.nudFreq)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.budBitrate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFramerate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSplitInterval)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.Label lblFormat;
        private System.Windows.Forms.ComboBox cmbFormat;
        private System.Windows.Forms.Label lblFreq;
        private System.Windows.Forms.NumericUpDown nudFreq;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbScreen;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown budBitrate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nudFramerate;
        private System.Windows.Forms.Button btnbrs;
        private System.Windows.Forms.FolderBrowserDialog fbdSave;
        private System.Windows.Forms.CheckBox chkSplit;
        private System.Windows.Forms.NumericUpDown nudSplitInterval;
        private System.Windows.Forms.NotifyIcon nicon;
    }
}

