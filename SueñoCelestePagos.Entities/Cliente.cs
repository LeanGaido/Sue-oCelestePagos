using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities
{
    [Table("tblClientes")]
    public class Cliente
    {
        public int ID { get; set; }

        public string Nombre { get; set; }

        public string Apellido { get; set; }

        public string NombreCompleto { get { return Apellido + ", " + Nombre; } }
        
        [StringLength(8, ErrorMessage = "El Dni no puede tener mas de 8(Ocho) caracteres")]
        public string Dni { get; set; }

        [StringLength(5, ErrorMessage = "El Dni no puede tener mas de 5(Cinco) caracteres")]
        public string AreaCelular { get; set; }

        [StringLength(10, ErrorMessage = "El Dni no puede tener mas de 10(Diez) caracteres")]
        public string NumeroCelular { get; set; }

        public bool CelularConfirmado { get; set; }

        public string Celular { get { return AreaCelular + "" + NumeroCelular; } }

        public string Email { get; set; }

        public bool EmailConfirmado { get; set; }

        public DateTime FechaNacimiento { get; set; }

        public string Sexo { get; set; }

        public string Calle { get; set; }

        public string Altura { get; set; }

        public string Domicilio { get { return Calle + " " + Altura; } }

        [ForeignKey("Localidad")]
        public int LocalidadID { get; set; }

        public virtual Localidad Localidad { get; set; }
    }
}
