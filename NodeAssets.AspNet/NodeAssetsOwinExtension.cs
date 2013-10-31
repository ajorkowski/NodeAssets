using Microsoft.AspNet.SignalR;
using NodeAssets;

namespace Owin
{
    public static class NodeAssetsOwinExtension
    {
        public static IAppBuilder MapNodeAssets(this IAppBuilder builder, IAssets assets, ConnectionConfiguration liveCssConfig = null)
        {
            return assets.MapNodeAssets(builder, liveCssConfig);
        }
    }
}
