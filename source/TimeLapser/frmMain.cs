using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video.FFMPEG;
namespace TimeLapser {
    public partial class frmMain : Form {
        private bool recording = false;

        public frmMain() {
            InitializeComponent();
        }

        private async void btnGo_Click( object sender, EventArgs e ) {
            recording ^= true;
            if ( recording ) await Start();
            else await Stop();
        }

        private void SetR( bool b ) {
            btnGo.Text = b ? "Stop" : "Go";
            lblTime.Text = !b ? "Pending" : "";
        }

        private async Task Stop() {
            btnGo.Enabled = false;
            await Task.Delay( 3000 );
            btnGo.Enabled = true;
        }

        private async Task Start() {
            SetR(true);
            var outdir = txtPath.Text;
            var format = (VideoCodec) cmbFormat.SelectedItem;
            var delay = (int) nudFreq.Value;
            var screenId = ( (ScreenInfo) cmbScreen.SelectedItem ).Rect;
            var outfile = Path.Combine( outdir, DateTime.Now.ToFileTime() + ".avi" );
            var bitrate = (int)budBitrate.Value * 1024;
            var framerate = (int)nudFramerate.Value;
            await this.Record(outfile, screenId, delay, framerate, format, bitrate);
            SetR( false );
        }

        private async Task Record(string outfile, Rectangle rectangle = default( Rectangle ), int delay=500, int framerate = 30, VideoCodec format=VideoCodec.Default, int bitrate=24000) {
            var sw = new Stopwatch();
            sw.Start();
            var b = rectangle!=default(Rectangle)?rectangle: Screen.AllScreens.OrderBy(a=>a.Bounds.X).First().Bounds;
            int w = b.Width, h = b.Height, x = b.X, y = b.Y;
            var sz = new Size( w, h );
            using ( var outstream = new VideoFileWriter() ) {
                var pd = Path.GetDirectoryName( outfile );
                if ( !Directory.Exists( pd ) ) Directory.CreateDirectory( pd );
                outstream.Open(outfile, w, h, framerate, format, bitrate);
                using ( var bmp = new Bitmap( w, h ) ) {
                    using ( var gr = Graphics.FromImage( bmp ) ) {
                        while (recording)
                        {
                            lblTime.Text = sw.Elapsed.ToString("g");
                            try {
                                gr.CopyFromScreen( x, y, 0, 0, sz );
                                preprocessFrame( gr );
                                gr.Flush();
                                outstream.WriteVideoFrame( bmp );
                            }
                            catch {}
                            await Task.Delay( delay );
                        }
                    }
                }
            }
            sw.Stop();
        }

        private void preprocessFrame( Graphics gr ) {
            return;
        }

        private void Form1_Load( object sender, EventArgs e ) {
            cmbFormat.DataSource = (VideoCodec[]) Enum.GetValues( typeof( VideoCodec ) );
            var scr = Screen.AllScreens.OrderBy( a=>a.Bounds.X ).ToArray();
            
            var screens = Enumerable
                .Range(1, scr.Length)
                .Select(a => new ScreenInfo { Id = a, Name = scr[a - 1].DeviceName, Rect = scr[a-1].Bounds})
                .ToList();
            var mx = scr.Min( a => a.Bounds.X );
            var my = scr.Min( a => a.Bounds.Y );
            var w = scr.Max( a => a.Bounds.Width + a.Bounds.X ) - mx;
            var h = scr.Max(a => a.Bounds.Height + a.Bounds.Y) - my;
            screens.Add( new ScreenInfo {
                Id=screens.Count+1,
                Name = "All screens",
                Rect = new Rectangle( mx, my, w, h )
            } );
            cmbScreen.DataSource = screens;
            cmbFormat.SelectedIndex = 0;
            cmbScreen.SelectedIndex = cmbScreen.Items.Count - 1;
            txtPath.Text = Environment.GetFolderPath( Environment.SpecialFolder.MyVideos );
        }
    }

    class ScreenInfo {
        public Rectangle Rect;
        public int Id;
        public string Name;
        public override string ToString() {
            return string.Format( "{0}({1})", this.Name, this.Id );
        }
    }
}