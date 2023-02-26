namespace kasthack.TimeLapser.Core.Interfaces;

using kasthack.TimeLapser.Core.Models;

/// <summary>
/// Snapper factory.
/// </summary>
public interface ISnapperFactory
{
    /// <summary>
    /// Get snapper.
    /// </summary>
    /// <param name="type">Snapper type.</param>
    /// <returns>Snapper.</returns>
    ISnapper GetSnapper(SnapperType type);
}
