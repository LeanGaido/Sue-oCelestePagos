using System.ComponentModel.DataAnnotations.Schema;

namespace SueñoCelestePagos.Entities
{
    [Table("tblItemPago")]
    public class ItemPago
    {
        public int ID { get; set; }

        [ForeignKey("Pago")]
        public int PagoID { get; set; }

        public virtual Pago Pago { get; set; }

        public int quantity { get; set; }//quantity	Integer	NO

        public string description { get; set; }//description	String	SI

        public float amount { get; set; }//amount	Float	SI
    }
}