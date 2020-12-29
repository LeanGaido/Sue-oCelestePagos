using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities
{
    [Table("tblFechasLimiteVentaCartones")]
    public class FechaLimiteVentaCartones
    {
        public int ID { get; set; }

        public int Año { get; set; }

        public DateTime FechaDesde { get; set; }

        public DateTime FechaHasta { get; set; }

        public bool Vigente { get; set; }
    }
}
