using System;
using System.Drawing;
using Accord.Video.FFMPEG;

namespace TimeLapser {

    public class RecordSettings {
        public RecordSettings(){}
        public RecordSettings( string outputPath, Rectangle captureRectangle, int fps = 30, int interval = 500, VideoCodec codec = VideoCodec.MPEG4, int bitrate = 20, double? splitInterval = null, Action<TimeSpan> onFrameWritten = null, bool realtime=false ) {
            OutputPath = outputPath;
            CaptureRectangle = captureRectangle;
            OnFrameWritten = onFrameWritten;
            Interval = interval;
            Fps = fps;
            Codec = codec;
            Bitrate = bitrate;
            SplitInterval = splitInterval;
            Private = false;
            Realtime = realtime;
        }
        /// <summary>
        /// Split every N input seconds
        /// </summary>
        public double? SplitInterval { get; }
        /// <summary>
        /// Output bitrate
        /// </summary>
        public int Bitrate { get; }
        /// <summary>
        /// Output codec
        /// </summary>
        public VideoCodec Codec { get; }
        /// <summary>
        /// Capture rectangle
        /// </summary>
        public Rectangle CaptureRectangle { get; }
        /// <summary>
        /// Snap every N seconds
        /// </summary>
        public int Interval { get; }
        /// <summary>
        /// Output framerate
        /// </summary>
        public int Fps { get; }
        /// <summary>
        /// Output directory
        /// </summary>
        public string OutputPath { get; }
        /// <summary>
        /// I dunno
        /// </summary>
        public bool Private { get; set; }
        public Action<TimeSpan> OnFrameWritten { get; }
        /// <summary>
        /// Ignore interval and snap in realtime
        /// </summary>
        public bool Realtime { get; }
    }
}