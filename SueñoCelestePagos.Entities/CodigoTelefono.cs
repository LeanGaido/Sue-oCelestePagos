using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities
{
    [Table("tblCodigosTelefonos")]
    public class CodigoTelefono
    {
        public int ID { get; set; }

        public int Codigo { get; set; }

        public string Telefono { get; set; }

        public DateTime Expira { get; set; }
    }
}
