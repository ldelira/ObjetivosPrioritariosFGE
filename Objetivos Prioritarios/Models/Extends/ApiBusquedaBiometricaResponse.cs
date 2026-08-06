using System.Collections.Generic;

namespace Objetivos_Prioritarios.Models.Extends
{
    public class ApiBusquedaBiometricaResponse
    {
        public bool TieneFotografiaConsulta { get; set; }

        public bool TieneHuellaConsulta { get; set; }

        public int TotalCoincidencias { get; set; }

        public int TotalFotografia { get; set; }

        public int TotalHuella { get; set; }

        public int TotalCombinadas { get; set; }

        public List<ApiCoincidenciaBiometricaDto> Resultados { get; set; }

        public ApiBusquedaBiometricaResponse()
        {
            Resultados =
                new List<ApiCoincidenciaBiometricaDto>();
        }
    }

    public class ApiCoincidenciaBiometricaDto
    {
        public int IdPersona { get; set; }

        public int IdTbFuente { get; set; }

        public int? SimilitudFoto { get; set; }

        public int? SimilitudHuella { get; set; }

        public string TipoCoincidencia { get; set; }
    }
}