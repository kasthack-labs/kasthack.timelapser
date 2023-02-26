namespace kasthack.TimeLapser.Core.Impl.Encoding;
using System;
using System.Drawing;
using System.IO;

using Accord.Video.FFMPEG;

using kasthack.TimeLapser.Core.Interfaces;
using kasthack.TimeLapser.Core.Model;

using Microsoft.Extensions.Logging;

/// <summary>
/// FFMPEG/Accord output stream provider.
/// </summary>
/// <param name="Logger">Logger.</param>
internal record FfmpegOutputStreamProvider(ILogger<FfmpegOutputStreamProvider> Logger) : IOutputVideoStreamProvider
{
    // create output file with FFMPEG
    public IOutputVideoStream GetOutputStream(RecordSettings settings)
    {
        var outputFileName = $"timelapser-capture-{DateTimeOffset.Now:yyyy-MM-dd_HH-mm}.avi";
        var outfile = Path.Combine(settings.OutputPath, outputFileName);

        this.Logger.LogDebug(
                "Creating {outputFile}, resolution: {width}x{height}, FPS: {fps}, codec: {codec}, bitrate: {bitrate}",
                outfile,
                settings.CaptureRectangle.Width,
                settings.CaptureRectangle.Height,
                settings.Fps,
                settings.Codec,
                settings.Bitrate);

        if (!Directory.Exists(settings.OutputPath))
        {
            try
            {
                Directory.CreateDirectory(settings.OutputPath);
                this.Logger.LogInformation("Directory {outputPath} not found, created", settings.OutputPath);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to create directory {outputPath}", settings.OutputPath);
                throw;
            }
        }

        try
        {
            var outstream = new VideoFileWriter();
            outstream.Open(outfile, settings.CaptureRectangle.Width, settings.CaptureRectangle.Height, settings.Fps, settings.Codec, settings.Bitrate);
            this.Logger.LogInformation(
                "Created output file {outputFile}, resolution: {width}x{height}, FPS: {fps}, codec: {codec}, bitrate: {bitrate}",
                outfile,
                settings.CaptureRectangle.Width,
                settings.CaptureRectangle.Height,
                settings.Fps,
                settings.Codec,
                settings.Bitrate);
            return new FFmpegOutputVideoStream(outstream, this.Logger);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(
                ex,
                "Failed to create output file {outputFile}, resolution: {width}x{height}, FPS: {fps}, codec: {codec}, bitrate: {bitrate}",
                outfile,
                settings.CaptureRectangle.Width,
                settings.CaptureRectangle.Height,
                settings.Fps,
                settings.Codec,
                settings.Bitrate);
            throw;
        }
    }

    /// <summary>
    /// FFMPEG/Accord output stream.
    /// </summary>
    /// <param name="Writer">Underlying writer.</param>
    /// <param name="Logger">Logger.</param>
    private record FFmpegOutputVideoStream(VideoFileWriter Writer, ILogger Logger) : IOutputVideoStream
    {
        /// <inheritdoc/>
        public void Dispose() => this.Writer.Dispose();

        /// <inheritdoc/>
        public void WriteVideoFrame(Bitmap bitmap)
        {
            try
            {
                this.Logger.LogTrace("Writing frame");
                this.Writer.WriteVideoFrame(bitmap);
                this.Logger.LogTrace("Wrote frame");
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to write video frame");
                throw;
            }
        }
    }
}
