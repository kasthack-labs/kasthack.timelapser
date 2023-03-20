//Imported from: https://github.com/ShareX/ShareX/blob/e91fd94d2bfcecf276893fee1dd7d115939b6c0c/ShareX.ScreenCaptureLib/Enums.cs#L85
#pragma warning disable IDE0065,SA1200,SA1516,SA1201,SA1101,SA1401,SA1413,IDE0009,IDE0011,SA1503,SA1202,IDE0031,SA1208,SA1303,SA1513,RCS1059,SA1310,SA1005,SA1117,SA1515,SA1124,SA1602,SA1136

namespace kasthack.TimeLapser.Core.Impl.FFMpeg;

using System.ComponentModel;

public enum FFmpegVideoCodec
{
    [Description("H.264 / x264")]
    libx264,
    [Description("H.265 / x265")]
    libx265,
    [Description("VP8 (WebM)")]
    libvpx,
    [Description("VP9 (WebM)")]
    libvpx_vp9,
    [Description("MPEG-4 / Xvid")]
    libxvid,
    [Description("H.264 / NVENC")]
    h264_nvenc,
    [Description("HEVC / NVENC")]
    hevc_nvenc,
    [Description("H.264 / AMF")]
    h264_amf,
    [Description("HEVC / AMF")]
    hevc_amf,
    [Description("H.264 / Quick Sync")]
    h264_qsv,
    [Description("HEVC / Quick Sync")]
    hevc_qsv,
    [Description("GIF")]
    gif,
    [Description("WebP")]
    libwebp,
    [Description("APNG")]
    apng
}

public enum FFmpegAudioCodec
{
    [Description("AAC")]
    libvoaacenc,
    [Description("Opus")]
    libopus,
    [Description("Vorbis")]
    libvorbis,
    [Description("MP3")]
    libmp3lame
}

public enum FFmpegPreset
{
    [Description("Ultra fast")]
    ultrafast,
    [Description("Super fast")]
    superfast,
    [Description("Very fast")]
    veryfast,
    [Description("Faster")]
    faster,
    [Description("Fast")]
    fast,
    [Description("Medium")]
    medium,
    [Description("Slow")]
    slow,
    [Description("Slower")]
    slower,
    [Description("Very slow")]
    veryslow
}

public enum FFmpegNVENCPreset
{
    [Description("Default")]
    @default,
    [Description("High quality 2 passes")]
    slow,
    [Description("High quality 1 pass")]
    medium,
    [Description("High performance 1 pass")]
    fast,
    [Description("High performance")]
    hp,
    [Description("High quality")]
    hq,
    [Description("Bluray disk")]
    bd,
    [Description("Low latency")]
    ll,
    [Description("Low latency high quality")]
    llhq,
    [Description("Low latency high performance")]
    llhp,
    [Description("Lossless")]
    lossless,
    [Description("Lossless high performance")]
    losslesshp
}

public enum FFmpegAMFUsage
{
    [Description("Generic Transcoding")]
    transcoding = 0,
    [Description("Ultra Low Latency")]
    ultralowlatency = 1,
    [Description("Low Latency")]
    lowlatency = 2,
    [Description("Webcam")]
    webcam = 3
}

public enum FFmpegAMFQuality
{
    [Description("Prefer Speed")]
    speed = 0,
    [Description("Balanced")]
    balanced = 1,
    [Description("Prefer Quality")]
    quality = 2
}

public enum FFmpegQSVPreset
{
    [Description("Very fast")]
    veryfast,
    [Description("Faster")]
    faster,
    [Description("Fast")]
    fast,
    [Description("Medium")]
    medium,
    [Description("Slow")]
    slow,
    [Description("Slower")]
    slower,
    [Description("Very slow")]
    veryslow
}

public enum FFmpegTune
{
    film, animation, grain, stillimage, psnr, ssim, fastdecode, zerolatency
}

public enum FFmpegPaletteGenStatsMode
{
    full, diff
}

public enum FFmpegPaletteUseDither
{
    none,
    bayer,
    heckbert,
    floyd_steinberg,
    sierra2,
    sierra2_4a
}

public enum RegionCaptureMode
{
    Default,
    Annotation,
    ScreenColorPicker,
    Ruler,
    OneClick,
    Editor,
    TaskEditor
}
