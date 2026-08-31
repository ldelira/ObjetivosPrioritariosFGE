using System.Collections.Generic;

namespace Objetivos_Prioritarios.Models.Extends
{
    public class PermisosUsuarioDto
    {
        public int IdUsuario { get; set; }
        public string Login { get; set; }
        public string Nombre { get; set; }
        public string Puesto { get; set; }
        public bool Activo { get; set; }
        public bool EsAdministrador { get; set; }
        public List<string> Perfiles { get; set; }
        public List<string> Modulos { get; set; }
        public List<int> FuentesBusqueda { get; set; }
        public List<string> Permisos { get; set; }

        public PermisosUsuarioDto()
        {
            Login = "";
            Nombre = "";
            Puesto = "";
            Perfiles = new List<string>();
            Modulos = new List<string>();
            FuentesBusqueda = new List<int>();
            Permisos = new List<string>();
        }
    }
}