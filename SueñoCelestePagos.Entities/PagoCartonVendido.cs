using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities
{
    public class PagoCartonVendido
    {
        public int ID { get; set; }

        [ForeignKey("CartonVendido")]
        public int CartonVendidoID { get; set; }

        public virtual CartonVendido CartonVendido { get; set; }

        [ForeignKey("TipoDePago")]
        public int TipoDePagoID { get; set; }

        public virtual TipoDePago TipoDePago { get; set; }

        public int PagoID { get; set; }

        public int CuotaPlanID { get; set; }

        public int CuotaDebitoID { get; set; }

        public decimal Pago { get; set; }

        public bool Pagado { get; set; }

        public DateTime FechaDePago { get; set; }
    }
}
