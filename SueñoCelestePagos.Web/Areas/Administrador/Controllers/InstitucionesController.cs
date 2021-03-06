﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SueñoCelestePagos.Dal;
using SueñoCelestePagos.Entities;

namespace SueñoCelestePagos.Web.Areas.Administrador.Controllers
{
    public class InstitucionesController : Controller
    {
        private SueñoCelestePagosContext db = new SueñoCelestePagosContext();

        // GET: Administrador/Instituciones
        public ActionResult Index()
        {
            return View(db.Instituciones.ToList());
        }

        // GET: Administrador/Instituciones/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Institucion institucion = db.Instituciones.Find(id);
            if (institucion == null)
            {
                return HttpNotFound();
            }
            return View(institucion);
        }

        // GET: Administrador/Instituciones/Create
        public ActionResult Create()
        {
            ViewBag.LocalidadID = new SelectList(db.Localidades, "ID", "Descripcion");

            return View();
        }

        // POST: Administrador/Instituciones/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Nombre,Cuit,Calle,Altura,LocalidadID,AreaTelefono,NumeroTelefono")] Institucion institucion)
        {
            if (ModelState.IsValid)
            {
                db.Instituciones.Add(institucion);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.LocalidadID = new SelectList(db.Localidades, "ID", "Descripcion", institucion.LocalidadID);

            return View(institucion);
        }

        // GET: Administrador/Instituciones/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Institucion institucion = db.Instituciones.Find(id);
            if (institucion == null)
            {
                return HttpNotFound();
            }
            ViewBag.LocalidadID = new SelectList(db.Localidades, "ID", "Descripcion", institucion.LocalidadID);
            return View(institucion);
        }

        // POST: Administrador/Instituciones/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Nombre,Cuit,Calle,Altura,LocalidadID,AreaTelefono,NumeroTelefono")] Institucion institucion)
        {
            if (ModelState.IsValid)
            {
                db.Entry(institucion).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.LocalidadID = new SelectList(db.Localidades, "ID", "Descripcion", institucion.LocalidadID);
            return View(institucion);
        }

        public ActionResult Aporte(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Institucion institucion = db.Instituciones.Find(id);
            if (institucion == null)
            {
                return HttpNotFound();
            }
            ViewBag.LocalidadID = new SelectList(db.Localidades, "ID", "Descripcion");

            return View(institucion);
        }

        [HttpPost]
        public ActionResult Aporte(int? id, decimal Aporte, DateTime FechaAlta, DateTime? FechaBaja)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Institucion institucion = db.Instituciones.Find(id);
            if (institucion == null)
            {
                return HttpNotFound();
            }
            ViewBag.LocalidadID = new SelectList(db.Localidades, "ID", "Descripcion");

            if (Aporte < 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var AporteVigente = db.AporteInstitucion.Where(x => x.InstitucionID == id && x.FechaBaja != null).FirstOrDefault();
            if(AporteVigente == null)
            {
                AporteVigente = new AporteInstitucion();

                AporteVigente.InstitucionID = id.Value;
                AporteVigente.PorcAporte = Aporte;
                AporteVigente.FechaAlta = FechaAlta;
                AporteVigente.FechaBaja = FechaBaja;

                db.AporteInstitucion.Add(AporteVigente);
            }
            else
            {
                AporteVigente.FechaBaja = FechaAlta.AddDays(-1);

                AporteInstitucion NuevoAporte = new AporteInstitucion();

                NuevoAporte.InstitucionID = id.Value;
                NuevoAporte.PorcAporte = Aporte;
                NuevoAporte.FechaAlta = FechaAlta;
                NuevoAporte.FechaBaja = FechaBaja;

                db.AporteInstitucion.Add(NuevoAporte);
            }

            db.SaveChanges();

            return View();
        }

        // GET: Administrador/Instituciones/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Institucion institucion = db.Instituciones.Find(id);
            if (institucion == null)
            {
                return HttpNotFound();
            }
            return View(institucion);
        }

        // POST: Administrador/Instituciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Institucion institucion = db.Instituciones.Find(id);
            db.Instituciones.Remove(institucion);
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
