using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Objetivos_Prioritarios.Utils
{
    public class ImagenArchivoResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public byte[] Bytes { get; set; }
        public string MimeType { get; set; }
        public string NombreArchivo { get; set; }
    }
}