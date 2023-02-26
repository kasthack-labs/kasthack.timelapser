// #define PERF

namespace kasthack.TimeLapser.Core.Interfaces;

using kasthack.TimeLapser.Core.Model;

/// <summary>
/// Recorder interface.
/// </summary>
public interface IRecorder
{
    /// <summary>
    /// Is recording.
    /// </summary>
    bool Recording { get; }

    /// <summary>
    /// Start recording. Should throw if already recording.
    /// </summary>
    /// <param name="settings">Record settings.</param>
    void Start(RecordSettings settings);

    /// <summary>
    /// Stop recording. Should throw if not recording.
    /// </summary>
    void Stop();
}
