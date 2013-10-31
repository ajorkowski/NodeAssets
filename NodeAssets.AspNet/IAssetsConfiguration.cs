using Microsoft.Owin;
using NodeAssets.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Routing;

namespace NodeAssets.AspNet
{
    public interface IAssetsConfiguration
    {
        IAssetsConfiguration ConfigureCompilers(Func<ICompilerConfiguration, ICompilerConfiguration> compilerManagerFunc);
        IAssetsConfiguration ConfigureSourceManager(Func<ISourceManagerConfiguration, ISourceManagerConfiguration> sourceManagerFunc);
        IAssetsConfiguration LiveCss(bool live = true, string signalRNamespace = "nodeassets");
        IAssetsConfiguration Cache(bool cache = true);
        IAssetsConfiguration Compress(bool compress = true);
        IAssetsConfiguration SetRouteHandlerFunction(Func<string, FileInfo, IAssetsConfiguration, Func<IOwinContext, Task>> routeHandler);

        ICompilerConfiguration CompilerConfiguration { get; }
        ISourceManagerConfiguration SourceConfiguration { get; }
        bool IsLiveCss { get; }
        string Namespace { get; }
        bool UseCache { get; }
        bool UseCompress { get; }

        Func<string, FileInfo, IAssetsConfiguration, Func<IOwinContext, Task>> RouteHandlerFunction { get; }
    }
}
