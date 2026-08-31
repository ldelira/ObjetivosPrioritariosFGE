using System;
using System.Collections.Generic;

namespace Objetivos_Prioritarios.Models.Extends
{
    public class ApiBusquedaBiometricaResponse
    {
        public bool TieneNombreConsulta { get; set; }

        public bool TieneAliasConsulta { get; set; }

        public bool TieneFotografiaConsulta { get; set; }

        public bool TieneHuellaConsulta { get; set; }

        public int TotalCoincidencias { get; set; }

        public int TotalTexto { get; set; }

        public int TotalFotografia { get; set; }

        public int TotalHuella { get; set; }

        public int TotalCombinadas { get; set; }

        public int TotalFotoHuella { get; set; }

        public List<ApiCoincidenciaBiometricaDto> Resultados { get; set; }
    }

    public class ApiCoincidenciaBiometricaDto
    {
        public int IdPersona { get; set; }

        public int IdTbFuente { get; set; }

        public int? SimilitudFoto { get; set; }

        public int? SimilitudHuella { get; set; }

        public int? SimilitudNombre { get; set; }

        public int? SimilitudAlias { get; set; }

        public string AliasCoincidente { get; set; }

        public string TextoCoincidente { get; set; }

        public int CriteriosCumplidos { get; set; }

        public string TipoCoincidencia { get; set; }


        /*
         * ============================================================
         * MANDAMIENTOS JUDICIALES
         * ============================================================
         */

        public bool TieneAvisoMandamientos { get; set; }

        public int TotalMandamientos { get; set; }

        public List<ApiMandamientoJudicialDto> MandamientosJudiciales { get; set; }
    }

    public class ApiMandamientoJudicialDto
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