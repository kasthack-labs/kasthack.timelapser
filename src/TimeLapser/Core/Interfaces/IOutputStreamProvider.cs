namespace kasthack.TimeLapser.Core.Interfaces;

using kasthack.TimeLapser.Core.Model;

/// <summary>
/// Output video stream provider.
/// </summary>
public interface IOutputVideoStreamProvider
{
    /// <summary>
    /// Get output video stream.
    /// </summary>
    /// <param name="settings">Record settings.</param>
    /// <returns>Writeable video file.</returns>
    IOutputVideoStream GetOutputStream(RecordSettings settings);
}
