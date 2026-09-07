using System;
using System.Collections.Generic;

namespace Objetivos_Prioritarios.Models.Extends
{
    public class DetalleFGEADetenidoApiDto
    {
        public int ClavePerso { get; set; }

        public int IdTbFuente { get; set; }

        public string Fuente { get; set; }

        public string NumeroFicha { get; set; }

        public string NumeroFiliacion { get; set; }

        public string NombreCompleto { get; set; }

        public string AliasPrincipal { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public DateTime? FechaFiliacion { get; set; }

        public int? Edad { get; set; }

        public string Sexo { get; set; }

        public string Estatura { get; set; }

        public string Peso { get; set; }

        public string Observaciones { get; set; }

        public string Telefono { get; set; }


        /*
         * ============================================================
         * TRABAJO
         * ============================================================
         */

        public string LugarTrabajo { get; set; }

        public string UbicacionTrabajo { get; set; }

        public string TelefonoTrabajo { get; set; }


        /*
         * ============================================================
         * DOMICILIO BASE
         * ============================================================
         */

        public int? ClaveCalle { get; set; }

        public string Calle { get; set; }

        public string NumeroCasa { get; set; }

        public int? ClaveColonia { get; set; }

        public string Colonia { get; set; }

        public string CodigoPostal { get; set; }

        public int? ClaveMunicipio { get; set; }

        public string Municipio { get; set; }

        public int? ClaveEstado { get; set; }

        public string Estado { get; set; }

        public string CoordX { get; set; }

        public string CoordY { get; set; }


        /*
         * ============================================================
         * LISTAS
         * ============================================================
         */

        public List<DetalleFGEANombreApiDto> Nombres { get; set; }

        public List<DetalleFGEAAliasApiDto> Alias { get; set; }

        public List<DetalleFGEADomicilioApiDto> Domicilios { get; set; }

        public List<DetalleFGEAIngresoApiDto> Ingresos { get; set; }

        public List<DetalleFGEADelitoApiDto> Delitos { get; set; }


        public DetalleFGEADetenidoApiDto()
        {
            Fuente = string.Empty;
            NumeroFicha = string.Empty;
            NumeroFiliacion = string.Empty;
            NombreCompleto = string.Empty;
            AliasPrincipal = string.Empty;
            Sexo = string.Empty;
            Estatura = string.Empty;
            Peso = string.Empty;
            Observaciones = string.Empty;
            Telefono = string.Empty;

            LugarTrabajo = string.Empty;
            UbicacionTrabajo = string.Empty;
            TelefonoTrabajo = string.Empty;

            Calle = string.Empty;
            NumeroCasa = string.Empty;
            Colonia = string.Empty;
            CodigoPostal = string.Empty;
            Municipio = string.Empty;
            Estado = string.Empty;
            CoordX = string.Empty;
            CoordY = string.Empty;

            Nombres = new List<DetalleFGEANombreApiDto>();
            Alias = new List<DetalleFGEAAliasApiDto>();
            Domicilios = new List<DetalleFGEADomicilioApiDto>();
            Ingresos = new List<DetalleFGEAIngresoApiDto>();
            Delitos = new List<DetalleFGEADelitoApiDto>();
        }
    }


    public class DetalleFGEANombreApiDto
    {
        public int IdNomPerso { get; set; }

        public string Nombre { get; set; }

        public string ApPaterno { get; set; }

        public string ApMaterno { get; set; }

        public string NombreCompleto { get; set; }

        public DateTime? FechaCaptura { get; set; }


        public DetalleFGEANombreApiDto()
        {
            Nombre = string.Empty;
            ApPaterno = string.Empty;
            ApMaterno = string.Empty;
            NombreCompleto = string.Empty;
        }
    }


    public class DetalleFGEAAliasApiDto
    {
        public string Alias { get; set; }

        public DateTime? FechaAlias { get; set; }

        public DateTime? FechaCaptura { get; set; }


        public DetalleFGEAAliasApiDto()
        {
            Alias = string.Empty;
        }
    }


    public class DetalleFGEADomicilioApiDto
    {
        public int Id { get; set; }

        public DateTime? FechaAproxVivia { get; set; }

        public DateTime? FechaAlta { get; set; }

        public int? ClaveCalle { get; set; }

        public string Calle { get; set; }

        public string NumeroCasa { get; set; }

        public int? ClaveColonia { get; set; }

        public string Colonia { get; set; }

        public string CodigoPostal { get; set; }

        public int? ClaveMunicipio { get; set; }

        public string Municipio { get; set; }

        public int? ClaveEstado { get; set; }

        public string Estado { get; set; }

        public bool EsActual { get; set; }

        public string CoordX { get; set; }

        public string CoordY { get; set; }


        public DetalleFGEADomicilioApiDto()
        {
            Calle = string.Empty;
            NumeroCasa = string.Empty;
            Colonia = string.Empty;
            CodigoPostal = string.Empty;
            Municipio = string.Empty;
            Estado = string.Empty;
            CoordX = string.Empty;
            CoordY = string.Empty;
        }
    }


    public class DetalleFGEAIngresoApiDto
    {
        public int Id { get; set; }

        public DateTime? FechaIngreso { get; set; }

        public string HoraIngreso { get; set; }

        public DateTime? FechaDetencion { get; set; }

        public string HoraDetencion { get; set; }

        public string LugarDetencion { get; set; }

        public string MotivoDetencion { get; set; }

        public string PersonaDetiene { get; set; }

        public string DetenidoPor { get; set; }

        public string MinisterioPublico { get; set; }

        public string NumeroAvp { get; set; }

        public int? Consigna { get; set; }

        public string LugarConsigna { get; set; }

        public string ObservacionConsigna { get; set; }

        public int? Detenido { get; set; }

        public string DisposicionDetenido { get; set; }

        public string NumeroOrden { get; set; }

        public string EntidadFederativa { get; set; }

        public string SaleLibre { get; set; }

        public string PersonaSeEncontraba { get; set; }

        public DateTime? FechaCaptura { get; set; }

        public string CoordX { get; set; }

        public string CoordY { get; set; }


        public DetalleFGEAIngresoApiDto()
        {
            HoraIngreso = string.Empty;
            HoraDetencion = string.Empty;
            LugarDetencion = string.Empty;
            MotivoDetencion = string.Empty;
            PersonaDetiene = string.Empty;
            DetenidoPor = string.Empty;
            MinisterioPublico = string.Empty;
            NumeroAvp = string.Empty;
            LugarConsigna = string.Empty;
            ObservacionConsigna = string.Empty;
            DisposicionDetenido = string.Empty;
            NumeroOrden = string.Empty;
            EntidadFederativa = string.Empty;
            SaleLibre = string.Empty;
            PersonaSeEncontraba = string.Empty;
            CoordX = string.Empty;
            CoordY = string.Empty;
        }
    }


    public class DetalleFGEADelitoApiDto
    {
        public int Id { get; set; }

        public DateTime? FechaDelito { get; set; }

        public int? ClaveDelito { get; set; }

        public string Delito { get; set; }

        public string Observacion { get; set; }

        public DateTime? FechaCaptura { get; set; }


        public DetalleFGEADelitoApiDto()
        {
            Delito = string.Empty;
            Observacion = string.Empty;
        }
    }
}