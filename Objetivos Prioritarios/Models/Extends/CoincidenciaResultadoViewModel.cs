using System;
using System.Collections.Generic;

namespace Objetivos_Prioritarios.Models.Extends
{
    public class CoincidenciaResultadoViewModel
    {
        /*
         * ============================================================
         * IDENTIFICACIÓN DEL RESULTADO
         * ============================================================
         */

        public int IdCoincidencia { get; set; }

        public int IdPersona { get; set; }

        public int IdTbFuente { get; set; }

        public string NombreFuente { get; set; }

        public bool EsFuenteInformativa { get; set; }


        /*
         * ============================================================
         * INFORMACIÓN GENERAL
         * ============================================================
         */

        public string NombreCompleto { get; set; }

        public string OtrosNombres { get; set; }

        public string Alias { get; set; }

        public string Folio { get; set; }

        public string Expediente { get; set; }

        public string MunicipioClave { get; set; }

        public string Municipio { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public int Edad { get; set; }

        public string Sexo { get; set; }

        public string FotoUrl { get; set; }

        public DateTime FechaRegistro { get; set; }


        /*
         * ============================================================
         * IDENTIDAD FGEA
         * ============================================================
         */

        /*
         * Fuente 6:
         * Filiacion.dbo.Persona.CLAVE_PERSO
         *
         * Fuente 5:
         * Se llena únicamente cuando el Objetivo tiene
         * una sola CLAVE_PERSO válida relacionada.
         */
        public int? ClavePerso { get; set; }

        /*
         * Fuente 5:
         *
         * Un Objetivo puede tener varios nombres y esos nombres
         * pueden estar relacionados con distintas CLAVE_PERSO.
         *
         * Ejemplo:
         * "36190"
         * "36190, 45210"
         */
        public string ClavesPersoRelacionadas { get; set; }


        /*
         * ============================================================
         * FUENTE 6 - DETENIDOS FGEA
         * ============================================================
         */

        public string UltimoDelito { get; set; }

        public DateTime? FechaUltimoDelito { get; set; }


        /*
         * ============================================================
         * FUENTE 5 - OBJETIVOS PRIORITARIOS
         * ============================================================
         */

        public string GrupoDelictivo { get; set; }

        public string Puesto { get; set; }

        public string EstatusGrupo { get; set; }

        public string EstatusObjetivo { get; set; }


        /*
         * ============================================================
         * RESULTADO DE COINCIDENCIA
         * ============================================================
         */

        public string TipoCoincidencia { get; set; }

        public int CriteriosCumplidos { get; set; }

        public decimal SimilitudGlobal { get; set; }


        /*
         * ============================================================
         * EVIDENCIA TEXTUAL
         * ============================================================
         */

        public decimal PorcentajeNombre { get; set; }

        public decimal PorcentajeAlias { get; set; }

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
         * ============================================================
         * EVIDENCIA BIOMÉTRICA
         * ============================================================
         */

        public decimal PorcentajeFoto { get; set; }

        public decimal PorcentajeHuella { get; set; }


        /*
         * ============================================================
         * MANDAMIENTOS JUDICIALES
         * ============================================================
         */

        public bool TieneAvisoMandamientos { get; set; }

        public int TotalMandamientos { get; set; }

        public List<MandamientoJudicialViewModel> MandamientosJudiciales
        {
            get;
            set;
        } = new List<MandamientoJudicialViewModel>();


        /*
         * ============================================================
         * PROPIEDADES CALCULADAS
         * ============================================================
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