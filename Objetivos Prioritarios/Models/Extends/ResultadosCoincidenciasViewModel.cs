using System.Collections.Generic;

namespace Objetivos_Prioritarios.Models.Extends
{
    public class ResultadosCoincidenciasViewModel
    {
        public int TotalCoincidencias { get; set; }

        public int TotalTexto { get; set; }

        public int TotalFotografia { get; set; }

        public int TotalHuella { get; set; }

        /*
         * Dos o más criterios cumplidos.
         *
         * Puede ser:
         * Nombre + Alias
         * Nombre + Foto
         * Alias + Huella
         * Foto + Huella
         * etc.
         */
        public int TotalCombinadas { get; set; }

        /*
         * Exclusivamente:
         * Fotografía + Huella.
         *
         * Este es el valor que debe mostrarse
         * en la tarjeta "FOTO Y HUELLA".
         */
        public int TotalFotoHuella { get; set; }

        public bool TieneNombreConsulta { get; set; }

        public bool TieneAliasConsulta { get; set; }

        public bool TieneFotografiaConsulta { get; set; }

        public bool TieneHuellaConsulta { get; set; }

        public List<CoincidenciaResultadoViewModel> Coincidencias { get; set; }

        public CoincidenciaResultadoViewModel CoincidenciaSeleccionada { get; set; }

        public ResultadosCoincidenciasViewModel()
        {
            Coincidencias =
                new List<CoincidenciaResultadoViewModel>();
        }
    }
}