using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Objetivos_Prioritarios.Models
{
    public partial class tb_Victimas
    {
     
        public string NombreCompleto
        {

            get
            {
                return $"{nvarchar_nombre} {nvarchar_paterno} {nvarchar_materno}".Trim();
            }
        }

    }
}