using System;
using System.Collections.Generic;

namespace Objetivos_Prioritarios.Models.Extends
{
    public class DetalleObjetivoApiDto
    {
        public int IdObjetivo { get; set; }

        public bool Activo { get; set; }

        public string NombrePrincipal { get; set; }

        public List<string> OtrosNombres { get; set; } =
            new List<string>();

        public List<string> Alias { get; set; } =
            new List<string>();

        public List<int> ClavesPersoRelacionadas { get; set; } =
            new List<int>();

        public DateTime? FechaNacimiento { get; set; }

        public bool TieneFoto { get; set; }


        /*
         * ============================================================
         * ESTATUS DEL PROCESO
         * ============================================================
         */

        public int? IdEstatusProceso { get; set; }

        public string EstatusProceso { get; set; }

        public string DescripcionEstatusProceso { get; set; }

        public string ObservacionesFicha { get; set; }


        /*
         * ============================================================
         * DOMICILIOS
         * ============================================================
         */

        public List<DomicilioObjetivoApiDto> Domicilios { get; set; } =
            new List<DomicilioObjetivoApiDto>();


        /*
         * ============================================================
         * GRUPOS DELICTIVOS
         * ============================================================
         */

        public List<GrupoObjetivoApiDto> Grupos { get; set; } =
            new List<GrupoObjetivoApiDto>();


        /*
         * ============================================================
         * CARPETAS DE INVESTIGACIÓN
         * ============================================================
         */

        public List<CarpetaObjetivoApiDto> Carpetas { get; set; } =
            new List<CarpetaObjetivoApiDto>();


        /*
         * ============================================================
         * DETENCIONES / FILIACIÓN
         * ============================================================
         */

        public List<DetencionObjetivoApiDto> Detenciones { get; set; } =
            new List<DetencionObjetivoApiDto>();


        /*
         * ============================================================
         * MANDAMIENTOS VINCULADOS
         * ============================================================
         */

        public List<MandamientoVinculadoObjetivoApiDto> MandamientosVinculados { get; set; } =
            new List<MandamientoVinculadoObjetivoApiDto>();


        /*
         * ============================================================
         * ASUNTOS RELACIONADOS
         * ============================================================
         */

        public List<AsuntoRelacionadoObjetivoApiDto> AsuntosRelacionados { get; set; } =
            new List<AsuntoRelacionadoObjetivoApiDto>();
    }


    public class DomicilioObjetivoApiDto
    {
        public int IdInformacion { get; set; }

        public string Estado { get; set; }

        public string Municipio { get; set; }

        public string Colonia { get; set; }

        public string Calle { get; set; }

        public string Numero { get; set; }

        public string CP { get; set; }

        public string Observaciones { get; set; }

        public int? ClavePerso { get; set; }
    }


    public class GrupoObjetivoApiDto
    {
        public int IdObjetivoGrupo { get; set; }

        public int IdGrupo { get; set; }

        public string AliasGrupo { get; set; }

        public string Grupo { get; set; }

        public string Puesto { get; set; }

        public string Funcion { get; set; }

        public DateTime? FechaIngreso { get; set; }

        public DateTime? FechaSalida { get; set; }

        public string Observaciones { get; set; }
    }


    public class CarpetaObjetivoApiDto
    {
        public int IdCarpetaObjetivo { get; set; }

        public string NumeroCarpeta { get; set; }

        public int CveDelito { get; set; }

        public string Delito { get; set; }

        public string Observaciones { get; set; }

        public DateTime? FechaAlta { get; set; }
    }


    public class DetencionObjetivoApiDto
    {
        public int IdDetenido { get; set; }

        public string NumeroCarpeta { get; set; }

        public int? ClavePersona { get; set; }

        public DateTime? FechaDetencion { get; set; }

        public TimeSpan? HoraDetencion { get; set; }

        public DateTime? FechaIngreso { get; set; }

        public DateTime? FechaCapturaFiliacion { get; set; }

        public List<DelitoDetencionObjetivoApiDto> Delitos { get; set; } =
            new List<DelitoDetencionObjetivoApiDto>();
    }


    public class DelitoDetencionObjetivoApiDto
    {
        public int IdDelitoIngreso { get; set; }

        public int CveDelito { get; set; }

        public string Delito { get; set; }
    }


    public class MandamientoVinculadoObjetivoApiDto
    {
        public int IdOrdenAprehension { get; set; }

        public int IdMandamientoJudicial { get; set; }

        public int IdDelito { get; set; }

        public string Delito { get; set; }

        public int? IdEstadoProceso { get; set; }

        public string Tipo { get; set; }

        public DateTime? FechaEstatus { get; set; }

        public DateTime? FechaRegistro { get; set; }
    }


    public class AsuntoRelacionadoObjetivoApiDto
    {
        public int IdFichaAsunto { get; set; }

        public int IdAsuntoRelacionado { get; set; }

        public string Alias { get; set; }

        public string Descripcion { get; set; }

        public string NumeroCarpeta { get; set; }

        public DateTime? FechaAsunto { get; set; }

        public int? IdEstatusAsunto { get; set; }

        public string EstatusAsunto { get; set; }

        public int? IdRolParticipacion { get; set; }

        public string RolParticipacion { get; set; }

        public string DescripcionParticipacion { get; set; }

        public string ObservacionesRelacion { get; set; }

        public DateTime? FechaRelacion { get; set; }

        public bool ActivoAsunto { get; set; }
    }
}