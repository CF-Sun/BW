using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BW
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            string homepoage = System.Configuration.ConfigurationManager.AppSettings["homepoage"];
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = homepoage.Split('/')[0].ToString(), action = homepoage.Split('/')[1].ToString(), id = UrlParameter.Optional }
            );

        }
    }
}
