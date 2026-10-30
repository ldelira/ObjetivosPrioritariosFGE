using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Models.Extends;
using Objetivos_Prioritarios.Helpers;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Web;
using Objetivos_Prioritarios.Utils;

namespace Objetivos_Prioritarios.ControllersServices
{
    public   class BitacoraBusquedaService : BaseService
    {
        private static readonly HashSet<string> UsuariosSinBitacora =
            new HashSet<string>(
                new[]
                {
                    "UMORALES",
                    "LDELIRA",
                    "APADILLAA",
                    "ESALAS"
                },
                StringComparer.OrdinalIgnoreCase
            );

        public   long RegistrarInicio(BusquedaCoincidenciasViewModel modelo, string usuario, string ip, string userAgent)
        {
            if (
                !string.IsNullOrWhiteSpace(usuario) &&
                UsuariosSinBitacora.Contains(
                    usuario.Trim()
                )
            )
            {
                return 0;
            }

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

                GuardarArchivosConsulta(
                    registro.IdBitacoraBusqueda,
                    registro.FechaBusqueda,
                    modelo?.Fotografia,
                    huellas
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
                resultado.Archivos = ObtenerArchivos(idBitacoraBusqueda);
                resultado.FotografiaDisponible = resultado.Archivos.Any(x =>
                    string.Equals(
                        x.TipoArchivo,
                        "FOTO",
                        StringComparison.OrdinalIgnoreCase
                    )
                );

                return resultado;
            }
        }

        public BasicOperationResponse ProbarServidorArchivos()
        {
            try
            {
                string rutaBase = (ConfigurationManager.AppSettings["Bitacora.RutaBaseArchivos"] ?? "").Trim().TrimEnd('\\');
                string dominioRed = (ConfigurationManager.AppSettings["PersonasInteres.DominioRed"] ?? "").Trim();
                string usuarioRed = (ConfigurationManager.AppSettings["PersonasInteres.UsuarioRed"] ?? "").Trim();
                string passwordRed = ConfigurationManager.AppSettings["PersonasInteres.PasswordRed"];

                if (string.IsNullOrWhiteSpace(rutaBase))
                    return new BasicOperationResponse { IsSuccess = false, Message = "Bitacora.RutaBaseArchivos no está configurada." };

                if (string.IsNullOrWhiteSpace(dominioRed))
                    return new BasicOperationResponse { IsSuccess = false, Message = "PersonasInteres.DominioRed no está configurado." };

                if (string.IsNullOrWhiteSpace(usuarioRed))
                    return new BasicOperationResponse { IsSuccess = false, Message = "PersonasInteres.UsuarioRed no está configurado." };

                if (string.IsNullOrWhiteSpace(passwordRed))
                    return new BasicOperationResponse { IsSuccess = false, Message = "PersonasInteres.PasswordRed no está configurado." };

                var credentials = new UserCredentials(
                    dominioRed,
                    usuarioRed,
                    passwordRed
                );

                using (SafeAccessTokenHandle userHandle = credentials.LogonUser(LogonType.NewCredentials))
                {
                    WindowsIdentity.RunImpersonated(userHandle, () =>
                    {
                        Directory.CreateDirectory(rutaBase);

                        string rutaPrueba =
                            Path.Combine(
                                rutaBase,
                                "__prueba_bitacora_" + Guid.NewGuid().ToString("N") + ".tmp"
                            );

                        System.IO.File.WriteAllText(
                            rutaPrueba,
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        );

                        if (!System.IO.File.Exists(rutaPrueba))
                            throw new IOException("Se intentó crear el archivo de prueba, pero no apareció en el recurso.");

                        System.IO.File.Delete(rutaPrueba);
                    });
                }

                return new BasicOperationResponse
                {
                    IsSuccess = true,
                    Message = "Conexión y escritura correctas. Ruta base: " + rutaBase
                };
            }
            catch (Exception ex)
            {
                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = ex.GetBaseException().Message
                };
            }
        }


        public BitacoraArchivoContenidoViewModel ObtenerArchivoContenido(long idBitacoraArchivo)
        {
            if (idBitacoraArchivo <= 0) return null;

            BitacoraBusquedaArchivoViewModel archivo;

            using (var db = new Filiacion_MunicipiosBitacoraEntities())
            {
                archivo = db.Database.SqlQuery<BitacoraBusquedaArchivoViewModel>(
                    @"SELECT
                          IdBitacoraArchivo,
                          IdBitacoraBusqueda,
                          TipoArchivo,
                          NumeroArchivo,
                          RutaRelativa,
                          NombreOriginal,
                          FechaRegistro
                      FROM dbo.tb_BitacoraBusquedaArchivo
                      WHERE IdBitacoraArchivo = @IdBitacoraArchivo",
                    new SqlParameter(
                        "@IdBitacoraArchivo",
                        idBitacoraArchivo
                    )
                )
                .FirstOrDefault();
            }

            if (
                archivo == null ||
                string.IsNullOrWhiteSpace(archivo.RutaRelativa)
            )
            {
                return null;
            }

            string rutaBase =
                (ConfigurationManager.AppSettings["Bitacora.RutaBaseArchivos"] ?? "")
                    .Trim()
                    .TrimEnd('\\');

            string dominioRed =
                (ConfigurationManager.AppSettings["PersonasInteres.DominioRed"] ?? "")
                    .Trim();

            string usuarioRed =
                (ConfigurationManager.AppSettings["PersonasInteres.UsuarioRed"] ?? "")
                    .Trim();

            string passwordRed =
                ConfigurationManager.AppSettings["PersonasInteres.PasswordRed"];

            if (
                string.IsNullOrWhiteSpace(rutaBase) ||
                string.IsNullOrWhiteSpace(dominioRed) ||
                string.IsNullOrWhiteSpace(usuarioRed) ||
                string.IsNullOrWhiteSpace(passwordRed)
            )
            {
                return null;
            }

            string rutaCompleta =
                Path.GetFullPath(
                    Path.Combine(
                        rutaBase,
                        archivo.RutaRelativa
                    )
                );

            string rutaBaseCompleta =
                Path.GetFullPath(rutaBase)
                    .TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar
                    ) +
                Path.DirectorySeparatorChar;

            if (
                !rutaCompleta.StartsWith(
                    rutaBaseCompleta,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return null;
            }

            var credentials = new UserCredentials(
                dominioRed,
                usuarioRed,
                passwordRed
            );

            byte[] contenido = null;

            using (SafeAccessTokenHandle userHandle = credentials.LogonUser(LogonType.NewCredentials))
            {
                WindowsIdentity.RunImpersonated(userHandle, () =>
                {
                    if (System.IO.File.Exists(rutaCompleta))
                    {
                        contenido =
                            System.IO.File.ReadAllBytes(
                                rutaCompleta
                            );
                    }
                });
            }

            if (contenido == null || contenido.Length == 0)
            {
                return null;
            }

            return new BitacoraArchivoContenidoViewModel
            {
                Contenido = contenido,

                NombreArchivo =
                    string.IsNullOrWhiteSpace(
                        archivo.NombreOriginal
                    )
                        ? Path.GetFileName(rutaCompleta)
                        : archivo.NombreOriginal,

                ContentType =
                    MimeMapping.GetMimeMapping(
                        rutaCompleta
                    )
            };
        }

        private List<BitacoraBusquedaArchivoViewModel> ObtenerArchivos(long idBitacoraBusqueda)
        {
            using (var db = new Filiacion_MunicipiosBitacoraEntities())
            {
                return db.Database.SqlQuery<BitacoraBusquedaArchivoViewModel>(
                    @"SELECT
                          IdBitacoraArchivo,
                          IdBitacoraBusqueda,
                          TipoArchivo,
                          NumeroArchivo,
                          RutaRelativa,
                          NombreOriginal,
                          FechaRegistro
                      FROM dbo.tb_BitacoraBusquedaArchivo
                      WHERE IdBitacoraBusqueda = @IdBitacoraBusqueda
                      ORDER BY
                          CASE
                              WHEN TipoArchivo = 'FOTO' THEN 0
                              ELSE 1
                          END,
                          NumeroArchivo,
                          IdBitacoraArchivo",
                    new SqlParameter(
                        "@IdBitacoraBusqueda",
                        idBitacoraBusqueda
                    )
                )
                .ToList();
            }
        }

        private void GuardarArchivosConsulta(long idBitacoraBusqueda, DateTime fechaBusqueda, HttpPostedFileBase fotografia, List<HttpPostedFileBase> huellas)
        {
            if (idBitacoraBusqueda <= 0)
            {
                return;
            }

            bool tieneFoto =
                fotografia != null &&
                fotografia.ContentLength > 0 &&
                fotografia.InputStream != null;

            List<HttpPostedFileBase> huellasValidas =
                huellas == null
                    ? new List<HttpPostedFileBase>()
                    : huellas
                        .Where(x =>
                            x != null &&
                            x.ContentLength > 0 &&
                            x.InputStream != null
                        )
                        .ToList();

            if (!tieneFoto && huellasValidas.Count == 0)
            {
                return;
            }

            string rutaBase =
                (ConfigurationManager.AppSettings["Bitacora.RutaBaseArchivos"] ?? "")
                    .Trim()
                    .TrimEnd('\\');

            string dominioRed =
                (ConfigurationManager.AppSettings["PersonasInteres.DominioRed"] ?? "")
                    .Trim();

            string usuarioRed =
                (ConfigurationManager.AppSettings["PersonasInteres.UsuarioRed"] ?? "")
                    .Trim();

            string passwordRed =
                ConfigurationManager.AppSettings["PersonasInteres.PasswordRed"];

            if (
                string.IsNullOrWhiteSpace(rutaBase) ||
                string.IsNullOrWhiteSpace(dominioRed) ||
                string.IsNullOrWhiteSpace(usuarioRed) ||
                string.IsNullOrWhiteSpace(passwordRed)
            )
            {
                return;
            }

            string anio =
                fechaBusqueda.ToString("yyyy");

            string mes =
                fechaBusqueda.ToString("MM");

            string carpetaFotos =
                Path.Combine(
                    rutaBase,
                    anio,
                    mes,
                    "Fotos"
                );

            string carpetaHuellas =
                Path.Combine(
                    rutaBase,
                    anio,
                    mes,
                    "Huellas"
                );

            var credentials = new UserCredentials(
                dominioRed,
                usuarioRed,
                passwordRed
            );

            try
            {
                using (SafeAccessTokenHandle userHandle = credentials.LogonUser(LogonType.NewCredentials))
                {
                    WindowsIdentity.RunImpersonated(userHandle, () =>
                    {
                        if (tieneFoto)
                        {
                            Directory.CreateDirectory(
                                carpetaFotos
                            );

                            GuardarArchivoConsulta(
                                idBitacoraBusqueda,
                                fotografia,
                                "FOTO",
                                1,
                                Path.Combine(
                                    anio,
                                    mes,
                                    "Fotos"
                                ),
                                carpetaFotos,
                                idBitacoraBusqueda.ToString()
                            );
                        }

                        if (huellasValidas.Count > 0)
                        {
                            Directory.CreateDirectory(
                                carpetaHuellas
                            );

                            for (
                                int i = 0;
                                i < huellasValidas.Count;
                                i++
                            )
                            {
                                int numeroHuella = i + 1;

                                GuardarArchivoConsulta(
                                    idBitacoraBusqueda,
                                    huellasValidas[i],
                                    "HUELLA",
                                    numeroHuella,
                                    Path.Combine(
                                        anio,
                                        mes,
                                        "Huellas"
                                    ),
                                    carpetaHuellas,
                                    idBitacoraBusqueda +
                                    "_" +
                                    numeroHuella.ToString("00")
                                );
                            }
                        }
                    });
                }
            }
            catch
            {
                /*
                 * No bloqueamos la búsqueda biométrica si falla
                 * el resguardo visual de foto o huellas.
                 */
            }
        }

        private void GuardarArchivoConsulta(long idBitacoraBusqueda, HttpPostedFileBase archivo, string tipoArchivo, int numeroArchivo, string carpetaRelativa, string carpetaFisica, string nombreBase)
        {
            string extension =
                ObtenerExtensionPermitida(
                    archivo.FileName
                );

            string nombreArchivo =
                nombreBase +
                extension;

            string rutaRelativa =
                Path.Combine(
                    carpetaRelativa,
                    nombreArchivo
                );

            string rutaFisica =
                Path.Combine(
                    carpetaFisica,
                    nombreArchivo
                );

            Stream stream =
                archivo.InputStream;

            long posicionOriginal =
                stream.CanSeek
                    ? stream.Position
                    : 0;

            try
            {
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }

                using (
                    FileStream destino =
                        System.IO.File.Create(
                            rutaFisica
                        )
                )
                {
                    stream.CopyTo(
                        destino
                    );
                }

                using (
                    var db =
                        new Filiacion_MunicipiosBitacoraEntities()
                )
                {
                    db.Database.ExecuteSqlCommand(
                        @"INSERT INTO dbo.tb_BitacoraBusquedaArchivo
                          (
                              IdBitacoraBusqueda,
                              TipoArchivo,
                              NumeroArchivo,
                              RutaRelativa,
                              NombreOriginal
                          )
                          VALUES
                          (
                              @IdBitacoraBusqueda,
                              @TipoArchivo,
                              @NumeroArchivo,
                              @RutaRelativa,
                              @NombreOriginal
                          )",
                        new SqlParameter(
                            "@IdBitacoraBusqueda",
                            idBitacoraBusqueda
                        ),
                        new SqlParameter(
                            "@TipoArchivo",
                            tipoArchivo
                        ),
                        new SqlParameter(
                            "@NumeroArchivo",
                            numeroArchivo
                        ),
                        new SqlParameter(
                            "@RutaRelativa",
                            rutaRelativa
                        ),
                        new SqlParameter(
                            "@NombreOriginal",
                            Limitar(
                                Path.GetFileName(
                                    archivo.FileName ?? ""
                                ),
                                255
                            ) ?? nombreArchivo
                        )
                    );
                }
            }
            catch
            {
                try
                {
                    if (System.IO.File.Exists(rutaFisica))
                    {
                        System.IO.File.Delete(
                            rutaFisica
                        );
                    }
                }
                catch
                {
                }

                throw;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position =
                        posicionOriginal;
                }
            }
        }

        private string ObtenerExtensionPermitida(string nombreArchivo)
        {
            string extension =
                Path.GetExtension(
                    nombreArchivo ?? ""
                )
                .ToLowerInvariant();

            string[] extensionesPermitidas =
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp",
                ".bmp"
            };

            return extensionesPermitidas.Contains(extension)
                ? extension
                : ".jpg";
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
