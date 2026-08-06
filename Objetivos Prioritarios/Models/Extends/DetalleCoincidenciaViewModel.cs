using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Objetivos_Prioritarios.Models.Extends
{
    public class DetalleCoincidenciaViewModel
    {


        public CoincidenciaResultadoViewModel Coincidencia { get; set; }

        public bool TieneFotografiaConsulta { get; set; }

        public bool TieneHuellaConsulta { get; set; }


    }
}