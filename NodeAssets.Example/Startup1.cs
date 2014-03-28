using Microsoft.Owin;
using Owin;
using System.Text.RegularExpressions;
using System.Web.Hosting;

[assembly: OwinStartup(typeof(NodeAssets.Example.Startup1))]

namespace NodeAssets.Example
{
    public class Startup1
    {
        public void Configuration(IAppBuilder app)
        {
            // Switch this to see different modes
            var isProd = false;

            var assets = Assets
                .Initialise(config => config
                    .ConfigureCompilers(
                        compilers => compilers.WithDefaultNodeConfiguration(MapPath("~/Node"), MapPath("~/Node/node.exe")))
                    .ConfigureSourceManager(
                        source => source.UseDefaultConfiguration(MapPath("~/built"), isProd))
                    .Cache(isProd)
                    .Compress(isProd)
                    .LiveCss(!isProd))
                .SetupCssPile(pile => pile
                    // An Example regex where you will add files ending in .css
                    // but NOT files ending in .min.css
                    .AddDirectory("Styles", MapPath("~/Content"), true, new Regex("(?<!.min).css$")))
                .SetupJavascriptPile(pile =>
                {
                    pile.AddFile(MapPath("~/Scripts/jquery-2.1.0.js"));

                    if (!isProd)
                    {
                        pile.AddFile(MapPath("~/Scripts/jquery.signalR-2.0.3.js"));
                    }

                    return pile;
                });

            app.MapNodeAssets(assets);
        }

        private string MapPath(string basePath)
        {
            return HostingEnvironment.MapPath(basePath);
        }
    }
}
