// #define PERF

namespace kasthack.TimeLapser.Recording.Recorder;

using kasthack.TimeLapser.Recording.Models;

public interface IRecorder
{
    bool Recording { get; }

    void Start(RecordSettings settings);

    void Stop();
}
