using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using SueñoCelestePagos.Dal;
using SueñoCelestePagos.Entities;

namespace SueñoCelestePagos.Web.Areas.Administrador.Controllers
{
    public class FechasLimitesDebitoController : Controller
    {
        private SueñoCelestePagosContext db = new SueñoCelestePagosContext();

        // GET: Administrador/FechasLimitesDebito
        public ActionResult Index(int? page)
        {
            var fechasLimite = db.FechasLimitesDebito.ToList();

            if (page == null)
            {
                page = 1;
            }

            return View(fechasLimite.ToPagedList(page.Value, 10));
        }

        // GET: Administrador/FechasLimitesDebito/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FechaLimiteDebito fechaLimiteDebito = db.FechasLimitesDebito.Find(id);
            if (fechaLimiteDebito == null)
            {
                return HttpNotFound();
            }
            return View(fechaLimiteDebito);
        }

        // GET: Administrador/FechasLimitesDebito/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Administrador/FechasLimitesDebito/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Mes,Año,FechaLimite")] FechaLimiteDebito fechaLimiteDebito)
        {
            if (ModelState.IsValid)
            {
                db.FechasLimitesDebito.Add(fechaLimiteDebito);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(fechaLimiteDebito);
        }

        // GET: Administrador/FechasLimitesDebito/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FechaLimiteDebito fechaLimiteDebito = db.FechasLimitesDebito.Find(id);
            if (fechaLimiteDebito == null)
            {
                return HttpNotFound();
            }
            return View(fechaLimiteDebito);
        }

        // POST: Administrador/FechasLimitesDebito/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Mes,Año,FechaLimite")] FechaLimiteDebito fechaLimiteDebito)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fechaLimiteDebito).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fechaLimiteDebito);
        }

        // GET: Administrador/FechasLimitesDebito/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FechaLimiteDebito fechaLimiteDebito = db.FechasLimitesDebito.Find(id);
            if (fechaLimiteDebito == null)
            {
                return HttpNotFound();
            }
            return View(fechaLimiteDebito);
        }

        // POST: Administrador/FechasLimitesDebito/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FechaLimiteDebito fechaLimiteDebito = db.FechasLimitesDebito.Find(id);
            db.FechasLimitesDebito.Remove(fechaLimiteDebito);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
