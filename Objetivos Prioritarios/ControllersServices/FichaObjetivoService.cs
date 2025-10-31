using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.ControllersServices
{
    public class FichaObjetivoService : BaseService
    {

        public List<tb_CarpetasObjetivo> GetCarpetasList(bool? active, int? int_id_ficha_objetivo)
        {
            return db.tb_CarpetasObjetivo
                     .AsNoTracking()
                     .Where(x => x.int_id_ficha_objetivo == int_id_ficha_objetivo
                              && (active == null || x.bit_estatus == active))
                     .OrderByDescending(x => x.date_fecha_creacion)
                     .ToList();
        }

        // Alta basada en búsqueda en tabla externa
        public BasicOperationResponse AddCarpetaObjetivo(int int_id_ficha_objetivo,string numavp)
        {
            try
            {
                // Busca si ya existe en tb_CarpetasObjetivo
                var existente = db.tb_CarpetasObjetivo
                    .FirstOrDefault(x => x.int_id_ficha_objetivo == int_id_ficha_objetivo && x.numavp == numavp);

                // Si ya está activa, no la agrega
                if (existente != null && existente.bit_estatus)
                {
                
                    return new BasicOperationResponse()
                    {
                        IsSuccess = false,
                        Message = "La carpeta ya está activa en este objetivo."
                    };
                }

                // Si existe pero está desactivada, la reactivamos
                if (existente != null && !existente.bit_estatus)
                {
                    existente.bit_estatus = true;
                    db.SaveChanges();

                    return new BasicOperationResponse()
                    {
                        IsSuccess = true,
                        Message = "Se reactivó la carpeta existente."
                    };
                }

                // Busca en tabla fuente (ejemplo: tb_CarpetasInvestigacion)
                var carpetaOrigen = dbSIPJ.Sp_ObjPri_getCarpetasList(2,"","","",numavp);


                if (carpetaOrigen == null)
                {
                    return new BasicOperationResponse()
                    {
                        IsSuccess = false,
                        Message = "No se encontró la carpeta en el sistema general."
                    };
                }


                var carpetaFiltrada = carpetaOrigen
                .Select(c => new
                {
                    numavp = c.CarpetaInvestigacion,   // Carpeta de investigación
                    CveDelito = c.Cve_Delito,         // Clave de delito
                    Delito = c.Delito,                 // Nombre del delito
                    Observaciones = c.Obs,
                    FechaAlta=c.FechaAlta
                })
                .FirstOrDefault();


                // Crear nueva carpeta vinculada al objetivo
                var nueva = new tb_CarpetasObjetivo()
                {
                    int_id_ficha_objetivo = int_id_ficha_objetivo,
                    numavp = numavp,
                    Cve_Delito = Convert.ToInt32(carpetaFiltrada.CveDelito),
                    Delito = carpetaFiltrada.Delito,
                    nvarchar_observacion = carpetaFiltrada.Observaciones,
                    date_fecha_creacion = DateTime.Now,
                    bit_estatus = true,
                    date_fecha_alta_carpeta = carpetaFiltrada.FechaAlta
                };

                db.tb_CarpetasObjetivo.Add(nueva);
                db.SaveChanges();

                return new BasicOperationResponse()
                {
                    IsSuccess = true,
                    Message = "Carpeta agregada correctamente al objetivo."
                };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse()
                {
                    IsSuccess = false,
                    Message = "Error al agregar la carpeta (" + e.Message + ")"
                };
            }
        }

        // Desactivar
        public BasicOperationResponse DisableCarpeta(int id)
        {
            try
            {
                var registro = db.tb_CarpetasObjetivo.FirstOrDefault(x => x.int_id_carpeta_objetivo == id);
                if (registro == null)
                    return new BasicOperationResponse() { IsSuccess = false, Message = "No se encontró el registro" };

                registro.bit_estatus = false;
                db.SaveChanges();

                return new BasicOperationResponse()
                {
                    IsSuccess = true,
                    Message = "Se desactivó la carpeta correctamente."
                };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse()
                {
                    IsSuccess = false,
                    Message = "Error al desactivar (" + e.Message + ")"
                };
            }
        }

        // Activar
        public BasicOperationResponse ActivateCarpeta(int id)
        {
            try
            {
                var registro = db.tb_CarpetasObjetivo.FirstOrDefault(x => x.int_id_carpeta_objetivo == id);
                if (registro == null)
                    return new BasicOperationResponse() { IsSuccess = false, Message = "No se encontró el registro" };

                registro.bit_estatus = true;
                db.SaveChanges();

                return new BasicOperationResponse()
                {
                    IsSuccess = true,
                    Message = "Se activó la carpeta correctamente."
                };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse()
                {
                    IsSuccess = false,
                    Message = "Error al activar (" + e.Message + ")"
                };
            }
        }


        public tb_FichaObjetivo addorgetFichObjetivo(int int_id_objetivo)
        {
            var busqueda=db.tb_FichaObjetivo.AsNoTracking().Where(x=>x.int_id_objetivo== int_id_objetivo).FirstOrDefault();
            if (busqueda == null)
            {
                busqueda = new tb_FichaObjetivo()
                {
                    int_id_objetivo = int_id_objetivo,
                    bit_estatus = true,
                };
                db.tb_FichaObjetivo.Add(busqueda);
                db.SaveChanges();
            }
            return busqueda;

        }




        public List<Sp_ObjPri_getCarpetasList_Result> GetCarpetasNameNumAvpList(int movimiento,string nombre, string paterno, string materno, string numavp )
        {
            return dbSIPJ.Sp_ObjPri_getCarpetasList(movimiento,nombre,paterno,materno,numavp).ToList();
        }


        public List<sp_ObjPri_getObjetivoInfo_Result> GetObjetivosNameNumAvpList(int movimiento, string nombre, string paterno, string materno, string numavp)
        {
            return dbMand.sp_ObjPri_getObjetivoInfo(movimiento, nombre, paterno, materno, numavp,null).ToList();
        }



        public List<tb_OrdenAprehension> GetOrdenesList(bool? active, int? int_id_ficha_objetivo)
        {
            return db.tb_OrdenAprehension
                     .AsNoTracking()
                     .Where(x => x.int_id_ficha_objetivo == int_id_ficha_objetivo
                              && (active == null || x.bit_estatus == active))
                     .OrderByDescending(x => x.date_fecha_creacion)
                     .ToList();
        }

        // Alta basada en búsqueda en tabla externa
        public BasicOperationResponse AddOrdenObjetivo(int int_id_ficha_objetivo, int id_mandamiento)
        {
            try
            {
                // Busca si ya existe en tb_CarpetasObjetivo
                var existente = db.tb_OrdenAprehension
                    .FirstOrDefault(x => x.int_id_ficha_objetivo == int_id_ficha_objetivo && x.id_mandamiento_judicial == id_mandamiento);

                // Si ya está activa, no la agrega
                if (existente != null && existente.bit_estatus)
                {

                    return new BasicOperationResponse()
                    {
                        IsSuccess = false,
                        Message = "La orden ya está activa en este objetivo."
                    };
                }

                // Si existe pero está desactivada, la reactivamos
                if (existente != null && !existente.bit_estatus)
                {
                    existente.bit_estatus = true;
                    db.SaveChanges();

                    return new BasicOperationResponse()
                    {
                        IsSuccess = true,
                        Message = "Se reactivó la orden existente."
                    };
                }

                // Busca en tabla fuente (ejemplo: tb_CarpetasInvestigacion)
                var carpetaOrigen = dbMand.sp_ObjPri_getObjetivoInfo(3, "", "", "", "",id_mandamiento);


                if (carpetaOrigen == null)
                {
                    return new BasicOperationResponse()
                    {
                        IsSuccess = false,
                        Message = "No se encontró la orden en el sistema general."
                    };
                }


                var carpetaFiltrada = carpetaOrigen
                .Select(c => new
                {
                    id_delito = c.id_delito,         
                    delito = c.delito,                
                    id_estado_proceso = c.id_estado_proceso ,
                    tipo = c.tipo,
                    fecha_estatus=c.fecha_estatus_mandamiento
                })
                .FirstOrDefault();


                // Crear nueva carpeta vinculada al objetivo
                var nueva = new tb_OrdenAprehension()
                {
                    int_id_ficha_objetivo = int_id_ficha_objetivo,
                    id_mandamiento_judicial= id_mandamiento,
                    id_delito = Convert.ToInt32(carpetaFiltrada.id_delito),
                    delito = carpetaFiltrada.delito,
                    id_estado_proceso = carpetaFiltrada.id_estado_proceso,
                    tipo = carpetaFiltrada.tipo,
                    date_fecha_creacion = DateTime.Now,
                    bit_estatus = true,
                    date_fecha_estatus= carpetaFiltrada.fecha_estatus
                };

                db.tb_OrdenAprehension.Add(nueva);
                db.SaveChanges();

                return new BasicOperationResponse()
                {
                    IsSuccess = true,
                    Message = "Orden agregada correctamente al objetivo."
                };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse()
                {
                    IsSuccess = false,
                    Message = "Error al agregar la carpeta (" + e.Message + ")"
                };
            }
        }

        // Desactivar
        public BasicOperationResponse DisableOrden(int id)
        {
            try
            {
                var registro = db.tb_OrdenAprehension.FirstOrDefault(x => x.int_id_orden_aprehension == id);
                if (registro == null)
                    return new BasicOperationResponse() { IsSuccess = false, Message = "No se encontró el registro" };

                registro.bit_estatus = false;
                db.SaveChanges();

                return new BasicOperationResponse()
                {
                    IsSuccess = true,
                    Message = "Se desactivó la orden correctamente."
                };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse()
                {
                    IsSuccess = false,
                    Message = "Error al desactivar (" + e.Message + ")"
                };
            }
        }

        // Activar
        public BasicOperationResponse ActivateOrden(int id)
        {
            try
            {
                var registro = db.tb_OrdenAprehension.FirstOrDefault(x => x.int_id_orden_aprehension == id);
                if (registro == null)
                    return new BasicOperationResponse() { IsSuccess = false, Message = "No se encontró el registro" };

                registro.bit_estatus = true;
                db.SaveChanges();

                return new BasicOperationResponse()
                {
                    IsSuccess = true,
                    Message = "Se activó la orden correctamente."
                };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse()
                {
                    IsSuccess = false,
                    Message = "Error al activar (" + e.Message + ")"
                };
            }
        }

        public List<SP_ObjPri_getFiliObjetivoInfo_Result> GetFiliacionList(int movimiento, string nombre, string paterno, string materno, string numavp)
        {
            return dbFili.SP_ObjPri_getFiliObjetivoInfo(movimiento, nombre, paterno, materno, numavp, null).ToList();
        }

        // 🔸 Listar delitos asociados a una ficha
        public List<getListFiliacionRelacionada_Result> GetDelitosList(bool active, int int_id_ficha_objetivo)
        {
            return db.getListFiliacionRelacionada(int_id_ficha_objetivo,active).ToList();
        }

        // 🔸 Alta basada en búsqueda en sistema de Filiación (SP)
        public BasicOperationResponse AddDelitoFiliacion(int int_id_ficha_objetivo, int clave_persona)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var busquedaDetenido = db.tb_Detenido.AsNoTracking().Where(x => x.int_id_ficha_objetivo == int_id_ficha_objetivo && x.clave_persona == clave_persona).FirstOrDefault();
                    // Verifica si ya existe el registro en la tabla
                    if (busquedaDetenido != null)
                    {
                        int int_id_detenido = busquedaDetenido.int_id_detenido;
                        var existente = db.tb_Detenidos_DelitoIngreso
                            .FirstOrDefault(x => x.int_id_detenido == int_id_detenido);

                        if (existente != null && existente.bit_estatus)
                        {
                            return new BasicOperationResponse()
                            {
                                IsSuccess = false,
                                Message = "El delito ya está activo en este objetivo."
                            };
                        }

                        if (existente != null && !existente.bit_estatus)
                        {
                            existente.bit_estatus = true;
                            db.SaveChanges();

                            return new BasicOperationResponse()
                            {
                                IsSuccess = true,
                                Message = "Se reactivó el registro existente."
                            };
                        }
                    }
                    // 🔹 Consulta la información en sistema de Filiación
                    var infoFiliacion = dbFili.SP_ObjPri_getFiliObjetivoClavePersona(clave_persona).ToList();

                    if (infoFiliacion == null)
                    {
                        return new BasicOperationResponse()
                        {
                            IsSuccess = false,
                            Message = "No se encontró la información en el sistema de Filiación."
                        };
                    }

                    //🔸 Mapeo desde SP a la tabla local tb_Detenidos_DelitoIngreso


                    foreach (var item in infoFiliacion)
                    {
                        // Validar/parsear hora
                        TimeSpan horaDetencion;
                        if (!TimeSpan.TryParse(item.HORA_DET, out horaDetencion))
                        {
                            // Si prefieres, saltar este registro en vez de asignar 00:00
                            // continue;
                            horaDetencion = TimeSpan.Zero;
                        }

                        // Validar/parsear delito
                        int cveDelito = 0;
                        if (item.CLA_DELITO == null || !int.TryParse(item.CLA_DELITO.ToString(), out cveDelito))
                        {
                            // maneja según convenga: omitir, asignar 0 o lanzar
                            // continue;
                            cveDelito = 0;
                        }

                        var newDetenido = new tb_Detenido()
                        {
                            int_id_ficha_objetivo = int_id_ficha_objetivo,
                            numavp = item.NUM_AVP,
                            date_fecha_detencion = item.FEC_DET,
                            time_hora_detencion = horaDetencion,
                            date_fecha_creacion = DateTime.Now,
                            bit_estatus = true,
                            clave_persona = clave_persona,
                            date_fecha_ingreso = item.FE_INGRE,
                            date_fecha_captura_filiacion = item.Fec_Captu
                        };

                        db.tb_Detenido.Add(newDetenido);
                        db.SaveChanges(); // Necesario aquí para obtener newDetenido.int_id_detenido si es identity

                        var nuevo = new tb_Detenidos_DelitoIngreso()
                        {
                            int_id_detenido = newDetenido.int_id_detenido,
                            Cve_Delito = cveDelito,
                            Delito = item.DELITO_DESCRIP,
                            date_fecha_creacion = DateTime.Now,
                            bit_estatus = true
                        };

                        db.tb_Detenidos_DelitoIngreso.Add(nuevo);
                    }
                    
                    db.SaveChanges();

                    transaction.Commit();
                    return new BasicOperationResponse()
                    {
                        IsSuccess = true,
                        Message = "Registro agregado correctamente desde Filiación."
                    };
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return new BasicOperationResponse()
                    {
                        IsSuccess = false,
                        Message = "Error al agregar el registro (" + e.Message + ")"
                    };
                }
            }
        }

        // 🔸 Desactivar registro
        public BasicOperationResponse DisableDelito(int int_id_detenido)
        {
            try
            {
                var registro = db.tb_Detenido.FirstOrDefault(x => x.int_id_detenido == int_id_detenido);
                if (registro == null)
                    return new BasicOperationResponse() { IsSuccess = false, Message = "No se encontró el registro" };

                registro.bit_estatus = false;
                db.SaveChanges();

                return new BasicOperationResponse()
                {
                    IsSuccess = true,
                    Message = "Se desactivó correctamente."
                };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse()
                {
                    IsSuccess = false,
                    Message = "Error al desactivar (" + e.Message + ")"
                };
            }
        }

        // 🔸 Activar registro
        public BasicOperationResponse ActivateDelito(int int_id_detenido)
        {
            try
            {
                var registro = db.tb_Detenido.FirstOrDefault(x => x.int_id_detenido == int_id_detenido);
                if (registro == null)
                    return new BasicOperationResponse() { IsSuccess = false, Message = "No se encontró el registro" };

                registro.bit_estatus = true;
                db.SaveChanges();

                return new BasicOperationResponse()
                {
                    IsSuccess = true,
                    Message = "Se activó correctamente."
                };
            }
            catch (Exception e)
            {
                return new BasicOperationResponse()
                {
                    IsSuccess = false,
                    Message = "Error al activar (" + e.Message + ")"
                };
            }
        }


        public List<Nom_perso> getListNombresByClave(int clave_persona)
        {
            return dbFili.Nom_perso.AsNoTracking().Where(x=>x.CLAVE_PERSO==clave_persona).ToList();
        }


    }


}