using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities
{
    public class FechaLimiteDebito
    {
        public int ID { get; set; }

        public int Mes { get; set; }

        public int Año { get; set; }

        public DateTime FechaLimite { get; set; }
    }
}
