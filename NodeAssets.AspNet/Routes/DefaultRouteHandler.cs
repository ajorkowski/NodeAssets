using System;
using System.IO;
using System.Web;
using System.Web.Routing;

namespace NodeAssets.AspNet.Routes
{
    public sealed class DefaultRouteHandler : IRouteHandler
    {
        private IHttpHandler _handler;

        public DefaultRouteHandler(string pile, FileInfo info, IAssetsConfiguration config)
        {
            _handler = new DefaultHttpHandler(info, config);
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return _handler;
        }
    }
}
