using Microsoft.Owin;
using NodeAssets.Compilers;
using Owin;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web.Hosting;

[assembly: OwinStartup(typeof(NodeAssets.Example.Startup))]

namespace NodeAssets.Example
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Switch this to see different modes
            var isProd = false;

            var assets = Assets
                .Initialise(config => config
                    .ConfigureCompilers(
                        compilers => compilers
                            .WithDefaultNodeConfiguration(MapPath("~/Node"), MapPath("~/Node/node.exe"))
                            .CompilerFor(FileExtensions.Js, new PassthroughCompiler())
                            // Example using .NET minifiers
                            //.CompilerFor(FileExtensions.CssMin, new CssMinifyCompiler())
                            //.CompilerFor(FileExtensions.JsMin, new JsMinifyCompiler())
                            .CompilerFor(FileExtensions.Scss, new SassCompiler(MapPath("~/bin")))
                            .OnCompilerError(e => Trace.WriteLine(e.Message))
                            .OnCompileMeasurement(m => Trace.WriteLine(string.Format("{0} {1}: {2}", m.CompileTimeMilliseconds, m.Compiler, m.File))))
                    .ConfigureSourceManager(
                        source => source.UseDefaultConfiguration(MapPath("~/built"), isProd))
                    .Cache(isProd)
                    .Compress(isProd)
                    .LiveCss(!isProd)
                    // Example Cross domain configuration
                    //.CdnBasePath("http://localhost:8001")
                    //.EnableCORSForJS(false, new[] { "http://localhost:53260" })
                    )
                .SetupCssPile(pile => pile
                    // An Example regex where you will add files ending in .scss
                    // but NOT files ending in .min.scss
                    .AddDirectory("Styles", MapPath("~/Content"), false, new Regex("(?<!.min).scss$")))
                .SetupJavascriptPile(pile =>
                {
                    pile.AddFile(MapPath("~/Scripts/jquery-2.2.3.js"));

                    if (!isProd)
                    {
                        pile.AddFile(MapPath("~/Scripts/jquery.signalR-2.2.0.js"));
                    }

                    // Enable to illustrate cross domain errors
                    //pile.AddFile(MapPath("~/Scripts/throwErrorScript.coffee"));

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
