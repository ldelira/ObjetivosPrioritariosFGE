using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Objetivos_Prioritarios.Models
{
    public partial class tb_Usuarios
    {
        int unidadId = 0;
        public int UnidadId
        {

            get
            {
                return unidadId;
            }
            set
            {
                unidadId = value;
            }
        }

    }
}