using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;

namespace NodeAssets.AspNet.Routes
{
    public class MvcIgnoredRoute : Route
    {
        public MvcIgnoredRoute(string url, IRouteHandler handler)
            : base(url, handler)
        {
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            // This prevents MVC actions from picking up the route
            return null;
        }
    }
}
