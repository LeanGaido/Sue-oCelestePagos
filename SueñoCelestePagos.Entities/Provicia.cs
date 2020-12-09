using System.ComponentModel.DataAnnotations.Schema;

namespace SueñoCelestePagos.Entities
{
    [Table("tblProvicias")]
    public class Provicia
    {
        public int ID { get; set; }

        public string Nombre { get; set; }
    }
}