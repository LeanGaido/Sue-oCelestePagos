using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SueñoCelestePagos.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    "Default",                                              // Route name
            //    "{controller}/{action}/{id}",                           // URL with parameters
            //    new { controller = "Home", action = "Index", id = "" }  // Parameter defaults
            //);
            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Clientes", action = "Identificarse", id = UrlParameter.Optional },
                new[] { "SueñoCelestePagos.Web.Controllers" }
            );
        }
    }
}
