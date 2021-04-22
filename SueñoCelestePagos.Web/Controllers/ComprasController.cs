using Newtonsoft.Json;
using SueñoCelestePagos.Dal;
using SueñoCelestePagos.Entities;
using SueñoCelestePagos.Entities.Pago360.Request;
using SueñoCelestePagos.Entities.Pago360.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Newtonsoft.Json.Linq;
using SueñoCelestePagos.Entities.Pago360;
using SueñoCelestePagos.Utilities;

namespace SueñoCelestePagos.Web.Controllers
{
    public struct Cuotas
    {
        public int key { get; set; }
        public string value { get; set; }
    }

    public class ComprasController : Controller
    {
        private SueñoCelestePagosContext db = new SueñoCelestePagosContext();

        /***************************************************************************************/

        // GET: Compras
        public ActionResult ComprobarCompra()
        {
            DateTime hoy = DateTime.Now;

            var Cliente = ObtenerCliente();

            if (Cliente != null)
            {
                var compra = db.CartonesVendidos.Include(t => t.Carton).Where(x => x.ClienteID == Cliente.ID && x.Carton.Año == hoy.Year && x.PagoCancelado == false).FirstOrDefault();

                if (compra != null) //Check si el cliente ya compro el carton de este año
                {

                    switch (compra.TipoDePagoID)
                    {
                        case 1://Un Pago
                            {
                                if (compra.PagoRealizdo)
                                {
                                    return RedirectToAction("PagoRealizado");
                                }
                                else
                                {
                                    return RedirectToAction("PagoPendiente");
                                }
                            }
                        case 2://Plan de pago Manual
                            {
                                if (compra.PagoRealizdo)
                                {
                                    return RedirectToAction("PagoRealizado");
                                }
                                else
                                {
                                    return RedirectToAction("PlanDePago");
                                }
                            }
                        case 3://Plan de pago Debito Automatico
                            {
                                var adhesion = db.Adhesiones.Where(x => x.external_reference == compra.ID.ToString()).FirstOrDefault();
                                if (adhesion == null)
                                {
                                    return RedirectToAction("Adherirse", new { compra.CantCuotas });
                                }
                                switch (adhesion.state)
                                {
                                    case "signed":
                                        {
                                            if (compra.PagoRealizdo)
                                            {
                                                return RedirectToAction("DebitoRealizado");
                                            }
                                            else
                                            {
                                                return RedirectToAction("DebitoPendiente");
                                            }
                                            //return RedirectToAction("AdhesionRealizada");
                                        }
                                    //case "canceled":
                                    //    {
                                    //        if (compra.PagoRealizdo)
                                    //        {
                                    //            return RedirectToAction("DebitoRealizado");
                                    //        }
                                    //        else
                                    //        {
                                    //            return RedirectToAction("DebitoPendiente");
                                    //        }
                                    //        //return RedirectToAction("AdhesionCancelada");
                                    //    }
                                    default:
                                        return RedirectToAction("index");
                                }
                            }
                        default:
                            {
                                return RedirectToAction("index");
                            }
                    }
                }
                else
                {
                    return RedirectToAction("ElegirCarton");
                }
            }
            else
            {
                string dni = Session["ClienteDni"].ToString();

                string sexo = Session["ClienteSexo"].ToString();

                var CompraOffline = db.CompradoresSinCuentas.Where(x => x.dni == dni && x.Año == hoy.Year).FirstOrDefault();

                if (CompraOffline == null)
                {
                    return RedirectToAction("ElegirCarton");
                }
                else
                {
                    return RedirectToAction("ErrorCompra", new { MensajeError = "Ya hay una Compra registrada con este numero de dni." });
                }
            }

        }

        /***************************************************************************************/

        public ActionResult ElegirCarton(int? SearchType, string SearchString)
        {
            DateTime hoy = DateTime.Now;
            if (Session["ClienteDni"] == null || string.IsNullOrEmpty(Session["ClienteDni"].ToString()))
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/ElegirCarton" });
            }

            //var FechaLimite = db.FechaLimiteVentaCartones.Where(x => x.Año == hoy.Year).FirstOrDefault();
            //if(FechaLimite.Fecha > hoy)
            //{
            //    return RedirectToAction("Identificarse", "Compras");
            //}

            string dni = Session["ClienteDni"].ToString();
            string telefono = Session["ClienteTelefono"].ToString();
            string sexo = Session["ClienteSexo"].ToString();

            string area = telefono.Substring(0, telefono.IndexOf('-'));
            string numero = telefono.Substring(telefono.IndexOf('-') + 1);

            var reservado = db.CartonesReservados.Where(x => x.Dni == dni && x.Sexo == sexo && x.FechaReserva < hoy && x.FechaExpiracionReserva > hoy).FirstOrDefault();

            if (reservado != null)
            {
                Session["ReservaCarton"] = reservado.ID;

                var Cliente = ObtenerCliente();

                if (Cliente == null)
                {
                    return RedirectToAction("RegistroDatos", "Clientes");
                }
                else
                {
                    return RedirectToAction("index", "Compras");
                }
            }

            #region NumerosCartones Disponibles
            var listaCartones = ObtenerCartonesDisponibles(SearchType, SearchString);

            ViewBag.NumeroCarton = new SelectList(listaCartones, "ID", "Numero");
            #endregion

            return View();
        }

        [HttpPost]
        public ActionResult ReservarCarton(int NumeroCarton)
        {
            DateTime hoy = DateTime.Now;

            if (Session["ClienteDni"] == null)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/ElegirCarton" });
            }

            string dni = Session["ClienteDni"].ToString();
            string telefono = Session["ClienteTelefono"].ToString();
            string sexo = Session["ClienteSexo"].ToString();

            string area = telefono.Substring(0, telefono.IndexOf('-'));
            string numero = telefono.Substring(telefono.IndexOf('-') + 1);

            var reservado = db.CartonesReservados.Where(x => x.CartonID == NumeroCarton && x.FechaReserva < hoy && x.FechaExpiracionReserva > hoy).FirstOrDefault();

            if (reservado != null)
            {
                ViewBag.MensajeError = "El Carton esta Reservado o Comprado";

                return RedirectToAction("ElegirCarton", new { MensajeError = "El Carton esta Reservado o Comprado" });
            }

            CartonReservado cartonReservado = db.CartonesReservados.Where(x => x.Dni == dni && x.Sexo == sexo).FirstOrDefault();

            if (cartonReservado == null) cartonReservado = new CartonReservado();

            cartonReservado.CartonID = NumeroCarton;
            cartonReservado.Dni = dni;
            cartonReservado.Sexo = sexo;
            cartonReservado.FechaReserva = DateTime.Now;
            cartonReservado.FechaExpiracionReserva = DateTime.Now.AddMinutes(10);

            if (cartonReservado.ID == 0) db.CartonesReservados.Add(cartonReservado);

            db.SaveChanges();

            Session["ReservaCarton"] = cartonReservado.ID;

            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                return RedirectToAction("RegistroDatos", "Clientes");
            }
            else
            {
                return RedirectToAction("index", "Compras");
            }
        }

        [HttpPost]
        public ActionResult CancelarReserva()
        {
            try
            {
                DateTime hoy = DateTime.Now;

                var Cliente = ObtenerCliente();
                if (Cliente == null)
                {
                    Cliente = new Cliente();

                    Cliente.Dni = Session["ClienteDni"].ToString();
                    Cliente.Sexo = Session["ClienteSexo"].ToString();
                }

                var cartonReservado = db.CartonesReservados.Where(x => x.Dni == Cliente.Dni && x.Sexo == Cliente.Sexo).FirstOrDefault();

                db.CartonesReservados.Remove(cartonReservado);

                db.SaveChanges();

                return RedirectToAction("ErrorCompra", new { MensajeError = "La Reserva del Carton Expiro" });
            }
            catch (Exception e)
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "La Reserva del Carton Expiro" });
            }
        }

        /***************************************************************************************/

        public ActionResult Index()
        {
            DateTime hoy = DateTime.Now;

            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/Index" });
            }

            int CartonReservadoId = 0;

            if (!int.TryParse(Session["ReservaCarton"].ToString(), out CartonReservadoId))
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "Ocurrio un Error, Por Favor intente mas tarde" });
            }

            var cartonReservado = db.CartonesReservados.Where(x => x.ID == CartonReservadoId).FirstOrDefault();

            if (cartonReservado == null || cartonReservado.FechaExpiracionReserva <= DateTime.Now)
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "La Reserva del Carton Expiro" });
            }

            var reservado = db.CartonesReservados.Where(x => x.Dni == Cliente.Dni && x.Sexo == Cliente.Sexo && x.FechaReserva < hoy && x.FechaExpiracionReserva > hoy).FirstOrDefault();

            var tiempoRestante = reservado.FechaExpiracionReserva - hoy;

            ViewBag.Expira = tiempoRestante.Minutes.ToString().PadLeft(2, '0') + ":" + tiempoRestante.Seconds.ToString().PadLeft(2, '0');

            var cuotasManual = ObtenerCuotasPosibles(cartonReservado.CartonID);

            var cuotasDebito = ObtenerCuotasDebitoPosibles(cartonReservado.CartonID);

            ViewBag.CantCuotasManual = new SelectList(cuotasManual, "key", "value");

            ViewBag.CantCuotasDebito = new SelectList(cuotasDebito, "key", "value");

            ViewBag.TipoDePago = new SelectList(db.TiposDePago.Where(x => x.Activo), "ID", "Descripcion");

            return View();
        }

        [HttpPost]
        public ActionResult Index(int TipoDePago, int InstitucionId, int CantCuotas = 0)
        {
            int CartonVendidoId = 0, PagoCartonId = 0;
            int[] CuotasPlanDePagoId = new int[CantCuotas];
            Pago pago = new Pago();

            DateTime hoy = DateTime.Now;
            string url = "https://www.sueñocelestepago.com.ar/";

            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/Index" });
            }

            int CartonReservadoId = 0;

            if (!int.TryParse(Session["ReservaCarton"].ToString(), out CartonReservadoId))
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "Ocurrio un Error, Por Favor intente mas tarde" });
            }

            var cartonReservado = db.CartonesReservados.Where(x => x.ID == CartonReservadoId && x.Dni == Cliente.Dni).Include("Carton").FirstOrDefault();

            if (cartonReservado == null)
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "Ocurrio un Error, Por Favor intente mas tarde" });
            }

            if (cartonReservado.FechaExpiracionReserva <= DateTime.Now)
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "La Reserva del Carton Expiro" });
            }

            try
            {
                var Carton = db.Cartones.Where(x => x.ID == cartonReservado.CartonID).FirstOrDefault();

                CartonVendido cartonVendido = new CartonVendido();

                cartonVendido.CartonID = Carton.ID;
                cartonVendido.ClienteID = Cliente.ID;
                cartonVendido.TipoDePagoID = TipoDePago;
                cartonVendido.FechaVenta = hoy;
                cartonVendido.CantCuotas = CantCuotas;
                cartonVendido.EntidadID = InstitucionId;

                db.CartonesVendidos.Add(cartonVendido);
                db.SaveChanges();

                CartonVendidoId = cartonVendido.ID;

                #region Envio de Correo
                try
                {
                    string from = "no-reply@xn--sueocelestepago-0qb.com";
                    string usuario = "no-reply@xn--sueocelestepago-0qb.com";
                    string password = "GVbE3UMeME";

                    string subject = "Sueño Celeste la Gran Promocion";

                    string emailBody = ObtenerBodyEmailCompraPagoContado(Carton.Numero);

                    Email nuevoEmail = new Email();
                    nuevoEmail.SendEmail(emailBody, Cliente.Email, subject);
                }
                catch (Exception)
                {

                }
                #endregion

                if (cartonVendido.TipoDePagoID == 1)
                {
                    PagoCartonVendido pagoCarton = new PagoCartonVendido();

                    pagoCarton.CartonVendidoID = cartonVendido.ID;
                    pagoCarton.TipoDePagoID = 1;
                    pagoCarton.FechaDePago = hoy;

                    db.PagosCartonesVendidos.Add(pagoCarton);
                    db.SaveChanges();

                    PagoCartonId = pagoCarton.ID;

                    Pago360Request pago360 = new Pago360Request();

                    var FechaDeVencimiento = db.FechasDeVencimiento.Where(x => x.Mes == hoy.Month && x.Año == hoy.Year).FirstOrDefault();

                    if (hoy.Month == 12 && hoy > FechaDeVencimiento.PrimerVencimiento)
                    {
                        hoy = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(1);
                        FechaDeVencimiento = db.FechasDeVencimiento.Where(x => x.Mes == hoy.Month && x.Año == hoy.Year).FirstOrDefault();
                    }

                    if (hoy.Day > FechaDeVencimiento.PrimerVencimiento.Day)
                    {
                        FechaDeVencimiento = db.FechasDeVencimiento.Where(x => x.Mes == (hoy.Month + 1) && x.Año == hoy.Year).FirstOrDefault();
                    }

                    var numeroCarton = db.Cartones.Where(x => x.ID == cartonReservado.CartonID).FirstOrDefault();
                    var entidad = db.Instituciones.Find(cartonVendido.EntidadID);

                    pago360.description = "Pago Total del Carton Nro°: " + numeroCarton.Numero + " - " + entidad.Nombre;
                    pago360.first_due_date = FechaDeVencimiento.PrimerVencimiento.ToString("dd-MM-yyyy");
                    pago360.first_total = Carton.Precio;
                    pago360.second_due_date = FechaDeVencimiento.SegundoVencimiento.ToString("dd-MM-yyyy");
                    pago360.second_total = Carton.Precio;
                    pago360.payer_name = Cliente.NombreCompleto;
                    pago360.external_reference = pagoCarton.ID.ToString();//cartonVendido.ID.ToString();
                    pago360.payer_email = Cliente.Email;
                    pago360.back_url_success = url;// + "/PagoRealizado";
                    pago360.back_url_pending = url;// + "/PagoPendiente";
                    pago360.back_url_rejected = url;// + "/PagoCancelado";
                    //pago360.excluded_channels = new string[] { "credit_card" };
                    try
                    {
                        pago = Pagar(pago360);

                        db.Pagos.Add(pago);

                        //cartonVendido.PagoID = pago.id;

                        //pagoCarton.PagoID = pago.id;
                        //pagoCarton.Pago = pago.first_total;

                        var ReservaCarton = db.CartonesReservados.Where(x => x.ID == CartonReservadoId).FirstOrDefault();

                        db.CartonesReservados.Remove(ReservaCarton);

                        db.SaveChanges();

                        return Redirect(pago.checkout_url);
                    }
                    catch (Exception e)
                    {
                        if (PagoCartonId != 0)
                        {
                            db.PagosCartonesVendidos.Remove(pagoCarton);
                        }

                        if (CartonVendidoId != 0)
                        {
                            db.CartonesVendidos.Remove(cartonVendido);
                        }
                        db.SaveChanges();
                        return RedirectToAction("ErrorCompra", new { MensajeError = "Ocurrio un Error, Por Favor intente mas tarde" });
                    }
                }
                else if (cartonVendido.TipoDePagoID == 2)//Plan de Pagos Manual
                {
                    var ReservaCarton = db.CartonesReservados.Where(x => x.ID == CartonReservadoId).FirstOrDefault();

                    db.CartonesReservados.Remove(ReservaCarton);

                    //var FechasDeVencimiento = db.FechasDeVencimiento.Where(x => x.Mes == hoy.Month && x.Año == hoy.Year).FirstOrDefault();

                    //if (hoy.Month == 12 && hoy > FechasDeVencimiento.PrimerVencimiento)
                    //{
                    //    hoy = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(1);
                    //    FechasDeVencimiento = db.FechasDeVencimiento.Where(x => x.Mes == hoy.Month && x.Año == hoy.Year).FirstOrDefault();
                    //}

                    //if (hoy.Day > FechaDeVencimiento.PrimerVencimiento.Day)
                    //{
                    //    FechaDeVencimiento = db.FechasDeVencimiento.Where(x => x.Mes == (hoy.Month + 1) && x.Año == hoy.Year).FirstOrDefault();
                    //}

                    int MesCuota = hoy.Month;//FechasDeVencimiento.Mes;

                    for (int cuota = 1; cuota <= CantCuotas; cuota++)
                    {
                        if (MesCuota == 13)
                        {
                            MesCuota = 1;
                        }
                        CuotasPlanDePago cuotaPlanDePago = new CuotasPlanDePago();

                        cuotaPlanDePago.CartonVendidoID = cartonVendido.ID;
                        cuotaPlanDePago.NroCuota = cuota;
                        cuotaPlanDePago.MesCuota = MesCuota;
                        cuotaPlanDePago.AñoCuota = Carton.Año;

                        var vencimientos = db.FechasDeVencimiento.Where(x => x.Mes == MesCuota && x.Año == Carton.Año).FirstOrDefault();

                        cuotaPlanDePago.PrimerVencimiento = vencimientos.PrimerVencimiento;
                        cuotaPlanDePago.PrimerPrecioCuota = (Carton.Precio / CantCuotas);

                        cuotaPlanDePago.SeguntoVencimiento = vencimientos.SegundoVencimiento;
                        cuotaPlanDePago.SeguntoPrecioCuota = (Carton.Precio / CantCuotas);

                        db.CuotasPlanDePagos.Add(cuotaPlanDePago);
                        db.SaveChanges();

                        CuotasPlanDePagoId[cuota - 1] = cuotaPlanDePago.ID;
                        MesCuota++;
                    }

                    return RedirectToAction("PlanDePago");
                }
                else if (cartonVendido.TipoDePagoID == 3)//Plan de Pagos Debito
                {
                    return RedirectToAction("Adherirse", new { CantCuotas });
                }
            }
            catch (Exception e)
            {
                var CartonVendido = db.CartonesVendidos.Find(CartonVendidoId);
                db.SaveChanges();
                throw;
            }

            return View();
        }

        /***************************************************************************************/

        public ActionResult Adherirse(int CantCuotas)
        {
            DateTime hoy = DateTime.Now;
            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/ComprobarCompra" });
            }

            int CartonReservadoId = 0;

            if (!int.TryParse(Session["ReservaCarton"].ToString(), out CartonReservadoId))
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "Ocurrio un Error, Por Favor intente mas tarde" });
            }

            var cartonReservado = db.CartonesReservados.Where(x => x.ID == CartonReservadoId).FirstOrDefault();

            if (cartonReservado.FechaExpiracionReserva <= DateTime.Now)
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "La Reserva del Carton Expiro" });
            }

            var reservado = db.CartonesReservados.Where(x => x.Dni == Cliente.Dni && x.Sexo == Cliente.Sexo && x.FechaReserva < hoy && x.FechaExpiracionReserva > hoy).FirstOrDefault();

            var tiempoRestante = reservado.FechaExpiracionReserva - hoy;

            ViewBag.Expira = tiempoRestante.Minutes.ToString().PadLeft(2, '0') + ":" + tiempoRestante.Seconds.ToString().PadLeft(2, '0');

            ViewBag.CantCuotas = CantCuotas;

            return View(Cliente);
        }

        [HttpPost]
        public ActionResult Adherirse(int CantCuotas, string Email, string adhesion_holder_name, long cbu_holder_id_number, string cbu_number)
        {
            DateTime hoy = DateTime.Now;

            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/ComprobarCompra" });
            }

            int CartonReservadoId = 0;

            if (!int.TryParse(Session["ReservaCarton"].ToString(), out CartonReservadoId))
            {
                return RedirectToAction("ErrorCompra", new { MensajeError = "Ocurrio un Error, Por Favor intente mas tarde" });
            }

            AdhesionPago360Request adhesionPago360 = new AdhesionPago360Request();

            var CartonComprado = db.CartonesVendidos.Where(x => x.ClienteID == Cliente.ID && x.FechaVenta.Year == hoy.Year && x.PagoCancelado == false).FirstOrDefault();

            var ReservaCarton = db.CartonesReservados.Where(x => x.ID == CartonReservadoId).FirstOrDefault();

            db.CartonesReservados.Remove(ReservaCarton);

            var FechaDeVencimiento = db.FechasDeVencimiento.Where(x => x.Mes == hoy.Month && x.Año == hoy.Year).FirstOrDefault();

            adhesionPago360.adhesion_holder_name = adhesion_holder_name;
            adhesionPago360.email = Email;
            adhesionPago360.description = "Pago Total del Carton Nro°: " + CartonComprado.Carton.Numero;
            adhesionPago360.short_description = "S. Celeste";
            adhesionPago360.external_reference = CartonComprado.ID.ToString();
            adhesionPago360.cbu_number = cbu_number;
            adhesionPago360.cbu_holder_id_number = cbu_holder_id_number;
            adhesionPago360.cbu_holder_name = adhesion_holder_name;

            var adhesion = GenerarAdhesion(adhesionPago360);
            db.Adhesiones.Add(adhesion);

            db.SaveChanges();

            int c = 1, mesInicio = hoy.Month;
            float precioCuota = CartonComprado.Carton.Precio / CantCuotas;

            if (hoy.Day >= 7)
            {
                mesInicio++;
            }
            for (int mes = mesInicio; mes < mesInicio + CantCuotas; mes++)
            {
                CuotasDebito cuotasDebito = new CuotasDebito();

                var vencimiento = db.FechasDeVencimiento.Where(x => x.Mes == mes && x.Año == hoy.Year).FirstOrDefault();

                cuotasDebito.CartonVendidoID = CartonComprado.ID;
                cuotasDebito.NroCuota = c;
                cuotasDebito.MesCuota = mes;
                cuotasDebito.AñoCuota = hoy.Year;
                cuotasDebito.PrimerVencimiento = vencimiento.PrimerVencimiento;
                cuotasDebito.PrimerPrecioCuota = precioCuota;
                cuotasDebito.SeguntoVencimiento = vencimiento.SegundoVencimiento;
                cuotasDebito.SeguntoPrecioCuota = precioCuota;
                cuotasDebito.AdhesionID = adhesion.id;

                db.CuotasDebito.Add(cuotasDebito);
                c++;
            }

            db.SaveChanges();

            return RedirectToAction("DebitoPendiente");
        }

        public ActionResult CancelarAdhesion()
        {
            //if (Session["ClienteDni"] == null)
            //{
            //    return RedirectToAction("Identificarse", "Clientes");
            //}

            return View();
        }

        [HttpPost]
        public ActionResult CancelarAdhesion(int Respuesta)
        {
            DateTime hoy = DateTime.Now;
            string url = "http://www.sueñocelestepagos.com.ar/Compras";

            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/CancelarAdhesion" });
            }

            var CartonComprado = db.CartonesVendidos.AsNoTracking().Include(t => t.Carton).Where(x => x.ClienteID == Cliente.ID && x.FechaVenta.Year == hoy.Year && x.PagoCancelado == false).FirstOrDefault();

            var adhesion = db.Adhesiones.Where(x => x.external_reference == CartonComprado.ID.ToString()).FirstOrDefault();

            Adhesion adhesion360 = CancelarAdhesion360(adhesion.id);

            if (adhesion360.state == "canceled")
            {
                switch (Respuesta)
                {
                    case 1://1 Cancelar Compra de Carton
                        {
                            CartonComprado.PagoCancelado = true;

                            db.SaveChanges();
                            break;
                        }
                    case 2://2 Pagar restante de un Pago
                        {
                            var totalPagar = db.CuotasDebito.Where(x => x.CartonVendidoID == CartonComprado.ID &&
                                                                 x.CuotaPagada == false)
                                                     .Select(x => x.PrimerPrecioCuota)
                                                     .Sum();


                            var FechaDeVencimiento = db.FechasDeVencimiento.Where(x => x.Mes == hoy.Month && x.Año == hoy.Year).FirstOrDefault();

                            Pago360Request pago360 = new Pago360Request();

                            pago360.description = "Pago Total del Carton Nro°: " + CartonComprado.Carton.Numero;
                            pago360.first_due_date = FechaDeVencimiento.PrimerVencimiento.ToString("dd-MM-yyyy");
                            pago360.first_total = totalPagar;
                            pago360.second_due_date = FechaDeVencimiento.SegundoVencimiento.ToString("dd-MM-yyyy");
                            pago360.second_total = totalPagar;
                            pago360.payer_name = Cliente.NombreCompleto;
                            pago360.external_reference = CartonComprado.ID.ToString();
                            pago360.payer_email = Cliente.Email;
                            pago360.back_url_success = url + "/PagoRealizado";
                            pago360.back_url_pending = url + "/PagoPendiente";
                            pago360.back_url_rejected = url + "/PagoCancelado";
                            pago360.excluded_channels = new string[] { "credit_card" };

                            var pago = Pagar(pago360);
                            CartonComprado.TipoDePagoID = 1;

                            db.Pagos.Add(pago);

                            db.SaveChanges();
                            break;
                        }
                    case 3://3 Generar Plan de pago para las cuotas Restantes
                        {
                            var cuotasPagadas = db.CuotasDebito.Where(x => x.CartonVendidoID == CartonComprado.ID && x.CuotaPagada == true).ToList();

                            foreach (var cuotaPagada in cuotasPagadas)
                            {
                                var cuotaPlanDePago = CuotaDebitoACuotasPlanDePago(cuotaPagada);

                                db.CuotasPlanDePagos.Add(cuotaPlanDePago);
                            }

                            var cuotasImpagadas = db.CuotasDebito.Where(x => x.CartonVendidoID == CartonComprado.ID && x.CuotaPagada == false).ToList();

                            foreach (var cuotaImpaga in cuotasImpagadas)
                            {
                                var cuotaPlanDePago = CuotaDebitoACuotasPlanDePago(cuotaImpaga);

                                db.CuotasPlanDePagos.Add(cuotaPlanDePago);
                            }

                            CartonComprado.TipoDePagoID = 2;

                            db.SaveChanges();
                            break;
                        }
                    default:
                        break;
                }
            }

            return View();
        }

        /***************************************************************************************/

        public ActionResult PlanDePago()
        {
            if (Session["ClienteDni"] == null)
            {
                return RedirectToAction("Identificarse", "Clientes");
            }

            DateTime hoy = DateTime.Now;

            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/PlanDePago" });
            }

            var CartonComprado = db.CartonesVendidos.Where(x => x.ClienteID == Cliente.ID && x.FechaVenta.Year == hoy.Year && x.PagoCancelado == false).FirstOrDefault();

            ViewBag.Pagos = CartonComprado.Pagos;

            var CuotasPlanDePago = db.CuotasPlanDePagos.Where(x => x.CartonVendidoID == CartonComprado.ID).ToList();

            ViewBag.ValorCuota = CartonComprado.Carton.Precio / CuotasPlanDePago.Count;

            return View(CuotasPlanDePago);
        }

        /***************************************************************************************/

        public ActionResult PagarCuota(int cuota)
        {
            DateTime hoy = DateTime.Now;
            int PagoCartonId = 0;
            Pago pago = new Pago();

            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/PlanDePago" });
            }

            var CartonComprado = db.CartonesVendidos.AsNoTracking().Include(t => t.Carton).Where(x => x.ClienteID == Cliente.ID && x.FechaVenta.Year == hoy.Year && x.PagoCancelado == false).FirstOrDefault();

            var CuotasPlanDePago = db.CuotasPlanDePagos.Where(x => x.CartonVendidoID == CartonComprado.ID).ToList();

            var valorCuota = CartonComprado.Carton.Precio / CuotasPlanDePago.Count;

            var CuotaCarton = db.CuotasPlanDePagos.Where(x => x.ID == cuota && x.CartonVendidoID == CartonComprado.ID).FirstOrDefault();

            if(CuotaCarton.PrimerPrecioCuota != valorCuota)
            {
                CuotaCarton.PrimerPrecioCuota = valorCuota;
                CuotaCarton.SeguntoPrecioCuota = valorCuota;
            }

            decimal precioCuota = decimal.Parse(CuotaCarton.PrimerPrecioCuota.ToString());

            var Pago = db.Pagos.Where(x => x.id == CuotaCarton.PagoID && x.first_total == precioCuota).FirstOrDefault();

            if (Pago == null || pago.state == "expired" || Pago.first_total != precioCuota)
            {
                //var CuotaAnterior = db.CuotasPlanDePagos.Where(x => x.CartonVendidoID == CartonComprado.ID && x.MesCuota == CuotaCarton.MesCuota - 1).FirstOrDefault();

                //if (CuotaAnterior != null)
                //{
                //    CuotaCarton.PrimerPrecioCuota += CuotaAnterior.PrimerPrecioCuota;

                //    CuotaCarton.SeguntoPrecioCuota += CuotaAnterior.SeguntoPrecioCuota;
                //}

                PagoCartonVendido pagoCarton = new PagoCartonVendido();

                pagoCarton.CartonVendidoID = CuotaCarton.CartonVendidoID;
                pagoCarton.TipoDePagoID = 2;
                pagoCarton.FechaDePago = hoy;
                pagoCarton.CuotaPlanID = cuota;

                db.PagosCartonesVendidos.Add(pagoCarton);
                db.SaveChanges();

                string url = "https://www.sueñoceletepago.com.ar/";

                Pago360Request pago360 = new Pago360Request();

                var FechaDeVencimiento = db.FechasDeVencimiento.Where(x => x.Mes == hoy.Month && x.Año == hoy.Year).FirstOrDefault();

                if (hoy.Day > FechaDeVencimiento.PrimerVencimiento.Day)
                {
                    FechaDeVencimiento = db.FechasDeVencimiento.Where(x => x.Mes == (hoy.Month + 1) && x.Año == hoy.Year).FirstOrDefault();
                }

                var entidad = db.Instituciones.Find(CartonComprado.EntidadID);

                var Carton = db.Cartones.Where(x => x.ID == CartonComprado.CartonID).FirstOrDefault();

                pago360.description = "Pago Cuota Nro: " + CuotaCarton.NroCuota + " del Carton Nro°: " + Carton.Numero + " - " + entidad.Nombre;

                pago360.first_due_date = FechaDeVencimiento.PrimerVencimiento.ToString("dd-MM-yyyy");//CuotaCarton.PrimerVencimiento.ToString("dd-MM-yyyy");
                pago360.first_total = CuotaCarton.PrimerPrecioCuota;

                pago360.second_due_date = FechaDeVencimiento.SegundoVencimiento.ToString("dd-MM-yyyy");//CuotaCarton.SeguntoVencimiento.ToString("dd-MM-yyyy");
                pago360.second_total = CuotaCarton.SeguntoPrecioCuota;

                pago360.payer_name = Cliente.NombreCompleto;
                pago360.external_reference = pagoCarton.ID.ToString();
                pago360.payer_email = Cliente.Email;
                pago360.back_url_success = url;// + "/PagoRealizado";
                pago360.back_url_pending = url;// + "/PagoPendiente";
                pago360.back_url_rejected = url;// + "/PagoCancelado";
                //pago360.excluded_channels = new string[] { "credit_card" };

                try
                {
                    pago = Pagar(pago360);

                    db.Pagos.Add(pago);
                    CuotaCarton.PagoID = pago.id;

                    //pagoCarton.FechaDePago = hoy;
                    //pagoCarton.Pago = pago.first_total;

                    //db.PagosCartonesVendidos.Add(pagoCarton);

                    //pagoCarton.PagoID = pago.id;

                    db.SaveChanges();

                    return Redirect(pago.checkout_url);
                }
                catch (Exception e)
                {
                    if (PagoCartonId == 0)
                    {
                        db.PagosCartonesVendidos.Remove(pagoCarton);

                        db.SaveChanges();
                    }
                    return RedirectToAction("ErrorCompra", new { MensajeError = "Ocurrio un Error, Por Favor intente mas tarde" });
                }
            }
            else
            {
                if (Pago.state == "paid")
                {
                    return RedirectToAction("PlanDePago");
                }

                return Redirect(Pago.checkout_url);
            }
        }

        /***************************************************************************************/

        public Pago Pagar(Pago360Request pago360)
        {
            //retorno
            Pago pago = new Pago();

            //Respuesta de la Api
            string respuesta = "";

            //
            string pago360Js = JsonConvert.SerializeObject(pago360);

            //Local
            //Uri uri = new Uri("https://localhost:44382/api/Payment360?paymentRequest=" + HttpUtility.UrlEncode(pago360Js));

            //Server
            Uri uri = new Uri("http://localhost:90/api/Payment360?paymentRequest=" + HttpUtility.UrlEncode(pago360Js));

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

            ////requestFile.Headers.Add("authorization", "Bearer OTllZDJlZjA3NmNlOWQ4NzYzYzYzNjljMjU3YTNmZGYxNTQ3MGIwZGI2MjIwNjc2MDJkYjNmNmRiNWUyNTcxOA");

            return pago;
        }

        public Adhesion GenerarAdhesion(AdhesionPago360Request adhesionPago360)
        {
            Adhesion adhesion = new Adhesion();

            //Respuesta de la Api
            string respuesta = "";

            //
            string adhesionPago360Js = JsonConvert.SerializeObject(adhesionPago360);

            //Local
            Uri uri = new Uri("https://localhost:44382/api/Adhesion360?adhesionRequest=" + HttpUtility.UrlEncode(adhesionPago360Js));

            //Server
            //Uri uri = new Uri("http://localhost:90/api/Adhesion360?adhesionRequest=" + HttpUtility.UrlEncode(adhesionPago360Js));

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

                    AdhesionPago360Response adhesionResponse = new AdhesionPago360Response();

                    //var jsonObject = JObject.Parse(response.Content);

                    adhesionResponse = JsonConvert.DeserializeObject<AdhesionPago360Response>(respuesta);

                    adhesion.id = adhesionResponse.id;
                    adhesion.external_reference = adhesionResponse.external_reference;
                    adhesion.adhesion_holder_name = adhesionResponse.adhesion_holder_name;
                    adhesion.email = adhesionResponse.email;
                    adhesion.cbu_holder_name = adhesionResponse.cbu_holder_name;
                    adhesion.cbu_holder_id_number = adhesionResponse.cbu_holder_id_number;
                    adhesion.cbu_number = adhesionResponse.cbu_number;
                    adhesion.bank = adhesionResponse.bank;
                    adhesion.description = adhesionResponse.description;
                    adhesion.short_description = adhesionResponse.short_description;
                    adhesion.state = adhesionResponse.state;
                    adhesion.created_at = adhesionResponse.created_at;
                }
            }

            return adhesion;
        }

        public Adhesion CancelarAdhesion360(int adhesionId)
        {
            Adhesion adhesion = new Adhesion();

            //Respuesta de la Api
            string respuesta = "";

            //Local
            //Uri uri = new Uri("https://localhost:44382/api/Adhesion360?id=" + adhesionId);

            //Server
            Uri uri = new Uri("http://localhost:90/api/Adhesion360?id=" + adhesionId);

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

                    AdhesionPago360Response adhesionResponse = new AdhesionPago360Response();

                    //var jsonObject = JObject.Parse(response.Content);

                    adhesionResponse = JsonConvert.DeserializeObject<AdhesionPago360Response>(respuesta);

                    adhesion.id = adhesionResponse.id;
                    adhesion.external_reference = adhesionResponse.external_reference;
                    adhesion.adhesion_holder_name = adhesionResponse.adhesion_holder_name;
                    adhesion.email = adhesionResponse.email;
                    adhesion.cbu_holder_name = adhesionResponse.cbu_holder_name;
                    adhesion.cbu_holder_id_number = adhesionResponse.cbu_holder_id_number;
                    adhesion.cbu_number = adhesionResponse.cbu_number;
                    adhesion.bank = adhesionResponse.bank;
                    adhesion.description = adhesionResponse.description;
                    adhesion.short_description = adhesionResponse.short_description;
                    adhesion.state = adhesionResponse.state;
                    adhesion.created_at = adhesionResponse.created_at;
                }
            }

            return adhesion;
        }

        /***************************************************************************************/

        public ActionResult PagoPendiente()
        {
            DateTime hoy = DateTime.Now;

            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/PagoPendiente" });
            }

            var compra = db.CartonesVendidos.Include(t => t.Carton).Where(x => x.ClienteID == Cliente.ID && x.Carton.Año == hoy.Year && x.PagoCancelado == false).FirstOrDefault();

            if (compra != null) //Check si el cliente ya compro el carton de este año
            {
                switch (compra.TipoDePagoID)
                {
                    case 1://Un Pago
                        {
                            var PagoCarton = db.PagosCartonesVendidos.Where(x => x.CartonVendidoID == compra.ID).FirstOrDefault();

                            var pago360 = db.Pagos.Where(x => x.external_reference == PagoCarton.ID.ToString()).FirstOrDefault();

                            compra.CheckoutUrl = pago360.checkout_url;
                            if (compra.PagoRealizdo)
                            {
                                return RedirectToAction("PagoRealizado");
                            }
                            else
                            {
                                return View(compra);
                            }
                        }
                    case 2://Plan de pago Manual
                        {
                            if (compra.PagoRealizdo)
                            {
                                return RedirectToAction("PagoRealizado");
                            }
                            else
                            {
                                return RedirectToAction("PlanDePago");
                            }
                        }
                    case 3://Plan de pago Debito Automatico
                        {
                            if (compra.PagoRealizdo)
                            {
                                return RedirectToAction("DebitoRealizado");
                            }
                            else
                            {
                                return RedirectToAction("DebitoPendiente");
                            }
                        }
                }
            }

            return RedirectToAction("ComprobarCompra");
        }

        public ActionResult PagoRealizado()
        {
            DateTime hoy = DateTime.Now;

            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/PagoRealizado" });
            }

            var compra = db.CartonesVendidos.Include(t => t.Carton).Where(x => x.ClienteID == Cliente.ID && x.Carton.Año == hoy.Year && x.PagoCancelado == false).FirstOrDefault();

            if (compra != null) //Check si el cliente ya compro el carton de este año
            {

                switch (compra.TipoDePagoID)
                {
                    case 1://Un Pago
                        {
                            if (compra.PagoRealizdo)
                            {
                                return View(compra);
                            }
                            else
                            {
                                return RedirectToAction("PagoPendiente");
                            }
                        }
                    case 2://Plan de pago Manual
                        {
                            if (compra.PagoRealizdo)
                            {
                                return RedirectToAction("PagoRealizado");
                            }
                            else
                            {
                                return RedirectToAction("PlanDePago");
                            }
                        }
                    case 3://Plan de pago Debito Automatico
                        {
                            if (compra.PagoRealizdo)
                            {
                                return RedirectToAction("DebitoRealizado");
                            }
                            else
                            {
                                return RedirectToAction("DebitoPendiente");
                            }
                        }
                }
            }

            return RedirectToAction("ComprobarCompra");
        }

        public ActionResult AdhesionRealizada()
        {
            DateTime hoy = DateTime.Now;

            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/PagoRealizado" });
            }

            var compra = db.CartonesVendidos.Include(t => t.Carton).Where(x => x.ClienteID == Cliente.ID && x.Carton.Año == hoy.Year && x.PagoCancelado == false).FirstOrDefault();

            if (compra != null) //Check si el cliente ya compro el carton de este año
            {

                switch (compra.TipoDePagoID)
                {
                    case 1://Un Pago
                        {
                            if (compra.PagoRealizdo)
                            {
                                return View(compra);
                            }
                            else
                            {
                                return RedirectToAction("PagoPendiente");
                            }
                        }
                    case 2://Plan de pago Manual
                        {
                            if (compra.PagoRealizdo)
                            {
                                return RedirectToAction("PagoRealizado");
                            }
                            else
                            {
                                return RedirectToAction("PlanDePago");
                            }
                        }
                    case 3://Plan de pago Debito Automatico
                        {
                            if (compra.PagoRealizdo)
                            {
                                return RedirectToAction("DebitoRealizado");
                            }
                            else
                            {
                                return RedirectToAction("DebitoPendiente");
                            }
                        }
                }
            }

            return RedirectToAction("ComprobarCompra");
        }

        public ActionResult DebitoRealizado()
        {
            DateTime hoy = DateTime.Now;

            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/PagoRealizado" });
            }

            var compra = db.CartonesVendidos.Include(t => t.Carton).Where(x => x.ClienteID == Cliente.ID && x.Carton.Año == hoy.Year && x.PagoCancelado == false).FirstOrDefault();

            if (compra != null) //Check si el cliente ya compro el carton de este año
            {

                switch (compra.TipoDePagoID)
                {
                    case 1://Un Pago
                        {
                            if (compra.PagoRealizdo)
                            {
                                return RedirectToAction("PagoRealizado");
                            }
                            else
                            {
                                return RedirectToAction("PagoPendiente");
                            }
                        }
                    case 2://Plan de pago Manual
                        {
                            if (compra.PagoRealizdo)
                            {
                                return RedirectToAction("PagoRealizado");
                            }
                            else
                            {
                                return RedirectToAction("PlanDePago");
                            }
                        }
                    case 3://Plan de pago Debito Automatico
                        {
                            if (compra.PagoRealizdo)
                            {
                                return View(compra);
                            }
                            else
                            {
                                return RedirectToAction("DebitoPendiente");
                            }
                        }
                }
            }

            return RedirectToAction("ComprobarCompra");
        }

        public ActionResult DebitoPendiente()
        {
            DateTime hoy = DateTime.Now;

            var Cliente = ObtenerCliente();

            if (Cliente == null)
            {
                return RedirectToAction("Identificarse", "Clientes", new { returnUrl = "/Compras/PagoRealizado" });
            }

            var compra = db.CartonesVendidos.Include(t => t.Carton).Where(x => x.ClienteID == Cliente.ID && x.Carton.Año == hoy.Year && x.PagoCancelado == false).FirstOrDefault();

            if (compra != null) //Check si el cliente ya compro el carton de este año
            {

                switch (compra.TipoDePagoID)
                {
                    case 1://Un Pago
                        {
                            if (compra.PagoRealizdo)
                            {
                                return RedirectToAction("PagoRealizado");
                            }
                            else
                            {
                                return RedirectToAction("PagoPendiente");
                            }
                        }
                    case 2://Plan de pago Manual
                        {
                            if (compra.PagoRealizdo)
                            {
                                return RedirectToAction("PagoRealizado");
                            }
                            else
                            {
                                return RedirectToAction("PlanDePago");
                            }
                        }
                    case 3://Plan de pago Debito Automatico
                        {
                            if (compra.PagoRealizdo)
                            {
                                return RedirectToAction("DebitoRealizado");
                            }
                            else
                            {
                                return View(compra);
                            }
                        }
                }
            }

            return RedirectToAction("ComprobarCompra");
        }

        public ActionResult ErrorCompra(string MensajeError)
        {
            ViewBag.MensajeError = MensajeError;

            return View();
        }

        /***************************************************************************************/

        #region Webhook
        [System.Web.Http.HttpPost]
        public HttpResponseMessage WebhookListener([System.Web.Http.FromBody] Webhook pWebhook)
        {
            if (CambiarEstado(pWebhook))
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
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

                        if (Decimal.Parse(CartonComprado.Carton.Precio.ToString()) - 1 <= CartonComprado.Pagos)
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
        #endregion

        /***************************************************************************************/

        public Cliente ObtenerCliente()
        {
            try
            {
                string dni = Session["ClienteDni"].ToString();

                string sexo = Session["ClienteSexo"].ToString();

                if (string.IsNullOrEmpty(dni) || string.IsNullOrEmpty(sexo))
                {
                    return null;
                }

                return db.Clientes.Where(x => x.Dni == dni && x.Sexo == sexo).FirstOrDefault();
            }
            catch (Exception e)
            {
                return null;
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

        public string ObtenerBodyEmailCompraPagoPendiente(string NroCarton)
        {
            string body = "<meta charset='utf-8'><style type='text/css'> @media only screen and (max-width: 480px) { table { display: block !important; width: 100% !important; } td { width: 480px !important; } }</style><body style='font-family: 'Malgun Gothic', Arial, sans-serif; margin: 0; padding: 0; width: 100%; -webkit-text-size-adjust: none; -webkit-font-smoothing: antialiased;'> <table width='100%' bgcolor='#FFFFFF' border='0' cellspacing='0' cellpadding='0' id='background' style='height: 100% !important; margin: 0; padding: 0; width: 100% !important;'> <tr> <td align='center' valign='top'> <table width='600' border='0' bgcolor='#FFFFFF' cellspacing='0' cellpadding='0' id='header_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' bgcolor='#474544' cellspacing='0' cellpadding='0' id='header'> <tr> <td valign='top' class='header_content'> <h1 style='color: #F4F4F4; font-size: 24px; text-align: center;'> </h1> </td> </tr> </table> </td> </tr> </table> <table width='600' border='0' bgcolor='#26abff' cellspacing='0' cellpadding='20' id='preheader'> <tr> <td valign='top'> <table width='100%' border='0' cellspacing='0' cellpadding='0'> <tr> <td valign='top' width='600'> <div class='logo' style='text-align: center;'> <a href='javascript:void(0)'> <img src='https://www.xn--sueoceleste-3db.com/assets/images/logo.png'> </a> </div> </td> </tr> </table> </td> </tr> </table> <table width='600' border='0' bgcolor='#FFFFFF' cellspacing='0' cellpadding='0' id='header_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' bgcolor='#474544' cellspacing='0' cellpadding='0' id='header'> <tr> <td valign='top' class='header_content'> <h1 style='color: #F4F4F4; font-size: 24px; text-align: center;'> </h1> </td> </tr> </table> </td> </tr> </table> <table width='600' border='0' bgcolor='#f2f2f2' cellspacing='0' cellpadding='20' id='body_container'> <tr> <td align='center' valign='top' class='body_content'> <table width='100%' border='0' cellspacing='0' cellpadding='20'> <tr> <td valign='top'> <h2 style='font-size: 24px; text-align: justify;'>Notificación de Aviso de Deuda de Pago</h2> <p style='font-size: 16px; line-height: 22px; text-align: justify;'> Este aviso automático es para informar que se existe una deuda de pago por la compra del cartón Número [NumeroCarton] de Sueño Celeste. <br><br> Por favor, realice el pago correspondiente en breve, a fin de que se haga efectiva la compra de su cartón. <br><br> Muchas gracias. <br><br> Atentamente <br><br> <b>Sueño Celeste</b> </p> </td> </tr> </table> </td> </tr> </table> <table width='600' border='0' bgcolor='#26abff' cellspacing='0' cellpadding='20' id='contact_container'> <tr> <td align='center' valign='top'> <table width='100%' border='0' cellspacing='0' cellpadding='20' id='contact'> <tr> <td> <p style='color: #F4F4F4; font-size: 14px; line-height: 22px; text-align: center;'> San Martín 295, Morteros, Córdoba, Argentina <br> sceleste_mutual9@hotmail.com <br> +54 (3562) 404455 </p> </td> </tr> </table> </td> </tr> </table> </td> </tr> </table></body>";

            body = body.Replace("[NumeroCarton]", NroCarton);

            return body;
        }

        /***************************************************************************************/

        public List<Cuotas> ObtenerCuotasPosibles(int CartonId)
        {
            List<Cuotas> cuotas = new List<Cuotas>();

            DateTime hoy = DateTime.Today;
            int c = 1;

            //var FechaLimite = db.FechaLimiteVentaCartones.Where(x => x.Vigente).FirstOrDefault();

            var Carton = db.Cartones.Find(CartonId);

            var Meses = Carton.ValidoHasta.Value.Subtract(hoy).Days / (365.25 / 12);

            var mesesRound = Math.Round(Meses);

            if (mesesRound == 0)
            {
                mesesRound = 1;
            }

            for (int mes = 1; mes <= mesesRound; mes++)
            {
                var cuota = new Cuotas();

                cuota.key = c;
                cuota.value = c + " Cuota/s";

                cuotas.Add(cuota);
                c++;
            }

            #region Old
            //int CantCuotasPosibles = 12;
            //int c = 1;

            //var FechasDeVencimiento = db.FechasDeVencimiento.Where(x => x.Mes == hoy.Month && x.Año == hoy.Year).FirstOrDefault();

            //if (hoy.Month == 12 && hoy >= FechasDeVencimiento.PrimerVencimiento)
            //{
            //    hoy = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(1);
            //    FechasDeVencimiento = db.FechasDeVencimiento.Where(x => x.Mes == hoy.Month && x.Año == hoy.Year).FirstOrDefault();
            //}

            ////if (hoy >= FechasDeVencimiento.PrimerVencimiento)
            ////{
            ////    FechasDeVencimiento = db.FechasDeVencimiento.Where(x => x.Mes == hoy.Month + 1 && x.Año == hoy.Year).FirstOrDefault();
            ////}

            //for (int mes = FechasDeVencimiento.Mes; mes <= CantCuotasPosibles; mes++)
            //{
            //    var cuota = new Cuotas();

            //    cuota.key = c;
            //    cuota.value = c + " Cuota/s";

            //    cuotas.Add(cuota);
            //    c++;
            //}
            #endregion

            return cuotas;
        }

        public JsonResult ObtenerCuotasPosiblesJS()
        {
            int CartonReservadoId = 0;

            int.TryParse(Session["ReservaCarton"].ToString(), out CartonReservadoId);

            var cartonReservado = db.CartonesReservados.Where(x => x.ID == CartonReservadoId).FirstOrDefault();

            return Json(ObtenerCuotasPosibles(cartonReservado.CartonID), JsonRequestBehavior.AllowGet);
        }

        /***************************************************************************************/

        public List<Cuotas> ObtenerCuotasDebitoPosibles(int CartonId)
        {
            List<Cuotas> cuotas = new List<Cuotas>();

            DateTime hoy = DateTime.Today;

            int c = 1;

            //var FechaLimite = db.FechaLimiteVentaCartones.Where(x => x.Vigente).FirstOrDefault();

            var Carton = db.Cartones.Find(CartonId);

            var Meses = Carton.ValidoHasta.Value.Subtract(hoy).Days / (365.25 / 12);

            var mesesRound = Math.Round(Meses);

            if(mesesRound == 0)
            {
                mesesRound = 1;
            }

            for (int mes = 1; mes <= mesesRound; mes++)
            {
                var cuota = new Cuotas();

                cuota.key = c;
                cuota.value = c + " Cuota/s";

                cuotas.Add(cuota);
                c++;
            }

            #region old
            //int CantCuotasPosibles = 12;
            //int c = 1;

            ////var FechasDeVencimiento = db.FechasDeVencimiento.Where(x => x.Mes == hoy.Month && x.Año == hoy.Year).FirstOrDefault();
            //var FechasDeVencimiento = db.FechasLimitesDebito.Where(x => x.Mes == hoy.Month && x.Año == hoy.Year).FirstOrDefault();

            //if (hoy.Month == 12 && hoy >= FechasDeVencimiento.FechaLimite)
            //{
            //    hoy = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(1);
            //    FechasDeVencimiento = db.FechasLimitesDebito.Where(x => x.Mes == hoy.Month && x.Año == hoy.Year).FirstOrDefault();
            //}

            ////if (hoy >= FechasDeVencimiento.PrimerVencimiento)
            ////{
            ////    FechasDeVencimiento = db.FechasDeVencimiento.Where(x => x.Mes == hoy.Month + 1 && x.Año == hoy.Year).FirstOrDefault();
            ////}

            //for (int mes = FechasDeVencimiento.Mes; mes <= CantCuotasPosibles; mes++)
            //{
            //    var cuota = new Cuotas();

            //    cuota.key = c;
            //    cuota.value = c + " Cuota/s";

            //    cuotas.Add(cuota);
            //    c++;
            //}
            #endregion

            return cuotas;
        }

        public JsonResult ObtenerCuotasDebitoPosiblesJS()
        {
            int CartonReservadoId = 0;

            int.TryParse(Session["ReservaCarton"].ToString(), out CartonReservadoId);

            var cartonReservado = db.CartonesReservados.Where(x => x.ID == CartonReservadoId).FirstOrDefault();

            return Json(ObtenerCuotasDebitoPosibles(cartonReservado.CartonID), JsonRequestBehavior.AllowGet);
        }

        /***************************************************************************************/

        public JsonResult ObtenerInstituciones()
        {
            var cliente = ObtenerCliente();

            List<Cuotas> cuotas = new List<Cuotas>();
            List<Institucion> Instituciones = new List<Institucion>();

            if(cliente != null)
            {
                Instituciones.AddRange(db.Instituciones.Where(x => x.LocalidadID == cliente.LocalidadID).ToList());

                Instituciones.AddRange(db.Instituciones.Where(x => x.LocalidadID != cliente.LocalidadID).ToList());
            }
            else
            {
                Instituciones.AddRange(db.Instituciones.ToList());
            }

            foreach (var Institucion in Instituciones)
            {
                var cuota = new Cuotas();

                cuota.key = Institucion.ID;
                cuota.value = Institucion.Nombre;

                cuotas.Add(cuota);
            }

            return Json(cuotas, JsonRequestBehavior.AllowGet);
        }

        /***************************************************************************************/

        public CuotasPlanDePago CuotaDebitoACuotasPlanDePago(CuotasDebito cuotaPagada)
        {
            CuotasPlanDePago cuotaPlanDePago = new CuotasPlanDePago();

            cuotaPlanDePago.CartonVendidoID = cuotaPagada.CartonVendidoID;
            cuotaPlanDePago.NroCuota = cuotaPagada.NroCuota;
            cuotaPlanDePago.MesCuota = cuotaPagada.MesCuota;
            cuotaPlanDePago.AñoCuota = cuotaPagada.AñoCuota;
            cuotaPlanDePago.PrimerVencimiento = cuotaPagada.PrimerVencimiento;
            cuotaPlanDePago.PrimerPrecioCuota = cuotaPagada.PrimerPrecioCuota;
            cuotaPlanDePago.SeguntoVencimiento = cuotaPagada.SeguntoVencimiento;
            cuotaPlanDePago.SeguntoPrecioCuota = cuotaPagada.SeguntoPrecioCuota;
            cuotaPlanDePago.CuotaPagada = cuotaPagada.CuotaPagada;

            return cuotaPlanDePago;
        }

        /***************************************************************************************/

        public List<Carton> ObtenerCartonesDisponibles(int? SearchType, string SearchString)
        {
            DateTime hoy = DateTime.Now;

            //var FechaLimite = db.FechaLimiteVentaCartones.Where(x => x.Vigente).FirstOrDefault();

            //int año = hoy.Year;

            //var FechaDeVencimiento = db.FechasDeVencimiento.Where(x => x.Mes == hoy.Month && x.Año == hoy.Year).FirstOrDefault();

            //if (hoy.Month == 12 && hoy > FechaDeVencimiento.PrimerVencimiento)
            //{
            //    hoy = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(1);
            //}

            var compra = db.CartonesVendidos.Include(t => t.Carton).Where(x => x.Carton.ValidoDesde <= hoy && x.Carton.ValidoHasta >= hoy && x.PagoCancelado == false).ToList();

            var Cartones = db.Cartones.Where(x => x.ValidoDesde <= hoy && x.ValidoHasta >= hoy).ToList();

            if (!string.IsNullOrEmpty(SearchString))
            {
                switch (SearchType)
                {
                    case 1:
                        {
                            Cartones = Cartones.Where(x => x.Numero.Contains(SearchString)).ToList();
                            break;
                        }
                    case 2:
                        {
                            Cartones = Cartones.Where(x => x.Numero.EndsWith(SearchString)).ToList();
                            break;
                        }
                    default:
                        break;
                }
            }

            var listaCartones = Cartones.Where(x => !compra.Any(y => y.CartonID == x.ID)).ToList();

            var cartonesReservados = db.CartonesReservados.Where(x => x.FechaReserva <= hoy && x.FechaExpiracionReserva >= hoy).ToList();

            listaCartones = listaCartones.Where(x => !cartonesReservados.Any(y => y.CartonID == x.ID)).ToList();

            return listaCartones;
        }

        /***************************************************************************************/

        public JsonResult ObtenerNumeros(int? SearchType, string SearchString)
        {
            List<Carton> cartones = new List<Carton>();
            if (string.IsNullOrEmpty(SearchString))
            {
                cartones = ObtenerCartonesDisponibles(null, null);
            }
            else
            {
                cartones = ObtenerCartonesDisponibles(SearchType, SearchString);
            }

            //var numeros = cartones.Select(x => x.ID).ToArray();

            //Random _rdm = new Random();

            //int random = _rdm.Next(0, numeros.Length);

            return Json(cartones, JsonRequestBehavior.AllowGet);
        }

        /***************************************************************************************/

        public JsonResult GetReserva(int id)
        {
            bool reserva = true;
            try
            {
                DateTime hoy = DateTime.Now;

                string dni = Session["ClienteDni"].ToString();

                var cartonReservado = db.CartonesReservados.Where(x => x.FechaReserva <= hoy && 
                                                                       x.FechaExpiracionReserva >= hoy && 
                                                                       x.CartonID == id && 
                                                                       x.Dni != dni)
                                                           .FirstOrDefault();

                if(cartonReservado == null)
                {
                    reserva = false;
                }

                return Json(reserva, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(reserva, JsonRequestBehavior.AllowGet);
            }

        }

        public void ProbarEnvioCorreo(string email)
        {
            try
            {
                string from = "no-reply@xn--sueocelestepago-0qb.com";
                string usuario = "no-reply@xn--sueocelestepago-0qb.com";
                string password = "GVbE3UMeME";

                string subject = "Sueño Celeste la Gran Promocion";

                string emailBody = ObtenerBodyEmailCompraPagoContado("777");

                Email nuevoEmail = new Email();
                nuevoEmail.SendEmail(emailBody, email, subject);
            }
            catch (Exception)
            {

            }
        }
    }
}