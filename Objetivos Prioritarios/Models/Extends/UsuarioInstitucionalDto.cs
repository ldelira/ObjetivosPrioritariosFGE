using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Objetivos_Prioritarios.Models.Extends
{
    public class UsuarioInstitucionalDto
    {
        public int CveUsuario { get; set; }
        public string Login { get; set; }
        public string Nombre { get; set; }
        public string Paterno { get; set; }
        public string Materno { get; set; }
        public string NombreCompleto { get; set; }
        public string Puesto { get; set; }
        public string Area { get; set; }
        public string Agencia { get; set; }
        public string CveAgencia { get; set; }
        public int IdUnidad { get; set; }
        public string Unidad { get; set; }
        public bool ExisteEnObjetivos { get; set; }
        public bool ActivoEnObjetivos { get; set; }

        public UsuarioInstitucionalDto()
        {
            Login = "";
            Nombre = "";
            Paterno = "";
            Materno = "";
            NombreCompleto = "";
            Puesto = "";
            Area = "";
            Agencia = "";
            CveAgencia = "";
            Unidad = "";
        }
    }
}