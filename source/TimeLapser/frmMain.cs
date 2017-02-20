#define TESTING
using System;
using System.IO;
using System.Windows.Forms;
using Accord.Video.FFMPEG;

namespace TimeLapser {
    public partial class frmMain : Form {
        private readonly Recorder _recorder = new Recorder();
        private RecordSettings _settings;

        public frmMain() => InitializeComponent();

        private void btnGo_Click( object sender, EventArgs e ) {
            if ( _recorder.Recording ) {
                _recorder.Stop();
                SetR( false );
            }
            else {
                SetR(true);
                _settings = new RecordSettings(
                    outputPath: txtPath.Text,
                    captureRectangle: ( (ScreenInfo)cmbScreen.SelectedItem ).Rect,
                    fps: (int)nudFramerate.Value,
                    interval: (int)nudFreq.Value,
                    codec: (VideoCodec)cmbFormat.SelectedItem,
                    bitrate: (int)budBitrate.Value << 20,
                    splitInterval: chkSplit.Checked?(double?)nudSplitInterval.Value :null,
                    onFrameWritten: (a)=>BeginInvoke( (Action)(()=>lblTime.Text = $"Elapsed :{a.ToString("g") }") ),
                    realtime: chkRealtime.Checked
                );
                _recorder.Start(
                    _settings
                );
            }
        }

        private void SetR( bool recordRunning ) {
            btnGo.Text = recordRunning ? "Stop" : "Go";
            lblTime.Text = !recordRunning ? "Pending" : "";
            txtPath.Enabled
                = btnbrs.Enabled
                = nudFreq.Enabled
                = nudFramerate.Enabled
                = cmbFormat.Enabled
                = budBitrate.Enabled
                = cmbScreen.Enabled
                = chkSplit.Enabled
                = nudSplitInterval.Enabled
                = chkRealtime.Enabled
                = !recordRunning;
        }
        private void Form1_Load( object sender, EventArgs e ) {
            cmbFormat.DataSource = Enum.GetValues( typeof( VideoCodec ) ) as VideoCodec[];
            cmbScreen.DataSource = Recorder.GetScreenInfos();
            cmbFormat.SelectedIndex = 0;
            cmbScreen.SelectedIndex = cmbScreen.Items.Count - 1;
            txtPath.Text = Environment.GetFolderPath( Environment.SpecialFolder.MyVideos );
#if TESTING
            txtPath.Text = Path.Combine(txtPath.Text, "dbg_scr");
            chkSplit.Checked = true;
            nudSplitInterval.Value = 1;
            chkRealtime.Checked = true;
            cmbScreen.SelectedIndex = 1;
#endif
        }
        private void btnbrs_Click( object sender, EventArgs e ) {
            if ( fbdSave.ShowDialog() == DialogResult.OK ) txtPath.Text = fbdSave.SelectedPath;
        }
        private void checkBox1_CheckedChanged( object sender, EventArgs e ) => nudSplitInterval.Enabled = chkSplit.Checked;

        private void nicon_DoubleClick( object sender, EventArgs e ) {
            ShowInTaskbar = Visible = !( nicon.Visible = false);
            BringToFront();
            Activate();
            Show();
        }

        private void frmMain_SizeChanged( object sender, EventArgs e ) {
            if( WindowState == FormWindowState.Minimized ){
                ShowInTaskbar = Visible = !( nicon.Visible = true);
            }
        }

        private void chkRealtime_CheckedChanged(object sender, EventArgs e) => nudFreq.Enabled = !chkRealtime.Checked;
    }
}