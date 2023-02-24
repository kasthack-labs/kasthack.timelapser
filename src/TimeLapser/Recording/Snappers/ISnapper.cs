namespace kasthack.TimeLapser.Recording.Snappers
{
    using System;
    using System.Drawing;
    using System.Threading.Tasks;

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
        /// Caputes a frame. Return value must be assumed to be short-lived / pooled.
        /// </summary>
        /// <param name="timeout">Capture timeout in milliseconds.</param>
        /// <returns>Captured frame.</returns>
        Task<Bitmap> Snap(int timeout = 0);
    }
}
