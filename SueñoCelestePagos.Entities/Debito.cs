﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities
{
    public class Debito
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id { get; set; } //"id": 234825

        public string type { get; set; } //"type": "debit_request"

        public string state { get; set; } //"state": "pending"

        public DateTime created_at { get; set; } //"created_at": "2019-04-02T21:03:52-03:00"

        public DateTime first_due_date { get; set; } //"first_due_date": "2019-04-15T00:00:00-03:00"

        public decimal first_total { get; set; } //"first_total": 999

        public DateTime SecondDueDate { get; set; }

        public decimal SecondTotal { get; set; }

        public string description { get; set; } //"description": "Concepto del Pago"

        [ForeignKey("adhesion")]
        public int AdhesionId { get; set; }

        public Adhesion adhesion { get; set; } //"adhesion"

        [ForeignKey("CuotaDebito")]
        public int CuotaDebitoId { get; set; }

        public CuotasDebito CuotaDebito { get; set; }
    }
}
