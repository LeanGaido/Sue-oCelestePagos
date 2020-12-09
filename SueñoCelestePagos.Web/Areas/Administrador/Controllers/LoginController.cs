using SueñoCelestePagos.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SueñoCelestePagos.Web.Areas.Administrador.Controllers
{
    public class LoginController : Controller
    {
        // GET: Administrador/Login
        public ActionResult Index(LoginViewModel model)
        {
            model = TempData["loginModel"] as LoginViewModel;
            ModelState.Clear();
            if (model != null)
            {
                ModelState.AddModelError("UserName", "Intento de Sesion no Valido");
            }
            ViewBag.LoginViewModel = TempData["loginModel"] as LoginViewModel;
            return View(model);
        }
    }
}