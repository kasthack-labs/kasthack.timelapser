namespace kasthack.TimeLapser.Core.Impl.Recorders;
using System;

using kasthack.TimeLapser.Core.Interfaces;
using kasthack.TimeLapser.Core.Model;

internal class FFmpegRecorder : IRecorder
{
    public bool Recording { get; }

    public void Start(RecordSettings settings) => throw new NotImplementedException();
    public void Stop() => throw new NotImplementedException();
}
