namespace kasthack.TimeLapser
{
    using System;
    using System.Drawing;
    using System.Threading.Tasks;

    internal interface ISnapper : IDisposable
    {
        int MaxProcessingThreads { get; }

        void SetSource(Rectangle sourceRect);

        /*
         * _reusable_ bitmap
         * implementation must dispose it
         */
        Task<Bitmap> Snap(int timeout = 0);
    }
}
