using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Objetivos_Prioritarios.Models.Extends
{
    public class MandamientoJudicialViewModel
    {
        public int IdNombreMandamiento { get; set; }

        public int IdMandamiento { get; set; }

        public string NombreCompleto { get; set; }

        public string NumeroControl { get; set; }

        public string NumeroExpediente { get; set; }

        public string TipoMandamiento { get; set; }

        public string EstadoProceso { get; set; }
        public string Delito { get; set; }

        public DateTime? FechaExpedicion { get; set; }

        public DateTime? FechaAlta { get; set; }

        public int PorcentajeNombre { get; set; }
    }
}