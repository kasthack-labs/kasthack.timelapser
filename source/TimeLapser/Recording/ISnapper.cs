using System;
using System.Drawing;

namespace TimeLapser {
    interface ISnapper : IDisposable
    {
        void SetSource(Rectangle sourceRect);
        /*
         * _reusable_ bitmap
         * implementation must dispose it
         */
        Bitmap Snap(int timeout = 0);
        int MaxProcessingThreads { get; }
    }
}