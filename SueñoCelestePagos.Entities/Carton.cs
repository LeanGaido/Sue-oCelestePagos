using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities
{
    [Table("tblCartones")]
    public class Carton
    {
        public int ID { get; set; }

        public string Numero { get; set; }

        public int Año { get; set; }

        public DateTime? ValidoDesde { get; set; }

        public DateTime? ValidoHasta { get; set; }

        public float Precio { get; set; }
    }
}
