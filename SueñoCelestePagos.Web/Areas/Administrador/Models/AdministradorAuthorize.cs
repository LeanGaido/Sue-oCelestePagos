using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SueñoCelestePagos.Web.Areas.Administrador.Models
{
    public class AdministradorAuthorize : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(new
            RouteValueDictionary(new { Area = "Administrador", controller = "Login", action = "Index" }));
        }
    }
}