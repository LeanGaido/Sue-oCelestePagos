using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities
{
    [Table("tblCartonesReservados")]
    public class CartonReservado
    {
        public int ID { get; set; }

        public string Dni { get; set; }

        public string Sexo { get; set; }

        [ForeignKey("Carton")]
        public int CartonID { get; set; }

        public virtual Carton Carton { get; set; }

        public DateTime FechaReserva { get; set; }

        public DateTime FechaExpiracionReserva { get; set; }
    }
}
