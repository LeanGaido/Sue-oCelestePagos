﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities
{
    public class CuotasDebito
    {
        public int ID { get; set; }

        [ForeignKey("CartonVendido")]
        public int CartonVendidoID { get; set; }

        public virtual CartonVendido CartonVendido { get; set; }

        public int NroCuota { get; set; }

        public int MesCuota { get; set; }

        public int AñoCuota { get; set; }

        public DateTime PrimerVencimiento { get; set; }

        public float PrimerPrecioCuota { get; set; }

        public DateTime SeguntoVencimiento { get; set; }

        public float SeguntoPrecioCuota { get; set; }

        public DateTime? FechaPago { get; set; }

        public bool CuotaPagada { get; set; }

        public int AdhesionID { get; set; }

        public int DebitoID { get; set; }
    }
}
