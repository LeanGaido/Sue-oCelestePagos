﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities.VMs
{
    public class MesCampañaVm
    {
        public int Mes { get; set; }

        public int Año { get; set; }

        public string NombreMes { get; set; }

        public decimal Importe { get; set; }
    }
}
