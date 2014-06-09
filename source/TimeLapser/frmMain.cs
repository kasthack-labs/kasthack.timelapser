using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
            btnGo.Text = recording ? "Stop" : "Go";
            if ( recording ) await Start();
            else await Stop();
        }

        private async Task Stop() {
            btnGo.Enabled = false;
            await Task.Delay( 3000 );
            btnGo.Enabled = true;
        }

        private async Task Start() {
            var outdir = txtPath.Text;
            var format = (VideoCodec) cmbFormat.SelectedItem;
            var delay = (int) nudFreq.Value;
            var screenId = ( (ScreenInfo) cmbScreen.SelectedItem ).Id - 1;
            var outfile = Path.Combine( outdir, DateTime.Now.ToFileTime() + ".avi" );
            var bitrate = (int)budBitrate.Value * 1024;
            var framerate = (int)nudFramerate.Value;
            await this.Record(outfile, screenId, delay, framerate, format, bitrate);
        }

        private async Task Record(string outfile, int screenId=0, int delay=500, int framerate = 30, VideoCodec format=VideoCodec.Default, int bitrate=24000)
        {
            var screen = Screen.AllScreens.OrderBy(a=>a.Bounds.X).ToArray()[ screenId ];
            var b = screen.Bounds;
            int w = b.Width, h = b.Height, x = b.X, y = b.Y;
            var sz = new Size( w, h );
            using ( var outstream = new VideoFileWriter() ) {
                outstream.Open(outfile, w, h, framerate, format, bitrate);
                using ( var bmp = new Bitmap( w, h ) ) {
                    using ( var gr = Graphics.FromImage( bmp ) ) {
                        while ( this.recording ) {
                            gr.CopyFromScreen( x, y, 0, 0, sz );
                            gr.Flush();
                            outstream.WriteVideoFrame( bmp );
                            await Task.Delay( delay );
                        }
                    }
                }
            }
        }

        private void Form1_Load( object sender, EventArgs e ) {
            cmbFormat.DataSource = (VideoCodec[]) Enum.GetValues( typeof( VideoCodec ) );
            var scr = Screen.AllScreens.OrderBy( a=>a.Bounds.X ).ToArray();
            
            cmbScreen.DataSource = Enumerable.Range(1, scr.Length).Select(a => new ScreenInfo { Id = a, Name = scr[a - 1].DeviceName }).ToArray();
            cmbScreen.SelectedIndex = cmbFormat.SelectedIndex = 0;
        }
    }

    class ScreenInfo {
        public int Id;
        public string Name;
        public override string ToString() {
            return string.Format( "{0}({1})", this.Name, this.Id );
        }
    }
}