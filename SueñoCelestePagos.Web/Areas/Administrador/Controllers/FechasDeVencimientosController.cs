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
using SueñoCelestePagos.Web.Areas.Administrador.Models;

namespace SueñoCelestePagos.Web.Areas.Administrador.Controllers
{
    [AdministradorAuthorize]
    public class FechasDeVencimientosController : Controller
    {
        private SueñoCelestePagosContext db = new SueñoCelestePagosContext();

        // GET: ContentAdmin/FechasDeVencimientos
        public ActionResult Index(int? page)
        {
            var fechasDeVencimiento = db.FechasDeVencimiento.ToList();

            if (page == null)
            {
                page = 1;
            }

            return View(fechasDeVencimiento.ToPagedList(page.Value, 10));
        }

        // GET: ContentAdmin/FechasDeVencimientos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FechaDeVencimiento fechaDeVencimiento = db.FechasDeVencimiento.Find(id);
            if (fechaDeVencimiento == null)
            {
                return HttpNotFound();
            }
            return View(fechaDeVencimiento);
        }

        // GET: ContentAdmin/FechasDeVencimientos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ContentAdmin/FechasDeVencimientos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Mes,Año,PrimerVencimiento,SegundoVencimiento")] FechaDeVencimiento fechaDeVencimiento)
        {
            if (ModelState.IsValid)
            {
                db.FechasDeVencimiento.Add(fechaDeVencimiento);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(fechaDeVencimiento);
        }

        // GET: ContentAdmin/FechasDeVencimientos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FechaDeVencimiento fechaDeVencimiento = db.FechasDeVencimiento.Find(id);
            if (fechaDeVencimiento == null)
            {
                return HttpNotFound();
            }
            return View(fechaDeVencimiento);
        }

        // POST: ContentAdmin/FechasDeVencimientos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Mes,Año,PrimerVencimiento,SegundoVencimiento")] FechaDeVencimiento fechaDeVencimiento)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fechaDeVencimiento).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fechaDeVencimiento);
        }

        // GET: ContentAdmin/FechasDeVencimientos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FechaDeVencimiento fechaDeVencimiento = db.FechasDeVencimiento.Find(id);
            if (fechaDeVencimiento == null)
            {
                return HttpNotFound();
            }
            return View(fechaDeVencimiento);
        }

        // POST: ContentAdmin/FechasDeVencimientos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FechaDeVencimiento fechaDeVencimiento = db.FechasDeVencimiento.Find(id);
            db.FechasDeVencimiento.Remove(fechaDeVencimiento);
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
