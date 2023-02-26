namespace kasthack.TimeLapser.Core.Interfaces;

using System;
using System.Drawing;

/// <summary>
/// Output video stream.
/// </summary>
public interface IOutputVideoStream : IDisposable
{
    /// <summary>
    /// Write video frame.
    /// </summary>
    /// <param name="bitmap">Frame.</param>
    void WriteVideoFrame(Bitmap bitmap);
}
