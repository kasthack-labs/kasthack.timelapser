namespace kasthack.TimeLapser.Core.Interfaces
{
    using System;
    using System.Drawing;
    using System.Threading.Tasks;

    using kasthack.TimeLapser.Core.Models;

    /// <summary>
    /// Frame snapper.
    /// </summary>
    public interface ISnapper : IDisposable
    {
        /// <summary>
        /// Returns maximum number of threads to process <see cref="Snap">Snap</see> output. Currentlty used to indicate bitmap pool size.
        /// </summary>
        int MaxProcessingThreads { get; }

        /// <summary>
        /// Configures source rectangle.
        /// </summary>
        /// <param name="sourceRect">Screen area to capture.</param>
        void SetSource(Rectangle sourceRect);

        /// <summary>
        /// Captures frame. If timeout is specified, will wait for specified amount of time for frame to be available.
        /// </summary>
        /// <param name="timeout">Capture timeout in milliseconds.</param>
        /// <returns>Captured frame wrapped in <see cref="IPooledFrame">IPooledFrame</see>.</returns>
        Task<IPooledFrame> Snap(int timeout = 0);
    }
}
