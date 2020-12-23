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
using Ganss.Excel;

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

            return View(Compras.ToPagedList(page,15));
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

            foreach (var compra in Compras)
            {
                compra.DiasDesdeLaVenta = (hoy - compra.FechaVenta).Days;

                if (compra.DiasDesdeLaVenta >= Dias)
                {
                    compra.PagoCancelado = true;
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
            var Compras = db.CartonesVendidos.Where(x => x.PagoCancelado == false && x.PagoRealizdo == false).ToList();

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
                        if (pago360.state == "paid")
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
    }
}