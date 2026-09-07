using System;

namespace Objetivos_Prioritarios.Models.Extends
{
    public class DetalleC5ApiDto
    {
        /*
         * ============================================================
         * IDENTIFICADORES
         * ============================================================
         */

        public int IdDetenido { get; set; }

        public int IdDetencion { get; set; }

        public int IdTbFuente { get; set; }

        public string Fuente { get; set; }


        /*
         * ============================================================
         * PERSONA
         * ============================================================
         */

        public string NombreCompleto { get; set; }

        public string Nombre { get; set; }

        public string ApPaterno { get; set; }

        public string ApMaterno { get; set; }

        public string Alias { get; set; }

        public int? Edad { get; set; }

        public string Sexo { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public string Originario { get; set; }

        public string Ocupacion { get; set; }

        public string CaracteristicasDetenido { get; set; }

        public string Telefono { get; set; }

        public string Curp { get; set; }

        public string Rfc { get; set; }


        /*
         * ============================================================
         * REGISTRO
         * ============================================================
         */

        public string Folio { get; set; }

        public string Situacion { get; set; }

        public string Estatus { get; set; }

        public DateTime? FechaRegistro { get; set; }


        /*
         * ============================================================
         * DETENCIÓN
         * ============================================================
         */

        public string SerieMunicipio { get; set; }

        public string Consecutivo { get; set; }

        public int? Anio { get; set; }

        public DateTime? FechaDetencion { get; set; }

        public string HoraDetencion { get; set; }

        public string Fuero { get; set; }

        public string FueroDescripcion { get; set; }

        public int? NumeroDetenidos { get; set; }


        /*
         * ============================================================
         * UBICACIÓN
         * ============================================================
         */

        public string Calle { get; set; }

        public string Colonia { get; set; }

        public string NumeroExterior { get; set; }

        public string NumeroInterior { get; set; }

        public string Cp { get; set; }

        public string Municipio { get; set; }

        public decimal? Latitud { get; set; }

        public decimal? Longitud { get; set; }


        /*
         * ============================================================
         * HECHOS
         * ============================================================
         */

        public string Hecho { get; set; }

        public string Causa { get; set; }

        public string Observaciones { get; set; }


        /*
         * ============================================================
         * ASEGURAMIENTOS
         * ============================================================
         */

        public string Aseguramientos { get; set; }

        public string Envoltorios { get; set; }

        public string Armas { get; set; }

        public string Cartuchos { get; set; }

        public string Cargadores { get; set; }

        public decimal? Gramos { get; set; }

        public string Vehiculo { get; set; }


        /*
         * ============================================================
         * ADMINISTRATIVO
         * ============================================================
         */

        public string OficialTraslada { get; set; }
    }
}