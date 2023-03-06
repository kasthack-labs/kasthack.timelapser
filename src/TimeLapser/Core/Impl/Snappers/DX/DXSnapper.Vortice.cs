#define ParallelSnap
namespace kasthack.TimeLapser
{
    using System.Collections.Generic;

    using kasthack.TimeLapser.Core.Impl.Snappers.DX;
    using kasthack.TimeLapser.Core.Impl.Util;
    using kasthack.TimeLapser.Core.Interfaces;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Snapper, utilizing DirectX.
    /// </summary>
    internal partial class DXSnapper : DisposableBase, ISnapper
    {
        private (int AdapterIndex, int OutputIndex)[] GetCapturedOutputs()
        {
            this.logger.LogTrace("Getting captured outputs");

            var ret = new List<(int AdapterIndex, int OutputIndex)>(6); // most cases
            using var factory = Vortice.DXGI.DXGI.CreateDXGIFactory2<Vortice.DXGI.IDXGIFactory7>(false);
            for (var adapterIndex = 0; factory.EnumAdapters1(adapterIndex, out var adapter).Success; adapterIndex++)
            {
                using (adapter)
                {
                    for (var outputIndex = 0; adapter.EnumOutputs(outputIndex, out var output).Success; outputIndex++)
                    {
                        using (output)
                        {
                            if (output.Description.DesktopCoordinates.ToGDIRect().IntersectsWith(this.sourceRect.Value))
                            {
                                ret.Add((adapterIndex, outputIndex));
                            }
                        }
                    }
                }
            }

            ret.Reverse();

            this.logger.LogTrace("Got {count} captured outputs", ret.Count);
            return ret.ToArray();
        }
    }
}
