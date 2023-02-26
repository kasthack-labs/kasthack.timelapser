namespace kasthack.TimeLapser.Core.Interfaces;

using kasthack.TimeLapser.Core.Models;

using System.Collections.Generic;

/// <summary>
/// Screen info provider.
/// </summary>
public interface IScreenInfoProvider
{
    /// <summary>
    /// Get display infos.
    /// </summary>
    IList<ScreenInfo> GetScreenInfos();
}
