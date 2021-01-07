using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueñoCelestePagos.Entities.VMs
{
    public class ResumenAcumuladoVm
    {
        public string NroCarton { get; set; }

        public string NombreCompleto { get; set; }

        public string Dni { get; set; }

        public string Telefono { get; set; }

        public string Email { get; set; }

        public string Localidad { get; set; }

        public string Institucion { get; set; }

        public decimal PagoEnero { get; set; }

        public decimal PagoFebrero { get; set; }

        public decimal PagoMarzo { get; set; }

        public decimal PagoAbril { get; set; }

        public decimal PagoMayo { get; set; }

        public decimal PagoJunio { get; set; }

        public decimal PagoJulio { get; set; }

        public decimal PagoAgosto { get; set; }

        public decimal PagoSeptiembre { get; set; }

        public decimal PagoOctubre { get; set; }

        public decimal PagoNoviembre { get; set; }

        public decimal PagoDiciembre { get; set; }

        public decimal TotalPagos { get; set; }
    }
}
