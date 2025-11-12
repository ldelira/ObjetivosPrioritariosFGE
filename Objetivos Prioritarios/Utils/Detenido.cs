using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Objetivos_Prioritarios.Utils
{
    public class Detenido
    {
        public string Nombre { get; set; }
        public string Cartel { get; set; }
        public string Ocupacion { get; set; }
        public List<string> Delitos { get; set; }
        public List<string> Carpetas { get; set; }
        public List<string> Ordenes { get; set; }
        public string Estatus { get; set; }
        public string DescripcionEstatus { get; set; }
        public List<string> Asunto { get; set; }
        public List<Victima> Victimas { get; set; }
        public string Foto { get; set; }
        public string FechaNacimiento { get; set; }
        public string Edad { get; set; }
        public string Alias { get; set; }


    }


    public class Victima
    {
        public string Nombre { get; set; }
        public string Foto { get; set; }
    }
}