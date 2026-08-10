using MediaBrowser.Model.Net;
using Microsoft.Win32.SafeHandles;
using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using SimpleImpersonation;

namespace Objetivos_Prioritarios.ControllersServices
{
    public class AsuntoService : BaseService
    {

        // Obtener lista (por estatus)
        public List<tb_AsuntoRelacionado> GetAsuntosList(bool? actives)
        {
            bool activo = actives ?? true;
            return db.tb_AsuntoRelacionado
                     .AsNoTracking()
                     .Where(x => x.bit_estatus == activo)
                     .OrderByDescending(x => x.date_fecha_creacion)
                     .ToList();
        }

        // Obtener por id
        public tb_AsuntoRelacionado GetAsuntoById(int id)
        {
            return db.tb_AsuntoRelacionado
                     .AsNoTracking()
                     .FirstOrDefault(x => x.int_id_asunto_relacionado == id);
        }

        // Activar
        public BasicOperationResponse ActivateAsunto(int int_id_asunto_relacionado)
        {
            try
            {
                var entidad = db.tb_AsuntoRelacionado.FirstOrDefault(x => x.int_id_asunto_relacionado == int_id_asunto_relacionado);
                if (entidad == null) return new BasicOperationResponse { IsSuccess = false, Message = "Asunto no encontrado." };

                entidad.bit_estatus = true;
                db.SaveChanges();
                return new BasicOperationResponse { IsSuccess = true, Message = "Se activó el asunto satisfactoriamente." };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse { IsSuccess = false, Message = "Ocurrió un error al activar el asunto (" + e.Message + ")" };
            }
        }

        // Desactivar
        public BasicOperationResponse DisableAsunto(int int_id_asunto_relacionado)
        {
            try
            {
                var entidad = db.tb_AsuntoRelacionado.FirstOrDefault(x => x.int_id_asunto_relacionado == int_id_asunto_relacionado);
                if (entidad == null) return new BasicOperationResponse { IsSuccess = false, Message = "Asunto no encontrado." };

                entidad.bit_estatus = false;
                db.SaveChanges();
                return new BasicOperationResponse { IsSuccess = true, Message = "Se desactivó el asunto satisfactoriamente." };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse { IsSuccess = false, Message = "Ocurrió un error al desactivar el asunto (" + e.Message + ")" };
            }
        }

        // Guardar (insert/update)
        //public BasicOperationResponse SaveAsunto(tb_AsuntoRelacionado asunto, string usuario = "delira")
        //{
        //    try
        //    {
        //        if (asunto == null) return new BasicOperationResponse { IsSuccess = false, Message = "Datos inválidos." };

        //        if (asunto.int_id_asunto_relacionado == 0)
        //        {
        //            asunto.date_fecha_creacion = DateTime.Now;
        //            asunto.bit_estatus = true;
        //            // si quieres guardar usuario: agregar propiedad en el modelo o manejar de otra forma
        //            db.tb_AsuntoRelacionado.Add(asunto);
        //        }
        //        else
        //        {
        //            var found = db.tb_AsuntoRelacionado.FirstOrDefault(x => x.int_id_asunto_relacionado == asunto.int_id_asunto_relacionado);
        //            if (found == null) return new BasicOperationResponse { IsSuccess = false, Message = "Asunto no encontrado para actualizar." };

        //            // Actualiza campos que permitan editar
        //            found.nvarchar_alias = asunto.nvarchar_alias;
        //            found.nvarchar_descripcion = asunto.nvarchar_descripcion;
        //            found.date_fecha_asunto = asunto.date_fecha_asunto;
        //            found.numavp = asunto.numavp;
        //            found.int_id_estatus_asunto = asunto.int_id_estatus_asunto;
        //            // no tocamos date_fecha_creacion ni bit_estatus aquí salvo que sea necesario
        //        }

        //        db.SaveChanges();
        //        return new BasicOperationResponse { IsSuccess = true, Message = "Asunto guardado correctamente." };
        //    }
        //    catch (Exception e)
        //    {
        //        return new BasicOperationResponse { IsSuccess = false, Message = "Ocurrió un error al guardar el asunto (" + e.Message + ")" };
        //    }
        //}

        public BasicOperationResponse SaveAsunto(tb_AsuntoRelacionado asunto, string usuario = "delira")
        {
            try
            {
                if (asunto == null)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "Datos inválidos."
                    };
                }

                int idGuardado = 0;

                if (asunto.int_id_asunto_relacionado == 0)
                {
                    asunto.date_fecha_creacion = DateTime.Now;
                    asunto.bit_estatus = true;

                    db.tb_AsuntoRelacionado.Add(asunto);
                    db.SaveChanges();

                    idGuardado = asunto.int_id_asunto_relacionado;
                }
                else
                {
                    var found = db.tb_AsuntoRelacionado
                                  .FirstOrDefault(x => x.int_id_asunto_relacionado == asunto.int_id_asunto_relacionado);

                    if (found == null)
                    {
                        return new BasicOperationResponse
                        {
                            IsSuccess = false,
                            Message = "Asunto no encontrado para actualizar."
                        };
                    }

                    found.nvarchar_alias = asunto.nvarchar_alias;
                    found.nvarchar_descripcion = asunto.nvarchar_descripcion;
                    found.date_fecha_asunto = asunto.date_fecha_asunto;
                    found.numavp = asunto.numavp;
                    found.int_id_estatus_asunto = asunto.int_id_estatus_asunto;

                    db.SaveChanges();

                    idGuardado = found.int_id_asunto_relacionado;
                }

                return new BasicOperationResponse
                {
                    IsSuccess = true,
                    Message = "Asunto guardado correctamente.",
                    Id = idGuardado
                };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al guardar el asunto (" + e.Message + ")"
                };
            }
        }

        public List<tb_AsuntoVictimas> getListAsuntoVictima(bool activo, int int_id_asunto_relacionado)
        {
            return db.tb_AsuntoVictimas
                .Where(v => v.int_id_asunto_relacionado == int_id_asunto_relacionado && v.bit_estatus == activo).ToList();
        }


        public BasicOperationResponse DisableVictimaRelacionada(int int_id_asunto_victima)
        {
            try
            {
                var existe = db.tb_AsuntoVictimas.Where(x => x.int_id_asunto_victima == int_id_asunto_victima).FirstOrDefault();
                existe.bit_estatus = false;

                db.SaveChanges();

                return new BasicOperationResponse { IsSuccess = true, Message = "Víctima desactivada correctamente." };
            }
            catch (Exception ex)
            {
                return new BasicOperationResponse { IsSuccess = false, Message = ex.Message };
            }
        }

        public BasicOperationResponse ActivateVictimaRelacionada(int int_id_asunto_victima)
        {
            try
            {
                var existe = db.tb_AsuntoVictimas.Where(x => x.int_id_asunto_victima == int_id_asunto_victima).FirstOrDefault();
                existe.bit_estatus = true;

                db.SaveChanges();

                return new BasicOperationResponse { IsSuccess = true, Message = "Víctima activada correctamente." };
            }
            catch (Exception ex)
            {
                return new BasicOperationResponse { IsSuccess = false, Message = ex.Message };
            }
        }

        public BasicOperationResponse RemoveVictimaFromAsunto(int int_id_asunto_victima)
        {
            try
            {
                var rel = db.tb_AsuntoVictimas.Find(int_id_asunto_victima);
                if (rel != null)
                {
                    rel.bit_estatus = false;
                    db.SaveChanges();
                }
                return new BasicOperationResponse { IsSuccess = true, Message = "Víctima eliminada correctamente." };
            }
            catch (Exception ex)
            {
                return new BasicOperationResponse { IsSuccess = false, Message = ex.Message };
            }
        }


        public List<getNamePhotoList_Result> GetVictimasNamePhotoList(string nombre, string paterno, string materno)
        {
            return db.getNamePhotoList(nombre, paterno, materno).ToList();
        }

        public List<getListVictimasByNombre_Result> getListVictimas(string nombre, string paterno, string materno, int option, bool active)
        {

            return db.getListVictimasByNombre(nombre, paterno, materno, option, active).ToList();
        }



        public BasicOperationResponse AddDetenidoVictima(int id, int id_asunto_relacionado)
        {
            try
            {
                string msg = "";
                // Buscar el detenido en la base de filiación
                var busDet = dbFili.Nom_perso.AsNoTracking().FirstOrDefault(x => x.id == id);
                if (busDet == null)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No se encontró el detenido con el ID proporcionado."
                    };
                }

                string foto = null;
                try
                {
                    foto = getFotosDetenidosBase(busDet.CLAVE_PERSO);
                }
                catch
                {
                    foto = null;
                }


                // Si no hay foto, usar imagen local predeterminada
                if (string.IsNullOrEmpty(foto))
                {
                    try
                    {
                        string rutaImagen = System.Web.Hosting.HostingEnvironment.MapPath("~/images/NoDisponible.png");

                        if (System.IO.File.Exists(rutaImagen))
                        {
                            byte[] imagenBytes = System.IO.File.ReadAllBytes(rutaImagen);
                            foto = Convert.ToBase64String(imagenBytes);
                        }
                        else
                        {
                            foto = string.Empty;
                        }
                    }
                    catch
                    {
                        foto = string.Empty;
                    }
                }


                // Iniciar transacción
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var nuevaVictima = new tb_Victimas();
                        nuevaVictima = db.tb_Victimas.Where(x => x.nvarchar_nombre == busDet.NOMBRE && x.nvarchar_paterno == busDet.AP_PATERNO && x.nvarchar_materno == busDet.AP_MATERNO).FirstOrDefault();

                        if (nuevaVictima == null)
                        {

                            // Insertar víctima
                            nuevaVictima = new tb_Victimas
                            {
                                nvarchar_nombre = busDet.NOMBRE,
                                nvarchar_paterno = busDet.AP_PATERNO,
                                nvarchar_materno = busDet.AP_MATERNO,
                                nvarchar_foto = foto,
                                date_fecha_creacion = DateTime.Now,
                                bit_estatus = true
                            };

                            db.tb_Victimas.Add(nuevaVictima);
                            db.SaveChanges();
                        }

                        var nuevaRelacion = new tb_AsuntoVictimas();
                        nuevaRelacion = db.tb_AsuntoVictimas.Where(x => x.int_id_victima == nuevaVictima.int_id_victima && x.int_id_asunto_relacionado == id_asunto_relacionado).FirstOrDefault();

                        if (nuevaRelacion != null)
                        {
                            if (nuevaRelacion.bit_estatus == false)
                            {
                                nuevaRelacion.bit_estatus = true;
                                msg = "Se activo víctima ya existente en la relación.";

                            }
                            else
                            {
                                msg = "Víctima ya existente en la relación.";
                            }

                        }
                        else
                        {

                            // Insertar relación con el asunto
                            nuevaRelacion = new tb_AsuntoVictimas
                            {
                                int_id_asunto_relacionado = id_asunto_relacionado,
                                int_id_victima = nuevaVictima.int_id_victima,
                                date_fecha_creacion = DateTime.Now,
                                bit_estatus = true
                            };

                            db.tb_AsuntoVictimas.Add(nuevaRelacion);
                            msg = "Víctima agregada correctamente al asunto.";
                        }
                        db.SaveChanges();

                        // Confirmar transacción
                        transaction.Commit();

                        return new BasicOperationResponse
                        {
                            IsSuccess = true,
                            Message = msg
                        };
                    }
                    catch (Exception exTrans)
                    {
                        // Revertir cambios
                        transaction.Rollback();

                        return new BasicOperationResponse
                        {
                            IsSuccess = false,
                            Message = "Error durante la transacción: " + exTrans.Message
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al agregar la víctima: " + ex.Message
                };
            }
        }


        public string getFotosDetenidosBase(int clave_persona)
        {
            string fotoBase64 = "";
            var fili = dbFili.Persona.AsNoTracking().Where(x => x.CLAVE_PERSO == clave_persona).FirstOrDefault();

            if (fili != null)
            {
                var numFili = fili.NUM_FILIA == null ? "" : fili.NUM_FILIA.Replace("/", "").Trim();

                if (string.IsNullOrEmpty(numFili) || numFili == "*")
                {
                    fotoBase64 = "/Content/imagenes/Nodisponible.jpg";
                    return fotoBase64;
                }

                if (string.IsNullOrEmpty(numFili) || numFili.Length < 2)
                {
                    fotoBase64 = "/Content/imagenes/Nodisponible.jpg";
                    return fotoBase64;
                }

                var anio = numFili.Substring(0, 2);
                string path = "";
                byte[] fotoEncontrada = null;

                try
                {
                    NetworkShare net = new NetworkShare();
                    UserCredentials credentials = new UserCredentials("pgj.gob", "IIS_AEIC", "d8J!BKGzIH9Q@Ox");

                    using (SafeAccessTokenHandle userHandle = credentials.LogonUser(LogonType.Interactive))
                    {
                        WindowsIdentity.RunImpersonated(userHandle, () =>
                        {
                            // 1. Prioridad: Foto 2 (Default)
                            path = "\\\\59pgje\\fotos_det$\\" + anio + "\\" + numFili + "1.jpg";
                            if (System.IO.File.Exists(path))
                            {
                                fotoEncontrada = System.IO.File.ReadAllBytes(path);
                            }
                            else
                            {
                                // 2. Si no existe la 2, intenta con la 1
                                path = "\\\\59pgje\\fotos_det$\\" + anio + "\\" + numFili + "2.jpg";
                                if (System.IO.File.Exists(path))
                                {
                                    fotoEncontrada = System.IO.File.ReadAllBytes(path);
                                }
                                else
                                {
                                    // 3. Si tampoco existe la 1, intenta con la 3
                                    path = "\\\\59pgje\\fotos_det$\\" + anio + "\\" + numFili + "3.jpg";
                                    if (System.IO.File.Exists(path))
                                    {
                                        fotoEncontrada = System.IO.File.ReadAllBytes(path);
                                    }
                                }
                            }

                            // Solo convierte a Base64 la foto que realmente se encontró
                            if (fotoEncontrada != null && fotoEncontrada.Length > 0)
                            {
                                fotoBase64 = Convert.ToBase64String(fotoEncontrada);
                            }
                            else
                            {
                                fotoBase64 = "/Content/imagenes/Nodisponible.jpg";
                            }
                        });
                    }
                }
                catch
                {
                    fotoBase64 = "/Content/imagenes/Nodisponible.jpg";
                }
            }
            else
            {
                fotoBase64 = Url.Content("~/Content/image/Nodisponible.png");
            }

            return fotoBase64;
        }







        public BasicOperationResponse AddObjetivoVictima(int id_nombre, int id_asunto_relacionado)
        {
            try
            {
                string msg = "";
                // Buscar el detenido en la base de filiación
                var busNom = db.tb_NombreObjetivo.AsNoTracking().FirstOrDefault(x => x.int_id_nombre == id_nombre);
                if (busNom == null)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No se encontró el objetivo con el ID proporcionado."
                    };
                }
                var id_objetivo = busNom.int_id_objetivo;
                string foto = null;
                try
                {
                    var busObj = db.tb_Objetivo.Where(x => x.int_id_objetivo == id_objetivo).FirstOrDefault();

                    foto = busObj.nvarchar_foto;
                }
                catch
                {
                    foto = null;
                }


                // Si no hay foto, usar imagen local predeterminada
                if (string.IsNullOrEmpty(foto))
                {
                    try
                    {
                        string rutaImagen = System.Web.Hosting.HostingEnvironment.MapPath("~/images/NoDisponible.png");

                        if (System.IO.File.Exists(rutaImagen))
                        {
                            byte[] imagenBytes = System.IO.File.ReadAllBytes(rutaImagen);
                            foto = Convert.ToBase64String(imagenBytes);
                        }
                        else
                        {
                            foto = string.Empty;
                        }
                    }
                    catch
                    {
                        foto = string.Empty;
                    }
                }


                // Iniciar transacción
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var nuevaVictima = new tb_Victimas();
                        nuevaVictima = db.tb_Victimas.Where(x => x.nvarchar_nombre == busNom.nvarchar_nombre && x.nvarchar_paterno == busNom.nvarchar_paterno && x.nvarchar_materno == busNom.nvarchar_materno).FirstOrDefault();

                        if (nuevaVictima == null)
                        {

                            // Insertar víctima
                            nuevaVictima = new tb_Victimas
                            {
                                nvarchar_nombre = busNom.nvarchar_nombre,
                                nvarchar_paterno = busNom.nvarchar_paterno,
                                nvarchar_materno = busNom.nvarchar_materno,
                                nvarchar_foto = foto,
                                date_fecha_creacion = DateTime.Now,
                                bit_estatus = true
                            };

                            db.tb_Victimas.Add(nuevaVictima);
                            db.SaveChanges();
                        }

                        var nuevaRelacion = new tb_AsuntoVictimas();
                        nuevaRelacion = db.tb_AsuntoVictimas.Where(x => x.int_id_victima == nuevaVictima.int_id_victima && x.int_id_asunto_relacionado == id_asunto_relacionado).FirstOrDefault();

                        if (nuevaRelacion != null)
                        {
                            if (nuevaRelacion.bit_estatus == false)
                            {
                                nuevaRelacion.bit_estatus = true;
                                msg = "Se activo víctima ya existente en la relación.";

                            }
                            else
                            {
                                msg = "Víctima ya existente en la relación.";
                            }

                        }
                        else
                        {

                            // Insertar relación con el asunto
                            nuevaRelacion = new tb_AsuntoVictimas
                            {
                                int_id_asunto_relacionado = id_asunto_relacionado,
                                int_id_victima = nuevaVictima.int_id_victima,
                                date_fecha_creacion = DateTime.Now,
                                bit_estatus = true
                            };

                            db.tb_AsuntoVictimas.Add(nuevaRelacion);
                            msg = "Víctima agregada correctamente al asunto.";
                        }
                        db.SaveChanges();

                        // Confirmar transacción
                        transaction.Commit();

                        return new BasicOperationResponse
                        {
                            IsSuccess = true,
                            Message = msg
                        };
                    }
                    catch (Exception exTrans)
                    {
                        // Revertir cambios
                        transaction.Rollback();

                        return new BasicOperationResponse
                        {
                            IsSuccess = false,
                            Message = "Error durante la transacción: " + exTrans.Message
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al agregar la víctima: " + ex.Message
                };
            }
        }

        public BasicOperationResponse ReactivarVictima(int id, int id_victima, int id_asunto_relacionado)
        {
            var response = new BasicOperationResponse();
            string msg = "";

            try
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    if (id == 1)
                    {
                        try
                        {
                            // Buscar la víctima por ID
                            var victima = db.tb_Victimas
                                .FirstOrDefault(x => x.int_id_victima == id_victima);

                            if (victima != null)
                            {
                                if (victima.bit_estatus == false)
                                {
                                    // Reactivar víctima
                                    victima.bit_estatus = true;
                                    db.SaveChanges();

                                    transaction.Commit();
                                    msg = "Se reactivó la víctima exitosamente.";
                                    response.IsSuccess = true;
                                }
                                else
                                {
                                    msg = "La víctima ya está activa.";
                                    response.IsSuccess = false;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            msg = "Error al intentar reactivar la víctima: " + ex.Message;
                            response.IsSuccess = false;
                        }
                    }
                    else
                    {
                        // Buscar la víctima por ID
                        var victima = db.tb_Victimas
                            .FirstOrDefault(x => x.int_id_victima == id_victima);

                        if (victima != null)
                        {
                            if (victima.bit_estatus == true)
                            {
                                // Reactivar víctima
                                victima.bit_estatus = false;
                                db.SaveChanges();

                                transaction.Commit();
                                msg = "Se desactivo la víctima exitosamente.";
                                response.IsSuccess = true;
                            }
                            else
                            {
                                msg = "La víctima ya está inactiva.";
                                response.IsSuccess = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg = "Error general al procesar la reactivación: " + ex.Message;
                response.IsSuccess = false;
            }

            response.Message = msg;
            return response;
        }

        public BasicOperationResponse AddVictimaVictima(int id_victima, int id_asunto_relacionado)
        {
            try
            {
                string msg = "";
                // Buscar el detenido en la base de filiación
                var busVic = db.tb_Victimas.AsNoTracking().FirstOrDefault(x => x.int_id_victima == id_victima);
                if (busVic == null)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No se encontró la víctima con el ID proporcionado."
                    };
                }

                string foto = null;
                try
                {
                    foto = busVic.nvarchar_foto;
                }
                catch
                {
                    foto = null;
                }


                // Si no hay foto, usar imagen local predeterminada
                if (string.IsNullOrEmpty(foto))
                {
                    try
                    {
                        string rutaImagen = System.Web.Hosting.HostingEnvironment.MapPath("~/images/NoDisponible.png");

                        if (System.IO.File.Exists(rutaImagen))
                        {
                            byte[] imagenBytes = System.IO.File.ReadAllBytes(rutaImagen);
                            foto = Convert.ToBase64String(imagenBytes);
                        }
                        else
                        {
                            foto = string.Empty;
                        }
                    }
                    catch
                    {
                        foto = string.Empty;
                    }
                }


                // Iniciar transacción
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {

                        var nuevaRelacion = new tb_AsuntoVictimas();
                        nuevaRelacion = db.tb_AsuntoVictimas.Where(x => x.int_id_victima == busVic.int_id_victima && x.int_id_asunto_relacionado == id_asunto_relacionado).FirstOrDefault();

                        if (nuevaRelacion != null)
                        {
                            if (nuevaRelacion.bit_estatus == false)
                            {
                                nuevaRelacion.bit_estatus = true;
                                msg = "Se activo víctima ya existente en la relación.";

                            }
                            else
                            {
                                msg = "Víctima ya existente en la relación.";
                            }

                        }
                        else
                        {

                            // Insertar relación con el asunto
                            nuevaRelacion = new tb_AsuntoVictimas
                            {
                                int_id_asunto_relacionado = id_asunto_relacionado,
                                int_id_victima = busVic.int_id_victima,
                                date_fecha_creacion = DateTime.Now,
                                bit_estatus = true
                            };

                            db.tb_AsuntoVictimas.Add(nuevaRelacion);
                            msg = "Víctima agregada correctamente al asunto.";
                        }
                        db.SaveChanges();

                        // Confirmar transacción
                        transaction.Commit();

                        return new BasicOperationResponse
                        {
                            IsSuccess = true,
                            Message = msg
                        };
                    }
                    catch (Exception exTrans)
                    {
                        // Revertir cambios
                        transaction.Rollback();

                        return new BasicOperationResponse
                        {
                            IsSuccess = false,
                            Message = "Error durante la transacción: " + exTrans.Message
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al agregar la víctima: " + ex.Message
                };
            }
        }

        public BasicOperationResponse SaveVictimaService(tb_Victimas victima, int idllamada, int idasunto)
        {
            try
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (victima == null)
                        {
                            return new BasicOperationResponse
                            {
                                IsSuccess = false,
                                Message = "❌ Datos de víctima inválidos."
                            };
                        }
                        else
                        {

                            if (victima.int_id_victima != 0)
                            {
                                var existente = db.tb_Victimas.FirstOrDefault(x => x.int_id_victima == victima.int_id_victima);
                                if (existente != null)
                                {
                                    existente.nvarchar_nombre = victima.nvarchar_nombre;
                                    existente.nvarchar_paterno = victima.nvarchar_paterno;
                                    existente.nvarchar_materno = victima.nvarchar_materno;
                                    existente.nvarchar_foto = victima.nvarchar_foto;
                                    db.SaveChanges();
                                    transaction.Commit();
                                    return new BasicOperationResponse
                                    {
                                        IsSuccess = true,
                                        Message = "✅ Víctima actualizada correctamente."
                                    };
                                }
                            }
                            else
                            {
                                victima.date_fecha_creacion = DateTime.Now;
                                victima.bit_estatus = true;
                                db.tb_Victimas.Add(victima);
                                db.SaveChanges();
                            }



                            if (idllamada == 1)
                            {
                                int nuevoIdVictima = (db.tb_Victimas.Max(x => (int?)x.int_id_victima) ?? 0);

                                var VictimaAsunto = new tb_AsuntoVictimas
                                {
                                    int_id_asunto_relacionado = idasunto,
                                    int_id_victima = nuevoIdVictima,
                                    bit_estatus = true,
                                    date_fecha_creacion = DateTime.Now
                                };
                                db.tb_AsuntoVictimas.Add(VictimaAsunto);
                                db.SaveChanges();

                            }


                            transaction.Commit();

                            return new BasicOperationResponse
                            {
                                IsSuccess = true,
                                Message = "✅ Víctima guardada correctamente."
                            };
                        }
                    }
                    catch (Exception exTrans)
                    {
                        transaction.Rollback();

                        return new BasicOperationResponse
                        {
                            IsSuccess = false,
                            Message = "❌ Error durante la transacción: " + exTrans.Message
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "❌ Error al guardar la víctima: " + ex.Message
                };
            }
        }

        public BasicOperationResponse addObjetivoAsunto(
            int int_id_objetivo,
            int int_id_asunto_relacionado,
            string observaciones,
            int? int_id_rol_participacion = null,
            string descripcionParticipacion = null)
        {
            try
            {
                string msg = "";

                var busFicha = db.tb_FichaObjetivo
                                 .FirstOrDefault(x => x.int_id_objetivo == int_id_objetivo);

                if (busFicha == null)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No se encontró la ficha del objetivo."
                    };
                }

                var id_ficha = busFicha.int_id_ficha_objetivo;

                var busFichaAsunto = db.tb_FichaAsunto
                                       .FirstOrDefault(x =>
                                           x.int_id_asunto_relacionado == int_id_asunto_relacionado &&
                                           x.int_id_ficha_objetivo == id_ficha);

                if (busFichaAsunto != null)
                {
                    busFichaAsunto.nvarchar_observaciones = observaciones;
                    busFichaAsunto.int_id_rol_participacion = int_id_rol_participacion;
                    busFichaAsunto.nvarchar_descripcion_participacion = descripcionParticipacion;

                    if (busFichaAsunto.bit_estatus == false)
                    {
                        busFichaAsunto.bit_estatus = true;
                        msg = "Se activó el objetivo existente en la relación y se actualizó la participación.";
                    }
                    else
                    {
                        msg = "El objetivo ya existía en la relación; se actualizó la participación.";
                    }
                }
                else
                {
                    busFichaAsunto = new tb_FichaAsunto
                    {
                        int_id_asunto_relacionado = int_id_asunto_relacionado,
                        int_id_ficha_objetivo = id_ficha,
                        int_id_rol_participacion = int_id_rol_participacion,
                        nvarchar_descripcion_participacion = descripcionParticipacion,
                        nvarchar_observaciones = observaciones,
                        date_fecha_creacion = DateTime.Now,
                        bit_estatus = true
                    };

                    db.tb_FichaAsunto.Add(busFichaAsunto);

                    msg = "Objetivo agregado correctamente al asunto.";
                }

                db.SaveChanges();

                return new BasicOperationResponse
                {
                    IsSuccess = true,
                    Message = msg
                };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al relacionar el objetivo con el asunto (" + e.Message + ")"
                };
            }
        }


        public List<getListObjetivosRelacionadoAsunto_Result> getListObjetivosRelacionadoAsunto(int int_id_asunto_relacionado, bool activo)
        {
            return db.getListObjetivosRelacionadoAsunto(activo, int_id_asunto_relacionado).ToList();
        }
        public BasicOperationResponse ActivateObjetivoAsunto(int int_id_ficha_asunto)
        {
            try
            {
                var entidad = db.tb_FichaAsunto.FirstOrDefault(x => x.int_id_ficha_asunto == int_id_ficha_asunto);
                if (entidad == null) return new BasicOperationResponse { IsSuccess = false, Message = "Ficha asunto no encontrado." };

                entidad.bit_estatus = true;
                db.SaveChanges();
                return new BasicOperationResponse { IsSuccess = true, Message = "Se activó el objetivo en el asunto satisfactoriamente." };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse { IsSuccess = false, Message = "Ocurrió un error al activar el asunto (" + e.Message + ")" };
            }
        }

        // Desactivar
        public BasicOperationResponse DisableObjetivoAsunto(int int_id_ficha_asunto)
        {
            try
            {
                var entidad = db.tb_FichaAsunto.FirstOrDefault(x => x.int_id_ficha_asunto == int_id_ficha_asunto);
                if (entidad == null) return new BasicOperationResponse { IsSuccess = false, Message = "Ficha asunto no encontrado." };

                entidad.bit_estatus = false;
                db.SaveChanges();
                return new BasicOperationResponse { IsSuccess = true, Message = "Se desactivó el objetivo en el asunto satisfactoriamente." };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse { IsSuccess = false, Message = "Ocurrió un error al desactivar el asunto (" + e.Message + ")" };
            }
        }

        public List<cat_EstatusProceso> GetEstatusProcesos(bool? actives)
        {
            bool activo = actives ?? true;
            return db.cat_EstatusProceso
                     .AsNoTracking()
                     .Where(x => x.bit_estatus == activo)
                     .OrderByDescending(x => x.date_fecha_creacion)
                     .ToList();
        }

        public List<cat_RolParticipacionAsunto> GetRolesParticipacionAsunto(bool? actives)
        {
            bool activo = actives ?? true;

            return db.cat_RolParticipacionAsunto
                     .AsNoTracking()
                     .Where(x => x.bit_estatus == activo)
                     .OrderBy(x => x.nvarchar_rol)
                     .ToList();
        }


        //MODULO DE ASUNTOS QUE TIENE EL OBJETIVO


        public List<tb_FichaAsunto> GetAsuntosByFichaObjetivo(int int_id_ficha_objetivo, bool? active)
        {
            return db.tb_FichaAsunto
                     .Include(x => x.tb_AsuntoRelacionado)
                     .Include(x => x.cat_RolParticipacionAsunto)
                     .AsNoTracking()
                     .Where(x => x.int_id_ficha_objetivo == int_id_ficha_objetivo
                              && (active == null || x.bit_estatus == active))
                     .OrderByDescending(x => x.date_fecha_creacion)
                     .ToList();
        }

        public List<tb_AsuntoRelacionado> BuscarAsuntosParaRelacionar(string texto, bool? active)
        {
            bool activo = active ?? true;
            texto = (texto ?? "").Trim();

            var query = db.tb_AsuntoRelacionado
                          .AsNoTracking()
                          .Where(x => x.bit_estatus == activo);

            if (!string.IsNullOrWhiteSpace(texto))
            {
                query = query.Where(x =>
                    (x.nvarchar_alias != null && x.nvarchar_alias.Contains(texto)) ||
                    (x.nvarchar_descripcion != null && x.nvarchar_descripcion.Contains(texto)) ||
                    (x.numavp != null && x.numavp.Contains(texto))
                );
            }

            return query
                .OrderByDescending(x => x.date_fecha_creacion)
                .Take(50)
                .ToList();
        }


        public BasicOperationResponse CrearAsuntoYRelacionarObjetivo(
            tb_AsuntoRelacionado asunto,
            int int_id_objetivo,
            string observacionesRelacion,
            int? int_id_rol_participacion = null,
            string descripcionParticipacion = null)
        {
            try
            {
                if (asunto == null)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "Datos inválidos del asunto."
                    };
                }

                if (string.IsNullOrWhiteSpace(asunto.nvarchar_alias))
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "El alias del asunto es obligatorio."
                    };
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        asunto.date_fecha_creacion = DateTime.Now;
                        asunto.bit_estatus = true;

                        db.tb_AsuntoRelacionado.Add(asunto);
                        db.SaveChanges();

                        int idAsunto = asunto.int_id_asunto_relacionado;

                        var ficha = db.tb_FichaObjetivo
                                      .FirstOrDefault(x => x.int_id_objetivo == int_id_objetivo);

                        if (ficha == null)
                        {
                            transaction.Rollback();

                            return new BasicOperationResponse
                            {
                                IsSuccess = false,
                                Message = "No se encontró la ficha del objetivo para relacionar el asunto."
                            };
                        }

                        var relacion = db.tb_FichaAsunto
                                         .FirstOrDefault(x =>
                                             x.int_id_ficha_objetivo == ficha.int_id_ficha_objetivo &&
                                             x.int_id_asunto_relacionado == idAsunto);

                        if (relacion == null)
                        {
                            relacion = new tb_FichaAsunto
                            {
                                int_id_ficha_objetivo = ficha.int_id_ficha_objetivo,
                                int_id_asunto_relacionado = idAsunto,
                                int_id_rol_participacion = int_id_rol_participacion,
                                nvarchar_descripcion_participacion = descripcionParticipacion,
                                nvarchar_observaciones = observacionesRelacion,
                                date_fecha_creacion = DateTime.Now,
                                bit_estatus = true
                            };

                            db.tb_FichaAsunto.Add(relacion);
                        }
                        else
                        {
                            relacion.nvarchar_observaciones = observacionesRelacion;
                            relacion.int_id_rol_participacion = int_id_rol_participacion;
                            relacion.nvarchar_descripcion_participacion = descripcionParticipacion;
                            relacion.bit_estatus = true;
                        }

                        db.SaveChanges();
                        transaction.Commit();

                        return new BasicOperationResponse
                        {
                            IsSuccess = true,
                            Message = "Asunto creado y vinculado correctamente al objetivo.",
                            Id = idAsunto
                        };
                    }
                    catch (Exception exTrans)
                    {
                        transaction.Rollback();

                        return new BasicOperationResponse
                        {
                            IsSuccess = false,
                            Message = "Error durante la transacción: " + exTrans.Message
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al crear y vincular el asunto: " + ex.Message
                };
            }
        }


        public BasicOperationResponse RelacionarAsuntoObjetivo(
    int int_id_objetivo,
    int int_id_asunto_relacionado,
    int? int_id_rol_participacion,
    string nvarchar_descripcion_participacion,
    string observaciones)
        {
            var response = new BasicOperationResponse();

            try
            {
                var fichaObjetivo = db.tb_FichaObjetivo
                    .FirstOrDefault(x => x.int_id_objetivo == int_id_objetivo && x.bit_estatus == true);

                if (fichaObjetivo == null)
                {
                    response.IsSuccess = false;
                    response.Message = "No se encontró la ficha del objetivo.";
                    return response;
                }

                var relacionExistente = db.tb_FichaAsunto
                    .FirstOrDefault(x =>
                        x.int_id_ficha_objetivo == fichaObjetivo.int_id_ficha_objetivo &&
                        x.int_id_asunto_relacionado == int_id_asunto_relacionado);

                if (relacionExistente != null)
                {
                    if (relacionExistente.bit_estatus == true)
                    {
                        response.IsSuccess = false;
                        response.Message = "El asunto ya se encuentra relacionado con este objetivo.";
                        return response;
                    }

                    relacionExistente.bit_estatus = true;
                    relacionExistente.int_id_rol_participacion = int_id_rol_participacion;
                    relacionExistente.nvarchar_descripcion_participacion = nvarchar_descripcion_participacion;
                    relacionExistente.nvarchar_observaciones = observaciones;

                    db.SaveChanges();

                    response.IsSuccess = true;
                    response.Message = "El asunto fue relacionado correctamente.";
                    return response;
                }

                var nuevaRelacion = new tb_FichaAsunto
                {
                    int_id_ficha_objetivo = fichaObjetivo.int_id_ficha_objetivo,
                    int_id_asunto_relacionado = int_id_asunto_relacionado,
                    int_id_rol_participacion = int_id_rol_participacion,
                    nvarchar_descripcion_participacion = nvarchar_descripcion_participacion,
                    nvarchar_observaciones = observaciones,
                    bit_estatus = true,
                    date_fecha_creacion = DateTime.Now
                };

                db.tb_FichaAsunto.Add(nuevaRelacion);
                db.SaveChanges();

                response.IsSuccess = true;
                response.Message = "El asunto fue relacionado correctamente.";
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Ocurrió un error al relacionar el asunto: " + ex.Message;
                return response;
            }
        }


        public BasicOperationResponse EditarParticipacionObjetivoAsunto(
    int int_id_ficha_asunto,
    int? int_id_rol_participacion,
    string descripcionParticipacion,
    string observaciones)
        {
            try
            {
                var relacion = db.tb_FichaAsunto
                    .FirstOrDefault(x => x.int_id_ficha_asunto == int_id_ficha_asunto);

                if (relacion == null)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No se encontró la relación del objetivo con el asunto."
                    };
                }

                relacion.int_id_rol_participacion = int_id_rol_participacion;
                relacion.nvarchar_descripcion_participacion = descripcionParticipacion;
                relacion.nvarchar_observaciones = observaciones;

                db.SaveChanges();

                return new BasicOperationResponse
                {
                    IsSuccess = true,
                    Message = "Se actualizó la participación del objetivo en el asunto correctamente."
                };
            }
            catch (Exception ex)
            {
                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al actualizar la participación: " + ex.Message
                };
            }
        }



    }
}