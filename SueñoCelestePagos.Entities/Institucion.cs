using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities
{
    public class Institucion
    {
        public int ID { get; set; }

        public string Nombre { get; set; }

        public string Cuit { get; set; }

        public string Calle { get; set; }

        public int Altura { get; set; }

        [ForeignKey("Localidad")]
        public int LocalidadID { get; set; }

        public virtual Localidad Localidad { get; set; }

        public string Ciudad { get; set; }

        public string Provincia { get; set; }

        public string Domicilio { get { return Calle + " " + Altura + " " + Ciudad + " " + Provincia; } }

        [Display(Name = "Area")]
        public string AreaTelefono { get; set; }

        [Display(Name = "Numero")]
        public string NumeroTelefono { get; set; }

        public string Telefono { get { return AreaTelefono + " " + NumeroTelefono; } }
    }
}
