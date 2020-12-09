using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities
{
    public class FechaDeVencimiento
    {
        public int ID { get; set; }

        public int Mes { get; set; }

        public int Año { get; set; }

        public DateTime PrimerVencimiento { get; set; }

        public DateTime SegundoVencimiento { get; set; }
    }
}
