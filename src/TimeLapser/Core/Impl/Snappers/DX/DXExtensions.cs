namespace kasthack.TimeLapser.Core.Impl.Snappers.DX
{
    using SdgRectangle = System.Drawing.Rectangle;
    using DxRectangle = Vortice.RawRect; //SharpDX.Mathematics.Interop.RawRectangle;

    /// <summary>
    /// Extension methods for SharpDX.
    /// </summary>
    internal static class DXExtensions
    {
        public static int Height(this DxRectangle rect) => rect.Bottom - rect.Top;

        public static int Width(this DxRectangle rect) => rect.Right - rect.Left;

        public static SdgRectangle ToGDIRect(this DxRectangle rect) => new(rect.Left, rect.Top, rect.Width(), rect.Height());
    }
}
