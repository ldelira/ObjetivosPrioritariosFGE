using System;

namespace Objetivos_Prioritarios.Models.Extends
{
    public class DetalleFiscaliaWebApiDto
    {
        public int IdPersona { get; set; }

        public int IdTbFuente { get; set; }

        public string Fuente { get; set; }

        public string NombreCompleto { get; set; }

        public string Nombre { get; set; }

        public string Paterno { get; set; }

        public string Materno { get; set; }

        public int? Edad { get; set; }

        public string Sexo { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public DateTime? FechaAusencia { get; set; }

        public string LugarAusencia { get; set; }

        public string Estatura { get; set; }

        public string Peso { get; set; }

        public string Complexion { get; set; }

        public string Tez { get; set; }

        public string Cabello { get; set; }

        public string Ojos { get; set; }

        public string Nariz { get; set; }

        public string Boca { get; set; }

        public string SenasParticulares { get; set; }

        public string Vestimenta { get; set; }

        public string ResumenHechos { get; set; }

        public string Prioridad { get; set; }

        public DateTime? FechaAlta { get; set; }

        public bool TieneFoto { get; set; }

        public string FotoUrl { get; set; }
    }
}