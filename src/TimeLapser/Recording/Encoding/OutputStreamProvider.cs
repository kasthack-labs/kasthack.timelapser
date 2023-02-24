namespace kasthack.TimeLapser.Recording.Encoding;
using System;
using System.IO;

using Accord.Video.FFMPEG;

using kasthack.TimeLapser.Recording.Models;

using Microsoft.Extensions.Logging;

internal record OutputStreamProvider(ILogger<OutputStreamProvider> Logger) : IOutputStreamProvider
{
    // create output file with FFMPEG
    public VideoFileWriter GetOutputStream(RecordSettings settings)
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
            return outstream;
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

}
