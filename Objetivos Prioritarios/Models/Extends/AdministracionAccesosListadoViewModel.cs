using System;
using System.Collections.Generic;

namespace Objetivos_Prioritarios.Models.Extends
{
    public class UsuarioSistemaListadoViewModel
    {
        public int IdUsuario { get; set; }
        public string Login { get; set; }
        public string Nombre { get; set; }
        public string Puesto { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string UsuarioModificacion { get; set; }
        public List<string> Perfiles { get; set; } = new List<string>();
    }

    public class BitacoraBusquedaListadoViewModel
    {
        public long IdBitacoraBusqueda { get; set; }
        public string Usuario { get; set; }
        public DateTime FechaBusqueda { get; set; }
        public string ModoBusquedaTexto { get; set; }
        public string ModoCombinacion { get; set; }
        public string TextoBusqueda { get; set; }
        public string AliasBusqueda { get; set; }
        public int? PorcentajeMinimo { get; set; }
        public bool TieneFotografia { get; set; }
        public bool FotografiaDisponible { get; set; }
        public int CantidadHuellas { get; set; }
        public string HashFotografia { get; set; }
        public string HashHuellas { get; set; }
        public int TotalCoincidencias { get; set; }
        public long? TiempoMs { get; set; }
        public bool Exito { get; set; }
        public string Error { get; set; }
        public string Ip { get; set; }
        public string UserAgent { get; set; }
        public int TotalAccesos { get; set; }
    }

    public class BitacoraBusquedaDetalleViewModel : BitacoraBusquedaListadoViewModel
    {
        public List<BitacoraAccesoResultadoViewModel> Accesos { get; set; } = new List<BitacoraAccesoResultadoViewModel>();
    }

    public class BitacoraAccesoResultadoViewModel
    {
        public long IdBitacoraAcceso { get; set; }
        public DateTime FechaAcceso { get; set; }
        public int IdTbFuente { get; set; }
        public string Fuente { get; set; }
        public int IdPersona { get; set; }
        public int? IdCoincidencia { get; set; }
        public bool EsCoincidenciaDirecta { get; set; }
        public string TipoAccion { get; set; }
    }
}
