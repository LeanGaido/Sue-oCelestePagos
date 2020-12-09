using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities
{
    [Table("tblApplication")]
    public class Application
    {
        public int id { get; set; }

        public string descripcion { get; set; }

        public string listenerPago360 { get; set; }
    }
}
