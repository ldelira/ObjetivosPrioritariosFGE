using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.ControllersServices
{
    public class ObjetivoService : BaseService
    {

        public tb_Objetivo addObjetivoFicha()
        {
            try
            {
                var objetivo = new tb_Objetivo
                {
                    date_fecha_creacion = DateTime.Now,
                    nvarchar_usuario_creacion = "delira",
                    bit_estatus = true
                };

                var ficha = new tb_FichaObjetivo
                {
                    tb_Objetivo = objetivo, // se asocia directamente la entidad
                    bit_estatus = true
                };

                db.tb_Objetivo.Add(objetivo);
                db.tb_FichaObjetivo.Add(ficha);
                db.SaveChanges();

                return objetivo;
            }
            catch (Exception ex)
            {
                // Aquí puedes registrar el error en un log si tienes una función personalizada
                // EscribirLog("Error en addObjetivoFicha: " + ex.Message);

                throw new Exception("Ocurrió un error al agregar el objetivo y la ficha.", ex);
            }
        }

        public List<getListObjetivosPrioritarios_Result> getObjetivosList(bool? actives)
        {
            return db.getListObjetivosPrioritarios(actives).ToList();
        }
        public List<getListFichaObjetivos_Result> getFichaObjetivosList(bool? actives)
        {
            return db.getListFichaObjetivos(actives).ToList();
        }
        public tb_Objetivo getObjetivoById(int int_id_objetivo)
        {
            return db.tb_Objetivo.AsNoTracking().Where(x => x.int_id_objetivo == int_id_objetivo).FirstOrDefault();
        }
        public List<tb_NombreObjetivo> getNombreObetivoList(bool? active, int? int_id_objetivo)
        {

            return db.tb_NombreObjetivo.AsNoTracking().Where(x => x.int_id_objetivo == int_id_objetivo && x.bit_estatus == active).ToList();
        }

        public BasicOperationResponse DisableObjetivo(int int_id_objetivo)
        {
            try
            {
                var nombreSearch = db.tb_Objetivo.Where(x => x.int_id_objetivo == int_id_objetivo).FirstOrDefault();
                nombreSearch.bit_estatus = false;
                db.SaveChanges();
                return new BasicOperationResponse() { IsSuccess = true, Message = "Se desactivó el objetivo satisfactoriamente" };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse() { IsSuccess = false, Message = "A ocurrido un error al desactivar el objetivo (" + e.Message + ")" };
            }
        }
        public BasicOperationResponse ActivateObjetivo(int int_id_objetivo)
        {
            try
            {
                var nombreSearch = db.tb_Objetivo.Where(x => x.int_id_objetivo == int_id_objetivo).FirstOrDefault();
                nombreSearch.bit_estatus = true;
                db.SaveChanges();
                return new BasicOperationResponse() { IsSuccess = true, Message = "Se activó el objetivo satisfactoriamente" };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse() { IsSuccess = false, Message = "A ocurrido un error al activar el objetivo (" + e.Message + ")" };
            }
        }

        public BasicOperationResponse addNombreObjetivo(tb_NombreObjetivo nombreObjetivo)
        {

            bool primerRegistro = !ExisteIdObjetivo(nombreObjetivo.int_id_objetivo);
            try
            {
                nombreObjetivo.date_fecha_creacion = DateTime.Now;
                nombreObjetivo.bit_estatus = true;
                nombreObjetivo.nvarchar_usuario_creacion = "delira";
                nombreObjetivo.bit_principal = primerRegistro;
                db.tb_NombreObjetivo.Add(nombreObjetivo);
                db.SaveChanges();

                var contNombre = db.tb_NombreObjetivo.Where(x => x.int_id_objetivo == nombreObjetivo.int_id_objetivo).Count();
                return new BasicOperationResponse() { IsSuccess = true, Message = "Se agrego la nombre satisfactoriamente", ExtraData=nombreObjetivo.NombreCompleto,Id= contNombre };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse() { IsSuccess = false, Message = "A ocurrido un error al agregar el nombre (" + e.Message + ")" };
            }
        }

        public BasicOperationResponse addEditPhoto(HttpPostedFileBase foto, DateTime fechaNacimiento, int int_id_objetivo)
        {
            try
            {
                string base64String = null;
                if (foto != null && foto.ContentLength > 0)
                {

                    using (var ms = new MemoryStream())
                    {
                        foto.InputStream.CopyTo(ms);
                        byte[] fileBytes = ms.ToArray();

                        // Convertir a Base64
                        base64String = Convert.ToBase64String(fileBytes);
                    }

                    var busqueda = db.tb_Objetivo.Where(x => x.int_id_objetivo == int_id_objetivo).FirstOrDefault();
                    busqueda.date_fecha_nacimiento = fechaNacimiento;
                    busqueda.nvarchar_foto = base64String;
                }


                db.SaveChanges();
                return new BasicOperationResponse() { IsSuccess = true, Message = "Se guardo la foto y fecha de nacimiento satisfactoriamente" };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse() { IsSuccess = false, Message = "A ocurrido un error al guardar la foto y fecha de nacimiento (" + e.Message + ")" };
            }
        }


        public tb_NombreObjetivo getNombreById(int int_id_nombre)
        {
            return db.tb_NombreObjetivo.Where(x => x.int_id_nombre == int_id_nombre).FirstOrDefault();
        }

        public BasicOperationResponse editNombreObjetivo(tb_NombreObjetivo nombreObjetivo)
        {
            try
            {
                var nombreSearch = db.tb_NombreObjetivo.Where(x => x.int_id_nombre == nombreObjetivo.int_id_nombre).FirstOrDefault();
                nombreSearch.nvarchar_nombre = nombreObjetivo.nvarchar_nombre;
                nombreSearch.nvarchar_paterno = nombreObjetivo.nvarchar_paterno;
                nombreSearch.nvarchar_materno = nombreObjetivo.nvarchar_materno;

                db.SaveChanges();
                return new BasicOperationResponse() { IsSuccess = true, Message = "Se actualizo el nombre satisfactoriamente" };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse() { IsSuccess = false, Message = "A ocurrido un error al editar el nombre (" + e.Message + ")" };
            }
        }

        public BasicOperationResponse diasbleNombre(int int_id_nombre)
        {
            try
            {
                var nombreSearch = db.tb_NombreObjetivo.Where(x => x.int_id_nombre == int_id_nombre).FirstOrDefault();
                nombreSearch.bit_estatus = false;
                db.SaveChanges();
                return new BasicOperationResponse() { IsSuccess = true, Message = "Se desactivó el nombre satisfactoriamente" };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse() { IsSuccess = false, Message = "A ocurrido un error al desactivar el nombre (" + e.Message + ")" };
            }
        }
        public BasicOperationResponse activatNombre(int int_id_nombre)
        {
            try
            {
                var nombreSearch = db.tb_NombreObjetivo.Where(x => x.int_id_nombre == int_id_nombre).FirstOrDefault();
                nombreSearch.bit_estatus = true;
                db.SaveChanges();
                return new BasicOperationResponse() { IsSuccess = true, Message = "Se activó el nombre satisfactoriamente" };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse() { IsSuccess = false, Message = "A ocurrido un error al activar el nombre(" + e.Message + ")" };
            }
        }

        public List<tb_AliasObjetivo> getAliasObjetivoList(bool? active, int? int_id_objetivo)
        {
            var lista = db.tb_AliasObjetivo
                .AsNoTracking()
                .Where(x => x.int_id_objetivo == int_id_objetivo && x.bit_estatus == active)
                .ToList();

            if (lista == null || lista.Count == 0)
            {
                return new List<tb_AliasObjetivo>
        {
            new tb_AliasObjetivo
            {
                nvarchar_alias = "No hay alias asignados"
            }
        };
            }

            return lista;
        }


        public BasicOperationResponse addAliasObjetivo(tb_AliasObjetivo aliasObjetivo)
        {
            try
            {
                aliasObjetivo.date_fecha_creacion = DateTime.Now;
                aliasObjetivo.bit_estatus = true;
                aliasObjetivo.nvarchar_usuario_creacion = "delira";
                db.tb_AliasObjetivo.Add(aliasObjetivo);
                db.SaveChanges();
                return new BasicOperationResponse() { IsSuccess = true, Message = "Se agregó el alias satisfactoriamente" };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse() { IsSuccess = false, Message = "Ha ocurrido un error al agregar el alias (" + e.Message + ")" };
            }
        }

        public tb_AliasObjetivo getAliasById(int int_id_alias)
        {
            return db.tb_AliasObjetivo
                .Where(x => x.int_id_alias == int_id_alias)
                .FirstOrDefault();
        }

        public BasicOperationResponse editAliasObjetivo(tb_AliasObjetivo aliasObjetivo)
        {
            try
            {
                var aliasSearch = db.tb_AliasObjetivo
                    .Where(x => x.int_id_alias == aliasObjetivo.int_id_alias)
                    .FirstOrDefault();

                aliasSearch.nvarchar_alias = aliasObjetivo.nvarchar_alias;

                db.SaveChanges();
                return new BasicOperationResponse() { IsSuccess = true, Message = "Se actualizó el alias satisfactoriamente" };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse() { IsSuccess = false, Message = "Ha ocurrido un error al editar el alias (" + e.Message + ")" };
            }
        }

        public BasicOperationResponse disableAlias(int int_id_alias)
        {
            try
            {
                var aliasSearch = db.tb_AliasObjetivo
                    .Where(x => x.int_id_alias == int_id_alias)
                    .FirstOrDefault();

                aliasSearch.bit_estatus = false;
                db.SaveChanges();
                return new BasicOperationResponse() { IsSuccess = true, Message = "Se desactivó el alias satisfactoriamente" };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse() { IsSuccess = false, Message = "Ha ocurrido un error al desactivar el alias (" + e.Message + ")" };
            }
        }

        public BasicOperationResponse activateAlias(int int_id_alias)
        {
            try
            {
                var aliasSearch = db.tb_AliasObjetivo
                    .Where(x => x.int_id_alias == int_id_alias)
                    .FirstOrDefault();

                aliasSearch.bit_estatus = true;
                db.SaveChanges();
                return new BasicOperationResponse() { IsSuccess = true, Message = "Se activó el alias satisfactoriamente" };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse() { IsSuccess = false, Message = "Ha ocurrido un error al activar el alias (" + e.Message + ")" };
            }
        }


        public List<tb_InformacionObjetivo> getDomicilioObjetivoList(bool? active, int? int_id_objetivo)
        {
            return db.tb_InformacionObjetivo
                .AsNoTracking()
                .Where(x => x.int_id_objetivo == int_id_objetivo && x.bit_estatus == active)
                .ToList();
        }
        public List<getInfoObjetivoList_Result> getInfoObjetivoList(bool? active, int? int_id_objetivo)
        {
            return db.getInfoObjetivoList(int_id_objetivo, active).ToList();
        }
        public BasicOperationResponse addDomicilioObjetivo(tb_InformacionObjetivo domicilioObjetivo)
        {
            try
            {
                domicilioObjetivo.date_fecha_creacion = DateTime.Now;
                domicilioObjetivo.bit_estatus = true;
                domicilioObjetivo.nvarchar_usuario_creacion = "delira"; // TODO: cambiar por usuario actual si aplica
                db.tb_InformacionObjetivo.Add(domicilioObjetivo);
                db.SaveChanges();
                return new BasicOperationResponse() { IsSuccess = true, Message = "Se agregó el domicilio satisfactoriamente" };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse() { IsSuccess = false, Message = "Ha ocurrido un error al agregar el domicilio (" + e.Message + ")" };
            }
        }

        public tb_InformacionObjetivo getDomicilioById(int int_id_informacion)
        {
            return db.tb_InformacionObjetivo
                .Where(x => x.int_id_informacion == int_id_informacion)
                .FirstOrDefault();
        }

        public getInfoObjetivoListById_Result getInfoObjetivoListById(int int_id_informacion)
        {
            return db.getInfoObjetivoListById(int_id_informacion).FirstOrDefault();
        }

        public BasicOperationResponse editDomicilioObjetivo(tb_InformacionObjetivo domicilioObjetivo)
        {
            try
            {
                var domicilioSearch = db.tb_InformacionObjetivo
                    .Where(x => x.int_id_informacion == domicilioObjetivo.int_id_informacion)
                    .FirstOrDefault();

                if (domicilioSearch == null)
                    return new BasicOperationResponse() { IsSuccess = false, Message = "El domicilio no existe" };


                domicilioSearch.Cve_Calle = domicilioObjetivo.Cve_Calle;
                domicilioSearch.nvarchar_no_casa = domicilioObjetivo.nvarchar_no_casa;
                domicilioSearch.nvarchar_observaciones = domicilioObjetivo.nvarchar_observaciones;
                domicilioSearch.nvarchar_cp = domicilioObjetivo.nvarchar_cp;
                db.SaveChanges();
                return new BasicOperationResponse() { IsSuccess = true, Message = "Se actualizó el domicilio satisfactoriamente" };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse() { IsSuccess = false, Message = "Ha ocurrido un error al editar el domicilio (" + e.Message + ")" };
            }
        }

        public BasicOperationResponse disableDomicilio(int int_id_informacion)
        {
            try
            {
                var domicilioSearch = db.tb_InformacionObjetivo
                    .Where(x => x.int_id_informacion == int_id_informacion)
                    .FirstOrDefault();

                if (domicilioSearch == null)
                    return new BasicOperationResponse() { IsSuccess = false, Message = "El domicilio no existe" };

                domicilioSearch.bit_estatus = false;
                db.SaveChanges();
                return new BasicOperationResponse() { IsSuccess = true, Message = "Se desactivó el domicilio satisfactoriamente" };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse() { IsSuccess = false, Message = "Ha ocurrido un error al desactivar el domicilio (" + e.Message + ")" };
            }
        }

        public BasicOperationResponse activateDomicilio(int int_id_informacion)
        {
            try
            {
                var domicilioSearch = db.tb_InformacionObjetivo
                    .Where(x => x.int_id_informacion == int_id_informacion)
                    .FirstOrDefault();

                if (domicilioSearch == null)
                    return new BasicOperationResponse() { IsSuccess = false, Message = "El domicilio no existe" };

                domicilioSearch.bit_estatus = true;
                db.SaveChanges();
                return new BasicOperationResponse() { IsSuccess = true, Message = "Se activó el domicilio satisfactoriamente" };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse() { IsSuccess = false, Message = "Ha ocurrido un error al activar el domicilio (" + e.Message + ")" };
            }
        }




        // Obtener lista de grupos asociados a un objetivo
        public List<tb_ObjetivoGrupo> GetObjetivoGrupoList(bool? active, int? int_id_objetivo)
        {
            return db.tb_ObjetivoGrupo
                     .AsNoTracking()
                     .Where(x => x.int_id_objetivo == int_id_objetivo
                                 && (active == null || x.bit_estatus == active))
                     .ToList();
        }

        // Agregar un grupo delictivo a un objetivo
        public BasicOperationResponse AddObjetivoGrupo(tb_ObjetivoGrupo objetivoGrupo)
        {
            try
            {
                var busqueda = db.tb_ObjetivoGrupo.Where(x => x.int_id_grupo == objetivoGrupo.int_id_grupo && x.int_id_objetivo == objetivoGrupo.int_id_objetivo && x.bit_estatus == false).FirstOrDefault();
                if (busqueda == null)
                {
                    objetivoGrupo.date_fecha_creacion = DateTime.Now;
                    objetivoGrupo.bit_estatus = true;

                    db.tb_ObjetivoGrupo.Add(objetivoGrupo);
                }
                else
                {
                    busqueda.bit_estatus = true;
                    busqueda.date_fecha_ingreso = objetivoGrupo.date_fecha_ingreso;
                    busqueda.date_fecha_salida = objetivoGrupo.date_fecha_salida;
                    busqueda.nvarchar_observaciones = objetivoGrupo.nvarchar_observaciones;

                }
                db.SaveChanges();

                return new BasicOperationResponse()
                {
                    IsSuccess = true,
                    Message = "Se agregó el grupo delictivo al objetivo satisfactoriamente"
                };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse()
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al agregar el grupo delictivo (" + e.Message + ")"
                };
            }
        }

        // Obtener un registro específico por ID
        public tb_ObjetivoGrupo GetObjetivoGrupoById(int int_id_objetivo_grupo)
        {
            return db.tb_ObjetivoGrupo
                     .FirstOrDefault(x => x.int_id_objetivo_grupo == int_id_objetivo_grupo);
        }

        // Editar un grupo delictivo asignado
        public BasicOperationResponse EditObjetivoGrupo(tb_ObjetivoGrupo objetivoGrupo)
        {
            try
            {
                var registro = db.tb_ObjetivoGrupo
                                 .FirstOrDefault(x => x.int_id_objetivo_grupo == objetivoGrupo.int_id_objetivo_grupo);

                if (registro == null)
                    return new BasicOperationResponse() { IsSuccess = false, Message = "No se encontró el registro" };

                registro.int_id_grupo = objetivoGrupo.int_id_grupo;
                registro.date_fecha_ingreso = objetivoGrupo.date_fecha_ingreso;
                registro.date_fecha_salida = objetivoGrupo.date_fecha_salida;
                registro.nvarchar_observaciones = objetivoGrupo.nvarchar_observaciones;

                db.SaveChanges();

                return new BasicOperationResponse()
                {
                    IsSuccess = true,
                    Message = "Se actualizó el grupo delictivo del objetivo satisfactoriamente"
                };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse()
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al editar el grupo delictivo (" + e.Message + ")"
                };
            }
        }

        // Desactivar
        public BasicOperationResponse DisableObjetivoGrupo(int int_id_objetivo_grupo)
        {
            try
            {
                var registro = db.tb_ObjetivoGrupo
                                 .FirstOrDefault(x => x.int_id_objetivo_grupo == int_id_objetivo_grupo);

                if (registro == null)
                    return new BasicOperationResponse() { IsSuccess = false, Message = "No se encontró el registro" };

                registro.bit_estatus = false;
                db.SaveChanges();

                return new BasicOperationResponse()
                {
                    IsSuccess = true,
                    Message = "Se desactivó el grupo delictivo del objetivo"
                };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse()
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al desactivar (" + e.Message + ")"
                };
            }
        }

        // Activar
        public BasicOperationResponse ActivateObjetivoGrupo(int int_id_objetivo_grupo)
        {
            try
            {
                var registro = db.tb_ObjetivoGrupo
                                 .FirstOrDefault(x => x.int_id_objetivo_grupo == int_id_objetivo_grupo);

                if (registro == null)
                    return new BasicOperationResponse() { IsSuccess = false, Message = "No se encontró el registro" };

                registro.bit_estatus = true;
                db.SaveChanges();

                return new BasicOperationResponse()
                {
                    IsSuccess = true,
                    Message = "Se activó el grupo delictivo del objetivo"
                };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse()
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al activar (" + e.Message + ")"
                };
            }
        }



        public List<SP_ObjPri_getListinfogeneral_Result> GetInfoGeneralList(string nombre, string paterno, string materno)
        {
            return dbMand.SP_ObjPri_getListinfogeneral(nombre, paterno, materno).ToList();
        }

        public BasicOperationResponse addInfoGeneralObjetivo(tb_InformacionObjetivo infoGeneralObjetivo)
        {
            try
            {
                infoGeneralObjetivo.date_fecha_creacion = DateTime.Now;
                infoGeneralObjetivo.bit_estatus = true;
                infoGeneralObjetivo.nvarchar_usuario_creacion = "apadillaa";

                db.tb_InformacionObjetivo.Add(infoGeneralObjetivo);
                db.SaveChanges();

                return new BasicOperationResponse
                {
                    IsSuccess = true,
                    Message = "Se agregó la información general satisfactoriamente"
                };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Ha ocurrido un error al agregar la información general (" + e.Message + ")"
                };
            }
        }

        public BasicOperationResponse addInfoGeneralObjetivoNombre(tb_NombreObjetivo NombreObjetivo)
        {
            bool primerRegistro = !ExisteIdObjetivo(NombreObjetivo.int_id_objetivo);
            try
            {
                NombreObjetivo.date_fecha_creacion = DateTime.Now;
                NombreObjetivo.bit_estatus = true;
                NombreObjetivo.nvarchar_usuario_creacion = "apadillaa";
                NombreObjetivo.bit_principal = primerRegistro;
                db.tb_NombreObjetivo.Add(NombreObjetivo);
                db.SaveChanges();

                return new BasicOperationResponse
                {
                    IsSuccess = true,
                    Message = "Se agregó la información general satisfactoriamente"
                };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Ha ocurrido un error al agregar la información general (" + e.Message + ")"
                };
            }
        }

        public BasicOperationResponse addInfoGeneralObjetivoAlias(tb_AliasObjetivo AliasObjetivo)
        {
            try
            {
                AliasObjetivo.date_fecha_creacion = DateTime.Now;
                AliasObjetivo.bit_estatus = true;
                AliasObjetivo.nvarchar_usuario_creacion = "apadillaa";

                db.tb_AliasObjetivo.Add(AliasObjetivo);
                db.SaveChanges();

                return new BasicOperationResponse
                {
                    IsSuccess = true,
                    Message = "Se agregó la información general satisfactoriamente"
                };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Ha ocurrido un error al agregar la información general (" + e.Message + ")"
                };
            }
        }

        public bool ExisteClavePerso(int ClavePerso)
        {
            // Retorna true si existe en cualquiera de las tablas
            return db.tb_NombreObjetivo.Any(x => x.int_clave_perso == ClavePerso);
        }

        public bool ExisteIdObjetivo(int Id_Objetivo)
        {
            // Retorna true si existe en cualquiera de las tablas
            return db.tb_NombreObjetivo.Any(x => x.int_id_objetivo == Id_Objetivo);
        }

        public BasicOperationResponse MarcarNombreComoPrincipal(int IdNombre)
        {
            try
            {
                var PrincipalObjetivo = db.tb_NombreObjetivo.FirstOrDefault(x => x.int_id_nombre == IdNombre);

                if (PrincipalObjetivo == null)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "No se encontró el registro del nombre objetivo."
                    };
                }
                var registros = db.tb_NombreObjetivo
                    .Where(x => x.int_id_objetivo == PrincipalObjetivo.int_id_objetivo)
                    .ToList();
                registros.ForEach(x => x.bit_principal = false);
                PrincipalObjetivo.bit_principal = true;

                db.SaveChanges();

                return new BasicOperationResponse
                {
                    IsSuccess = true,
                    Message = "Se marcó como principal correctamente.",
                    ExtraData= PrincipalObjetivo.NombreCompleto
                };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Error al marcar como principal (" + e.Message + ")"
                };
            }
        }

        public BasicOperationResponse GuardarNuevaFoto(int IdObjetivo, string Foto)
        {
            try
            {
                // Buscar el objetivo por ID
                var objetivo = db.tb_Objetivo.FirstOrDefault(x => x.int_id_objetivo == IdObjetivo);

                // Guardar la cadena Base64 en el campo correspondiente
                objetivo.nvarchar_foto = Foto; // Asegúrate de que este campo exista en la entidad y en la BD

                db.SaveChanges();

                return new BasicOperationResponse
                {
                    IsSuccess = true,
                    Message = "La foto fue guardada correctamente en el objetivo."
                };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Error al guardar la foto (" + e.Message + ")"
                };
            }
        }

        public string ObtenerNombreCompletoPrincipal(int? idObjetivo)
        {
            return db.tb_NombreObjetivo
                .Where(x => x.int_id_objetivo == idObjetivo && x.bit_principal == true)
                .Select(x => x.nvarchar_nombre + " " + x.nvarchar_paterno + " " + x.nvarchar_materno)
                .FirstOrDefault();
        }

        public List<tb_AlbumFichaObjetivo> getAlbumFichaObjetivo(bool active)
        {
            return db.tb_AlbumFichaObjetivo.AsNoTracking().Where(x => x.bit_estatus == active).ToList();
        }

        public List<string> ObtenerNombreCompletoSecundarios(int? idObjetivo)
        {
            var nombres = db.tb_NombreObjetivo
                .Where(x => x.int_id_objetivo == idObjetivo && x.bit_principal == false)
                .Select(x => x.nvarchar_nombre + " " + x.nvarchar_paterno + " " + x.nvarchar_materno)
                .ToList();

            if (nombres == null || nombres.Count == 0)
            {
                return new List<string> { "No hay más nombres asignados" };
            }

            return nombres;
        }


        public int GetIdFichaObjetivo(int idobjetivo)
        {
            var idficha = db.tb_FichaObjetivo
                            .AsNoTracking()
                            .Where(x => x.int_id_objetivo == idobjetivo)
                            .OrderByDescending(x => x.int_id_ficha_objetivo)
                            .Select(x => (int?)x.int_id_ficha_objetivo)
                            .FirstOrDefault() ?? 0;
            return idficha;
        }

        public string GetEstatusObjetivo(int? idficha)
        {
            var idestatus = db.tb_FichaObjetivo
                            .AsNoTracking()
                            .Where(x => x.int_id_ficha_objetivo == idficha)
                            .OrderByDescending(x => x.int_id_ficha_objetivo)
                            .Select(x => (int?)x.int_id_estatus_proceso)
                            .FirstOrDefault() ?? 0;

            var estatus = db.cat_EstatusProceso
                            .AsNoTracking()
                            .Where(x => x.int_id_estatus_proceso == idestatus)
                            .OrderByDescending(x => x.int_id_estatus_proceso)
                            .Select(x => x.nvarchar_estatus)
                            .FirstOrDefault() ?? "";
            return estatus;
        }

        public string GetObservacionObjetivo(int? idficha)
        {
            var observacion = db.tb_FichaObjetivo
                     .AsNoTracking()
                     .Where(x => x.int_id_ficha_objetivo == idficha)
                     .OrderByDescending(x => x.int_id_ficha_objetivo)
                     .Select(x => x.nvarchar_observaciones)
                     .FirstOrDefault() ?? "";

            return observacion;
        }

        public string GetDescripcionEstatusObjetivo(int? idficha)
        {
            var descripcion = db.tb_FichaObjetivo
                     .AsNoTracking()
                     .Where(x => x.int_id_ficha_objetivo == idficha)
                     .OrderByDescending(x => x.int_id_ficha_objetivo)
                     .Select(x => x.nvarchar_descripcion_estatus)
                     .FirstOrDefault() ?? "";

            return descripcion;
        }


        public List<string> GetGruposDelictivosObjetivo(int? idobjetivo)
        {
            if (idobjetivo == null)
                return new List<string> { "No hay grupos asignados" };

            // 1. Obtener todos los IDs de grupos asociados al objetivo
            var idsGrupo = db.tb_ObjetivoGrupo
                .AsNoTracking()
                .Where(x => x.int_id_objetivo == idobjetivo)
                .Select(x => x.int_id_grupo)
                .ToList();

            if (idsGrupo == null || idsGrupo.Count == 0)
                return new List<string> { "No hay grupos asignados" };

            // 2. Obtener los nombres de los grupos
            var nombresGrupos = db.tb_Grupo_Delictivo
                .AsNoTracking()
                .Where(g => idsGrupo.Contains(g.int_id_grupo))
                .OrderBy(g => g.nvarchar_grupo)
                .Select(g => g.nvarchar_grupo)
                .ToList();

            if (nombresGrupos == null || nombresGrupos.Count == 0)
                return new List<string> { "No hay grupos asignados" };

            return nombresGrupos;
        }


        public string GetFotoObjetivo(int? idObjetivo)
        {
            var foto = db.tb_Objetivo
                     .AsNoTracking()
                     .Where(x => x.int_id_objetivo == idObjetivo)
                     .Select(x => x.nvarchar_foto)
                     .FirstOrDefault() ?? "";

            return foto;
        }

    }
}