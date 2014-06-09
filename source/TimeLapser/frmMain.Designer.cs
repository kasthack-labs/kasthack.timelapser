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
            ((System.ComponentModel.ISupportInitialize)(this.nudFreq)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.budBitrate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFramerate)).BeginInit();
            this.SuspendLayout();
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(10, 30);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(268, 20);
            this.txtPath.TabIndex = 0;
            // 
            // lblPath
            // 
            this.lblPath.AutoSize = true;
            this.lblPath.Location = new System.Drawing.Point(10, 13);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(63, 13);
            this.lblPath.TabIndex = 1;
            this.lblPath.Text = "Output path";
            // 
            // lblFormat
            // 
            this.lblFormat.AutoSize = true;
            this.lblFormat.Location = new System.Drawing.Point(10, 57);
            this.lblFormat.Name = "lblFormat";
            this.lblFormat.Size = new System.Drawing.Size(39, 13);
            this.lblFormat.TabIndex = 2;
            this.lblFormat.Text = "Format";
            // 
            // cmbFormat
            // 
            this.cmbFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFormat.FormattingEnabled = true;
            this.cmbFormat.Location = new System.Drawing.Point(10, 74);
            this.cmbFormat.Name = "cmbFormat";
            this.cmbFormat.Size = new System.Drawing.Size(97, 21);
            this.cmbFormat.TabIndex = 1;
            // 
            // lblFreq
            // 
            this.lblFreq.AutoSize = true;
            this.lblFreq.Location = new System.Drawing.Point(10, 102);
            this.lblFreq.Name = "lblFreq";
            this.lblFreq.Size = new System.Drawing.Size(42, 13);
            this.lblFreq.TabIndex = 4;
            this.lblFreq.Text = "Interval";
            // 
            // nudFreq
            // 
            this.nudFreq.Location = new System.Drawing.Point(10, 131);
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
            this.nudFreq.Size = new System.Drawing.Size(97, 20);
            this.nudFreq.TabIndex = 3;
            this.nudFreq.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // btnGo
            // 
            this.btnGo.Location = new System.Drawing.Point(191, 235);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(87, 23);
            this.btnGo.TabIndex = 6;
            this.btnGo.Text = "Go";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(10, 235);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(46, 13);
            this.lblTime.TabIndex = 7;
            this.lblTime.Text = "Pending";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 172);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Screen";
            // 
            // cmbScreen
            // 
            this.cmbScreen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScreen.FormattingEnabled = true;
            this.cmbScreen.Location = new System.Drawing.Point(10, 197);
            this.cmbScreen.Name = "cmbScreen";
            this.cmbScreen.Size = new System.Drawing.Size(268, 21);
            this.cmbScreen.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(139, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Bitrate(kbps)";
            // 
            // budBitrate
            // 
            this.budBitrate.Location = new System.Drawing.Point(142, 74);
            this.budBitrate.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.budBitrate.Name = "budBitrate";
            this.budBitrate.Size = new System.Drawing.Size(136, 20);
            this.budBitrate.TabIndex = 2;
            this.budBitrate.Value = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(139, 102);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Framerate";
            // 
            // nudFramerate
            // 
            this.nudFramerate.Location = new System.Drawing.Point(142, 131);
            this.nudFramerate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudFramerate.Name = "nudFramerate";
            this.nudFramerate.Size = new System.Drawing.Size(136, 20);
            this.nudFramerate.TabIndex = 4;
            this.nudFramerate.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
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
            this.Name = "Form1";
            this.Text = "Timelapser by kasthack";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudFreq)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.budBitrate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFramerate)).EndInit();
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
    }
}

