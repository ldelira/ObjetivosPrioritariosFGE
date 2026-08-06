using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Objetivos_Prioritarios.Models.Extends
{
    public class ResultadosCoincidenciasViewModel
    {

        public int TotalCoincidencias { get; set; }

        public int TotalFotografia { get; set; }

        public int TotalHuella { get; set; }

        public int TotalCombinadas { get; set; }

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