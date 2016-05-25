using Microsoft.Owin;
using NodeAssets.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NodeAssets.AspNet
{
    public class AssetsConfiguration : IAssetsConfiguration
    {
        public IAssetsConfiguration ConfigureCompilers(Func<ICompilerConfiguration, ICompilerConfiguration> compilerManagerFunc)
        {
            CompilerConfiguration = compilerManagerFunc(new CompilerConfiguration()) as CompilerConfiguration;
            return this;
        }

        public IAssetsConfiguration ConfigureSourceManager(Func<ISourceManagerConfiguration, ISourceManagerConfiguration> sourceManagerFunc)
        {
            SourceConfiguration = sourceManagerFunc(new SourceManagerConfiguration());
            return this;
        }

        public IAssetsConfiguration Cache(bool cache = true)
        {
            UseCache = cache;
            return this;
        }

        public IAssetsConfiguration Compress(bool compress = true)
        {
            UseCompress = compress;
            return this;
        }

        public IAssetsConfiguration LiveCss(bool live = true, string signalRNamespace = "nodeassets")
        {
            IsLiveCss = live;
            Namespace = signalRNamespace;
            return this;
        }

        public IAssetsConfiguration CdnBasePath(string path)
        {
            CdnPath = path;
            return this;
        }

        public IAssetsConfiguration EnableCORSForJS(bool isWildcard, IEnumerable<string> domains = null, bool useVaryOriginHeader = true)
        {
            CORSWildcard = isWildcard;
            CORSDomains = domains == null ? null : domains.ToList();
            CORSVaryOriginHeader = useVaryOriginHeader;
            return this;
        }

        public IAssetsConfiguration SetRouteHandlerFunction(Func<string, FileInfo, IAssetsConfiguration, Func<IOwinContext, Task>> routeHandler)
        {
            RouteHandlerFunction = routeHandler;
            return this;
        }

        public bool UseCache { get; private set; }
        public bool UseCompress { get; private set; }
        public bool IsLiveCss { get; private set; }
        public string Namespace { get; private set; }
        public string CdnPath { get; private set; }
        public bool CORSEnabled { get { return CORSWildcard || (CORSDomains != null && CORSDomains.Count > 0); } }
        public bool CORSWildcard { get; private set; }
        public bool CORSVaryOriginHeader { get; private set; }
        public List<string> CORSDomains { get; private set; }
        public CompilerConfiguration CompilerConfiguration { get; private set; }
        public ISourceManagerConfiguration SourceConfiguration { get; private set; }
        public Func<string, FileInfo, IAssetsConfiguration, Func<IOwinContext, Task>> RouteHandlerFunction { get; private set; }
    }
}
