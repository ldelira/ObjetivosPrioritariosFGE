using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Objetivos_Prioritarios.Models.Extends
{
    public class PerfilUsuarioViewModel
    {
        public int IdPerfil { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Seleccionado { get; set; }

        public PerfilUsuarioViewModel()
        {
            Clave = "";
            Nombre = "";
            Descripcion = "";
        }
    }
}