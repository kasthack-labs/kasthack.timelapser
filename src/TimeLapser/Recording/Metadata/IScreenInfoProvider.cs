namespace kasthack.TimeLapser.Recording.Metadata;

using kasthack.TimeLapser.Recording.Models;
using System.Collections.Generic;

public interface IScreenInfoProvider
{
    IList<ScreenInfo> GetScreenInfos();
}
