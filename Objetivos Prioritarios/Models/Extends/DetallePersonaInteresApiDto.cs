using System;
using System.Collections.Generic;

namespace Objetivos_Prioritarios.Models.Extends
{
    public class DetallePersonaInteresApiDto
    {
        public int IdPersona { get; set; }

        public int IdTbFuente { get; set; }

        public string Fuente { get; set; }

        public string Nombre { get; set; }

        public string Paterno { get; set; }

        public string Materno { get; set; }

        public string NombreCompleto { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public int? Edad { get; set; }

        public string Estatura { get; set; }

        public string Sexo { get; set; }

        public string Observaciones { get; set; }

        public DateTime? FechaRegistro { get; set; }

        public int? IdFotoPrincipal { get; set; }

        public int TotalFotografias { get; set; }

        public int TotalHuellas { get; set; }

        public string EstadoBusqueda { get; set; }

        public DetallePersonaInteresPeriodoApiDto PeriodoActivo { get; set; }

        public List<DetallePersonaInteresFotoApiDto> Fotografias { get; set; }

        public List<DetallePersonaInteresHuellaApiDto> Huellas { get; set; }

        public List<DetallePersonaInteresPeriodoApiDto> PeriodosBusqueda { get; set; }
    }


    public class DetallePersonaInteresFotoApiDto
    {
        public int IdFoto { get; set; }

        public int IdTipoFoto { get; set; }

        public string TipoFoto { get; set; }

        public DateTime? FechaRegistro { get; set; }

        public bool Activo { get; set; }
    }


    public class DetallePersonaInteresHuellaApiDto
    {
        public int IdFicha { get; set; }

        public DateTime? FechaRegistro { get; set; }

        public bool Activo { get; set; }
    }


    public class DetallePersonaInteresPeriodoApiDto
    {
        public int IdPeriodoBusqueda { get; set; }

        public DateTime FechaInicioBusqueda { get; set; }

        public DateTime? FechaFinBusqueda { get; set; }

        public DateTime? FechaRegistro { get; set; }

        public bool Activo { get; set; }

        public int? IdEstatusPeriodoBusqueda { get; set; }

        public string Estatus { get; set; }

        public string Observaciones { get; set; }

        public DateTime? FechaCancelacion { get; set; }

        public string MotivoCancelacion { get; set; }

        public int DiasTotales { get; set; }

        public int DiasRestantes { get; set; }
    }
}