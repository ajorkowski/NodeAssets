using System;
using System.Web.Routing;
using NodeAssets.Core;

namespace NodeAssets.AspNet
{
    public interface IAssets
    {
        IAssets SetupJavascriptPile(Func<IPile, IPile> pileFunc);
        IAssets SetupCssPile(Func<IPile, IPile> pileFunc);

        string FindJsAssets(bool includeGlobal, params string[] piles);
        string FindCssAssets(bool includeGlobal, params string[] piles);

        void PrepareRoutes(RouteCollection routes);
    }
}
