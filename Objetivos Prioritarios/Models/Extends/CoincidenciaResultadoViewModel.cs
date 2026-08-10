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

        public int CriteriosCumplidos { get; set; }

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
        /*
 * Nombre legible de la fuente.
 *
 * Ejemplo:
 * C5 - Detenidos
 * Detenidos FGEA
 * Objetivos prioritarios
 */
        public string NombreFuente { get; set; }

        /*
         * Indica que la fuente no representa evidencia
         * biométrica principal.
         *
         * Actualmente aplica a Mandamientos Judiciales.
         */
        public bool EsFuenteInformativa { get; set; }

        /*
         * Porcentaje de coincidencia contra el alias.
         */
        public decimal PorcentajeAlias { get; set; }

        /*
         * Mejor porcentaje entre nombre y alias.
         */
        public decimal PorcentajeTexto { get; set; }

        /*
         * NOMBRE
         * ALIAS
         * NOMBRE_Y_ALIAS
         */
        public string OrigenCoincidenciaTexto { get; set; }

        /*
         * Nombre o alias específico que produjo
         * la coincidencia.
         *
         * Ejemplo:
         * PEPE
         * JOSÉ JUAN PÉREZ HUERTA
         */
        public string TextoCoincidente { get; set; }

        /*
         * Indica si el resultado tuvo alguna
         * coincidencia textual.
         */
        public bool TieneCoincidenciaTexto
        {
            get
            {
                return
                    PorcentajeNombre > 0 ||
                    PorcentajeAlias > 0;
            }
        }

        /*
         * Indica si el resultado tuvo alguna
         * coincidencia biométrica.
         */
        public bool TieneCoincidenciaBiometrica
        {
            get
            {
                return
                    PorcentajeFoto > 0 ||
                    PorcentajeHuella > 0;
            }
        }
    }
}