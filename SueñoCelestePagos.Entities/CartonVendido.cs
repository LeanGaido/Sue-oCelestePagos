using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities
{
    [Table("tblCartonesVendidos")]
    public class CartonVendido
    {
        public int ID { get; set; }

        [ForeignKey("Carton")]
        public int CartonID { get; set; }

        public virtual Carton Carton { get; set; }

        [ForeignKey("Cliente")]
        public int ClienteID { get; set; }

        public virtual Cliente Cliente { get; set; }

        public DateTime FechaVenta { get; set; }

        [NotMapped]
        public int DiasDesdeLaVenta { get; set; }

        public decimal Pagos { get; set; }

        [ForeignKey("TipoDePago")]
        public int TipoDePagoID { get; set; }

        public virtual TipoDePago TipoDePago { get; set; }

        public int? CantCuotas { get; set; }

        public int? EntidadID { get; set; }

        [NotMapped]
        public string CheckoutUrl { get; set; }

        public bool PagoRealizdo { get; set; }

        public bool PagoCancelado { get; set; }

        public DateTime? FechaPago { get; set; }
    }
}
