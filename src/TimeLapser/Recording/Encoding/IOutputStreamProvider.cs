namespace kasthack.TimeLapser.Recording.Encoding;

using Accord.Video.FFMPEG;

using kasthack.TimeLapser.Recording.Models;

public interface IOutputStreamProvider
{
    VideoFileWriter GetOutputStream(RecordSettings settings);
}
