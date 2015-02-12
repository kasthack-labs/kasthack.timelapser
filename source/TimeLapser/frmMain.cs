using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using AForge.Video.FFMPEG;
using Timer = System.Timers.Timer;

namespace TimeLapser {
    public partial class frmMain : Form {
        private readonly Recorder _recorder = new Recorder();
        private RecordSettings _settings;

        public frmMain() { InitializeComponent(); }

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
                    splitInterval: chkSplit.Checked?(int?)(int)nudSplitInterval.Value :null,
                    onFrameWritten: (a)=>BeginInvoke( (Action)(()=>lblTime.Text = string.Format( "Elapsed :{0}", a.ToString("g") )) )
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
                = !recordRunning;
        }
        private void Form1_Load( object sender, EventArgs e ) {
            cmbFormat.DataSource = Enum.GetValues( typeof( VideoCodec ) ) as VideoCodec[];
            cmbScreen.DataSource = Recorder.GetScreenInfos();
            cmbFormat.SelectedIndex = 0;
            cmbScreen.SelectedIndex = cmbScreen.Items.Count - 1;
            txtPath.Text = Environment.GetFolderPath( Environment.SpecialFolder.MyVideos );
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
    }

    public class Recorder {
        public bool Recording { get; private set; } = false;
        private ManualResetEventSlim StopWaiter = new ManualResetEventSlim();
        private Stopwatch _stopwatch;
        public event EventHandler FrameWritten;

        public void Start( RecordSettings settings ) {
            var timer = new Timer();
            if (Recording)
                throw new InvalidOperationException("Recording is already started");
            Recording = true;
            new Thread( () => StartInternal(settings) ).Start();
        }

        private void StartInternal( RecordSettings settings ) {
            try {
                _stopwatch = _stopwatch ?? new Stopwatch();
                _stopwatch.Reset();
                _stopwatch.Start();
                StopWaiter.Reset();
                if ( !Directory.Exists( settings.OutputPath ) ) Directory.CreateDirectory( settings.OutputPath );
                while ( Recording ) {
                    var outfile = Path.Combine( settings.OutputPath, DateTime.Now.ToFileTime().ToString() + ".avi" );
                    using (var outstream = new VideoFileWriter()) {
                        var sourceRect = settings.CaptureRectangle;
                        var w = sourceRect.Width;
                        var h = sourceRect.Height;

                        outstream.Open(
                            outfile,
                            w,
                            h,
                            settings.Fps,
                            settings.Codec,
                            settings.Bitrate );

                        var mpf = settings.SplitInterval * 60000 / settings.Interval;
                        using ( var bmp = new Bitmap( w, h ) ) {
                            using ( var gr = Graphics.FromImage( bmp ) ) {
                                for ( var i = 0; (mpf==null||i < mpf) && Recording; i++ ) {
                                    try {
                                        gr.CopyFromScreen( sourceRect.X, sourceRect.Y, 0, 0, sourceRect.Size );
                                        PreprocessFrame( gr, settings );
                                        gr.Flush();
                                        outstream.WriteVideoFrame( bmp );
                                        settings.OnFrameWritten?.Invoke( _stopwatch.Elapsed );
                                    }
                                    catch { }
                                    Thread.Sleep( settings.Interval );
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex) {
                //global
            }
            finally {
                _stopwatch?.Stop();
                StopWaiter.Set();
            }
        }

        private void PreprocessFrame( Graphics gr, RecordSettings settings ) {
            if ( !settings.Private ) return;
            gr.Clear( Color.Black );
            gr.Flush();
        }

        public void Stop() {
            Recording = false;
            StopWaiter.Wait();
        }

        public static IEnumerable<ScreenInfo> GetScreenInfos() {
            var scr = Screen.AllScreens.OrderBy( a => a.Bounds.X ).ToArray();
            var screens = Enumerable.Range( 1, scr.Length )
                        .Select( a => new ScreenInfo { Id = a, Name = scr[ a - 1 ].DeviceName, Rect = scr[ a - 1 ].Bounds } )
                        .ToList();
            var mx = scr.Min( a => a.Bounds.X );
            var my = scr.Min( a => a.Bounds.Y );
            var w = scr.Max( a => a.Bounds.Width + a.Bounds.X ) - mx;
            var h = scr.Max( a => a.Bounds.Height + a.Bounds.Y ) - my;
            screens.Add( new ScreenInfo { Id = screens.Count + 1, Name = "All screens", Rect = new Rectangle( mx, my, w, h ) } );
            return screens.ToArray();
        }
    }

    public class RecordSettings {
        public RecordSettings( string outputPath, Rectangle captureRectangle, int fps = 30, int interval = 500, VideoCodec codec = VideoCodec.MPEG4, int bitrate = 20, int? splitInterval = null, Action<TimeSpan> onFrameWritten = null ) {
            OutputPath = outputPath;
            CaptureRectangle = captureRectangle;
            OnFrameWritten = onFrameWritten;
            Interval = interval;
            Fps = fps;
            Codec = codec;
            Bitrate = bitrate;
            SplitInterval = splitInterval;
            Private = false;
        }
        public int? SplitInterval { get; }
        public int Bitrate { get; }
        public VideoCodec Codec { get; }
        public Rectangle CaptureRectangle { get; }
        public int Interval { get; }
        public int Fps { get; }
        public string OutputPath { get; }
        public bool Private { get; set; }
        public Action<TimeSpan> OnFrameWritten { get; }
    }

    public class ScreenInfo {
        public Rectangle Rect;
        public int Id;
        public string Name;
        public override string ToString() => string.Format( "{0}({1})", Name, Id );
    }
}