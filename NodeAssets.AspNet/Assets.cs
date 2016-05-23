using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using NodeAssets.AspNet;
using NodeAssets.AspNet.Routes;
using NodeAssets.AspNet.Scripts;
using NodeAssets.Core;
using NodeAssets.Core.SourceManager;
using Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NodeAssets
{
    public class Assets : IAssets
    {
        private static IAssets _assets;
        public static IAssets Initialise(Func<IAssetsConfiguration, IAssetsConfiguration> configFunc)
        {
            var config = new AssetsConfiguration();
            _assets = new Assets(configFunc(config) as AssetsConfiguration);
            return _assets;
        }

        public static string Js(bool includeGlobal, params string[] otherAssets)
        {
            if(_assets == null) { throw new InvalidOperationException("You must initialise your assests first"); }

            return _assets.FindJsAssets(includeGlobal, otherAssets);
        }

        public static string Css(bool includeGlobal, params string[] otherAssets)
        {
            if (_assets == null) { throw new InvalidOperationException("You must initialise your assests first"); }

            return _assets.FindCssAssets(includeGlobal, otherAssets);
        }

        private const string Global = "global";

        private readonly AssetsConfiguration _config;
        private readonly ISourceManager _jsManager;
        private readonly ISourceManager _cssManager;
        private readonly Func<string, FileInfo, Func<IOwinContext, Task>> _routeHandler;

        private IPile _jsPile;
        private IPile _cssPile;

        public Assets(AssetsConfiguration config)
        {
            if (config.CompilerConfiguration == null) { throw new ArgumentException("The compilers were not configured"); }
            if (config.SourceConfiguration == null) { throw new ArgumentException("The sources were not configured"); }

            _config = config;
            _jsManager = config.SourceConfiguration.GetSourceManager(".js");
            _cssManager = config.SourceConfiguration.GetSourceManager(".css");

            if(config.RouteHandlerFunction == null)
            {
                _routeHandler = (pile, file) => new DefaultHandler(file, config).Execute;
            }
            else
            {
                _routeHandler = (pile, file) => config.RouteHandlerFunction(pile, file, config);
            }
        }

        public IAssets SetupJavascriptPile(Func<IPile, IPile> pileFunc)
        {
            if (_jsPile != null) { throw new InvalidOperationException("The javascript pile has already been initialised"); }

            // Watch JS files only so that we can just press 'refresh' on the browser
            // whenever we make a js change
            _jsPile = pileFunc(new Pile(_config.IsLiveCss));
            if (_jsPile != null)
            {
                _jsManager.SetPileAsSource(_jsPile, _config.CompilerConfiguration);
            }

            return this;
        }

        public IAssets SetupCssPile(Func<IPile, IPile> pileFunc)
        {
            if (_cssPile != null) { throw new InvalidOperationException("The css pile has already been initialised"); }

            // Watch the css if we are live
            _cssPile = pileFunc(new Pile(_config.IsLiveCss));
            if (_cssPile != null)
            {
                _cssManager.SetPileAsSource(_cssPile, _config.CompilerConfiguration);

                if(_config.IsLiveCss)
                {
                    var destPile = _cssManager.FindDestinationPile();
                    destPile.FileCreated += CssOnFileUpdated;
                    destPile.FileDeleted += CssOnFileUpdated;
                    destPile.FileUpdated += CssOnFileUpdated;
                }
            }

            return this;
        }

        public string FindJsAssets(bool includeGlobal, params string[] otherAssets)
        {
            if(_jsManager == null || _jsPile == null)
            {
                throw new InvalidOperationException("Javascript assets are not being tracked");
            }

            var destPile = _jsManager.FindDestinationPile();
            var builder = new StringBuilder();

            // Do urls first
            foreach (var url in CombineUrls(destPile, includeGlobal, otherAssets))
            {
                builder.Append(WrapInJsTag(url));
            }

            // Then files
            foreach (var file in CombineFiles(destPile, includeGlobal, otherAssets))
            {
                builder.Append(WrapInJsTag(file));
            }

            // Insert Css live script if applicable
            if (includeGlobal && _config.IsLiveCss && _cssManager != null && _cssPile != null)
            {
                string script = GetCssLiveScript();

                builder.Append("<script type=\"text/javascript\">");
                builder.Append(script);
                builder.Append("</script>");
            }

            return builder.ToString();
        }

        public string FindCssAssets(bool includeGlobal, params string[] otherAssets)
        {
            if (_cssManager == null || _cssPile == null)
            {
                throw new InvalidOperationException("CSS assets are not being tracked");
            }

            var destPile = _cssManager.FindDestinationPile();
            var builder = new StringBuilder();

            // Do urls first
            int count = 0;
            foreach (var url in CombineUrls(destPile, includeGlobal, otherAssets))
            {
                builder.Append(WrapInCssTag(url, "url" + count));
                count++;
            }

            // Then files
            foreach (var file in CombineFiles(destPile, includeGlobal, otherAssets))
            {
                var id = string.IsNullOrWhiteSpace(_config.CdnPath) ? file : file.Replace(_config.CdnPath, string.Empty);
                id = id.Replace("/", "-").Trim('-');
                builder.Append(WrapInCssTag(file, id));
            }

            return builder.ToString();
        }

        public IAppBuilder MapNodeAssets(IAppBuilder appBuilder, ConnectionConfiguration configuration = null)
        {
            // NOTE: All the routes are based off 'Map' so we dont have to use middleware etc
            if (_config.IsLiveCss)
            {
                var config = configuration ?? new ConnectionConfiguration();
                appBuilder.Map("/" + _config.Namespace, map =>
                {
                    map.RunSignalR<LiveCssConnection>(config);
                });
            }

            // Add js routes
            if (_jsManager != null && _jsPile != null)
            {
                var pileManager = _jsManager.FindDestinationPile();
                HandleRoutes(pileManager, appBuilder);
            }

            // Add css routes
            if (_cssManager != null && _cssPile != null)
            {
                var pileManager = _cssManager.FindDestinationPile();
                HandleRoutes(pileManager, appBuilder);
            }

            return appBuilder;
        }

        private IEnumerable<string> CombineUrls(IPile piles, bool global, IEnumerable<string> other)
        {
            if(global)
            {
                foreach (var uri in piles.FindUrls(Global))
                {
                    yield return uri.ToString();
                }
            }

            if (other != null)
            {
                foreach (var pile in other)
                {
                    foreach (var uri in piles.FindUrls(pile))
                    {
                        yield return uri.ToString();
                    }
                }
            }
        }

        private IEnumerable<string> CombineFiles(IPile piles, bool global, IEnumerable<string> other)
        {
            var basePath = string.IsNullOrWhiteSpace(_config.CdnPath) ? VirtualPathUtility.ToAbsolute("~/") : _config.CdnPath.TrimEnd('/', '\\') + VirtualPathUtility.ToAbsolute("~/");
            if (global)
            {
                var files = piles.FindFiles(Global).ToList();
                if (files.Any())
                {
                    foreach (var file in files)
                    {
                        // multiple routes for the same pile
                        // Add the '/' at the start to make it absolute
                        yield return basePath + FindFilePath(piles, Global, file);
                    }
                }
            }

            if (other != null)
            {
                foreach (var pile in other)
                {
                    var files = piles.FindFiles(pile).ToList();
                    if (!files.Any()) { continue; }

                    foreach (var file in files)
                    {
                        // multiple routes for the same pile
                        yield return basePath + FindFilePath(piles, pile, file);
                    }
                }
            }
        }

        private string WrapInJsTag(string src)
        {
            return "<script type=\"text/javascript\" src=\"" + src + "\"></script>";
        }

        private string WrapInCssTag(string src, string id)
        {
            return "<link id=\"" + id + "\" href=\"" + src + "\" rel=\"stylesheet\" type=\"text/css\" />";
        }

        private void HandleRoutes(IPile destPile, IAppBuilder appBuilder)
        {
            var pathDict = new List<string>();
            foreach (var pile in destPile.FindAllPiles())
            {
                var files = destPile.FindFiles(pile).ToList();
                if (!files.Any()) { continue; }

                foreach (var file in files)
                {
                    // Avoid doubling up on paths (Can happen when combining)
                    var path = "/" + FindFilePath(destPile, pile, file, true);
                    if (!pathDict.Contains(path))
                    {
                        var runFunc = _routeHandler(pile, file);
                        appBuilder.Map(path, map => map.Run(runFunc));
                        pathDict.Add(path);
                    }
                }
            }
        }

        private string _cssLiveScript;
        private string GetCssLiveScript()
        {
            return _cssLiveScript ?? (_cssLiveScript = ScriptFinder.GetScript("NodeAssets.AspNet.Scripts.cssLive.js").Replace("{0}", VirtualPathUtility.ToAbsolute("~/").TrimStart('/') + _config.Namespace));
        }

        private string FindFilePath(IPile destPile, string pile, FileInfo file, bool excludeVersion = false)
        {
            var path = new StringBuilder("assets/");
            path.Append(pile);
            path.Append("/");
            path.Append(file.Name);

            if(!excludeVersion && _config.UseCache)
            {
                path.Append("?v=");
                path.Append(destPile.FindFileHash(file));
            }

            return path.ToString();
        }

        private void CssOnFileUpdated(object sender, FileChangedEvent fileChangedEvent)
        {
            // Need to add the / at the start to make it absolute
            // Catch any exceptions as the virtualpathutility may not be initialised so do not need to broadcast
            try
            {
                var basePath = string.IsNullOrWhiteSpace(_config.CdnPath) ? VirtualPathUtility.ToAbsolute("~/") : _config.CdnPath.TrimEnd('/', '\\') + VirtualPathUtility.ToAbsolute("~/");
                var path = basePath + FindFilePath(_cssPile, fileChangedEvent.Pile, fileChangedEvent.File);
                var id = (VirtualPathUtility.ToAbsolute("~/") + FindFilePath(_cssPile, fileChangedEvent.Pile, fileChangedEvent.File, true)).Replace("/", "-").Trim('-');
                BroadcastCssChange(id, path);
            }
            catch (Exception) { }
        }

        private static void BroadcastCssChange(string id, string css)
        {
            var connectonManager = GlobalHost.ConnectionManager;
            var connection = connectonManager.GetConnectionContext<LiveCssConnection>().Connection;

            connection.Broadcast(new {id, css});
        }
    }
}
