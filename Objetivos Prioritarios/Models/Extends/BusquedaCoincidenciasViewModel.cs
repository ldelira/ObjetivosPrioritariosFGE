using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.Models.Extends
{
        public class BusquedaCoincidenciasViewModel
        {
            public HttpPostedFileBase Fotografia { get; set; }

            public HttpPostedFileBase Huella { get; set; }

            public string Municipio { get; set; }

            public int EdadMinima { get; set; }

            public int EdadMaxima { get; set; }

            public string Sexo { get; set; }

            public string TipoCoincidencia { get; set; }

            public int PorcentajeMinimo { get; set; }

            public List<SelectListItem> Municipios { get; set; }

            public List<SelectListItem> Sexos { get; set; }

            public List<SelectListItem> TiposCoincidencia { get; set; }

            public BusquedaCoincidenciasViewModel()
            {
                EdadMinima = 18;
                EdadMaxima = 99;
                PorcentajeMinimo = 70;

                Municipios = new List<SelectListItem>();
                Sexos = new List<SelectListItem>();
                TiposCoincidencia = new List<SelectListItem>();
            }
        }
    
}