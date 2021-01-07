using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities.VMs
{
    public class ResumenCampañaVm
    {
        public string NroCarton { get; set; }

        public string NombreCompleto { get; set; }

        public string Dni { get; set; }

        public string Telefono { get; set; }

        public string Email { get; set; }

        public string Localidad { get; set; }

        public string Institucion { get; set; }

        public List<MesCampañaVm> MesesCampaña { get; set; }

        public decimal TotalPagos { get; set; }
    }
}
