using System.Web.Mvc;
using System.Web.Routing;

namespace IDFCFastTagApi
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Keep the existing API-style URL pattern but route it through MVC
            routes.MapRoute(
                name: "ApiAsMvc",
                url: "api/{controller}/{action}",
                defaults: new { action = UrlParameter.Optional }
            );
        }
    }
}

