namespace kasthack.TimeLapser.Core.Impl.Snappers.Factory;
using System;

using kasthack.TimeLapser.Core.Impl.Snappers.SDGSnapper;
using kasthack.TimeLapser.Core.Interfaces;
using kasthack.TimeLapser.Core.Models;

/// <summary>
/// Snapper factory.
/// </summary>
/// <param name="DxSnapperFactory">Snapper factory from DI.</param>
/// <param name="SdgSnapperFactory">Snapper factory from DI.</param>
internal record SnapperFactory(
    Func<DXSnapper> DxSnapperFactory,
    Func<SDGSnapper> SdgSnapperFactory) : ISnapperFactory
{
    public ISnapper GetSnapper(SnapperType type) => type switch
    {
        SnapperType.DirectX => this.DxSnapperFactory(),
        SnapperType.Legacy => this.SdgSnapperFactory(),
        _ => throw new ArgumentOutOfRangeException($"Invalid snapper: {type}"),
    };
}
