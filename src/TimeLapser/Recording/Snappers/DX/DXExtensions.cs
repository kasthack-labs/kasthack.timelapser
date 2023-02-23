namespace kasthack.TimeLapser.Recording.Snappers.DX
{
    using System.Drawing;

    using SharpDX.Mathematics.Interop;

    internal static class DXExtensions
    {
        public static int Height(this RawRectangle rect) => rect.Bottom - rect.Top;

        public static int Width(this RawRectangle rect) => rect.Right - rect.Left;

        public static Rectangle ToGDIRect(this RawRectangle rect) => new(rect.Left, rect.Top, rect.Width(), rect.Height());
    }
}
