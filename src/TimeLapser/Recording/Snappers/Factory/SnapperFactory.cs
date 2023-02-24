namespace kasthack.TimeLapser.Recording.Snappers.Factory;
using System;

using kasthack.TimeLapser.Recording.Models;

internal record SnapperFactory(
    Func<DXSnapper> DxSnapperFactory,
    Func<SDGSnapper.SDGSnapper> SdgSnapperFactory) : ISnapperFactory
{
    public ISnapper GetSnapper(SnapperType type) => type switch
    {
        SnapperType.DirectX => this.DxSnapperFactory(),
        SnapperType.Legacy => this.SdgSnapperFactory(),
        _ => throw new ArgumentOutOfRangeException($"Invalid snapper: {type}"),
    };
}
