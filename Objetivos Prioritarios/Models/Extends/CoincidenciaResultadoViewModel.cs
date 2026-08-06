using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Objetivos_Prioritarios.Models.Extends
{
    public class CoincidenciaResultadoViewModel
    {


        public int IdCoincidencia { get; set; }

        public string NombreCompleto { get; set; }

        public string Alias { get; set; }

        public string Folio { get; set; }

        public string Expediente { get; set; }

        public string MunicipioClave { get; set; }

        public string Municipio { get; set; }

        public int Edad { get; set; }

        public string Sexo { get; set; }

        public string FotoUrl { get; set; }

        public string TipoCoincidencia { get; set; }

        public decimal PorcentajeNombre { get; set; }

        public decimal PorcentajeFoto { get; set; }

        public decimal PorcentajeHuella { get; set; }

        public decimal SimilitudGlobal { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int IdPersona { get; set; }

        public int IdTbFuente { get; set; }

        public bool TieneAvisoMandamientos { get; set; }

        public int TotalMandamientos { get; set; }

        public List<MandamientoJudicialViewModel> MandamientosJudiciales
        {
            get;
            set;
        } = new List<MandamientoJudicialViewModel>();

    }
}