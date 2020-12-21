using System.ComponentModel.DataAnnotations.Schema;

namespace SueñoCelestePagos.Entities
{
    [Table("tblTiposDePagos")]
    public class TipoDePago
    {
        public int ID { get; set; }

        public string Descripcion { get; set; }

        public bool Activo { get; set; }
    }
}