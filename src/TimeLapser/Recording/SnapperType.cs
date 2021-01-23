namespace kasthack.TimeLapser
{
    using System;
    using System.Drawing;

    using Accord.Video.FFMPEG;

    public enum SnapperType
    {
        /// <summary>
        /// DirectX snapper.
        /// </summary>
        DirectX,

        /// <summary>
        /// System.Drawing snapper.
        /// </summary>
        Legacy,
    }
}
