using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Models.Extends;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Objetivos_Prioritarios.ControllersServices
{
    public static class BitacoraBusquedaService
    {
        public static long RegistrarInicio(BusquedaCoincidenciasViewModel modelo, string usuario, string ip, string userAgent)
        {
            List<HttpPostedFileBase> huellas = ObtenerHuellas(modelo);
            using (var db = new Filiacion_MunicipiosBitacoraEntities())
            {
                var registro = new tb_BitacoraBusqueda
                {
                    Usuario = string.IsNullOrWhiteSpace(usuario) ? "SIN_USUARIO" : Limitar(usuario, 100),
                    FechaBusqueda = DateTime.Now,
                    ModoBusquedaTexto = Limitar(modelo?.ModoBusquedaTexto, 30),
                    ModoCombinacion = Limitar(modelo?.ModoCombinacion, 30),
                    TextoBusqueda = Limitar(ObtenerTextoBusqueda(modelo), 300),
                    AliasBusqueda = Limitar(modelo?.AliasBusqueda, 300),
                    PorcentajeMinimo = modelo?.PorcentajeMinimo,
                    TieneFotografia = modelo?.Fotografia != null && modelo.Fotografia.ContentLength > 0,
                    CantidadHuellas = huellas.Count,
                    HashFotografia = CalcularHash(modelo?.Fotografia),
                    HashHuellas = string.Join("|", huellas.Select(CalcularHash).Where(x => !string.IsNullOrWhiteSpace(x))),
                    TotalCoincidencias = 0,
                    TiempoMs = null,
                    Exito = false,
                    Error = null,
                    Ip = Limitar(ip, 50),
                    UserAgent = Limitar(userAgent, 500)
                };

                db.tb_BitacoraBusqueda.Add(registro);
                db.SaveChanges();
                return registro.IdBitacoraBusqueda;
            }
        }

        public static void RegistrarExito(long idBitacoraBusqueda, int totalCoincidencias, long tiempoMs)
        {
            if (idBitacoraBusqueda <= 0) return;
            using (var db = new Filiacion_MunicipiosBitacoraEntities())
            {
                var registro = db.tb_BitacoraBusqueda.Find(idBitacoraBusqueda);
                if (registro == null) return;
                registro.TotalCoincidencias = totalCoincidencias;
                registro.TiempoMs = tiempoMs;
                registro.Exito = true;
                registro.Error = null;
                db.SaveChanges();
            }
        }

        public static void RegistrarError(long idBitacoraBusqueda, long tiempoMs, string error)
        {
            if (idBitacoraBusqueda <= 0) return;
            using (var db = new Filiacion_MunicipiosBitacoraEntities())
            {
                var registro = db.tb_BitacoraBusqueda.Find(idBitacoraBusqueda);
                if (registro == null) return;
                registro.TiempoMs = tiempoMs;
                registro.Exito = false;
                registro.Error = Limitar(error, 1000);
                db.SaveChanges();
            }
        }

        public static void RegistrarAcceso(long idBitacoraBusqueda, int idTbFuente, int idPersona, int? idCoincidencia, bool esCoincidenciaDirecta, string tipoAccion)
        {
            if (idBitacoraBusqueda <= 0) return;
            using (var db = new Filiacion_MunicipiosBitacoraEntities())
            {
                db.tb_BitacoraAccesoResultado.Add(new tb_BitacoraAccesoResultado
                {
                    IdBitacoraBusqueda = idBitacoraBusqueda,
                    FechaAcceso = DateTime.Now,
                    IdTbFuente = idTbFuente,
                    IdPersona = idPersona,
                    IdCoincidencia = idCoincidencia,
                    EsCoincidenciaDirecta = esCoincidenciaDirecta,
                    TipoAccion = Limitar(tipoAccion, 50)
                });
                db.SaveChanges();
            }
        }

        private static List<HttpPostedFileBase> ObtenerHuellas(BusquedaCoincidenciasViewModel modelo)
        {
            var huellas = new List<HttpPostedFileBase>();
            if (modelo?.Huellas != null) huellas.AddRange(modelo.Huellas.Where(x => x != null && x.ContentLength > 0));
            if (modelo?.Huella != null && modelo.Huella.ContentLength > 0) huellas.Add(modelo.Huella);
            return huellas;
        }

        private static string ObtenerTextoBusqueda(BusquedaCoincidenciasViewModel modelo)
        {
            if (modelo == null) return null;
            if (!string.IsNullOrWhiteSpace(modelo.NombreBusqueda)) return modelo.NombreBusqueda.Trim();
            return string.IsNullOrWhiteSpace(modelo.TextoBusqueda) ? null : modelo.TextoBusqueda.Trim();
        }

        private static string CalcularHash(HttpPostedFileBase archivo)
        {
            if (archivo == null || archivo.ContentLength <= 0 || archivo.InputStream == null || !archivo.InputStream.CanSeek) return null;
            Stream stream = archivo.InputStream;
            long posicionOriginal = stream.Position;
            try
            {
                stream.Position = 0;
                using (SHA256 sha = SHA256.Create())
                {
                    byte[] hash = sha.ComputeHash(stream);
                    var texto = new StringBuilder(hash.Length * 2);
                    foreach (byte b in hash) texto.Append(b.ToString("x2"));
                    return texto.ToString();
                }
            }
            finally
            {
                stream.Position = posicionOriginal;
            }
        }

        private static string Limitar(string valor, int longitud)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;
            valor = valor.Trim();
            return valor.Length <= longitud ? valor : valor.Substring(0, longitud);
        }
    }
}
