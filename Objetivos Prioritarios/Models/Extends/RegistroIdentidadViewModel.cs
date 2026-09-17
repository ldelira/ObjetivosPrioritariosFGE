namespace Objetivos_Prioritarios.Models.Extends
{
    public class RegistroIdentidadViewModel
    {
        public int IdTbFuente { get; set; }
        public int IdPersona { get; set; }
        public object DetalleInstitucional { get; set; }
        public CoincidenciaResultadoViewModel CoincidenciaDirecta { get; set; }
        public string FotoUrl { get; set; }
        public bool TieneFotografiaConsulta { get; set; }
        public bool EsCoincidenciaDirecta => CoincidenciaDirecta != null;
        public bool PuedeCompararFotografia => TieneFotografiaConsulta &&
            EsCoincidenciaDirecta && CoincidenciaDirecta.PorcentajeFoto > 0;
    }

    public class PanelIdentidadViewModel
    {
        public CoincidenciaResultadoViewModel Principal { get; set; }
        public bool TieneFotografiaConsulta { get; set; }
    }
}
