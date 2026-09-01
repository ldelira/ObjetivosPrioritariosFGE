using System;
using System.Collections.Generic;


namespace Objetivos_Prioritarios.Models.Extends
{
    public class AdministracionUsuarioViewModel
    {
        public UsuarioInstitucionalDto Usuario { get; set; }
        public List<PerfilUsuarioViewModel> Perfiles { get; set; }

        public AdministracionUsuarioViewModel()
        {
            Perfiles = new List<PerfilUsuarioViewModel>();
        }
    }
}