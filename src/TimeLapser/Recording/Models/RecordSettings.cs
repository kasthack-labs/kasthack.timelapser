namespace kasthack.TimeLapser.Recording.Models
{
    using System;
    using System.Drawing;

    using Accord.Video.FFMPEG;

    public class RecordSettings
    {
        public RecordSettings()
        {
        }

        public RecordSettings(
            string outputPath,
            Rectangle captureRectangle,
            int fps = 30,
            int interval = 500,
            VideoCodec codec = VideoCodec.MPEG4,
            int bitrate = 20,
            double? splitInterval = null,
            Action<TimeSpan> onFrameWritten = null,
            bool realtime = false,
            SnapperType snapperType = SnapperType.DirectX)
        {
            this.OutputPath = outputPath;
            this.CaptureRectangle = captureRectangle;
            this.OnFrameWritten = onFrameWritten;
            this.Interval = interval;
            this.Fps = fps;
            this.Codec = codec;
            this.Bitrate = bitrate;
            this.SplitInterval = splitInterval;
            this.Private = false;
            this.Realtime = realtime;
            this.SnapperType = snapperType;
        }

        /// <summary>
        /// Gets split every N input seconds.
        /// </summary>
        public double? SplitInterval { get; }

        /// <summary>
        /// Gets output bitrate.
        /// </summary>
        public int Bitrate { get; }

        /// <summary>
        /// Gets output codec.
        /// </summary>
        public VideoCodec Codec { get; }

        /// <summary>
        /// Gets capture rectangle.
        /// </summary>
        public Rectangle CaptureRectangle { get; }

        /// <summary>
        /// Gets interval between snapshots in seconds.
        /// </summary>
        public int Interval { get; }

        /// <summary>
        /// Gets output framerate.
        /// </summary>
        public int Fps { get; }

        /// <summary>
        /// Gets output directory.
        /// </summary>
        public string OutputPath { get; }

        /// <summary>
        /// Gets or sets a value indicating whether i dunno.
        /// </summary>
        public bool Private { get; set; }

        public Action<TimeSpan> OnFrameWritten { get; }

        /// <summary>
        /// Gets a value indicating whether ignore interval and snap in realtime.
        /// </summary>
        public bool Realtime { get; }

        public SnapperType SnapperType { get; }
    }
}
