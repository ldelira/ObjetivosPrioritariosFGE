using Objetivos_Prioritarios.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Objetivos_Prioritarios.Utils
{
    public class BasicOperationResponse
    {
        public bool IsSuccess { get; set; } = false;
        public string Message { get; set; }
        public bool HasWarning { get; set; } = false;
        public string ExtraData { get; set; }

        public tb_Usuarios user { get; set; }
        public int Id { get; set; }

    }
}