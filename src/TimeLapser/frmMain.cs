#if DEBUG
#define TESTING
#endif

namespace kasthack.TimeLapser
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;

    using Accord.Video.FFMPEG;

    using kasthack.TimeLapser.Properties;

    public partial class FrmMain : Form
    {
        private readonly Recorder recorder = new();
        private readonly ScreenInfo formScreenInfo;
        private RecordSettings settings;
        public FrmMain()
        {
            this.InitializeComponent();
            this.ApplyLocale();
            this.trayIcon.Icon = this.Icon = Resources.icon;
            this.formScreenInfo = new ScreenInfo { Id = 31337, Name = Locale.Locale.BehindThisWindowDragAndResizeToTune };
        }

        private void ApplyLocale()
        {
            this.Text = Locale.Locale.ProgramName;
            this.lblFreq.Text = Locale.Locale.IntervalMs;
            this.lblPath.Text = Locale.Locale.OutputPath;
            this.chkRealtime.Text = Locale.Locale.Realtime;
            this.lblFramerate.Text = Locale.Locale.Framerate;
            this.lblSnapper.Text = Locale.Locale.Recorder;
            this.lblFormat.Text = Locale.Locale.Format;
            this.lblBitrate.Text = Locale.Locale.Bitrate;
            this.lblScreen.Text = Locale.Locale.Screen;
            this.chkSplit.Text = Locale.Locale.SplitEveryNMinutes;
            this.lblTime.Text = Locale.Locale.Pending;
            this.btnGo.Text = Locale.Locale.StartRecording;
            this.trayIcon.Text = Locale.Locale.ProgramName;
        }

        private void StartRecordingClicked(object sender, EventArgs e)
        {
            if (this.recorder.Recording)
            {
                this.recorder.Stop();
                this.SetRecordingState(false);
            }
            else
            {
                this.SetRecordingState(true);
                this.settings = new RecordSettings(
                    outputPath: this.txtPath.Text,
                    captureRectangle: ((ScreenInfo)this.cmbScreen.SelectedItem).Rect,
                    fps: (int)this.nudFramerate.Value,
                    interval: (int)this.nudFreq.Value,
                    codec: (VideoCodec)this.cmbFormat.SelectedItem,
                    bitrate: (int)this.budBitrate.Value << 20,
                    splitInterval: this.chkSplit.Checked ? (double?)this.nudSplitInterval.Value : null,
                    onFrameWritten: (a) => this.BeginInvoke((Action)(() => this.lblTime.Text = string.Format(Locale.Locale.ElapsedFormatStirng, a))),
                    realtime: this.chkRealtime.Checked,
                    snapperType: (SnapperType)this.cmbSnapper.SelectedItem);
                this.recorder.Start(this.settings);
            }
        }

        private void SetRecordingState(bool recordRunning)
        {
            this.FormBorderStyle = recordRunning ? FormBorderStyle.FixedSingle : FormBorderStyle.Sizable;
            this.btnGo.Text = recordRunning ? Locale.Locale.StopRecording : Locale.Locale.StartRecording;
            this.lblTime.Text = !recordRunning ? Locale.Locale.Pending : string.Empty;
            this.txtPath.Enabled
                = this.btnbrs.Enabled
                = this.nudFreq.Enabled
                = this.nudFramerate.Enabled
                = this.cmbFormat.Enabled
                = this.budBitrate.Enabled
                = this.cmbScreen.Enabled
                = this.chkSplit.Enabled
                = this.nudSplitInterval.Enabled
                = this.chkRealtime.Enabled
                = this.cmbSnapper.Enabled
                = !recordRunning;
        }

        private void FormLoad(object sender, EventArgs e)
        {
            var screenInfos = ScreenInfo.GetScreenInfos();
            this.UpdateFormScreenInfo();
            screenInfos.Add(this.formScreenInfo);

            this.cmbSnapper.DataSource = Enum.GetValues(typeof(SnapperType)) as SnapperType[];
            this.cmbFormat.DataSource = Enum.GetValues(typeof(VideoCodec)) as VideoCodec[];
            this.cmbScreen.DataSource = screenInfos;
            this.cmbFormat.SelectedIndex = 0;

            // enable legacy recorder for win7 & earlier
            if (Environment.OSVersion.Version <= new Version(6, 1))
            {
                this.cmbSnapper.SelectedIndex = 1;
            }

            this.cmbScreen.SelectedIndex = this.cmbScreen.Items.Count - 1;
            this.txtPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
#if TESTING
            this.txtPath.Text = Path.Combine(this.txtPath.Text, "dbg_scr");
            this.chkSplit.Checked = true;
            this.nudSplitInterval.Value = 1;
            this.chkRealtime.Checked = true;
            this.cmbScreen.SelectedIndex = 1;
#endif
        }

        private void BrowseDirectoryClicked(object sender, EventArgs e) => this.txtPath.Text = this.fbdSave.ShowDialog() == DialogResult.OK ? this.fbdSave.SelectedPath : this.txtPath.Text;

        private void SplitCheckChanged(object sender, EventArgs e) => this.nudSplitInterval.Enabled = this.chkSplit.Checked;

        private void StatusIconClicked(object sender, EventArgs e)
        {
            this.ShowInTaskbar = this.Visible = !(this.trayIcon.Visible = false);
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void HandleSizeChanged(object sender, EventArgs e) => this.ShowInTaskbar = this.WindowState == FormWindowState.Minimized ? this.Visible = !(this.trayIcon.Visible = true) : this.ShowInTaskbar;

        private void RealtimeCheckChanged(object sender, EventArgs e) => this.nudFreq.Enabled = !this.chkRealtime.Checked;

        private void FrmMain_ResizeEnd(object sender, EventArgs e) => this.UpdateFormScreenInfo();

        private void FrmMain_Move(object sender, EventArgs e) => this.UpdateFormScreenInfo();

        private void UpdateFormScreenInfo() => this.formScreenInfo.Rect = ScreenInfo.NormalizeRectangle(new Rectangle(this.Location, this.Size));
    }
}
