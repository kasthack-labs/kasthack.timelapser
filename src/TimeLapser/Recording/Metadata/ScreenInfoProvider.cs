namespace kasthack.TimeLapser.Recording.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    using kasthack.TimeLapser.Recording.Models;

    using Microsoft.Extensions.Logging;

    public record ScreenInfoProvider(ILogger<ScreenInfoProvider> Logger) : IScreenInfoProvider
    {
        public IList<ScreenInfo> GetScreenInfos()
        {
            this.Logger.LogTrace("Getting screen info");
            try
            {
                var screens = Screen.AllScreens.OrderBy(screen => screen.Bounds.X).ToArray();
                var screenInfos = Enumerable.Range(0, screens.Length)
                            .Select(screenId => new ScreenInfo { Id = screenId + 1, Name = screens[screenId].DeviceName, Rectangle = screens[screenId].Bounds, })
                            .ToList();

                if (screenInfos.Count > 1)
                {
                    var leftmost = screens.Min(screen => screen.Bounds.X);
                    var topmost = screens.Min(screen => screen.Bounds.Y);
                    var width = screens.Max(screen => screen.Bounds.Width + screen.Bounds.X) - leftmost;
                    var height = screens.Max(screen => screen.Bounds.Height + screen.Bounds.Y) - topmost;
                    var allScreens = new ScreenInfo { Id = screenInfos.Count + 1, Name = Locale.Locale.AllScreens, Rectangle = new Rectangle(leftmost, topmost, width, height) };
                    screenInfos.Add(allScreens);
                    this.Logger.LogTrace("Found multiple screens, added all screens input: {screenInfo}", allScreens);
                }

                this.Logger.LogDebug("Got screen info. Found {screenCount} screens", screens.Length);
                return screenInfos.ToList();
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to get screen info");
                throw;
            }
        }
    }
}
