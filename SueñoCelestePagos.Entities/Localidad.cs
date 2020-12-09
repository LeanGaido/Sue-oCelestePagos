using System.ComponentModel.DataAnnotations.Schema;

namespace SueñoCelestePagos.Entities
{
    [Table("tblLocalidad")]
    public class Localidad
    {
        public int ID { get; set; }

        public string Nombre { get; set; }

        public string CodPostal { get; set; }

        [ForeignKey("Provincia")]
        public int ProvinciaID { get; set; }

        public virtual Provicia Provincia { get; set; }

        public string Descripcion { get { return Nombre + " " + CodPostal; } }
    }
}