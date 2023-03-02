namespace kasthack.TimeLapser.Core.Model
{
    using System;
    using System.Drawing;

    using Accord.Video.FFMPEG;

    using kasthack.TimeLapser.Core.Models;
    /// <summary>
    /// Record settings.
    /// </summary>
    /// <param name="OutputPath"> Gets output directory. </param>
    /// <param name="CaptureRectangle"> Gets capture rectangle. </param>
    /// <param name="SplitInterval"> Gets split every N input seconds. </param>
    /// <param name="Bitrate"> Gets output bitrate. </param>
    /// <param name="Codec"> Gets output codec. </param>
    /// <param name="Interval"> Gets interval between snapshots in seconds. </param>
    /// <param name="Fps"> Gets output framerate. </param>
    /// <param name="Realtime"> Gets a value indicating whether ignore interval and snap in realtime. </param>
    /// <param name="SnapperType">Snapper to use.</param>
    public record RecordSettings(
        string OutputPath,
        Rectangle CaptureRectangle,
        double? SplitInterval = null,
        int Bitrate = 20,
        VideoCodec Codec = VideoCodec.MPEG4,
        int Interval = 500,
        int Fps = 30,
        bool Realtime = false,
        SnapperType SnapperType = SnapperType.DirectX)
    {
    }
}
