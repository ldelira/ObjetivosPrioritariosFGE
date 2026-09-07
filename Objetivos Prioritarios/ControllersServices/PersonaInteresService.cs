using Microsoft.Win32.SafeHandles;
using Objetivos_Prioritarios.Helpers;
using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Principal;

//using System.Net;
using System.Web;

namespace Objetivos_Prioritarios.ControllersServices
{
    public class PersonaInteresService : BaseService
    {
        private const int ESTATUS_PERIODO_ACTIVO = 1;
        private const int ESTATUS_PERIODO_REEMPLAZADO = 2;
        private const int ESTATUS_PERIODO_CANCELADO = 3;
        private const int ESTATUS_PERIODO_VENCIDO = 4;
        public List<tb_Persona> GetPersonasInteresList()
        {
            return dbFiliMuni.tb_Persona
                .AsNoTracking()
                .OrderByDescending(x => x.FechaRegistro)
                .ToList();
        }

        public tb_Persona GetPersonaInteresById(int idPersona)
        {
            return dbFiliMuni.tb_Persona
                .Include(x => x.tb_Fotografia)
                .FirstOrDefault(x => x.idPersona == idPersona);
        }

        public BasicOperationResponse SavePersonaInteres(
            int idPersona,
            string nombre,
            string paterno,
            string materno,
            int? edadAproximada,
            DateTime? fechaNacimientoExacta,
            string estatura,
            string sexo,
            string observaciones,
            int usuarioRegistro)
        {
            try
            {
                nombre = string.IsNullOrWhiteSpace(nombre) ? " " : nombre.Trim();
                paterno = string.IsNullOrWhiteSpace(paterno) ? " " : paterno.Trim();
                materno = string.IsNullOrWhiteSpace(materno) ? " " : materno.Trim();

                DateTime? fechaNacimiento = null;

                if (fechaNacimientoExacta.HasValue)
                {
                    fechaNacimiento = fechaNacimientoExacta.Value;
                }
                else if (edadAproximada.HasValue && edadAproximada.Value > 0)
                {
                    int anioNacimiento = DateTime.Now.Year - edadAproximada.Value;
                    fechaNacimiento = new DateTime(anioNacimiento, 1, 1);
                }

                if (idPersona == 0)
                {
                    var nueva = new tb_Persona
                    {
                        Nombre = nombre,
                        Paterno = paterno,
                        Materno = materno,
                        FechaNacimiento = fechaNacimiento,
                        Estatura = string.IsNullOrWhiteSpace(estatura) ? null : estatura.Trim(),
                        Sexo = string.IsNullOrWhiteSpace(sexo) ? null : (sexo.Trim()=="M"?"1":"0"),
                        Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim(),
                        FechaRegistro = DateTime.Now,
                        UsuarioRegistro = usuarioRegistro
                    };

                    dbFiliMuni.tb_Persona.Add(nueva);
                    dbFiliMuni.SaveChanges();

                    return new BasicOperationResponse
                    {
                        IsSuccess = true,
                        Message = "Persona de interés registrada correctamente.",
                        Id = nueva.idPersona
                    };
                }

                var persona = dbFiliMuni.tb_Persona.FirstOrDefault(x => x.idPersona == idPersona);

                if (persona == null)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No se encontró la persona de interés."
                    };
                }

                persona.Nombre = nombre;
                persona.Paterno = paterno;
                persona.Materno = materno;
                persona.FechaNacimiento = fechaNacimiento;
                persona.Estatura = string.IsNullOrWhiteSpace(estatura) ? null : estatura.Trim();
                persona.Sexo = string.IsNullOrWhiteSpace(sexo) ? null : sexo.Trim();
                persona.Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim();

                dbFiliMuni.SaveChanges();

                return new BasicOperationResponse
                {
                    IsSuccess = true,
                    Message = "Persona de interés actualizada correctamente.",
                    Id = persona.idPersona
                };
            }
            catch (Exception ex)
            {
                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al guardar la persona de interés: " + ex.Message
                };
            }
        }

        public string GetNombreMostrar(tb_Persona persona)
        {
            if (persona == null)
                return "Persona sin identificar";

            string nombreCompleto = string.Join(" ", new[]
            {
                persona.Nombre,
                persona.Paterno,
                persona.Materno
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

            if (string.IsNullOrWhiteSpace(nombreCompleto))
                return "Persona sin identificar";

            return nombreCompleto;
        }

        public int? GetEdadAproximada(DateTime? fechaNacimiento)
        {
            if (!fechaNacimiento.HasValue)
                return null;

            var hoy = DateTime.Today;
            int edad = hoy.Year - fechaNacimiento.Value.Year;

            if (fechaNacimiento.Value.Date > hoy.AddYears(-edad))
                edad--;

            if (edad < 0 || edad > 130)
                return null;

            return edad;
        }
        public List<tb_Fotografia> GetFotografiasPersonaInteres(int idPersona)
        {
            return dbFiliMuni.tb_Fotografia
                .AsNoTracking()
                .Where(x =>
                    x.idPersona == idPersona &&
                    x.Activo == true
                )
                .OrderByDescending(x => x.FechaRegistro)
                .ToList();
        }


        public BasicOperationResponse SaveFotografiaPersonaInteres(
    int idPersona,
    int idTipoFoto,
    HttpPostedFileBase archivo,
    int usuarioRegistro)
        {
            const int tamanioMaximoBytes = 10 * 1024 * 1024; // 10 MB

            try
            {
                if (idPersona <= 0)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "Primero debes guardar la persona de interés."
                    };
                }

                var persona = dbFiliMuni.tb_Persona
                    .FirstOrDefault(x => x.idPersona == idPersona);

                if (persona == null)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No se encontró la persona de interés."
                    };
                }

                if (archivo == null || archivo.ContentLength <= 0)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "Selecciona una imagen válida."
                    };
                }

                if (archivo.ContentLength > tamanioMaximoBytes)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "La fotografía no debe superar los 10 MB."
                    };
                }

                string extension = Path.GetExtension(archivo.FileName ?? "").ToLowerInvariant();

                var extensionesPermitidas = new[]
                {
            ".jpg",
            ".jpeg",
            ".png",
            ".bmp",
            ".webp"
        };

                if (string.IsNullOrWhiteSpace(extension) || !extensionesPermitidas.Contains(extension))
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "El archivo debe ser una imagen JPG, JPEG, PNG, BMP o WEBP."
                    };
                }

                if (idTipoFoto <= 0)
                {
                    idTipoFoto = 1;
                }

                byte[] bytes;

                using (var ms = new MemoryStream())
                {
                    archivo.InputStream.CopyTo(ms);
                    bytes = ms.ToArray();
                }

                if (bytes == null || bytes.Length == 0)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No fue posible leer el contenido de la imagen."
                    };
                }

                /*
 * ============================================================
 * EMBEDDING FACIAL
 * ============================================================
 *
 * Fuente 3 = Personas de Interés
 *
 * Únicamente usamos la fotografía FRONTAL.
 *
 * 1 = Frontal
 * 2 = Perfil izquierdo
 * 3 = Perfil derecho
 * 4 = Otra
 * ============================================================
 */

                ResultadoEmbeddingApi resultadoEmbedding = null;


                if (idTipoFoto == 1)
                {
                    resultadoEmbedding =
                        BiometriaApiHelper
                            .GuardarOActualizarEmbedding(
                                idPersona,
                                3,
                                bytes,
                                archivo.FileName,
                                archivo.ContentType
                            );


                    if (!resultadoEmbedding.Success)
                    {
                        return new BasicOperationResponse
                        {
                            IsSuccess = false,

                            Message =
                                "No se guardó la fotografía frontal porque no fue posible "
                                + "generar el embedding facial. "
                                + resultadoEmbedding.Message
                        };
                    }
                }


                string base64 = Convert.ToBase64String(bytes);

                string rutaBase = ConfigurationManager.AppSettings["PersonasInteres.RutaBaseArchivos"];
                string dominioRed = ConfigurationManager.AppSettings["PersonasInteres.DominioRed"];
                string usuarioRed = ConfigurationManager.AppSettings["PersonasInteres.UsuarioRed"];
                string passwordRed = ConfigurationManager.AppSettings["PersonasInteres.PasswordRed"];

                rutaBase = (rutaBase ?? "").Trim().TrimEnd('\\');
                dominioRed = (dominioRed ?? "").Trim();
                usuarioRed = (usuarioRed ?? "").Trim();

                if (string.IsNullOrWhiteSpace(rutaBase))
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No está configurada PersonasInteres.RutaBaseArchivos."
                    };
                }

                if (string.IsNullOrWhiteSpace(dominioRed))
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No está configurado PersonasInteres.DominioRed."
                    };
                }

                if (string.IsNullOrWhiteSpace(usuarioRed))
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No está configurado PersonasInteres.UsuarioRed."
                    };
                }

                if (string.IsNullOrWhiteSpace(passwordRed))
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No está configurado PersonasInteres.PasswordRed."
                    };
                }

                string carpetaPersona = Path.Combine(rutaBase, idPersona.ToString());
                string carpetaFotos = Path.Combine(carpetaPersona, "Fotos");

                string nombreArchivo =
                    "foto_" +
                    DateTime.Now.ToString("yyyyMMdd_HHmmssfff") +
                    "_" +
                    Guid.NewGuid().ToString("N").Substring(0, 8) +
                    extension;

                string rutaArchivoFinal = Path.Combine(carpetaFotos, nombreArchivo);

                var credentials = new UserCredentials(
                    dominioRed,
                    usuarioRed,
                    passwordRed
                );

                /*
                 * Si con NewCredentials no te deja escribir, cámbialo por Interactive.
                 * En tu otro proyecto usabas Interactive y te funcionó.
                 */
                using (SafeAccessTokenHandle userHandle = credentials.LogonUser(LogonType.NewCredentials))
                {
                    WindowsIdentity.RunImpersonated(userHandle, () =>
                    {
                        Directory.CreateDirectory(carpetaFotos);

                        System.IO.File.WriteAllBytes(
                            rutaArchivoFinal,
                            bytes
                        );
                    });
                }

                tb_Fotografia foto = null;

                try
                {
                    foto = new tb_Fotografia
                    {
                        idPersona = idPersona,
                        idTipoFoto = idTipoFoto,
                        RutaArchivo = rutaArchivoFinal,
                        ArchivoB64 = base64,
                        FechaRegistro = DateTime.Now,
                        UsuarioRegistro = usuarioRegistro,
                        Activo=true
                    };

                    dbFiliMuni.tb_Fotografia.Add(foto);
                    dbFiliMuni.SaveChanges();

                    return new BasicOperationResponse
                    {
                        IsSuccess = true,
                        Message = "Fotografía guardada correctamente.",
                        Id = foto.idFoto
                    };
                }
                catch
                {
                    try
                    {
                        using (SafeAccessTokenHandle userHandle = credentials.LogonUser(LogonType.NewCredentials))
                        {
                            WindowsIdentity.RunImpersonated(userHandle, () =>
                            {
                                if (System.IO.File.Exists(rutaArchivoFinal))
                                {
                                    System.IO.File.Delete(rutaArchivoFinal);
                                }
                            });
                        }
                    }
                    catch
                    {
                        // No se reemplaza el error original de base de datos.
                    }

                    throw;
                }
            }
            catch (Exception ex)
            {
                var errorReal = ex.GetBaseException();

                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al guardar la fotografía: " + errorReal.Message
                };
            }
        }

        //public BasicOperationResponse SaveFotografiaPersonaInteres(
        //    int idPersona,
        //    int idTipoFoto,
        //    HttpPostedFileBase archivo,
        //    int usuarioRegistro)
        //{
        //    try
        //    {
        //        if (idPersona <= 0)
        //        {
        //            return new BasicOperationResponse
        //            {
        //                IsSuccess = false,
        //                Message = "Primero debes guardar la persona de interés."
        //            };
        //        }

        //        var persona = dbFiliMuni.tb_Persona.FirstOrDefault(x => x.idPersona == idPersona);

        //        if (persona == null)
        //        {
        //            return new BasicOperationResponse
        //            {
        //                IsSuccess = false,
        //                Message = "No se encontró la persona de interés."
        //            };
        //        }

        //        if (archivo == null || archivo.ContentLength <= 0)
        //        {
        //            return new BasicOperationResponse
        //            {
        //                IsSuccess = false,
        //                Message = "Selecciona una imagen válida."
        //            };
        //        }

        //        string extension = Path.GetExtension(archivo.FileName);

        //        if (string.IsNullOrWhiteSpace(extension))
        //        {
        //            extension = ".jpg";
        //        }

        //        extension = extension.ToLower();

        //        var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".webp" };

        //        if (!extensionesPermitidas.Contains(extension))
        //        {
        //            return new BasicOperationResponse
        //            {
        //                IsSuccess = false,
        //                Message = "El archivo debe ser una imagen válida: JPG, PNG, BMP o WEBP."
        //            };
        //        }

        //        byte[] bytes;

        //        using (var ms = new MemoryStream())
        //        {
        //            archivo.InputStream.CopyTo(ms);
        //            bytes = ms.ToArray();
        //        }

        //        string base64 = Convert.ToBase64String(bytes);

        //        string rutaShare = ConfigurationManager.AppSettings["PersonasInteres.RutaShare"];
        //        string rutaBase = ConfigurationManager.AppSettings["PersonasInteres.RutaBaseArchivos"];
        //        string usuarioRed = ConfigurationManager.AppSettings["PersonasInteres.UsuarioRed"];
        //        string passwordRed = ConfigurationManager.AppSettings["PersonasInteres.PasswordRed"];

        //        if (string.IsNullOrWhiteSpace(rutaBase))
        //        {
        //            return new BasicOperationResponse
        //            {
        //                IsSuccess = false,
        //                Message = "No está configurada la ruta base de archivos."
        //            };
        //        }

        //        if (string.IsNullOrWhiteSpace(rutaShare))
        //        {
        //            rutaShare = rutaBase;
        //        }

        //        string carpetaPersona = Path.Combine(rutaBase, idPersona.ToString());
        //        string carpetaFotos = Path.Combine(carpetaPersona, "Fotos");

        //        string nombreArchivo = "foto_" + DateTime.Now.ToString("yyyyMMdd_HHmmssfff") + extension;
        //        string rutaArchivoFinal = Path.Combine(carpetaFotos, nombreArchivo);

        //        if (!string.IsNullOrWhiteSpace(usuarioRed) && !string.IsNullOrWhiteSpace(passwordRed))
        //        {
        //            using (new NetworkConnection(rutaShare, usuarioRed, passwordRed))
        //            {
        //                if (!Directory.Exists(carpetaPersona))
        //                {
        //                    Directory.CreateDirectory(carpetaPersona);
        //                }

        //                if (!Directory.Exists(carpetaFotos))
        //                {
        //                    Directory.CreateDirectory(carpetaFotos);
        //                }

        //                System.IO.File.WriteAllBytes(rutaArchivoFinal, bytes);
        //            }
        //        }
        //        else
        //        {
        //            if (!Directory.Exists(carpetaPersona))
        //            {
        //                Directory.CreateDirectory(carpetaPersona);
        //            }

        //            if (!Directory.Exists(carpetaFotos))
        //            {
        //                Directory.CreateDirectory(carpetaFotos);
        //            }

        //            System.IO.File.WriteAllBytes(rutaArchivoFinal, bytes);
        //        }

        //        var foto = new tb_Fotografia
        //        {
        //            idPersona = idPersona,
        //            idTipoFoto = idTipoFoto <= 0 ? 1 : idTipoFoto,
        //            RutaArchivo = rutaArchivoFinal,
        //            ArchivoB64 = base64,
        //            FechaRegistro = DateTime.Now,
        //            UsuarioRegistro = usuarioRegistro
        //        };

        //        dbFiliMuni.tb_Fotografia.Add(foto);
        //        dbFiliMuni.SaveChanges();

        //        return new BasicOperationResponse
        //        {
        //            IsSuccess = true,
        //            Message = "Fotografía guardada correctamente.",
        //            Id = foto.idFoto
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new BasicOperationResponse
        //        {
        //            IsSuccess = false,
        //            Message = "Ocurrió un error al guardar la fotografía: " + ex.Message
        //        };
        //    }
        //}

        public string GetTipoFotoTexto(int idTipoFoto)
        {
            switch (idTipoFoto)
            {
                case 1:
                    return "Frontal";
                case 2:
                    return "Perfil izquierdo";
                case 3:
                    return "Perfil derecho";
                case 4:
                    return "Otra";
                default:
                    return "Sin tipo";
            }
        }


        public List<tb_FichaDecadactilar> GetHuellasPersonaInteres(int idPersona)
        {
            return dbFiliMuni.tb_FichaDecadactilar
                .AsNoTracking()
                .Where(x => x.idPersona == idPersona
                         && (x.Activo == null || x.Activo == true)
                         && x.IdTbFuente == 3)
                .OrderByDescending(x => x.FechaRegistro)
                .ToList();
        }

        public BasicOperationResponse SaveHuellasPersonaInteres(
            int idPersona,
            List<HttpPostedFileBase> archivos,
            int usuarioRegistro)
        {
            const int tamanioMaximoBytes = 10 * 1024 * 1024; // 10 MB por imagen

            var rutasCreadas = new List<string>();

            try
            {
                if (idPersona <= 0)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "Primero debes guardar la persona de interés."
                    };
                }

                var persona = dbFiliMuni.tb_Persona
                    .FirstOrDefault(x => x.idPersona == idPersona);

                if (persona == null)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No se encontró la persona de interés."
                    };
                }

                if (archivos == null || archivos.Count == 0)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "Selecciona al menos una imagen de huella."
                    };
                }

                archivos = archivos
                    .Where(x => x != null && x.ContentLength > 0)
                    .ToList();

                if (archivos.Count == 0)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "Selecciona al menos una imagen de huella válida."
                    };
                }

                var extensionesPermitidas = new[]
                {
            ".jpg",
            ".jpeg",
            ".png",
            ".bmp",
            ".webp"
        };

                string rutaBase = ConfigurationManager.AppSettings["PersonasInteres.RutaBaseArchivos"];
                string dominioRed = ConfigurationManager.AppSettings["PersonasInteres.DominioRed"];
                string usuarioRed = ConfigurationManager.AppSettings["PersonasInteres.UsuarioRed"];
                string passwordRed = ConfigurationManager.AppSettings["PersonasInteres.PasswordRed"];

                rutaBase = (rutaBase ?? "").Trim().TrimEnd('\\');
                dominioRed = (dominioRed ?? "").Trim();
                usuarioRed = (usuarioRed ?? "").Trim();

                if (string.IsNullOrWhiteSpace(rutaBase))
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No está configurada PersonasInteres.RutaBaseArchivos."
                    };
                }

                if (string.IsNullOrWhiteSpace(dominioRed))
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No está configurado PersonasInteres.DominioRed."
                    };
                }

                if (string.IsNullOrWhiteSpace(usuarioRed))
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No está configurado PersonasInteres.UsuarioRed."
                    };
                }

                if (string.IsNullOrWhiteSpace(passwordRed))
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No está configurado PersonasInteres.PasswordRed."
                    };
                }

                string carpetaPersona = Path.Combine(rutaBase, idPersona.ToString());
                string carpetaHuellas = Path.Combine(carpetaPersona, "Huellas");

                var archivosProcesados = new List<Tuple<string, byte[]>>();

                foreach (var archivo in archivos)
                {
                    if (archivo.ContentLength > tamanioMaximoBytes)
                    {
                        return new BasicOperationResponse
                        {
                            IsSuccess = false,
                            Message = "Una de las huellas supera los 10 MB."
                        };
                    }

                    string extension = Path.GetExtension(archivo.FileName ?? "").ToLowerInvariant();

                    if (string.IsNullOrWhiteSpace(extension) || !extensionesPermitidas.Contains(extension))
                    {
                        return new BasicOperationResponse
                        {
                            IsSuccess = false,
                            Message = "Todas las huellas deben ser imágenes JPG, JPEG, PNG, BMP o WEBP."
                        };
                    }

                    byte[] bytes;

                    using (var ms = new MemoryStream())
                    {
                        archivo.InputStream.CopyTo(ms);
                        bytes = ms.ToArray();
                    }

                    if (bytes == null || bytes.Length == 0)
                    {
                        return new BasicOperationResponse
                        {
                            IsSuccess = false,
                            Message = "No fue posible leer una de las imágenes de huella."
                        };
                    }

                    string nombreArchivo =
                        "huella_" +
                        DateTime.Now.ToString("yyyyMMdd_HHmmssfff") +
                        "_" +
                        Guid.NewGuid().ToString("N").Substring(0, 8) +
                        extension;

                    string rutaArchivoFinal = Path.Combine(carpetaHuellas, nombreArchivo);

                    archivosProcesados.Add(new Tuple<string, byte[]>(rutaArchivoFinal, bytes));
                }

                var credentials = new UserCredentials(
                    dominioRed,
                    usuarioRed,
                    passwordRed
                );

                using (SafeAccessTokenHandle userHandle = credentials.LogonUser(LogonType.NewCredentials))
                {
                    WindowsIdentity.RunImpersonated(userHandle, () =>
                    {
                        Directory.CreateDirectory(carpetaHuellas);

                        foreach (var item in archivosProcesados)
                        {
                            string rutaArchivo = item.Item1;
                            byte[] bytes = item.Item2;

                            System.IO.File.WriteAllBytes(rutaArchivo, bytes);
                            rutasCreadas.Add(rutaArchivo);
                        }
                    });
                }

                using (var transaction = dbFiliMuni.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var ruta in rutasCreadas)
                        {
                            var huella = new tb_FichaDecadactilar
                            {
                                idPersona = idPersona,
                                RutaHuella = ruta,
                                FechaRegistro = DateTime.Now,
                                Activo = true,
                                IdTbFuente = 3
                            };

                            dbFiliMuni.tb_FichaDecadactilar.Add(huella);
                        }

                        dbFiliMuni.SaveChanges();
                        transaction.Commit();

                        return new BasicOperationResponse
                        {
                            IsSuccess = true,
                            Message = "Huellas guardadas correctamente.",
                            Id = idPersona
                        };
                    }
                    catch
                    {
                        transaction.Rollback();

                        try
                        {
                            using (SafeAccessTokenHandle userHandle = credentials.LogonUser(LogonType.NewCredentials))
                            {
                                WindowsIdentity.RunImpersonated(userHandle, () =>
                                {
                                    foreach (var ruta in rutasCreadas)
                                    {
                                        if (System.IO.File.Exists(ruta))
                                        {
                                            System.IO.File.Delete(ruta);
                                        }
                                    }
                                });
                            }
                        }
                        catch
                        {
                            // No reemplazamos el error original de base de datos.
                        }

                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                var errorReal = ex.GetBaseException();

                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al guardar las huellas: " + errorReal.Message
                };
            }
        }


        public string GetHuellaBase64DesdeRuta(string rutaHuella)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rutaHuella))
                    return "";

                string dominioRed = ConfigurationManager.AppSettings["PersonasInteres.DominioRed"];
                string usuarioRed = ConfigurationManager.AppSettings["PersonasInteres.UsuarioRed"];
                string passwordRed = ConfigurationManager.AppSettings["PersonasInteres.PasswordRed"];

                dominioRed = (dominioRed ?? "").Trim();
                usuarioRed = (usuarioRed ?? "").Trim();

                if (string.IsNullOrWhiteSpace(dominioRed) ||
                    string.IsNullOrWhiteSpace(usuarioRed) ||
                    string.IsNullOrWhiteSpace(passwordRed))
                {
                    return "";
                }

                byte[] bytes = null;

                var credentials = new UserCredentials(
                    dominioRed,
                    usuarioRed,
                    passwordRed
                );

                using (SafeAccessTokenHandle userHandle = credentials.LogonUser(LogonType.NewCredentials))
                {
                    WindowsIdentity.RunImpersonated(userHandle, () =>
                    {
                        if (System.IO.File.Exists(rutaHuella))
                        {
                            bytes = System.IO.File.ReadAllBytes(rutaHuella);
                        }
                    });
                }

                if (bytes == null || bytes.Length == 0)
                    return "";

                string extension = Path.GetExtension(rutaHuella ?? "").ToLowerInvariant();

                string mime = "image/jpeg";

                if (extension == ".png")
                    mime = "image/png";
                else if (extension == ".bmp")
                    mime = "image/bmp";
                else if (extension == ".webp")
                    mime = "image/webp";

                return "data:" + mime + ";base64," + Convert.ToBase64String(bytes);
            }
            catch
            {
                return "";
            }
        }


        public ImagenArchivoResponse GetHuellaArchivoPersonaInteres(int idFicha)
        {
            try
            {
                if (idFicha <= 0)
                {
                    return new ImagenArchivoResponse
                    {
                        IsSuccess = false,
                        Message = "Huella inválida."
                    };
                }

                var huella = dbFiliMuni.tb_FichaDecadactilar
                    .AsNoTracking()
                    .FirstOrDefault(x => x.idFicha == idFicha
                                      && x.IdTbFuente == 3
                                      && (x.Activo == null || x.Activo == true));

                if (huella == null)
                {
                    return new ImagenArchivoResponse
                    {
                        IsSuccess = false,
                        Message = "No se encontró la huella."
                    };
                }

                if (string.IsNullOrWhiteSpace(huella.RutaHuella))
                {
                    return new ImagenArchivoResponse
                    {
                        IsSuccess = false,
                        Message = "La huella no tiene ruta registrada."
                    };
                }

                string dominioRed = ConfigurationManager.AppSettings["PersonasInteres.DominioRed"];
                string usuarioRed = ConfigurationManager.AppSettings["PersonasInteres.UsuarioRed"];
                string passwordRed = ConfigurationManager.AppSettings["PersonasInteres.PasswordRed"];

                dominioRed = (dominioRed ?? "").Trim();
                usuarioRed = (usuarioRed ?? "").Trim();

                if (string.IsNullOrWhiteSpace(dominioRed) ||
                    string.IsNullOrWhiteSpace(usuarioRed) ||
                    string.IsNullOrWhiteSpace(passwordRed))
                {
                    return new ImagenArchivoResponse
                    {
                        IsSuccess = false,
                        Message = "No están configuradas las credenciales del servidor de archivos."
                    };
                }

                byte[] bytes = null;

                var credentials = new UserCredentials(
                    dominioRed,
                    usuarioRed,
                    passwordRed
                );

                using (var userHandle = credentials.LogonUser(LogonType.NewCredentials))
                {
                    WindowsIdentity.RunImpersonated(userHandle, () =>
                    {
                        if (System.IO.File.Exists(huella.RutaHuella))
                        {
                            bytes = System.IO.File.ReadAllBytes(huella.RutaHuella);
                        }
                    });
                }

                if (bytes == null || bytes.Length == 0)
                {
                    return new ImagenArchivoResponse
                    {
                        IsSuccess = false,
                        Message = "No fue posible leer el archivo de la huella."
                    };
                }

                string extension = Path.GetExtension(huella.RutaHuella ?? "").ToLowerInvariant();

                string mimeType = "image/jpeg";

                if (extension == ".png")
                    mimeType = "image/png";
                else if (extension == ".bmp")
                    mimeType = "image/bmp";
                else if (extension == ".webp")
                    mimeType = "image/webp";

                return new ImagenArchivoResponse
                {
                    IsSuccess = true,
                    Bytes = bytes,
                    MimeType = mimeType,
                    NombreArchivo = Path.GetFileName(huella.RutaHuella)
                };
            }
            catch (Exception ex)
            {
                var errorReal = ex.GetBaseException();

                return new ImagenArchivoResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener la huella: " + errorReal.Message
                };
            }
        }


        public ImagenArchivoResponse GetFotoPrincipalPersonaInteres(int idPersona)
        {
            try
            {
                if (idPersona <= 0)
                {
                    return new ImagenArchivoResponse
                    {
                        IsSuccess = false,
                        Message = "Persona inválida."
                    };
                }

                var foto = dbFiliMuni.tb_Fotografia
                    .AsNoTracking()
                    .Where(x => x.idPersona == idPersona)
                    .OrderByDescending(x => x.idTipoFoto == 1)
                    .ThenByDescending(x => x.FechaRegistro)
                    .ThenByDescending(x => x.idFoto)
                    .FirstOrDefault();

                if (foto == null)
                {
                    return new ImagenArchivoResponse
                    {
                        IsSuccess = false,
                        Message = "La persona no tiene fotografías registradas."
                    };
                }

                byte[] bytes = null;

                if (!string.IsNullOrWhiteSpace(foto.ArchivoB64))
                {
                    string base64 = foto.ArchivoB64;

                    if (base64.Contains(","))
                    {
                        base64 = base64.Split(',')[1];
                    }

                    bytes = Convert.FromBase64String(base64);
                }
                else if (!string.IsNullOrWhiteSpace(foto.RutaArchivo))
                {
                    string dominioRed = ConfigurationManager.AppSettings["PersonasInteres.DominioRed"];
                    string usuarioRed = ConfigurationManager.AppSettings["PersonasInteres.UsuarioRed"];
                    string passwordRed = ConfigurationManager.AppSettings["PersonasInteres.PasswordRed"];

                    var credentials = new UserCredentials(
                        dominioRed,
                        usuarioRed,
                        passwordRed
                    );

                    using (var userHandle = credentials.LogonUser(LogonType.NewCredentials))
                    {
                        WindowsIdentity.RunImpersonated(userHandle, () =>
                        {
                            if (System.IO.File.Exists(foto.RutaArchivo))
                            {
                                bytes = System.IO.File.ReadAllBytes(foto.RutaArchivo);
                            }
                        });
                    }
                }

                if (bytes == null || bytes.Length == 0)
                {
                    return new ImagenArchivoResponse
                    {
                        IsSuccess = false,
                        Message = "No fue posible cargar la fotografía."
                    };
                }

                string extension = Path.GetExtension(foto.RutaArchivo ?? "").ToLowerInvariant();

                string mimeType = "image/jpeg";

                if (extension == ".png")
                    mimeType = "image/png";
                else if (extension == ".bmp")
                    mimeType = "image/bmp";
                else if (extension == ".webp")
                    mimeType = "image/webp";

                return new ImagenArchivoResponse
                {
                    IsSuccess = true,
                    Bytes = bytes,
                    MimeType = mimeType,
                    NombreArchivo = "foto_persona_" + idPersona
                };
            }
            catch (Exception ex)
            {
                var errorReal = ex.GetBaseException();

                return new ImagenArchivoResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al cargar la fotografía: " + errorReal.Message
                };
            }
        }



        public tb_PersonaPeriodoBusqueda GetPeriodoBusquedaActivo(int idPersona)
        {
            return dbFiliMuni.tb_PersonaPeriodoBusqueda
                .AsNoTracking()
                .Where(x => x.idPersona == idPersona && x.Activo == true)
                .OrderByDescending(x => x.FechaRegistro)
                .FirstOrDefault();
        }

        public List<tb_PersonaPeriodoBusqueda> GetHistorialPeriodosBusqueda(int idPersona)
        {
            return dbFiliMuni.tb_PersonaPeriodoBusqueda
        .Include(x => x.cat_EstatusPeriodoBusqueda)
        .AsNoTracking()
        .Where(x => x.idPersona == idPersona)
        .OrderByDescending(x => x.Activo)
        .ThenByDescending(x => x.FechaRegistro)
        .ToList();
        }

        public int CalcularDiasTotales(DateTime fechaInicio, DateTime? fechaFin)
        {
            if (!fechaFin.HasValue)
                return 0;

            return (fechaFin.Value.Date - fechaInicio.Date).Days + 1;
        }

        public int CalcularDiasRestantes(DateTime? fechaFin)
        {
            if (!fechaFin.HasValue)
                return 0;

            if (fechaFin.Value.Date < DateTime.Today)
                return 0;

            return (fechaFin.Value.Date - DateTime.Today).Days + 1;
        }

        //public string GetEstatusVisualPeriodo(tb_PersonaPeriodoBusqueda periodo)
        //{
        //    if (periodo == null)
        //        return "Sin periodo";

        //    if (periodo.Activo &&
        //        periodo.FechaFinBusqueda.HasValue &&
        //        periodo.FechaFinBusqueda.Value.Date < DateTime.Today)
        //    {
        //        return "Vencido";
        //    }

        //    if (periodo.cat_EstatusPeriodoBusqueda != null &&
        //        !string.IsNullOrWhiteSpace(periodo.cat_EstatusPeriodoBusqueda.Nombre))
        //    {
        //        return periodo.cat_EstatusPeriodoBusqueda.Nombre;
        //    }

        //    return "Sin estatus";
        //}

        public BasicOperationResponse SavePeriodoBusquedaPersonaInteres(
            int idPersona,
            DateTime fechaInicioBusqueda,
            DateTime? fechaFinBusqueda,
            string observaciones,
            int usuarioRegistro)
        {
            try
            {
                if (idPersona <= 0)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "Primero debes guardar la persona de interés."
                    };
                }

                var persona = dbFiliMuni.tb_Persona
                    .FirstOrDefault(x => x.idPersona == idPersona);

                if (persona == null)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No se encontró la persona de interés."
                    };
                }

                if (!fechaFinBusqueda.HasValue)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "La fecha fin del periodo es obligatoria."
                    };
                }

                if (fechaFinBusqueda.Value.Date < fechaInicioBusqueda.Date)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "La fecha fin no puede ser menor a la fecha inicio."
                    };
                }

                using (var transaction = dbFiliMuni.Database.BeginTransaction())
                {
                    try
                    {
                        var periodoActivo = dbFiliMuni.tb_PersonaPeriodoBusqueda
                            .Where(x => x.idPersona == idPersona && x.Activo == true)
                            .ToList();

                        foreach (var periodo in periodoActivo)
                        {
                            periodo.Activo = false;
                            periodo.idEstatusPeriodoBusqueda = ESTATUS_PERIODO_REEMPLAZADO;
                            periodo.FechaCancelacion = DateTime.Now;
                            periodo.UsuarioCancelacion = usuarioRegistro;
                            periodo.MotivoCancelacion = "Reemplazado por nuevo periodo de búsqueda.";
                        }

                        var nuevoPeriodo = new tb_PersonaPeriodoBusqueda
                        {
                            idPersona = idPersona,
                            FechaInicioBusqueda = fechaInicioBusqueda.Date,
                            FechaFinBusqueda = fechaFinBusqueda.Value.Date,
                            FechaRegistro = DateTime.Now,
                            UsuarioRegistro = usuarioRegistro,
                            Activo = true,
                            idEstatusPeriodoBusqueda = ESTATUS_PERIODO_ACTIVO,
                            Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim()
                        };

                        dbFiliMuni.tb_PersonaPeriodoBusqueda.Add(nuevoPeriodo);
                        dbFiliMuni.SaveChanges();

                        transaction.Commit();

                        return new BasicOperationResponse
                        {
                            IsSuccess = true,
                            Message = "Periodo de búsqueda guardado correctamente.",
                            Id = nuevoPeriodo.idPeriodoBusqueda
                        };
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                var errorReal = ex.GetBaseException();

                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al guardar el periodo de búsqueda: " + errorReal.Message
                };
            }
        }

        public BasicOperationResponse CancelarPeriodoBusquedaPersonaInteres(
            int idPeriodoBusqueda,
            string motivoCancelacion,
            int usuarioCancelacion)
        {
            try
            {
                if (idPeriodoBusqueda <= 0)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "Periodo inválido."
                    };
                }

                var periodo = dbFiliMuni.tb_PersonaPeriodoBusqueda
                    .FirstOrDefault(x => x.idPeriodoBusqueda == idPeriodoBusqueda);

                if (periodo == null)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No se encontró el periodo de búsqueda."
                    };
                }

                periodo.Activo = false;
                periodo.idEstatusPeriodoBusqueda = ESTATUS_PERIODO_CANCELADO;
                periodo.FechaCancelacion = DateTime.Now;
                periodo.UsuarioCancelacion = usuarioCancelacion;
                periodo.MotivoCancelacion = string.IsNullOrWhiteSpace(motivoCancelacion)
                    ? "Cancelado manualmente."
                    : motivoCancelacion.Trim();

                dbFiliMuni.SaveChanges();

                return new BasicOperationResponse
                {
                    IsSuccess = true,
                    Message = "Periodo de búsqueda cancelado correctamente.",
                    Id = periodo.idPeriodoBusqueda
                };
            }
            catch (Exception ex)
            {
                var errorReal = ex.GetBaseException();

                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al cancelar el periodo de búsqueda: " + errorReal.Message
                };
            }
        }


        public string GetEstatusVisualPeriodo(tb_PersonaPeriodoBusqueda periodo)
        {
            if (periodo == null)
                return "Sin búsqueda";

            if (periodo.Activo &&
                periodo.FechaFinBusqueda.HasValue &&
                periodo.FechaFinBusqueda.Value.Date < DateTime.Today)
            {
                return "Vencido";
            }

            if (periodo.cat_EstatusPeriodoBusqueda != null &&
                !string.IsNullOrWhiteSpace(periodo.cat_EstatusPeriodoBusqueda.Nombre))
            {
                return periodo.cat_EstatusPeriodoBusqueda.Nombre;
            }

            if (periodo.Activo)
                return "Activo";

            return "Sin búsqueda";
        }


        public ImagenArchivoResponse GetFotoArchivoPersonaInteres(int idFoto)
        {
            try
            {
                if (idFoto <= 0)
                {
                    return new ImagenArchivoResponse
                    {
                        IsSuccess = false,
                        Message = "Fotografía inválida."
                    };
                }

                var foto = dbFiliMuni.tb_Fotografia
                    .AsNoTracking()
                    .FirstOrDefault(x => x.idFoto == idFoto);

                if (foto == null)
                {
                    return new ImagenArchivoResponse
                    {
                        IsSuccess = false,
                        Message = "No se encontró la fotografía."
                    };
                }

                byte[] bytes = null;

                /*
                 * Primero intentamos obtenerla del Base64.
                 */
                if (!string.IsNullOrWhiteSpace(foto.ArchivoB64))
                {
                    string base64 =
                        foto.ArchivoB64
                            .Trim();

                    if (
                        base64.StartsWith(
                            "data:image",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        int posicionComa =
                            base64.IndexOf(',');

                        if (posicionComa >= 0)
                        {
                            base64 =
                                base64.Substring(
                                    posicionComa + 1
                                );
                        }
                    }

                    bytes =
                        Convert.FromBase64String(
                            base64
                        );
                }
                /*
                 * Si no tiene Base64, intentamos desde archivo.
                 */
                else if (!string.IsNullOrWhiteSpace(foto.RutaArchivo))
                {
                    string dominioRed =
                        ConfigurationManager.AppSettings[
                            "PersonasInteres.DominioRed"
                        ];

                    string usuarioRed =
                        ConfigurationManager.AppSettings[
                            "PersonasInteres.UsuarioRed"
                        ];

                    string passwordRed =
                        ConfigurationManager.AppSettings[
                            "PersonasInteres.PasswordRed"
                        ];

                    dominioRed =
                        (dominioRed ?? "").Trim();

                    usuarioRed =
                        (usuarioRed ?? "").Trim();

                    var credentials =
                        new UserCredentials(
                            dominioRed,
                            usuarioRed,
                            passwordRed
                        );

                    using (
                        SafeAccessTokenHandle userHandle =
                            credentials.LogonUser(
                                LogonType.NewCredentials
                            )
                    )
                    {
                        WindowsIdentity.RunImpersonated(
                            userHandle,
                            () =>
                            {
                                if (
                                    System.IO.File.Exists(
                                        foto.RutaArchivo
                                    )
                                )
                                {
                                    bytes =
                                        System.IO.File.ReadAllBytes(
                                            foto.RutaArchivo
                                        );
                                }
                            }
                        );
                    }
                }

                if (
                    bytes == null ||
                    bytes.Length == 0
                )
                {
                    return new ImagenArchivoResponse
                    {
                        IsSuccess = false,
                        Message = "No fue posible cargar la fotografía."
                    };
                }

                string extension =
                    Path.GetExtension(
                        foto.RutaArchivo ?? ""
                    )
                    .ToLowerInvariant();

                string mimeType =
                    "image/jpeg";

                if (extension == ".png")
                    mimeType = "image/png";
                else if (extension == ".bmp")
                    mimeType = "image/bmp";
                else if (extension == ".webp")
                    mimeType = "image/webp";

                return new ImagenArchivoResponse
                {
                    IsSuccess = true,
                    Bytes = bytes,
                    MimeType = mimeType,
                    NombreArchivo =
                        "foto_" +
                        foto.idFoto +
                        extension
                };
            }
            catch (Exception ex)
            {
                var errorReal =
                    ex.GetBaseException();

                return new ImagenArchivoResponse
                {
                    IsSuccess = false,
                    Message =
                        "Ocurrió un error al cargar la fotografía: "
                        + errorReal.Message
                };
            }
        }

    }
}