using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.Models.Extends
{
    public class BusquedaCoincidenciasViewModel
    {
        /* =========================================================
         * CRITERIOS TEXTUALES
         * ========================================================= */

        /// <summary>
        /// Nombre conocido de la persona.
        /// Ejemplo: BENJAMIN.
        /// </summary>
        public string NombreBusqueda { get; set; }

        /// <summary>
        /// Alias conocido de la persona.
        /// Ejemplo: EL BENJAS.
        /// </summary>
        public string AliasBusqueda { get; set; }

        /*
         * Se conserva temporalmente para evitar romper
         * código anterior mientras se realiza la migración.
         *
         * Después podrá eliminarse cuando la vista,
         * el controlador y la API utilicen únicamente:
         *
         * NombreBusqueda
         * AliasBusqueda
         */
        public string TextoBusqueda { get; set; }

        /*
         * Valores permitidos:
         *
         * NOMBRE
         * ALIAS
         * AMBOS
         */
        public string ModoBusquedaTexto { get; set; }

        /*
         * Valores permitidos:
         *
         * PRIORIZAR
         * ESTRICTO
         *
         * PRIORIZAR:
         * Conserva candidatos y coloca primero a quienes
         * cumplan una mayor cantidad de criterios.
         *
         * ESTRICTO:
         * Exige que se cumplan todos los criterios
         * capturados por el usuario.
         */
        public string ModoCombinacion { get; set; }


        /* =========================================================
         * BIOMETRÍA
         * ========================================================= */

        public HttpPostedFileBase Fotografia { get; set; }

        public HttpPostedFileBase Huella { get; set; }


        /* =========================================================
         * FILTROS AVANZADOS
         * ========================================================= */

        public string Municipio { get; set; }

        public int? EdadMinima { get; set; }

        public int? EdadMaxima { get; set; }

        public string Sexo { get; set; }


        /* =========================================================
         * CONFIGURACIÓN DE RESULTADOS
         * ========================================================= */

        public string TipoCoincidencia { get; set; }

        public int PorcentajeMinimo { get; set; }


        /* =========================================================
         * CATÁLOGOS
         * ========================================================= */

        public List<SelectListItem> Municipios { get; set; }

        public List<SelectListItem> Sexos { get; set; }

        public List<SelectListItem> TiposCoincidencia { get; set; }


        /* =========================================================
         * PROPIEDADES AUXILIARES
         * ========================================================= */

        public bool TieneNombreBusqueda
        {
            get
            {
                return !string.IsNullOrWhiteSpace(
                    NombreBusqueda
                );
            }
        }


        public bool TieneAliasBusqueda
        {
            get
            {
                return !string.IsNullOrWhiteSpace(
                    AliasBusqueda
                );
            }
        }


        public bool TieneTextoBusqueda
        {
            get
            {
                return
                    TieneNombreBusqueda ||
                    TieneAliasBusqueda ||
                    !string.IsNullOrWhiteSpace(
                        TextoBusqueda
                    );
            }
        }


        public bool TieneFotografia
        {
            get
            {
                return
                    Fotografia != null &&
                    Fotografia.ContentLength > 0;
            }
        }


        public bool TieneHuella
        {
            get
            {
                return
                    Huella != null &&
                    Huella.ContentLength > 0;
            }
        }


        public bool TieneAlgunCriterioBusqueda
        {
            get
            {
                return
                    TieneNombreBusqueda ||
                    TieneAliasBusqueda ||
                    TieneFotografia ||
                    TieneHuella;
            }
        }


        public bool EsModoEstricto
        {
            get
            {
                return string.Equals(
                    ModoCombinacion,
                    "ESTRICTO",
                    System.StringComparison.OrdinalIgnoreCase
                );
            }
        }


        public bool EsModoPriorizar
        {
            get
            {
                return !EsModoEstricto;
            }
        }


        /* =========================================================
         * CONSTRUCTOR
         * ========================================================= */

        public BusquedaCoincidenciasViewModel()
        {
            NombreBusqueda =
                string.Empty;

            AliasBusqueda =
                string.Empty;

            TextoBusqueda =
                string.Empty;

            /*
             * Inicia buscando por nombre.
             */
            ModoBusquedaTexto =
                "NOMBRE";

            /*
             * El modo recomendado conserva candidatos
             * y los ordena por evidencias cumplidas.
             */
            ModoCombinacion =
                "PRIORIZAR";

            /*
             * Los filtros de edad quedan vacíos.
             * Solo deben aplicarse cuando el usuario
             * capture expresamente un valor.
             */
            EdadMinima =
                null;

            EdadMaxima =
                null;

            PorcentajeMinimo =
                70;

            Municipio =
                string.Empty;

            Sexo =
                string.Empty;

            TipoCoincidencia =
                string.Empty;

            Municipios =
                new List<SelectListItem>();

            Sexos =
                new List<SelectListItem>();

            TiposCoincidencia =
                new List<SelectListItem>();
        }
    }
}