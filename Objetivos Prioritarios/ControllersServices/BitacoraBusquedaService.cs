using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Models.Extends;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Objetivos_Prioritarios.ControllersServices
{
    public   class BitacoraBusquedaService
    {
        public   long RegistrarInicio(BusquedaCoincidenciasViewModel modelo, string usuario, string ip, string userAgent)
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

                GuardarFotografiaConsulta(
                    registro.IdBitacoraBusqueda,
                    modelo?.Fotografia
                );

                return registro.IdBitacoraBusqueda;
            }
        }

        public   void RegistrarExito(long idBitacoraBusqueda, int totalCoincidencias, long tiempoMs)
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

        public void RegistrarError(long idBitacoraBusqueda, long tiempoMs, string error)
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

        public   void RegistrarAcceso(long idBitacoraBusqueda, int idTbFuente, int idPersona, int? idCoincidencia, bool esCoincidenciaDirecta, string tipoAccion)
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

        public   List<BitacoraBusquedaListadoViewModel> ObtenerBitacora(string usuario = null, int limite = 200)
        {
            limite = Math.Max(1, Math.Min(limite, 500));

            using (var db = new Filiacion_MunicipiosBitacoraEntities())
            {
                var consulta =
                    db.tb_BitacoraBusqueda
                        .AsNoTracking()
                        .AsQueryable();

                if (!string.IsNullOrWhiteSpace(usuario))
                {
                    string usuarioFiltro = usuario.Trim().ToUpper();

                    consulta =
                        consulta.Where(x =>
                            x.Usuario != null &&
                            x.Usuario.ToUpper().Contains(usuarioFiltro)
                        );
                }

                List<BitacoraBusquedaListadoViewModel> resultado =
                    consulta
                        .OrderByDescending(x => x.IdBitacoraBusqueda)
                        .Take(limite)
                        .Select(x => new BitacoraBusquedaListadoViewModel
                        {
                            IdBitacoraBusqueda = x.IdBitacoraBusqueda,
                            Usuario = x.Usuario,
                            FechaBusqueda = x.FechaBusqueda,
                            ModoBusquedaTexto = x.ModoBusquedaTexto,
                            ModoCombinacion = x.ModoCombinacion,
                            TextoBusqueda = x.TextoBusqueda,
                            AliasBusqueda = x.AliasBusqueda,
                            PorcentajeMinimo = x.PorcentajeMinimo,
                            TieneFotografia = x.TieneFotografia,
                            CantidadHuellas = x.CantidadHuellas,
                            HashFotografia = x.HashFotografia,
                            HashHuellas = x.HashHuellas,
                            TotalCoincidencias = x.TotalCoincidencias,
                            TiempoMs = x.TiempoMs,
                            Exito = x.Exito,
                            Error = x.Error,
                            Ip = x.Ip,
                            UserAgent = x.UserAgent,
                            TotalAccesos = x.tb_BitacoraAccesoResultado.Count()
                        })
                        .ToList();

                foreach (BitacoraBusquedaListadoViewModel item in resultado)
                {
                    item.FotografiaDisponible =
                        !string.IsNullOrWhiteSpace(
                            ObtenerRutaFotografiaConsulta(
                                item.IdBitacoraBusqueda
                            )
                        );
                }

                return resultado;
            }
        }

        public   BitacoraBusquedaDetalleViewModel ObtenerDetalle(long idBitacoraBusqueda)
        {
            if (idBitacoraBusqueda <= 0) return null;

            using (var db = new Filiacion_MunicipiosBitacoraEntities())
            {
                tb_BitacoraBusqueda registro =
                    db.tb_BitacoraBusqueda
                        .AsNoTracking()
                        .FirstOrDefault(x =>
                            x.IdBitacoraBusqueda == idBitacoraBusqueda
                        );

                if (registro == null) return null;

                BitacoraBusquedaDetalleViewModel resultado =
                    new BitacoraBusquedaDetalleViewModel
                    {
                        IdBitacoraBusqueda = registro.IdBitacoraBusqueda,
                        Usuario = registro.Usuario,
                        FechaBusqueda = registro.FechaBusqueda,
                        ModoBusquedaTexto = registro.ModoBusquedaTexto,
                        ModoCombinacion = registro.ModoCombinacion,
                        TextoBusqueda = registro.TextoBusqueda,
                        AliasBusqueda = registro.AliasBusqueda,
                        PorcentajeMinimo = registro.PorcentajeMinimo,
                        TieneFotografia = registro.TieneFotografia,
                        FotografiaDisponible =
                            !string.IsNullOrWhiteSpace(
                                ObtenerRutaFotografiaConsulta(
                                    registro.IdBitacoraBusqueda
                                )
                            ),
                        CantidadHuellas = registro.CantidadHuellas,
                        HashFotografia = registro.HashFotografia,
                        HashHuellas = registro.HashHuellas,
                        TotalCoincidencias = registro.TotalCoincidencias,
                        TiempoMs = registro.TiempoMs,
                        Exito = registro.Exito,
                        Error = registro.Error,
                        Ip = registro.Ip,
                        UserAgent = registro.UserAgent
                    };

                resultado.Accesos =
                    db.tb_BitacoraAccesoResultado
                        .AsNoTracking()
                        .Where(x =>
                            x.IdBitacoraBusqueda == idBitacoraBusqueda
                        )
                        .OrderBy(x => x.FechaAcceso)
                        .ToList()
                        .Select(x => new BitacoraAccesoResultadoViewModel
                        {
                            IdBitacoraAcceso = x.IdBitacoraAcceso,
                            FechaAcceso = x.FechaAcceso,
                            IdTbFuente = x.IdTbFuente,
                            Fuente = ObtenerNombreFuente(x.IdTbFuente),
                            IdPersona = x.IdPersona,
                            IdCoincidencia = x.IdCoincidencia,
                            EsCoincidenciaDirecta = x.EsCoincidenciaDirecta,
                            TipoAccion = x.TipoAccion
                        })
                        .ToList();

                resultado.TotalAccesos = resultado.Accesos.Count;

                return resultado;
            }
        }

        public   string ObtenerRutaFotografiaConsulta(long idBitacoraBusqueda)
        {
            if (idBitacoraBusqueda <= 0 || HttpContext.Current == null) return null;

            string carpeta =
                HttpContext.Current.Server.MapPath(
                    "~/App_Data/BitacoraBusquedaFotos"
                );

            if (!Directory.Exists(carpeta)) return null;

            string[] archivos =
                Directory.GetFiles(
                    carpeta,
                    idBitacoraBusqueda + ".*"
                );

            return archivos.FirstOrDefault();
        }

        private   void GuardarFotografiaConsulta(long idBitacoraBusqueda, HttpPostedFileBase fotografia)
        {
            if (
                idBitacoraBusqueda <= 0 ||
                fotografia == null ||
                fotografia.ContentLength <= 0 ||
                fotografia.InputStream == null ||
                HttpContext.Current == null
            )
            {
                return;
            }

            try
            {
                string extension =
                    Path.GetExtension(
                        fotografia.FileName ?? ""
                    )
                    .ToLowerInvariant();

                string[] extensionesPermitidas =
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };

                if (!extensionesPermitidas.Contains(extension))
                {
                    extension = ".jpg";
                }

                string carpeta =
                    HttpContext.Current.Server.MapPath(
                        "~/App_Data/BitacoraBusquedaFotos"
                    );

                Directory.CreateDirectory(carpeta);

                string ruta =
                    Path.Combine(
                        carpeta,
                        idBitacoraBusqueda + extension
                    );

                Stream stream = fotografia.InputStream;
                long posicionOriginal = stream.CanSeek ? stream.Position : 0;

                try
                {
                    if (stream.CanSeek) stream.Position = 0;

                    using (FileStream destino = File.Create(ruta))
                    {
                        stream.CopyTo(destino);
                    }
                }
                finally
                {
                    if (stream.CanSeek) stream.Position = posicionOriginal;
                }
            }
            catch
            {
                /*
                 * La bitácora principal y el hash ya quedaron guardados.
                 * Un problema al conservar la copia visual no debe
                 * impedir la búsqueda biométrica.
                 */
            }
        }

        private   string ObtenerNombreFuente(int idTbFuente)
        {
            switch (idTbFuente)
            {
                case 1:
                    return "C5 - Detenidos";
                case 2:
                    return "CAPEA / FEMDLP";
                case 3:
                    return "Personas de Interés";
                case 4:
                    return "Mandamientos Judiciales";
                case 5:
                    return "Objetivos Prioritarios";
                case 6:
                    return "FGEA - Detenidos";
                case 7:
                    return "Alerta Amber";
                case 8:
                    return "Protocolo Alba";
                default:
                    return "Fuente " + idTbFuente;
            }
        }


        private   List<HttpPostedFileBase> ObtenerHuellas(BusquedaCoincidenciasViewModel modelo)
        {
            var huellas = new List<HttpPostedFileBase>();
            if (modelo?.Huellas != null) huellas.AddRange(modelo.Huellas.Where(x => x != null && x.ContentLength > 0));
            if (modelo?.Huella != null && modelo.Huella.ContentLength > 0) huellas.Add(modelo.Huella);
            return huellas;
        }

        private   string ObtenerTextoBusqueda(BusquedaCoincidenciasViewModel modelo)
        {
            if (modelo == null) return null;
            if (!string.IsNullOrWhiteSpace(modelo.NombreBusqueda)) return modelo.NombreBusqueda.Trim();
            return string.IsNullOrWhiteSpace(modelo.TextoBusqueda) ? null : modelo.TextoBusqueda.Trim();
        }

        private   string CalcularHash(HttpPostedFileBase archivo)
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

        private   string Limitar(string valor, int longitud)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;
            valor = valor.Trim();
            return valor.Length <= longitud ? valor : valor.Substring(0, longitud);
        }
    }
}
