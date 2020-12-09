using Ganss.Excel;
using PagedList;
using SueñoCelestePagos.Dal;
using SueñoCelestePagos.Web.Areas.Administrador.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SueñoCelestePagos.Web.Areas.Administrador.Controllers
{
    [AdministradorAuthorize]
    public class ClientesController : Controller
    {
        private SueñoCelestePagosContext db = new SueñoCelestePagosContext();

        // GET: Administrador/Clientes
        public ActionResult Index(int page = 1)
        {
            var Clientes = db.Clientes.Include(t => t.Localidad).ToList();

            return View(Clientes.ToPagedList(page,15));
        }

        public FileContentResult ClientesConPagos()
        {
            DateTime hoy = DateTime.Today;

            var pagosRealizados = db.PagosCartonesVendidos.Include(t => t.CartonVendido).Include(t => t.CartonVendido.Carton).Where(x => x.Pagado && x.CartonVendido.Carton.Año == hoy.Year).ToList();

            var CartonesId = pagosRealizados.Select(x => x.CartonVendidoID).ToList().Distinct();

            var ClientesId = db.CartonesVendidos.Where(x => CartonesId.Contains(x.ID)).Select(x => x.ClienteID).Distinct();

            //var clientes = db.Clientes.Where(x => ClientesId.Contains(x.ID)).ToList();

            var clientes = (from oClientes in db.Clientes
                            join oCartonComprado in db.CartonesVendidos on oClientes.ID equals oCartonComprado.ClienteID
                            join oCarton in db.Cartones on oCartonComprado.CartonID equals oCarton.ID
                            join oLocalidad in db.Localidades on oClientes.LocalidadID equals oLocalidad.ID
                            join oProvincia in db.Provicias on oLocalidad.ProvinciaID equals oProvincia.ID
                            where ClientesId.Contains(oClientes.ID)
                            select new
                            {
                                NroCarton = oCarton.Numero,
                                Nombre = oClientes.Nombre,
                                Apellido = oClientes.Apellido,
                                Dni = oClientes.Dni,
                                AreaCelular = oClientes.AreaCelular,
                                NumeroCelular = oClientes.NumeroCelular,
                                Email = oClientes.Email,
                                FechaNacimiento = oClientes.FechaNacimiento,
                                Sexo = oClientes.Sexo,
                                CalleDomicilio = oClientes.Calle,
                                AlturaDomicilio = oClientes.Altura,
                                Localidad = oLocalidad.Nombre,
                                CodPostal = oLocalidad.CodPostal,
                                Provincia = oProvincia.Nombre
                            }).ToList();

            ExcelMapper mapper = new ExcelMapper();

            var newFile = Server.MapPath("~/Archivos/Exportacion/Clientes/clientes" + hoy.Day + hoy.Month + hoy.Year + ".xlsx");

            mapper.Save(newFile, clientes, "SheetName", true);

            String mimeType = MimeMapping.GetMimeMapping(newFile);

            byte[] stream = System.IO.File.ReadAllBytes(newFile);

            /*
             String file = Server.MapPath("~/ParentDir/ChildDir" + fileName);
             String mimeType = MimeMapping.GetMimeMapping(path);

             byte[] stream = System.IO.File.ReadAllBytes(file);
             */

            return File(stream, mimeType);
        }
    }
}