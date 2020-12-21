using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities
{
    [Table("tblAportesIntitucion")]
    public class AporteInstitucion
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("Institucion")]
        public int InstitucionID { get; set; }

        public virtual Institucion Institucion { get; set; }

        public decimal PorcAporte { get; set; }

        public DateTime FechaAlta { get; set; }

        public DateTime? FechaBaja { get; set; }
    }
}
