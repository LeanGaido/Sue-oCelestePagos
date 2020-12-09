﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities.Pago360.Request
{
    public class SolicitudDebitoRequest
    {
        public int adhesion_id { get; set; }//adhesion_id Integer SI
        public string first_due_date { get; set; }//first_due_date Date    SI
        public float first_total { get; set; }//first_total Float   SI
        public string second_due_date { get; set; }//second_due_date Date    NO
        public float second_total { get; set; }//second_total Float   NO
        public string description { get; set; }//description String NO
        //public object metadata { get; set; }//metadata Object  NO
    }
}
