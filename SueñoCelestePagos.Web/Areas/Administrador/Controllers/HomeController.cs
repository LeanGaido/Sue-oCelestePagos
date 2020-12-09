using SueñoCelestePagos.Web.Areas.Administrador.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SueñoCelestePagos.Web.Areas.Administrador.Controllers
{
    [AdministradorAuthorize]
    public class HomeController : Controller
    {
        // GET: Administrador/Home
        public ActionResult Index()
        {
            return View();
        }
    }
}