namespace kasthack.TimeLapser
{
    partial class FrmMain
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
                components.Dispose(); trayIcon.Dispose();
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
            this.txtPath = new System.Windows.Forms.TextBox();
            this.lblPath = new System.Windows.Forms.Label();
            this.lblFormat = new System.Windows.Forms.Label();
            this.cmbFormat = new System.Windows.Forms.ComboBox();
            this.lblFreq = new System.Windows.Forms.Label();
            this.nudFreq = new System.Windows.Forms.NumericUpDown();
            this.btnGo = new System.Windows.Forms.Button();
            this.lblTime = new System.Windows.Forms.Label();
            this.lblScreen = new System.Windows.Forms.Label();
            this.cmbScreen = new System.Windows.Forms.ComboBox();
            this.lblBitrate = new System.Windows.Forms.Label();
            this.budBitrate = new System.Windows.Forms.NumericUpDown();
            this.lblFramerate = new System.Windows.Forms.Label();
            this.nudFramerate = new System.Windows.Forms.NumericUpDown();
            this.btnbrs = new System.Windows.Forms.Button();
            this.fbdSave = new System.Windows.Forms.FolderBrowserDialog();
            this.chkSplit = new System.Windows.Forms.CheckBox();
            this.nudSplitInterval = new System.Windows.Forms.NumericUpDown();
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.chkRealtime = new System.Windows.Forms.CheckBox();
            this.lblSnapper = new System.Windows.Forms.Label();
            this.cmbSnapper = new System.Windows.Forms.ComboBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudFreq)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.budBitrate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFramerate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSplitInterval)).BeginInit();
            this.SuspendLayout();
            // 
            // txtPath
            // 
            this.txtPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPath.Location = new System.Drawing.Point(16, 58);
            this.txtPath.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(462, 31);
            this.txtPath.TabIndex = 0;
            // 
            // lblPath
            // 
            this.lblPath.AutoSize = true;
            this.lblPath.Location = new System.Drawing.Point(16, 25);
            this.lblPath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(124, 25);
            this.lblPath.TabIndex = 1;
            this.lblPath.Text = "__Output path";
            // 
            // lblFormat
            // 
            this.lblFormat.AutoSize = true;
            this.lblFormat.Location = new System.Drawing.Point(16, 331);
            this.lblFormat.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFormat.Name = "lblFormat";
            this.lblFormat.Size = new System.Drawing.Size(83, 25);
            this.lblFormat.TabIndex = 2;
            this.lblFormat.Text = "__Format";
            // 
            // cmbFormat
            // 
            this.cmbFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFormat.FormattingEnabled = true;
            this.cmbFormat.Location = new System.Drawing.Point(202, 326);
            this.cmbFormat.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.cmbFormat.Name = "cmbFormat";
            this.cmbFormat.Size = new System.Drawing.Size(106, 33);
            this.cmbFormat.TabIndex = 1;
            // 
            // lblFreq
            // 
            this.lblFreq.AutoSize = true;
            this.lblFreq.Location = new System.Drawing.Point(16, 118);
            this.lblFreq.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFreq.Name = "lblFreq";
            this.lblFreq.Size = new System.Drawing.Size(123, 25);
            this.lblFreq.TabIndex = 4;
            this.lblFreq.Text = "__ Interval(ms)";
            // 
            // nudFreq
            // 
            this.nudFreq.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudFreq.Location = new System.Drawing.Point(202, 114);
            this.nudFreq.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
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
            this.nudFreq.Size = new System.Drawing.Size(226, 31);
            this.nudFreq.TabIndex = 3;
            this.nudFreq.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // btnGo
            // 
            this.btnGo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGo.Location = new System.Drawing.Point(458, 546);
            this.btnGo.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(111, 44);
            this.btnGo.TabIndex = 6;
            this.btnGo.Text = "__Go";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.StartRecordingClicked);
            // 
            // lblTime
            // 
            this.lblTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(16, 555);
            this.lblTime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(90, 25);
            this.lblTime.TabIndex = 7;
            this.lblTime.Text = "__Pending";
            // 
            // lblScreen
            // 
            this.lblScreen.AutoSize = true;
            this.lblScreen.Location = new System.Drawing.Point(16, 405);
            this.lblScreen.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblScreen.Name = "lblScreen";
            this.lblScreen.Size = new System.Drawing.Size(78, 25);
            this.lblScreen.TabIndex = 8;
            this.lblScreen.Text = "__Screen";
            // 
            // cmbScreen
            // 
            this.cmbScreen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbScreen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScreen.FormattingEnabled = true;
            this.cmbScreen.Location = new System.Drawing.Point(205, 394);
            this.cmbScreen.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.cmbScreen.Name = "cmbScreen";
            this.cmbScreen.Size = new System.Drawing.Size(315, 33);
            this.cmbScreen.TabIndex = 5;
            // 
            // lblBitrate
            // 
            this.lblBitrate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblBitrate.AutoSize = true;
            this.lblBitrate.Location = new System.Drawing.Point(318, 331);
            this.lblBitrate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblBitrate.Name = "lblBitrate";
            this.lblBitrate.Size = new System.Drawing.Size(132, 25);
            this.lblBitrate.TabIndex = 10;
            this.lblBitrate.Text = "__Bitrate(mbps)";
            // 
            // budBitrate
            // 
            this.budBitrate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.budBitrate.Location = new System.Drawing.Point(458, 328);
            this.budBitrate.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
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
            this.budBitrate.Size = new System.Drawing.Size(111, 31);
            this.budBitrate.TabIndex = 2;
            this.budBitrate.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // lblFramerate
            // 
            this.lblFramerate.AutoSize = true;
            this.lblFramerate.Location = new System.Drawing.Point(16, 185);
            this.lblFramerate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFramerate.Name = "lblFramerate";
            this.lblFramerate.Size = new System.Drawing.Size(105, 25);
            this.lblFramerate.TabIndex = 12;
            this.lblFramerate.Text = "__Framerate";
            // 
            // nudFramerate
            // 
            this.nudFramerate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudFramerate.Location = new System.Drawing.Point(202, 181);
            this.nudFramerate.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.nudFramerate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudFramerate.Name = "nudFramerate";
            this.nudFramerate.Size = new System.Drawing.Size(226, 31);
            this.nudFramerate.TabIndex = 4;
            this.nudFramerate.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // btnbrs
            // 
            this.btnbrs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnbrs.Location = new System.Drawing.Point(486, 58);
            this.btnbrs.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnbrs.Name = "btnbrs";
            this.btnbrs.Size = new System.Drawing.Size(36, 31);
            this.btnbrs.TabIndex = 13;
            this.btnbrs.Text = "...";
            this.btnbrs.UseVisualStyleBackColor = true;
            this.btnbrs.Click += new System.EventHandler(this.BrowseDirectoryClicked);
            // 
            // chkSplit
            // 
            this.chkSplit.AutoSize = true;
            this.chkSplit.Checked = true;
            this.chkSplit.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSplit.Location = new System.Drawing.Point(16, 478);
            this.chkSplit.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.chkSplit.Name = "chkSplit";
            this.chkSplit.Size = new System.Drawing.Size(220, 29);
            this.chkSplit.TabIndex = 14;
            this.chkSplit.Text = "__Split every N minutes";
            this.chkSplit.UseVisualStyleBackColor = true;
            this.chkSplit.CheckedChanged += new System.EventHandler(this.SplitCheckChanged);
            // 
            // nudSplitInterval
            // 
            this.nudSplitInterval.Location = new System.Drawing.Point(247, 475);
            this.nudSplitInterval.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
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
            this.nudSplitInterval.Size = new System.Drawing.Size(62, 31);
            this.nudSplitInterval.TabIndex = 15;
            this.nudSplitInterval.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // trayIcon
            // 
            this.trayIcon.Text = "__Timelapser by kasthack";
            this.trayIcon.Click += new System.EventHandler(this.StatusIconClicked);
            // 
            // chkRealtime
            // 
            this.chkRealtime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkRealtime.AutoSize = true;
            this.chkRealtime.Location = new System.Drawing.Point(450, 114);
            this.chkRealtime.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.chkRealtime.Name = "chkRealtime";
            this.chkRealtime.Size = new System.Drawing.Size(119, 29);
            this.chkRealtime.TabIndex = 16;
            this.chkRealtime.Text = "__Realtime";
            this.chkRealtime.UseVisualStyleBackColor = true;
            this.chkRealtime.CheckedChanged += new System.EventHandler(this.RealtimeCheckChanged);
            // 
            // lblSnapper
            // 
            this.lblSnapper.AutoSize = true;
            this.lblSnapper.Location = new System.Drawing.Point(16, 248);
            this.lblSnapper.Name = "lblSnapper";
            this.lblSnapper.Size = new System.Drawing.Size(96, 25);
            this.lblSnapper.TabIndex = 17;
            this.lblSnapper.Text = "__Recorder";
            // 
            // cmbSnapper
            // 
            this.cmbSnapper.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbSnapper.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSnapper.FormattingEnabled = true;
            this.cmbSnapper.Location = new System.Drawing.Point(205, 242);
            this.cmbSnapper.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbSnapper.Name = "cmbSnapper";
            this.cmbSnapper.Size = new System.Drawing.Size(364, 33);
            this.cmbSnapper.TabIndex = 18;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Location = new System.Drawing.Point(533, 395);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(36, 31);
            this.btnRefresh.TabIndex = 19;
            this.btnRefresh.Text = "⟳";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(533, 58);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(36, 31);
            this.btnBrowse.TabIndex = 20;
            this.btnBrowse.Text = "📁";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // FrmMain
            // 
            this.AcceptButton = this.btnGo;
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(578, 608);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.cmbFormat);
            this.Controls.Add(this.cmbSnapper);
            this.Controls.Add(this.lblSnapper);
            this.Controls.Add(this.chkRealtime);
            this.Controls.Add(this.nudSplitInterval);
            this.Controls.Add(this.chkSplit);
            this.Controls.Add(this.btnbrs);
            this.Controls.Add(this.nudFramerate);
            this.Controls.Add(this.lblFramerate);
            this.Controls.Add(this.budBitrate);
            this.Controls.Add(this.lblBitrate);
            this.Controls.Add(this.cmbScreen);
            this.Controls.Add(this.lblScreen);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.btnGo);
            this.Controls.Add(this.nudFreq);
            this.Controls.Add(this.lblFreq);
            this.Controls.Add(this.lblFormat);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.txtPath);
            this.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 664);
            this.Name = "FrmMain";
            this.Text = "___Timelapser by kasthack";
            this.Load += new System.EventHandler(this.FormLoad);
            this.ResizeBegin += new System.EventHandler(this.FrmMain_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.FrmMain_ResizeEnd);
            this.SizeChanged += new System.EventHandler(this.HandleSizeChanged);
            this.Move += new System.EventHandler(this.FrmMain_Move);
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
        private System.Windows.Forms.Label lblScreen;
        private System.Windows.Forms.ComboBox cmbScreen;
        private System.Windows.Forms.Label lblBitrate;
        private System.Windows.Forms.NumericUpDown budBitrate;
        private System.Windows.Forms.Label lblFramerate;
        private System.Windows.Forms.NumericUpDown nudFramerate;
        private System.Windows.Forms.Button btnbrs;
        private System.Windows.Forms.FolderBrowserDialog fbdSave;
        private System.Windows.Forms.CheckBox chkSplit;
        private System.Windows.Forms.NumericUpDown nudSplitInterval;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private System.Windows.Forms.CheckBox chkRealtime;
        private System.Windows.Forms.Label lblSnapper;
        private System.Windows.Forms.ComboBox cmbSnapper;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnBrowse;
    }
}

