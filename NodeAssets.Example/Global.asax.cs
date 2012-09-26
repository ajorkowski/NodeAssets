using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NodeAssets;

namespace NodeAssets.Example
{
    public class MvcApplication : HttpApplication
    {
        public void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new {controller = "Home", action = "Index", id = UrlParameter.Optional} // Parameter defaults
                );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

#if DEBUG
            bool isProd = false;
#else
            bool isProd = true;
#endif

            Assets
                .Initialise(config => config
                    .ConfigureCompilers(
                        compilers => compilers.WithDefaultNodeConfiguration(Server.MapPath("~/Node")))
                    .ConfigureSourceManager(
                        source =>
                        source.UseDefaultConfiguration(Server.MapPath("~/built"), isProd))
                    .Cache(isProd)
                    .Compress(isProd)
                    .LiveCss(!isProd))
                .SetupCssPile(pile => pile
                    // An Example regex where you will add files ending in .css
                    // but NOT files ending in .min.css
                    .AddDirectory("Styles", Server.MapPath("~/Content"), false, new Regex("(?<!.min).css$")))
                .SetupJavascriptPile(pile =>
                {
                    pile.AddFile(Server.MapPath("~/Scripts/jquery-1.7.2.js"));

                    if (!isProd)
                    {
                        pile.AddFile(Server.MapPath("~/Scripts/jquery.signalR-0.5.1.js"));
                    }

                    return pile;
                })
                .PrepareRoutes(RouteTable.Routes);

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}