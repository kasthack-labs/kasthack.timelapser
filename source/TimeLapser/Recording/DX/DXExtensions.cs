using System;
using System.Drawing;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System.Drawing.Imaging;
using SharpDX;
using SharpDX.Direct3D11;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace kasthack.TimeLapser {
    internal static class DXExtensions {
        public static int Height(this RawRectangle rect) => rect.Bottom - rect.Top;
        public static int Width(this RawRectangle rect) => rect.Right - rect.Left;
        public static Rectangle ToGDIRect(this RawRectangle rect) => new Rectangle(rect.Left, rect.Top, rect.Width(), rect.Height());
    }
}
