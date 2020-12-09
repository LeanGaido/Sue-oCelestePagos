using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities
{
    [Table("tblCompradoresSinCuenta")]
    public class CompradoresSinCuenta
    {
        public int ID { get; set; }

        public string dni { get; set; }

        public int Año { get; set; }
    }
}
