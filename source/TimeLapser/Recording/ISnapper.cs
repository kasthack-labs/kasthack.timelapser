using System;
using System.Drawing;

namespace TimeLapser {
    interface ISnapper : IDisposable
    {
        void SetSource(Rectangle sourceRect);
        /*
         * may return reusable objects!
         * don't rely on it and copy the data
         */
        Bitmap Snap(int timeout = 0);
    }
}