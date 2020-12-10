using SueñoCelestePagos.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Dal
{
    public class SueñoCelestePagosContext : DbContext
    {
        public SueñoCelestePagosContext() : base("DefaultConnection")
        { }

        public DbSet<Adhesion> Adhesiones { get; set; }
        public DbSet<Application> applications { get; set; }
        public DbSet<Carton> Cartones { get; set; }
        public DbSet<CartonReservado> CartonesReservados { get; set; }
        public DbSet<CartonVendido> CartonesVendidos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<CodigoTelefono> CodigosTelefonos { get; set; }
        public DbSet<CompradoresSinCuenta> CompradoresSinCuentas { get; set; }
        public DbSet<CuotasDebito> CuotasDebito { get; set; }
        public DbSet<CuotasPlanDePago> CuotasPlanDePagos { get; set; }
        public DbSet<Debito> Debitos { get; set; }
        public DbSet<FechaDeVencimiento> FechasDeVencimiento { get; set; }
        public DbSet<FechaLimiteDebito> FechasLimitesDebito { get; set; }
        public DbSet<Institucion> Instituciones { get; set; }
        public DbSet<ItemPago> ItemsPagos { get; set; }
        public DbSet<Localidad> Localidades { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<PagoCartonVendido> PagosCartonesVendidos { get; set; }
        //public DbSet<PlanDePago> PlanesDePagos { get; set; }
        public DbSet<Provicia> Provicias { get; set; }
        public DbSet<TipoDePago> TiposDePago { get; set; }
    }
}
