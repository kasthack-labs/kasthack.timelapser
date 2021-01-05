namespace kasthack.TimeLapser
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
            this.chkRealtime = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudFreq)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.budBitrate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFramerate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSplitInterval)).BeginInit();
            this.SuspendLayout();
            // 
            // txtPath
            // 
            resources.ApplyResources(this.txtPath, "txtPath");
            this.txtPath.Name = "txtPath";
            // 
            // lblPath
            // 
            resources.ApplyResources(this.lblPath, "lblPath");
            this.lblPath.Name = "lblPath";
            // 
            // lblFormat
            // 
            resources.ApplyResources(this.lblFormat, "lblFormat");
            this.lblFormat.Name = "lblFormat";
            // 
            // cmbFormat
            // 
            resources.ApplyResources(this.cmbFormat, "cmbFormat");
            this.cmbFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFormat.FormattingEnabled = true;
            this.cmbFormat.Name = "cmbFormat";
            // 
            // lblFreq
            // 
            resources.ApplyResources(this.lblFreq, "lblFreq");
            this.lblFreq.Name = "lblFreq";
            // 
            // nudFreq
            // 
            resources.ApplyResources(this.nudFreq, "nudFreq");
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
            this.nudFreq.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // btnGo
            // 
            resources.ApplyResources(this.btnGo, "btnGo");
            this.btnGo.Name = "btnGo";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // lblTime
            // 
            resources.ApplyResources(this.lblTime, "lblTime");
            this.lblTime.Name = "lblTime";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // cmbScreen
            // 
            resources.ApplyResources(this.cmbScreen, "cmbScreen");
            this.cmbScreen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScreen.FormattingEnabled = true;
            this.cmbScreen.Name = "cmbScreen";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // budBitrate
            // 
            resources.ApplyResources(this.budBitrate, "budBitrate");
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
            this.budBitrate.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // nudFramerate
            // 
            resources.ApplyResources(this.nudFramerate, "nudFramerate");
            this.nudFramerate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudFramerate.Name = "nudFramerate";
            this.nudFramerate.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // btnbrs
            // 
            resources.ApplyResources(this.btnbrs, "btnbrs");
            this.btnbrs.Name = "btnbrs";
            this.btnbrs.UseVisualStyleBackColor = true;
            this.btnbrs.Click += new System.EventHandler(this.btnbrs_Click);
            // 
            // fbdSave
            // 
            resources.ApplyResources(this.fbdSave, "fbdSave");
            // 
            // chkSplit
            // 
            resources.ApplyResources(this.chkSplit, "chkSplit");
            this.chkSplit.Name = "chkSplit";
            this.chkSplit.UseVisualStyleBackColor = true;
            this.chkSplit.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // nudSplitInterval
            // 
            resources.ApplyResources(this.nudSplitInterval, "nudSplitInterval");
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
            this.nudSplitInterval.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // nicon
            // 
            resources.ApplyResources(this.nicon, "nicon");
            this.nicon.DoubleClick += new System.EventHandler(this.nicon_DoubleClick);
            // 
            // chkRealtime
            // 
            resources.ApplyResources(this.chkRealtime, "chkRealtime");
            this.chkRealtime.Name = "chkRealtime";
            this.chkRealtime.UseVisualStyleBackColor = true;
            this.chkRealtime.CheckedChanged += new System.EventHandler(this.chkRealtime_CheckedChanged);
            // 
            // frmMain
            // 
            this.AcceptButton = this.btnGo;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkRealtime);
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
            this.MaximizeBox = false;
            this.Name = "frmMain";
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
        private System.Windows.Forms.CheckBox chkRealtime;
    }
}

