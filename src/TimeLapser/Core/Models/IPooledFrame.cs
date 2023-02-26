namespace kasthack.TimeLapser.Core.Models;
using System;
using System.Drawing;

/// <summary>
/// Pooled frame.
/// </summary>
public interface IPooledFrame : IDisposable
{
    /// <summary>
    /// Frame value. Should not be disposed directly, as it is pooled. Dispose parent object instead.
    /// </summary>
    Bitmap Value { get; }
}
