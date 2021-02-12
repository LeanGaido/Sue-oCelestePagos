using PagedList;
using SueñoCelestePagos.Dal;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using SueñoCelestePagos.Entities.Pago360.Request;
using SueñoCelestePagos.Entities;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using SueñoCelestePagos.Entities.Pago360.Response;
using System.Text;
using SueñoCelestePagos.Web.Areas.Administrador.Models;
using SueñoCelestePagos.Entities.Pago360;
using SueñoCelestePagos.Entities.VMs;
using SueñoCelestePagos.Utilities;
using System.Dynamic;
using Ganss.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using ClosedXML.Excel;

namespace SueñoCelestePagos.Web.Areas.Administrador.Controllers
{
    [AdministradorAuthorize]
    public class ComprasController : Controller
    {
        private SueñoCelestePagosContext db = new SueñoCelestePagosContext();

        /*****************************************************************************************/

        // GET: Administrador/Compras
        public ActionResult Index(int? Año, int page = 1)
        {
            if(Año == null)
            {
                Año = DateTime.Today.Year;
            }

            ViewBag.Año = Año;

            //ActualizarEstado();

            var Compras = db.CartonesVendidos.Include(t => t.Carton)
                                             .Include(t => t.Cliente)
                                             .Include(t => t.TipoDePago)
                                             .Where(x => x.Carton.Año == Año && x.PagoCancelado == false)
                                             .OrderByDescending(x => x.FechaVenta)
                                             .ToList();

            return View(Compras);
        }

        /*****************************************************************************************/

        public ActionResult ResumenAcumulado(int? Campaña, int InstitucionesID = 0, int page = 1)
        {
            var Instituciones = db.Instituciones.ToList();

            Instituciones.Insert(0, new Institucion() { ID = 0, Nombre = "" });

            ViewBag.InstitucionesID = new SelectList(Instituciones, "ID", "Nombre", InstitucionesID);

            var Campañas = db.Campañas.ToList();

            if(Campaña == null)
            {
                Campaña = Campañas.First().ID;
                ViewBag.Campaña = new SelectList(Campañas, "ID", "Año", Campaña);
            }
            else
            {
                ViewBag.Campaña = new SelectList(Campañas, "ID", "Año");
            }

            ViewBag.CampañaID = Campaña;
            ViewBag.InstitucionID = InstitucionesID;

            //List<ResumenAcumuladoVm> resumenAcumulado = ObtenerResumenAcumulado(Año, InstitucionesID);

            List<ResumenCampañaVm> resumenCampañaVm = ObtenerResumenAcumuladoV2(Campaña.Value, InstitucionesID);

            return View(resumenCampañaVm.ToPagedList(page, 15));
        }

        [AllowAnonymous]
        public FileContentResult DescargarResumenAcumulado(int? Campaña, int InstitucionesID = 0)
        {
            List<CartonVendido> CartonesVendido = new List<CartonVendido>();

            var Campañas = db.Campañas.Where(x => x.ID == Campaña).FirstOrDefault();

            List<ResumenCampañaVm> resumenCampañaVm = ObtenerResumenAcumuladoV2(Campaña.Value, InstitucionesID);

            IList<Dictionary<string, string>> resumenAcumulado = new List<Dictionary<string, string>>();

            #region Encabezado
            var encabezadoResumenCampaña = new Dictionary<string, string>(){
                {"Carton", "Carton"},
                {"Cliente", "Cliente"},
                {"Dni", "Dni"},
                {"Telefono", "Telefono"},
                {"Email", "Email"},
                {"Localidad", "Localidad"},
                {"Institucion", "Institucion"}
            };

            foreach (var mes in resumenCampañaVm.First().MesesCampaña)
            {
                encabezadoResumenCampaña.Add(mes.NombreMes + "/" + mes.Año, mes.NombreMes + "/" + mes.Año);
            }

            resumenAcumulado.Add(encabezadoResumenCampaña);
            #endregion

            #region Detalle
            foreach (var item in resumenCampañaVm)
            {
                if (item.NroCarton == null)
                {
                    continue;
                }
                var detalleResumenCampaña = new Dictionary<string, string>(){
                    {"Carton", item.NroCarton},
                    {"Cliente", item.NombreCompleto},
                    {"Dni", item.Dni},
                    {"Telefono", item.Telefono},
                    {"Email", item.Email},
                    {"Localidad", item.Localidad},
                    {"Institucion", item.Institucion}
                };
                foreach (var mes in item.MesesCampaña)
                {
                    detalleResumenCampaña.Add(mes.NombreMes + "/" + mes.Año, "$" + mes.Importe.ToString());
                }

                resumenAcumulado.Add(detalleResumenCampaña);
            }
            #endregion

            #region Totales
            var totalesResumenCampaña = new Dictionary<string, string>(){
                {"Carton", ""},
                {"Cliente", ""},
                {"Dni", ""},
                {"Telefono", ""},
                {"Email", ""},
                {"Localidad", ""},
                {"Institucion", "Totales"}
            };

            foreach (var item in resumenCampañaVm)
            {
                if (item.NroCarton == null)
                {
                    continue;
                }
                foreach (var mes in item.MesesCampaña)
                {
                    if(totalesResumenCampaña.ContainsKey(mes.NombreMes + "/" + mes.Año))
                    {
                        var importe = decimal.Parse(totalesResumenCampaña[mes.NombreMes + "/" + mes.Año].Remove(0,1));

                        importe += mes.Importe;

                        totalesResumenCampaña[mes.NombreMes + "/" + mes.Año] = "$" + importe.ToString();
                    }
                    else
                    {
                        totalesResumenCampaña[mes.NombreMes + "/" + mes.Año] = "$" + mes.Importe.ToString();
                    }
                }
            }

            resumenAcumulado.Add(totalesResumenCampaña);
            #endregion

            #region Exportacion a Excel
            string newFile = Server.MapPath("~/Archivos/Exportacion/compras/compras" + Campañas.Año + ".xlsx");

            try
            {
                //Si ya existe un documento con el nombre asignado, se le cambia el nombre al ya existente
                if (System.IO.File.Exists(newFile))
                {
                    var hoy = DateTime.Now;
                    string destinationOldFile = Server.MapPath("~/Archivos/Exportacion/compras/compras" + Campañas.Año + "-" + hoy.Hour + hoy.Minute + hoy.Second + "-" + hoy.Day + hoy.Month + hoy.Year + ".xlsx");
                    System.IO.File.Move(newFile, destinationOldFile);
                }
            }
            catch (Exception iox)
            {
                //Tratar Errores
                throw;
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Resumen Acumulado Campaña " + Campañas.Año);
                int fila = 1;//"fila" es la fila del excel en la estaria escribiendo y la inicializo en 1
                for (int indice = 0; indice < resumenAcumulado.Count; indice++)//"indice" seria la posicion del List<Dictionary<string,string>> que estoy iterando
                {
                    int columna = 1;//"columna" es la columna del excel en la estaria escribiendo y la inicializo en 1
                    var value = resumenAcumulado[indice];
                    foreach (string NombrePropiedad in value.Keys)
                    {
                        //"NombrePropiedad" es el nombre que se le asigno como Key al Dictionary<string,string> al momento de crearlo
                        var valor = value[NombrePropiedad];//de esta forma obtengo el Value del Dictionary<string,string>

                        //Escriobo en la fila y en la columna actual del excel el valor que obtuve
                        worksheet.Cell(fila, columna).Value = valor;

                        columna++;//Avanzo a la sig Columna
                    }

                    fila++;//Avanzo a la sig fila
                }
                workbook.SaveAs(newFile);
            }
            #endregion

            #region Descarga del Excel
            String mimeType = MimeMapping.GetMimeMapping(newFile);

            byte[] stream = System.IO.File.ReadAllBytes(newFile);

            return File(stream, mimeType, "compras" + Campañas.Año + ".xlsx");
            #endregion
        }

        private List<ResumenAcumuladoVm> ObtenerResumenAcumulado(int? Año, int InstitucionesID)
        {

            List<ResumenAcumuladoVm> resumenAcumulado = new List<ResumenAcumuladoVm>();

            var CartonesVendido = db.CartonesVendidos.Where(x => x.PagoCancelado == false &&
                                                                 x.FechaVenta.Year == Año)
                                                     .Include(x => x.Carton)
                                                     .Include(x => x.Cliente)
                                                     .ToList();

            if (InstitucionesID != 0)
            {
                CartonesVendido = CartonesVendido.Where(x => x.EntidadID == InstitucionesID).ToList();
            }

            ResumenAcumuladoVm totalMensual = new ResumenAcumuladoVm();
            totalMensual.Institucion = "Totales Mensuales";

            foreach (var CartonVendido in CartonesVendido)
            {
                var pagos = db.PagosCartonesVendidos.Where(x => x.CartonVendidoID == CartonVendido.ID && x.Pagado == true)
                                                    .ToList();

                if (pagos.Count >= 1)
                {
                    ResumenAcumuladoVm nuevoResumenAcumulado = new ResumenAcumuladoVm();

                    nuevoResumenAcumulado.NroCarton = CartonVendido.Carton.Numero;
                    nuevoResumenAcumulado.NombreCompleto = CartonVendido.Cliente.NombreCompleto;
                    nuevoResumenAcumulado.Dni = CartonVendido.Cliente.Dni;
                    nuevoResumenAcumulado.Telefono = CartonVendido.Cliente.Celular;
                    nuevoResumenAcumulado.Email = CartonVendido.Cliente.Email;
                    nuevoResumenAcumulado.Localidad = CartonVendido.Cliente.Localidad.Descripcion;

                    var institucion = db.Instituciones.Where(x => x.ID == CartonVendido.EntidadID).FirstOrDefault();
                    nuevoResumenAcumulado.Institucion = (institucion == null) ? "" : institucion.Nombre;

                    foreach (var pago in pagos)
                    {
                        switch (pago.FechaDePago.Month)
                        {
                            case 1:
                                nuevoResumenAcumulado.PagoEnero += pago.Pago;
                                totalMensual.PagoEnero += pago.Pago;
                                break;
                            case 2:
                                nuevoResumenAcumulado.PagoFebrero += pago.Pago;
                                totalMensual.PagoFebrero += pago.Pago;
                                break;
                            case 3:
                                nuevoResumenAcumulado.PagoMarzo += pago.Pago;
                                totalMensual.PagoMarzo += pago.Pago;
                                break;
                            case 4:
                                nuevoResumenAcumulado.PagoAbril += pago.Pago;
                                totalMensual.PagoAbril += pago.Pago;
                                break;
                            case 5:
                                nuevoResumenAcumulado.PagoMayo += pago.Pago;
                                totalMensual.PagoMayo += pago.Pago;
                                break;
                            case 6:
                                nuevoResumenAcumulado.PagoJunio += pago.Pago;
                                totalMensual.PagoJunio += pago.Pago;
                                break;
                            case 7:
                                nuevoResumenAcumulado.PagoJulio += pago.Pago;
                                totalMensual.PagoJulio += pago.Pago;
                                break;
                            case 8:
                                nuevoResumenAcumulado.PagoAgosto += pago.Pago;
                                totalMensual.PagoAgosto += pago.Pago;
                                break;
                            case 9:
                                nuevoResumenAcumulado.PagoSeptiembre += pago.Pago;
                                totalMensual.PagoSeptiembre += pago.Pago;
                                break;
                            case 10:
                                nuevoResumenAcumulado.PagoOctubre += pago.Pago;
                                totalMensual.PagoOctubre += pago.Pago;
                                break;
                            case 11:
                                nuevoResumenAcumulado.PagoNoviembre += pago.Pago;
                                totalMensual.PagoNoviembre += pago.Pago;
                                break;
                            case 12:
                                nuevoResumenAcumulado.PagoDiciembre += pago.Pago;
                                totalMensual.PagoDiciembre += pago.Pago;
                                break;
                        }

                        nuevoResumenAcumulado.TotalPagos += pago.Pago;
                    }

                    resumenAcumulado.Add(nuevoResumenAcumulado);

                    totalMensual.TotalPagos += nuevoResumenAcumulado.TotalPagos;
                }

            }

            resumenAcumulado.Add(totalMensual);

            return resumenAcumulado;
        }
        private List<ResumenCampañaVm> ObtenerResumenAcumuladoV2(int CampañaId, int InstitucionesID)
        {

            List<ResumenCampañaVm> resumenAcumulado = new List<ResumenCampañaVm>();

            var Campaña = db.Campañas.Where(x => x.ID == CampañaId).FirstOrDefault();

            var CartonesVendido = db.CartonesVendidos.Where(x => x.PagoCancelado == false &&
                                                                 x.Carton.ValidoDesde == Campaña.FechaInicio &&
                                                                 x.Carton.ValidoHasta == Campaña.FechaFin)
                                                     .Include(x => x.Carton)
                                                     .Include(x => x.Cliente)
                                                     .OrderBy(x => x.FechaVenta)
                                                     .ToList();

            if (InstitucionesID != 0)
            {
                CartonesVendido = CartonesVendido.Where(x => x.EntidadID == InstitucionesID).ToList();
            }

            ResumenCampañaVm totalesResumenAcumulado = new ResumenCampañaVm();
            totalesResumenAcumulado.Institucion = "Totales";
            List<MesCampañaVm> Totates = ObtenerListadoMeses(Campaña);

            foreach (var CartonVendido in CartonesVendido)
            {

                var pagos = db.PagosCartonesVendidos.Where(x => x.CartonVendidoID == CartonVendido.ID && x.Pagado == true)
                                                    .ToList();

                if (pagos.Count >= 1)
                {
                    ResumenCampañaVm nuevoResumenAcumulado = new ResumenCampañaVm();

                    nuevoResumenAcumulado.NroCarton = CartonVendido.Carton.Numero;
                    nuevoResumenAcumulado.NombreCompleto = CartonVendido.Cliente.NombreCompleto;
                    nuevoResumenAcumulado.Dni = CartonVendido.Cliente.Dni;
                    nuevoResumenAcumulado.Telefono = CartonVendido.Cliente.Celular;
                    nuevoResumenAcumulado.Email = CartonVendido.Cliente.Email;
                    nuevoResumenAcumulado.Localidad = CartonVendido.Cliente.Localidad.Descripcion;

                    var institucion = db.Instituciones.Where(x => x.ID == CartonVendido.EntidadID).FirstOrDefault();
                    nuevoResumenAcumulado.Institucion = (institucion == null) ? "" : institucion.Nombre;

                    List<MesCampañaVm> PagosCampañaVm = ObtenerListadoMeses(Campaña);

                    foreach (var pago in pagos)
                    {
                        var pagoCampaña = PagosCampañaVm.Where(x => x.Mes == pago.FechaDePago.Month && x.Año == pago.FechaDePago.Year).FirstOrDefault();

                        pagoCampaña.Importe = pago.Pago;
                        
                        var totalMes = Totates.Where(x => x.Mes == pago.FechaDePago.Month && x.Año == pago.FechaDePago.Year).FirstOrDefault();
                        totalMes.Importe += pago.Pago;
                    }

                    nuevoResumenAcumulado.MesesCampaña = PagosCampañaVm;

                    resumenAcumulado.Add(nuevoResumenAcumulado);
                }

            }

            totalesResumenAcumulado.MesesCampaña = Totates;

            resumenAcumulado.Add(totalesResumenAcumulado);

            return resumenAcumulado;
        }

        private static List<MesCampañaVm> ObtenerListadoMeses(Campaña Campaña)
        {
            List<MesCampañaVm> mesesCampañaVm = new List<MesCampañaVm>();

            //int CantMeses = ((Campaña.FechaFin.Year - Campaña.FechaInicio.Year) * 12) + Campaña.FechaFin.Month - Campaña.FechaInicio.Month;

            var FechaInicio = new DateTime(Campaña.FechaInicio.Year, Campaña.FechaInicio.Month, 1);

            var Meses = Campaña.FechaFin.Subtract(FechaInicio).Days / (365.25 / 12);
            var CantMeses = Math.Round(Meses);
            int mesInicial = Campaña.FechaInicio.Month;
            int añoInicial = Campaña.FechaInicio.Year;

            for (int Mes = 0; Mes <= CantMeses; Mes++)
            {
                string nombreMes = "";
                switch (mesInicial)
                {
                    case 1:
                        nombreMes = "Enero";
                        break;
                    case 2:
                        nombreMes = "Febrero";
                        break;
                    case 3:
                        nombreMes = "Marzo";
                        break;
                    case 4:
                        nombreMes = "Abril";
                        break;
                    case 5:
                        nombreMes = "Mayo";
                        break;
                    case 6:
                        nombreMes = "Junio";
                        break;
                    case 7:
                        nombreMes = "Julio";
                        break;
                    case 8:
                        nombreMes = "Agosto";
                        break;
                    case 9:
                        nombreMes = "Septiembre";
                        break;
                    case 10:
                        nombreMes = "Octubre";
                        break;
                    case 11:
                        nombreMes = "Noviembre";
                        break;
                    case 12:
                        nombreMes = "Diciembre";
                        break;
                }

                mesesCampañaVm.Add(new MesCampañaVm() { Mes = mesInicial, Año = añoInicial, NombreMes = nombreMes, Importe = 0 });

                if(mesInicial == 12)
                {
                    mesInicial = 1;
                    añoInicial++;
                }
                else
                {
                    mesInicial++;
                }
            }

            return mesesCampañaVm;
        }

        /*****************************************************************************************/

        public ActionResult UploadFile()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            //List<VmAlert> alerts = new List<VmAlert>();
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    List<string> rows = new List<string>();

                    using (var reader = new StreamReader(file.InputStream))
                    {
                        int linea = 0;
                        while (!reader.EndOfStream)
                        {
                            if(linea == 0)
                            {
                                linea++;
                                continue;
                            }

                            var line = reader.ReadLine();

                            rows.Add(line);
                        }
                    }

                    foreach (var row in rows)
                    {
                        var data = row.Split(',');
                        Webhook webhook = new Webhook();
                        switch (data[3])
                        {
                            case "adhesion":
                                webhook.entity_name = "adhesion";
                                webhook.entity_id = int.Parse(data[2]);
                                switch (data[4])
                                {
                                    case "Autorizado":
                                        webhook.type = "signed";
                                        break;
                                    case "Cancelado":
                                        webhook.type = "canceled";
                                        break;
                                }
                                break;
                            case "Solicitud de Débito en CBU":
                                webhook.entity_name = "debit_request";
                                webhook.entity_id = int.Parse(data[2]);
                                switch (data[4])
                                {
                                    case "Pagado":
                                        webhook.type = "paid";
                                        break;
                                    case "Cancelado":
                                        webhook.type = "canceled";
                                        break;
                                    case "Rechazado":
                                        webhook.type = "rejected";
                                        break;
                                    case "Revertido":
                                        webhook.type = "reverted";
                                        break;
                                }
                                break;
                            case "Solicitud de Pago":
                                webhook.entity_name = "payment_request";
                                webhook.entity_id = int.Parse(data[2]);
                                switch (data[4])
                                {
                                    case "Pagado":
                                        webhook.type = "paid";
                                        break;
                                    case "Vencido":
                                        webhook.type = "expired";
                                        break;
                                    case "Revertido":
                                        webhook.type = "reverted";
                                        break;
                                }
                                break;
                        }

                        CambiarEstado(webhook);
                    }
                }
                else
                {
                    string errorMessage = "Ocurrio un Error, documento vacio o no valido.";
                    //alerts.Add(new VmAlert("danger", errorMessage, true));
                }
            }
            catch (Exception)
            {
                string errorMessage = "Ocurrio un Error, por favor intente mas tarde.";
                //alerts.Add(new VmAlert("danger", errorMessage, true));
                //Logger.Error(ex, "Something bad happened");
            }
            //ViewBag.Alerts = alerts;
            return View();
        }

        /*****************************************************************************************/

        public ActionResult Details(int id, int page)
        {
            var Compra = db.CartonesVendidos.Include(t => t.Carton)
                                            .Include(t => t.Cliente)
                                            .Include(t => t.TipoDePago)
                                            .Where(x => x.ID == id)
                                            .FirstOrDefault();

            ViewBag.Page = page;

            return View(Compra);
        }

        public ActionResult Pagos(int id, int page, int Page = 1)
        {
            ViewBag.Page = page;

            var Compra = db.CartonesVendidos.Include(t => t.Carton)
                                            .Include(t => t.Cliente)
                                            .Include(t => t.TipoDePago)
                                            .Where(x => x.ID == id)
                                            .FirstOrDefault();

            ViewBag.Compra = Compra;

            var Cliente = db.Clientes.Where(x => x.ID == Compra.ClienteID).FirstOrDefault();

            ViewBag.Cliente = Cliente;
            ViewBag.NumeroCarton = Compra.Carton.Numero;
            ViewBag.FechaVenta = Compra.FechaVenta;

            List<PagoCartonVendido> Pagos = new List<PagoCartonVendido>();

            Pagos = db.PagosCartonesVendidos.Where(x => x.CartonVendidoID == id).ToList();

            return View(Pagos);
        }

        /*****************************************************************************************/

        public ActionResult DebitarCuotas()
        {
            DateTime hoy = DateTime.Today;

            var cartonesComprados = db.CartonesVendidos.Where(x => x.FechaVenta.Year == hoy.Year && x.PagoCancelado == false && x.TipoDePagoID == 3).ToList();

            string[] cartonesCompradosId = new string[cartonesComprados.Count];

            foreach (var cartonComprado in cartonesComprados)
            {
                cartonesCompradosId[cartonesComprados.IndexOf(cartonComprado)] = cartonComprado.ID.ToString();
            }

            var adhesiones = db.Adhesiones.Where(x => cartonesCompradosId.Any(y => y == x.external_reference) && x.state == "signed").ToList();

            var ahesionesId = adhesiones.Select(x => x.id).ToArray();

            var cuotasADebitar = db.CuotasDebito.Where(x => ahesionesId.Any(y => y == x.AdhesionID) && x.DebitoID == 0 && x.MesCuota == hoy.Month).ToList();

            foreach (var cuotaADebitar in cuotasADebitar)
            {
                var CuotaVencida = db.CuotasDebito.Where(x => x.CartonVendidoID == cuotaADebitar.CartonVendidoID && x.MesCuota == cuotaADebitar.MesCuota - 1).FirstOrDefault();

                if (CuotaVencida != null && CuotaVencida.PrimerVencimiento > hoy)
                {
                    cuotaADebitar.PrimerPrecioCuota += CuotaVencida.PrimerPrecioCuota;

                    cuotaADebitar.SeguntoPrecioCuota += CuotaVencida.SeguntoPrecioCuota;
                }

                SolicitudDebitoRequest solicitudDebito = new SolicitudDebitoRequest();

                solicitudDebito.adhesion_id = cuotaADebitar.AdhesionID;
                solicitudDebito.first_due_date = cuotaADebitar.PrimerVencimiento.ToString("dd-MM-yyyy");
                solicitudDebito.first_total = cuotaADebitar.PrimerPrecioCuota;
                solicitudDebito.second_due_date = cuotaADebitar.SeguntoVencimiento.ToString("dd-MM-yyyy");
                solicitudDebito.second_total = cuotaADebitar.SeguntoPrecioCuota;
                solicitudDebito.description = "Sueño Celeste cuota " + cuotaADebitar.NroCuota + " correspondiente al mes " + cuotaADebitar.MesCuota;

                var debito = RealizarDebito(solicitudDebito, cuotaADebitar.ID);

                db.Debitos.Add(debito);

                debito.first_due_date = cuotaADebitar.PrimerVencimiento;
                debito.SecondDueDate = cuotaADebitar.SeguntoVencimiento;
                debito.AdhesionId = cuotaADebitar.AdhesionID;

                cuotaADebitar.DebitoID = debito.id;

                db.SaveChanges();

                PagoCartonVendido pagoCarton = new PagoCartonVendido();

                pagoCarton.CartonVendidoID = cuotaADebitar.CartonVendidoID;
                pagoCarton.TipoDePagoID = 3;
                pagoCarton.PagoID = debito.id;
                pagoCarton.FechaDePago = hoy;

                db.PagosCartonesVendidos.Add(pagoCarton);
                db.SaveChanges();
            }

            return View();
        }

        public Debito RealizarDebito(SolicitudDebitoRequest solicitudDebito, int CuotaDebitoId)
        {
            Debito debito = new Debito();
            //Respuesta de la Api
            string respuesta = "";

            //
            string solicitudDebito360Js = JsonConvert.SerializeObject(solicitudDebito);

            //Local
            Uri uri = new Uri("https://localhost:44382/api/Debit?debitRequest=" + HttpUtility.UrlEncode(solicitudDebito360Js));

            //Server
            //Uri uri = new Uri("http://localhost:90/api/Debit?debitRequest=" + HttpUtility.UrlEncode(solicitudDebito360Js));

            HttpWebRequest requestFile = (HttpWebRequest)WebRequest.Create(uri);

            requestFile.ContentType = "application/html";
            requestFile.Headers.Add("authorization", "Bearer OTllZDJlZjA3NmNlOWQ4NzYzYzYzNjljMjU3YTNmZGYxNTQ3MGIwZGI2MjIwNjc2MDJkYjNmNmRiNWUyNTcxOA");

            HttpWebResponse webResp = requestFile.GetResponse() as HttpWebResponse;

            if (requestFile.HaveResponse)
            {
                if (webResp.StatusCode == HttpStatusCode.OK || webResp.StatusCode == HttpStatusCode.Accepted)
                {
                    StreamReader respReader = new StreamReader(webResp.GetResponseStream(), Encoding.GetEncoding("utf-8"/*"iso-8859-1"*/));

                    respuesta = respReader.ReadToEnd();

                    SolicitudDebitoResponse debitoResponse = new SolicitudDebitoResponse();

                    //var jsonObject = JObject.Parse(response.Content);

                    debitoResponse = JsonConvert.DeserializeObject<SolicitudDebitoResponse>(respuesta);

                    debito.id = debitoResponse.id;
                    debito.type = debitoResponse.type;
                    debito.state = debitoResponse.state;
                    debito.created_at = debitoResponse.created_at;
                    debito.first_due_date = debitoResponse.first_due_date;
                    debito.first_total = debitoResponse.first_total;
                    debito.SecondDueDate = debitoResponse.SecondDueDate;
                    debito.SecondTotal = debitoResponse.SecondTotal;
                    debito.description = debitoResponse.description;
                    debito.AdhesionId = debitoResponse.AdhesionId;
                    debito.CuotaDebitoId = CuotaDebitoId;
                }
            }

            return debito;
        }

        /*****************************************************************************************/

        public ActionResult VentasACancelar(int page = 1)
        {
            int Año = DateTime.Today.Year;
            DateTime hoy = DateTime.Today;

            var Pagos = db.PagosCartonesVendidos.ToList();

            var Compras = db.CartonesVendidos.Where(x => x.PagoCancelado == false &&
                                                         x.PagoRealizdo == false)
                                             .ToList();

            Compras = Compras.Where(x => !Pagos.Any(y => y.CartonVendidoID == x.ID) ||
                                         Pagos.Any(y => y.CartonVendidoID == x.ID && y.Pagado == false)).ToList();

            foreach (var compra in Compras)
            {
                compra.DiasDesdeLaVenta = (hoy - compra.FechaVenta).Days;
            }

            return View(Compras.ToPagedList(page, 15));
        }

        public ActionResult CancelarVenta(int id, int page)
        {
            ViewBag.Page = page;

            var Compra = db.CartonesVendidos.Find(id);

            return View(Compra);
        }

        [HttpPost]
        [ActionName("CancelarVenta")]
        public ActionResult CancelarVentaConfirmed(int id, int page)
        {
            ViewBag.Page = page;

            var Compra = db.CartonesVendidos.Find(id);

            Compra.PagoCancelado = true;

            db.SaveChanges();

            string from = "no-reply@xn--sueocelestepago-0qb.com";
            string usuario = "no-reply@xn--sueocelestepago-0qb.com";
            string password = "GVbE3UMeME";

            string subject = "Sueño Celeste la Gran Promocion";

            string emailBody = ObtenerBodyEmailCompraCancelada(Compra.Carton.Numero);

            Email nuevoEmail = new Email();
            nuevoEmail.SendEmail(emailBody, Compra.Cliente.Email, subject);

            return RedirectToAction("VentasACancelar");
        }

        public ActionResult CancelarVentas(int Dias)
        {
            int Año = DateTime.Today.Year;
            DateTime hoy = DateTime.Today;
            List<CartonVendido> ComprasACancelar = new List<CartonVendido>();

            var Pagos = db.PagosCartonesVendidos.ToList();

            var Compras = db.CartonesVendidos.Where(x => x.PagoCancelado == false &&
                                                         x.PagoRealizdo == false)
                                             .ToList();

            Compras = Compras.Where(x => !Pagos.Any(y => y.CartonVendidoID == x.ID) ||
                                         Pagos.Any(y => y.CartonVendidoID == x.ID && y.Pagado == false)).ToList();

            foreach (var compra in Compras)
            {
                compra.DiasDesdeLaVenta = (hoy - compra.FechaVenta).Days;

                if (compra.DiasDesdeLaVenta >= Dias)
                {
                    ComprasACancelar.Add(compra);
                }
            }

            return View(ComprasACancelar);
        }

        [HttpPost]
        [ActionName("CancelarVentas")]
        public ActionResult CancelarVentasConfirmed(int Dias)
        {
            int Año = DateTime.Today.Year;
            DateTime hoy = DateTime.Today;
            List<CartonVendido> ComprasACancelar = new List<CartonVendido>();

            var Pagos = db.PagosCartonesVendidos.ToList();

            var Compras = db.CartonesVendidos.Where(x => x.PagoCancelado == false &&
                                                         x.PagoRealizdo == false)
                                             .ToList();

            Compras = Compras.Where(x => !Pagos.Any(y => y.CartonVendidoID == x.ID) ||
                                         Pagos.Any(y => y.CartonVendidoID == x.ID && y.Pagado == false)).ToList();

            string from = "no-reply@xn--sueocelestepago-0qb.com";
            string usuario = "no-reply@xn--sueocelestepago-0qb.com";
            string password = "GVbE3UMeME";

            string subject = "Sueño Celeste la Gran Promocion";

            foreach (var compra in Compras)
            {
                compra.DiasDesdeLaVenta = (hoy - compra.FechaVenta).Days;

                if (compra.DiasDesdeLaVenta >= Dias)
                {
                    compra.PagoCancelado = true;

                    string emailBody = ObtenerBodyEmailCompraCancelada(compra.Carton.Numero);

                    Email nuevoEmail = new Email();
                    nuevoEmail.SendEmail(emailBody, compra.Cliente.Email, subject);
                }
            }

            db.SaveChanges();

            return RedirectToAction("VentasACancelar");
        }

        /*****************************************************************************************/

        public FileContentResult Compras(int? Mes, int? Año)
        {
            List<CartonVendido> CartonesVendido = new List<CartonVendido>();

            string newFile = "";

            if (Mes != null && Año != null)
            {
                CartonesVendido = db.CartonesVendidos.Where(x => x.PagoRealizdo == true &&
                                                                 x.FechaPago.Value.Month == Mes &&
                                                                 x.FechaPago.Value.Year == Año).ToList();
                newFile = Server.MapPath("~/Archivos/Exportacion/compras/compras" + Mes + Año + ".xlsx");
            }
            else
            {
                return null;
            }

            var compras = (from oCompras in CartonesVendido
                           select new
                           {
                               oCompras.Carton.Numero,
                               oCompras.Cliente.NombreCompleto,
                               oCompras.Cliente.Dni,
                               oCompras.Cliente.Celular,
                               oCompras.Cliente.Email,
                               oCompras.FechaVenta,
                               oCompras.FechaPago
                           }).ToList();

            ExcelMapper mapper = new ExcelMapper();

            mapper.Save(newFile, compras, "SheetName", true);

            String mimeType = MimeMapping.GetMimeMapping(newFile);

            byte[] stream = System.IO.File.ReadAllBytes(newFile);

            return File(stream, mimeType);
        }

        /*****************************************************************************************/

        public void ActualizarEstado()
        {
            var pagosPendientes = db.Pagos.Where(x => x.state == "pending").ToList();

            foreach (var pagoPendiente in pagosPendientes)
            {
                var pago = ObtenerPago(pagoPendiente.id);

                pagoPendiente.state = pago.state;

                int pagoCartonID = int.Parse(pago.external_reference);

                var pagoCarton = db.PagosCartonesVendidos.Where(x => x.ID == pagoCartonID).FirstOrDefault();
                if(pagoCarton != null)
                {
                    if(pago.state == "paid")
                    {
                        pagoCarton.Pagado = true;
                        pagoCarton.FechaDePago = DateTime.Now;
                        pagoCarton.Pago = pago.first_total;

                        var CartonComprado = db.CartonesVendidos.Where(x => x.ID == pagoCarton.CartonVendidoID).FirstOrDefault();

                        CartonComprado.Pagos += pago.first_total;

                        if (Decimal.Parse(CartonComprado.Carton.Precio.ToString()) == CartonComprado.Pagos)
                        {
                            CartonComprado.PagoRealizdo = true;
                            CartonComprado.FechaPago = DateTime.Now;
                        }
                    }

                    db.SaveChanges();
                }
            }
        }

        public void ActualizarCompras()
        {
            var Compras = db.CartonesVendidos.Where(x => /*x.PagoCancelado == false &&*/ x.PagoRealizdo == false).ToList();

            foreach (var compra in Compras)
            {
                var PagosCompra = db.PagosCartonesVendidos.Where(x => x.CartonVendidoID == compra.ID).ToList();
                decimal pagos = 0;
                foreach (var pagoCompra in PagosCompra)
                {
                    string pagoCompraID = pagoCompra.ID.ToString();
                    if (pagoCompra.Pagado)
                    {
                        pagos += pagoCompra.Pago;

                        if (Decimal.Parse(compra.Carton.Precio.ToString()) == pagos)
                        {
                            compra.PagoRealizdo = true;
                            compra.FechaPago = DateTime.Now;
                            compra.Pagos = pagos;
                        }
                    }
                    else
                    {
                        var pago360 = db.Pagos.Where(x => x.external_reference == pagoCompraID).FirstOrDefault();
                        if (pago360 != null && pago360.state == "paid")
                        {
                            pagoCompra.Pagado = true;
                            pagoCompra.FechaDePago = DateTime.Now;
                            pagoCompra.Pago = pago360.first_total;

                            pagos += pago360.first_total;
                            //compra.Pagos += 

                            if (Decimal.Parse(compra.Carton.Precio.ToString()) == pagos)
                            {
                                compra.PagoRealizdo = true;
                                compra.FechaPago = DateTime.Now;
                                compra.Pagos = pagos;
                            }
                        }
                    }
                }
            }

            db.SaveChanges();
        }

        /*****************************************************************************************/
        
        public Pago ObtenerPago(int id)
        {
            //retorno
            Pago pago = new Pago();

            //Respuesta de la Api
            string respuesta = "";

            //Local
            //Uri uri = new Uri("https://localhost:44382/api/GetPayment360?id=" + id);

            //Server
            Uri uri = new Uri("http://localhost:90/api/GetPayment360?id=" + id);

            HttpWebRequest requestFile = (HttpWebRequest)WebRequest.Create(uri);

            requestFile.ContentType = "application/html";
            requestFile.Headers.Add("authorization", "Bearer OTllZDJlZjA3NmNlOWQ4NzYzYzYzNjljMjU3YTNmZGYxNTQ3MGIwZGI2MjIwNjc2MDJkYjNmNmRiNWUyNTcxOA");

            HttpWebResponse webResp = requestFile.GetResponse() as HttpWebResponse;

            if (requestFile.HaveResponse)
            {
                if (webResp.StatusCode == HttpStatusCode.OK || webResp.StatusCode == HttpStatusCode.Accepted)
                {
                    StreamReader respReader = new StreamReader(webResp.GetResponseStream(), Encoding.GetEncoding("utf-8"/*"iso-8859-1"*/));

                    respuesta = respReader.ReadToEnd();

                    Pago360Response payment = new Pago360Response();

                    //var jsonObject = JObject.Parse(response.Content);

                    payment = JsonConvert.DeserializeObject<Pago360Response>(respuesta);

                    pago.id = payment.id;
                    pago.type = payment.type;
                    pago.state = payment.state;
                    pago.created_at = payment.created_at;
                    pago.external_reference = payment.external_reference;
                    pago.payer_name = payment.payer_name;
                    pago.payer_email = payment.payer_email;
                    pago.description = payment.description;
                    pago.first_due_date = payment.first_due_date;
                    pago.first_total = payment.first_total;
                    pago.second_due_date = payment.second_due_date;
                    pago.second_total = payment.second_total;
                    pago.barcode = payment.barcode;
                    pago.checkout_url = payment.checkout_url;
                    pago.barcode_url = payment.barcode_url;
                    pago.pdf_url = payment.pdf_url;
                    pago.excluded_channels = (payment.excluded_channels != null) ? string.Join(";", payment.excluded_channels) : "";
                }
            }

            //requestFile.Headers.Add("authorization", "Bearer OTllZDJlZjA3NmNlOWQ4NzYzYzYzNjljMjU3YTNmZGYxNTQ3MGIwZGI2MjIwNjc2MDJkYjNmNmRiNWUyNTcxOA");

            return pago;
        }

        public bool CambiarEstado(Webhook pwebhook)
        {
            bool cambioEstado = false;
            try
            {
                switch (pwebhook.entity_name)
                {
                    case "payment_request":
                        var pago = db.Pagos.Where(x => x.id == pwebhook.entity_id).FirstOrDefault();

                        pago.state = pwebhook.type;

                        int pagoID = int.Parse(pago.external_reference);

                        var pagoCarton = db.PagosCartonesVendidos.Where(x => x.ID == pagoID).FirstOrDefault();

                        if (pwebhook.type == "paid")
                        {
                            pagoCarton.Pagado = true;
                            pagoCarton.FechaDePago = DateTime.Now;
                            pagoCarton.Pago = pago.first_total;

                            if (pagoCarton.CuotaPlanID != 0)
                            {
                                var CuotaPlan = db.CuotasPlanDePagos.Where(x => x.ID == pagoCarton.CuotaPlanID).FirstOrDefault();

                                CuotaPlan.CuotaPagada = true;
                                CuotaPlan.PagoID = pagoID;
                            }
                        }

                        var CartonComprado = db.CartonesVendidos.Where(x => x.ID == pagoCarton.CartonVendidoID).FirstOrDefault();

                        CartonComprado.Pagos += pago.first_total;

                        if (Decimal.Parse(CartonComprado.Carton.Precio.ToString()) - 1 == CartonComprado.Pagos)
                        {
                            CartonComprado.PagoRealizdo = true;
                            CartonComprado.FechaPago = DateTime.Now;

                            var Carton = db.Cartones.Where(x => x.ID == CartonComprado.CartonID).FirstOrDefault();
                            var Cliente = db.Clientes.Where(x => x.ID == CartonComprado.ClienteID).FirstOrDefault();

                            string from = "no-reply@xn--sueocelestepago-0qb.com";
                            string usuario = "no-reply@xn--sueocelestepago-0qb.com";
                            string password = "GVbE3UMeME";

                            string subject = "Sueño Celeste la Gran Promocion";

                            string emailBody = "";

                            //Enviar Correo pago Completado
                            if (CartonComprado.TipoDePagoID == 1)
                            {
                                emailBody = ObtenerBodyEmailCompraPagoContado(Carton.Numero);
                            }
                            else if (CartonComprado.TipoDePagoID == 2)
                            {
                                emailBody = ObtenerBodyEmailCompraPagoCuotas(Carton.Numero);
                            }

                            Email nuevoEmail = new Email();
                            nuevoEmail.SendEmail(emailBody, Cliente.Email, subject);
                        }

                        db.SaveChanges();
                        cambioEstado = true;
                        break;
                    case "adhesion":
                        var adhesion = db.Adhesiones.Where(x => x.id == pwebhook.entity_id).FirstOrDefault();

                        adhesion.state = pwebhook.type;

                        db.SaveChanges();

                        cambioEstado = true;
                        break;
                    case "debit_request":
                        var debito = db.Debitos.Where(x => x.id == pwebhook.entity_id).FirstOrDefault();

                        debito.state = pwebhook.type;

                        db.SaveChanges();


                        var pagoCartonDebito = db.PagosCartonesVendidos.Where(x => x.PagoID == debito.id).FirstOrDefault();

                        if (pwebhook.type == "paid")
                        {
                            pagoCartonDebito.Pagado = true;
                            pagoCartonDebito.FechaDePago = DateTime.Now;
                            pagoCartonDebito.Pago = debito.first_total;
                        }

                        var cuotaDebito = db.CuotasDebito.Where(x => x.ID == debito.CuotaDebitoId).FirstOrDefault();

                        if (pwebhook.type == "paid")
                        {
                            cuotaDebito.CuotaPagada = true;
                            cuotaDebito.FechaPago = DateTime.Now;

                            db.SaveChanges();
                        }

                        var cuotasImpagas = db.CuotasDebito.Where(x => x.CartonVendidoID == cuotaDebito.CartonVendidoID && x.CuotaPagada == false).ToList();

                        if (cuotasImpagas.Count < 1)
                        {
                            var cartonDebito = db.CartonesVendidos.Where(x => x.ID == cuotaDebito.CartonVendidoID).FirstOrDefault();

                            cartonDebito.PagoRealizdo = true;
                            cartonDebito.FechaPago = DateTime.Now;

                            db.SaveChanges();
                        }

                        cambioEstado = true;
                        break;
                    default:
                        break;
                }
                return cambioEstado;
            }
            catch (Exception e)
            {
                cambioEstado = false;
                return cambioEstado;
            }
        }

        public string ObtenerBodyEmailCompraPagoContado(string NroCarton)
        {
            string body = "<meta charset='utf-8'><style type='text/css'> @media only screen and (max-width: 480px) { table { display: block !important; width: 100% !important; } td { width: 480px !important; } }</style><body style='font-family: 'Malgun Gothic', Arial, sans-serif; margin: 0; padding: 0; width: 100%; -webkit-text-size-adjust: none; -webkit-font-smoothing: antialiased;'> <table width='100%' bgcolor='#FFFFFF' border='0' cellspacing='0' cellpadding='0' id='background' style='height: 100% !important; margin: 0; padding: 0; width: 100% !important;'> <tr> <td align='center' valign='top'> <table width='600' border='0' bgcolor='#FFFFFF' cellspacing='0' cellpadding='0' id='header_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' bgcolor='#474544' cellspacing='0' cellpadding='0' id='header'> <tr> <td valign='top' class='header_content'> <h1 style='color: #F4F4F4; font-size: 24px; text-align: center;'> </h1> </td> </tr> </table> <!-- // END #header --> </td> </tr> </table> <!-- // END #header_container --> <table width='600' border='0' bgcolor='#26abff' cellspacing='0' cellpadding='20' id='preheader'> <tr> <td valign='top'> <table width='100%' border='0' cellspacing='0' cellpadding='0'> <tr> <td valign='top' width='600'> <div class='logo' style='text-align: center;'> <a href='javascript:void(0)'> <img src='https://www.xn--sueoceleste-3db.com/assets/images/logo.png'> </a> </div> </td> </tr> </table> </td> </tr> </table> <!-- // END #preheader --> <table width='600' border='0' bgcolor='#FFFFFF' cellspacing='0' cellpadding='0' id='header_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' bgcolor='#474544' cellspacing='0' cellpadding='0' id='header'> <tr> <td valign='top' class='header_content'> <h1 style='color: #F4F4F4; font-size: 24px; text-align: center;'> </h1> </td> </tr> </table> <!-- // END #header --> </td> </tr> </table> <!-- // END #header_container --> <table width='600' border='0' bgcolor='#f2f2f2' cellspacing='0' cellpadding='20' id='body_container'> <tr> <td align='center' valign='top' class='body_content'> <table width='100%' border='0' cellspacing='0' cellpadding='20'> <tr> <td valign='top'> <h2 style='font-size: 28px; text-align: justify;'>Notificación por Compra de Cartón de Sueño Celeste</h2> <p style='font-size: 16px; line-height: 22px; text-align: justify;'> Este aviso automático es para confirmar que la compra del cartón Número [NumeroCarton] se ha realizado con éxito. <br><br> En breve, le estaremos enviando el cartón impreso a su domicilio. <br><br> Muchas gracias por su compra. <br><br> Atentamente <br><br> <b>Sueño Celeste</b> </p> </td> </tr> </table> </td> </tr> </table> <!-- // END #body_container --> <table width='600' border='0' bgcolor='#26abff' cellspacing='0' cellpadding='20' id='contact_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' cellspacing='0' cellpadding='20' id='contact'> <tr> <td> <p style='color: #F4F4F4; font-size: 14px; line-height: 22px; text-align: center;'> San Martín 295, Morteros, Córdoba, Argentina <br> sceleste_mutual9@hotmail.com <br> +54 (3562) 404455 </p> </td> </tr> </table> <!-- // END #contact --> </td> </tr> </table> <!-- // END #contact_container --> </td> </tr> </table> <!-- // END #background --></body>";

            body = body.Replace("[NumeroCarton]", NroCarton);

            return body;
        }
        public string ObtenerBodyEmailCompraPagoCuotas(string NroCarton)
        {
            string body = "<meta charset='utf-8'><style type='text/css'> @media only screen and (max-width: 480px) { table { display: block !important; width: 100% !important; } td { width: 480px !important; } }</style><body style='font-family: 'Malgun Gothic', Arial, sans-serif; margin: 0; padding: 0; width: 100%; -webkit-text-size-adjust: none; -webkit-font-smoothing: antialiased;'> <table width='100%' bgcolor='#FFFFFF' border='0' cellspacing='0' cellpadding='0' id='background' style='height: 100% !important; margin: 0; padding: 0; width: 100% !important;'> <tr> <td align='center' valign='top'> <table width='600' border='0' bgcolor='#FFFFFF' cellspacing='0' cellpadding='0' id='header_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' bgcolor='#474544' cellspacing='0' cellpadding='0' id='header'> <tr> <td valign='top' class='header_content'> <h1 style='color: #F4F4F4; font-size: 24px; text-align: center;'> </h1> </td> </tr> </table> <!-- // END #header --> </td> </tr> </table> <!-- // END #header_container --> <table width='600' border='0' bgcolor='#26abff' cellspacing='0' cellpadding='20' id='preheader'> <tr> <td valign='top'> <table width='100%' border='0' cellspacing='0' cellpadding='0'> <tr> <td valign='top' width='600'> <div class='logo' style='text-align: center;'> <a href='javascript:void(0)'> <img src='https://www.xn--sueoceleste-3db.com/assets/images/logo.png'> </a> </div> </td> </tr> </table> </td> </tr> </table> <!-- // END #preheader --> <table width='600' border='0' bgcolor='#FFFFFF' cellspacing='0' cellpadding='0' id='header_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' bgcolor='#474544' cellspacing='0' cellpadding='0' id='header'> <tr> <td valign='top' class='header_content'> <h1 style='color: #F4F4F4; font-size: 24px; text-align: center;'> </h1> </td> </tr> </table> <!-- // END #header --> </td> </tr> </table> <!-- // END #header_container --> <table width='600' border='0' bgcolor='#f2f2f2' cellspacing='0' cellpadding='20' id='body_container'> <tr> <td align='center' valign='top' class='body_content'> <table width='100%' border='0' cellspacing='0' cellpadding='20'> <tr> <td valign='top'> <h2 style='font-size: 24px; text-align: justify;'>Notificación de Pago en Cuotas Finalizado</h2> <p style='font-size: 16px; line-height: 22px; text-align: justify;'> Este aviso automático es para informar que se ha completado el plan de pago en cuotas por la compra del cartón Número [NumeroCarton] de Sueño Celeste. <br><br> En breve, le estaremos enviando el cartón impreso a su domicilio. <br><br> Muchas gracias. <br><br> Atentamente <br><br> <b>Sueño Celeste</b> </p> </td> </tr> </table> </td> </tr> </table> <!-- // END #body_container --> <table width='600' border='0' bgcolor='#26abff' cellspacing='0' cellpadding='20' id='contact_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' cellspacing='0' cellpadding='20' id='contact'> <tr> <td> <p style='color: #F4F4F4; font-size: 14px; line-height: 22px; text-align: center;'> San Martín 295, Morteros, Córdoba, Argentina <br> sceleste_mutual9@hotmail.com <br> +54 (3562) 404455 </p> </td> </tr> </table> <!-- // END #contact --> </td> </tr> </table> <!-- // END #contact_container --> </td> </tr> </table> <!-- // END #background --></body>";

            body = body.Replace("[NumeroCarton]", NroCarton);

            return body;
        }

        public string ObtenerBodyEmailCompraCancelada(string NroCarton)
        {
            string body = "<style type='text/css'> @media only screen and (max-width: 480px) { table { display: block !important; width: 100% !important; } td { width: 480px !important; } }</style><body style='font-family: 'Malgun Gothic', Arial, sans-serif; margin: 0; padding: 0; width: 100%; -webkit-text-size-adjust: none; -webkit-font-smoothing: antialiased;'> <table width='100%' bgcolor='#FFFFFF' border='0' cellspacing='0' cellpadding='0' id='background' style='height: 100% !important; margin: 0; padding: 0; width: 100% !important;'> <tr> <td align='center' valign='top'> <table width='600' border='0' bgcolor='#FFFFFF' cellspacing='0' cellpadding='0' id='header_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' bgcolor='#474544' cellspacing='0' cellpadding='0' id='header'> <tr> <td valign='top' class='header_content'> <h1 style='color: #F4F4F4; font-size: 24px; text-align: center;'> </h1> </td> </tr> </table> </td> </tr> </table> <table width='600' border='0' bgcolor='#26abff' cellspacing='0' cellpadding='20' id='preheader'> <tr> <td valign='top'> <table width='100%' border='0' cellspacing='0' cellpadding='0'> <tr> <td valign='top' width='600'> <div class='logo' style='text-align: center;'> <a href='javascript:void(0)'> <img src='https://www.xn--sueoceleste-3db.com/assets/images/logo.png'> </a> </div> </td> </tr> </table> </td> </tr> </table> <table width='600' border='0' bgcolor='#FFFFFF' cellspacing='0' cellpadding='0' id='header_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' bgcolor='#474544' cellspacing='0' cellpadding='0' id='header'> <tr> <td valign='top' class='header_content'> <h1 style='color: #F4F4F4; font-size: 24px; text-align: center;'> </h1> </td> </tr> </table> </td> </tr> </table> <table width='600' border='0' bgcolor='#f2f2f2' cellspacing='0' cellpadding='20' id='body_container'> <tr> <td align='center' valign='top' class='body_content'> <table width='100%' border='0' cellspacing='0' cellpadding='20'> <tr> <td valign='top'> <h2 style='font-size: 24px; text-align: justify;'>Notificación de Cancelación de Compra</h2> <p style='font-size: 16px; line-height: 22px; text-align: justify;'> Este aviso automático es para informar que ha cancelado la compra del cartón Número [NumeroCarton] de Sueño Celeste. <br><br> Atentamente <br><br> <b>Sueño Celeste</b> </p> </td> </tr> </table> </td> </tr> </table> <table width='600' border='0' bgcolor='#26abff' cellspacing='0' cellpadding='20' id='contact_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' cellspacing='0' cellpadding='20' id='contact'> <tr> <td> <p style='color: #F4F4F4; font-size: 14px; line-height: 22px; text-align: center;'> San Martín 295, Morteros, Córdoba, Argentina <br> sceleste_mutual9@hotmail.com <br> +54 (3562) 404455 </p> </td> </tr> </table> </td> </tr> </table> </td> </tr> </table></body>";

            body = body.Replace("[NumeroCarton]", NroCarton);

            return body;
        }

        /*****************************************************************************************/


    }
}