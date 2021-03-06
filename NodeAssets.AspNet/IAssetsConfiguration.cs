﻿using Microsoft.Owin;
using NodeAssets.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NodeAssets.AspNet
{
    public interface IAssetsConfiguration
    {
        IAssetsConfiguration ConfigureCompilers(Func<ICompilerConfiguration, ICompilerConfiguration> compilerManagerFunc);
        IAssetsConfiguration ConfigureSourceManager(Func<ISourceManagerConfiguration, ISourceManagerConfiguration> sourceManagerFunc);
        IAssetsConfiguration LiveCss(bool live = true, string signalRNamespace = "nodeassets");
        IAssetsConfiguration Cache(bool cache = true);
        IAssetsConfiguration Compress(bool compress = true);
        IAssetsConfiguration CdnBasePath(string path);
        IAssetsConfiguration EnableCORSForJS(bool isWildcard, IEnumerable<string> domains = null, bool useVaryHeader = false);
        IAssetsConfiguration SetRouteHandlerFunction(Func<string, FileInfo, IAssetsConfiguration, Func<IOwinContext, Task>> routeHandler);
    }
}
