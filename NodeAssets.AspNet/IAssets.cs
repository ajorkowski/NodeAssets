using Microsoft.AspNet.SignalR;
using NodeAssets.Core;
using Owin;
using System;

namespace NodeAssets
{
    public interface IAssets
    {
        IAssets SetupJavascriptPile(Func<IPile, IPile> pileFunc);
        IAssets SetupCssPile(Func<IPile, IPile> pileFunc);

        string FindJsAssets(bool includeGlobal, params string[] piles);
        string FindCssAssets(bool includeGlobal, params string[] piles);

        IAppBuilder MapNodeAssets(IAppBuilder appBuilder, ConnectionConfiguration configuration = null);
    }
}
