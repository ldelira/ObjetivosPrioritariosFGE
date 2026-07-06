using Newtonsoft.Json;
using Objetivos_Prioritarios.ControllersServices;
using Objetivos_Prioritarios.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.Controllers
{
    public class AsuntosController : ABaseController
    {

        private PartialViewResult SubView(string folder, string viewName, object model = null)
        {
            string path = $"~/Views/Asuntos/{folder}/{viewName}.cshtml";
            return PartialView(path, model);
        }

        public ActionResult Index()
        {
            ViewBag.title = "";
            return View();
        }

        public PartialViewResult AsuntosListPartial(bool? actives)
        {
            if (actives == null) actives = true;
            ViewBag.Actives = actives;
            return PartialView();
        }

        [HttpPost]
        public JsonResult FillAsuntosList(bool? active)
        {
            var lista = AsuntoService.GetAsuntosList(active);

            var listaRet = lista.Select(a => new
            {
                int_id_asunto_relacionado = a.int_id_asunto_relacionado,
                nvarchar_alias = a.nvarchar_alias,
                nvarchar_descripcion = a.nvarchar_descripcion,
                date_fecha_asunto = a.date_fecha_asunto,
                numavp = a.numavp
            })
            .ToList();


            return Json(listaRet, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddEditAsuntos(int? int_id_asunto_relacionado)
        {
            ViewBag.title = "ASUNTOS RELACIONADOS";

            ViewBag.EstatusAsuntoList = AsuntoService.db.cat_EstatusAsunto.ToList();
            ViewBag.RolesParticipacionAsunto = AsuntoService.GetRolesParticipacionAsunto(true).ToList();

            ViewBag.Actives = true;
            if (int_id_asunto_relacionado != null)
            {
                var busqueda = AsuntoService.GetAsuntoById((int)int_id_asunto_relacionado);
                return View(busqueda);
            }
            else
            {
                tb_AsuntoRelacionado asunto = new tb_AsuntoRelacionado
                {
                    date_fecha_creacion = DateTime.Now,
                    bit_estatus = true
                    // nvarchar_usuario_creacion = "delira" // si tu modelo lo tiene
                };
                // Si quieres persistir inmediatamente (como en Objetivos), descomenta:
                // AsuntoService.db.tb_AsuntoRelacionado.Add(asunto);
                // AsuntoService.db.SaveChanges();
                return View(asunto);
            }
        }

        [HttpPost]
        public JsonResult SaveAsunto(tb_AsuntoRelacionado model)
        {
            var resp = AsuntoService.SaveAsunto(model);
            return Json(resp);
        }

        [HttpPost]
        public JsonResult ActivateAsunto(int id)
        {
            var resp = AsuntoService.ActivateAsunto(id);
            return Json(resp);
        }

        [HttpPost]
        public JsonResult DisableAsunto(int id)
        {
            var resp = AsuntoService.DisableAsunto(id);
            return Json(resp);
        }






        #region ParcialDetenidos


        public PartialViewResult DetenidoListPartial(string nombre, string paterno, string materno)
        {

            var model = FichaObjetivoService.GetFiliacionList(1, nombre, paterno, materno, null).ToList();

            return SubView("Victimas", "DetenidoListPartial", model);
        }
        [HttpPost]
        public JsonResult getListNombresDetenido(int clave_persona)
        {
            var result = FichaObjetivoService.getListNombresByClave(clave_persona);

            var resultRet = result.Select(r => new
            {
                CLAVE_PERSO = r.CLAVE_PERSO,
                id = r.id,
                NOMBRE = r.NOMBRE,
                AP_PATERNO = r.AP_PATERNO,
                AP_MATERNO = r.AP_MATERNO,
                NOMBRE_COMPLETO = r.NOMBRE + " " + (r.AP_PATERNO == null ? "" : r.AP_PATERNO) + " " + (r.AP_MATERNO == null ? "" : r.AP_MATERNO)
            }).ToList();

            return Json(resultRet);
        }

        [HttpPost]
        public JsonResult addDetenidoVictima(int id, int id_asunto_relacionado)
        {
            return Json(AsuntoService.AddDetenidoVictima(id, id_asunto_relacionado));

        }

        #endregion

        #region ParcialObjetivoPrioritario


        public PartialViewResult ObjetivoPrioritarioListPartial(string nombre, string paterno, string materno, int opcion)
        {
            ViewBag.opcion = opcion;
            ViewBag.RolesParticipacionAsunto = AsuntoService.GetRolesParticipacionAsunto(true).ToList();

            var model = AsuntoService.GetVictimasNamePhotoList(nombre, paterno, materno).ToList();

            return SubView("Victimas", "ObjetivoPrioritarioListPartial", model);
        }

        [HttpPost]
        public JsonResult GetEstatusProcesos()
        {
            var estatus = AsuntoService.GetEstatusProcesos(true).ToList();

            var estatusResult = estatus.Select(x => new
            {
                x.int_id_estatus_proceso,
                x.nvarchar_estatus

            }).ToList();
            return Json(estatusResult, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult getListNombresObjetivo(int id_objetivo)
        {
            var result = ObjetivoService.getNombreObetivoList(true, id_objetivo);

            var selected = result.Select(r => new
            {
                id_nombre = r.int_id_nombre,
                nombre_completo = r.nvarchar_nombre + " " + (r.nvarchar_paterno == null ? "" : r.nvarchar_paterno) + " " + (r.nvarchar_materno == null ? "" : r.nvarchar_materno),
                paterno = r.nvarchar_paterno,
                materno = r.nvarchar_materno,
                nombre = r.nvarchar_nombre,
                bit_principal = r.bit_principal
            }).ToList();

            return Json(selected);
        }

        [HttpPost]
        public JsonResult addObjetivoVictima(int id_nombre, int id_asunto_relacionado)
        {
            return Json(AsuntoService.AddObjetivoVictima(id_nombre, id_asunto_relacionado));

        }
        #endregion

        #region ParcialDetenidos


        public PartialViewResult VictimasListPartial(string nombre, string paterno, string materno, int opcion, bool active)
        {

            var model = AsuntoService.getListVictimas(nombre, paterno, materno, opcion, active).ToList();
            ViewBag.opcion = opcion;
            ViewBag.active = active;

            return SubView("Victimas", "VictimasListPartial", model);
        }

        [HttpPost]
        public JsonResult addVictimaVictima(int id_victima, int id_asunto_relacionado)
        {
            return Json(AsuntoService.AddVictimaVictima(id_victima, id_asunto_relacionado));

        }


        [HttpPost]
        public JsonResult ReactivarVictima(int id, int id_victima, int id_asunto_relacionado)
        {
            return Json(AsuntoService.ReactivarVictima(id, id_victima, id_asunto_relacionado));
        }
        #endregion


        #region VictimasRelacionadas

        public PartialViewResult VictimasRelacionadasListPartial(bool? actives)
        {
            if (actives == null) actives = true;
            ViewBag.Actives = actives;
            return SubView("Victimas", "VictimasRelacionadasListPartial");

        }

        [HttpPost]
        public JsonResult FillVictimasRelacionadasList(bool activo, int int_id_asunto_relacionado)
        {
            var vict = AsuntoService.getListAsuntoVictima(activo, int_id_asunto_relacionado);
            var victimas = vict
                .Select(v => new
                {
                    v.int_id_asunto_victima,
                    v.tb_Victimas.int_id_victima,
                    NombreCompleto = v.tb_Victimas.nvarchar_nombre + " " + v.tb_Victimas.nvarchar_paterno + " " + v.tb_Victimas.nvarchar_materno,
                    //FotoBase63 = v.tb_Victimas.nvarchar_foto,
                    Isfoto = v.tb_Victimas.nvarchar_foto == null ? "SIN FOTO" : "CON FOTO"
                })
                .ToList();

            return Json(victimas, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DisableVictimaRelacionada(int int_id_asunto_victima)
        {
            return Json(AsuntoService.DisableVictimaRelacionada(int_id_asunto_victima));
        }

        [HttpPost]
        public JsonResult ActivateVictimaRelacionada(int int_id_asunto_victima)
        {
            return Json(AsuntoService.ActivateVictimaRelacionada(int_id_asunto_victima));

        }

        #endregion



        public PartialViewResult AddEditVictimaPartial(int? id, int? idllamada, int? idasunto)
        {
            ViewBag.llamada = idllamada;
            ViewBag.idasunto = idasunto;
            tb_Victimas model = new tb_Victimas();

            if (id != null && id > 0)
            {
                model = AsuntoService.db.tb_Victimas.FirstOrDefault(x => x.int_id_victima == id);
            }

            return SubView("Victimas", "AddEditVictimaPartial", model);
        }


        #region ObjetivosRelacionadoAsunto
        [HttpPost]
        public JsonResult addObjetivoAsunto(
            int int_id_objetivo,
            int int_id_asunto_relacionado,
            string observaciones,
            int? int_id_rol_participacion,
            string descripcionParticipacion)
        {
            return Json(AsuntoService.addObjetivoAsunto(
                int_id_objetivo,
                int_id_asunto_relacionado,
                observaciones,
                int_id_rol_participacion,
                descripcionParticipacion));
        }

        public PartialViewResult ObjetivoRelacionadoAsuntoListPartial(bool? actives)
        {
            if (actives == null) actives = true;

            ViewBag.Actives = actives;
            ViewBag.RolesParticipacionAsunto = AsuntoService.GetRolesParticipacionAsunto(true).ToList();

            return PartialView();
        }


        [HttpPost]
        public JsonResult FillObjetivoRelacionadoAsuntoList(int int_id_asunto_relacionado, bool? active)
        {
            var lista = AsuntoService.getListObjetivosRelacionadoAsunto(int_id_asunto_relacionado, (bool)active).ToList();

            var idsFichaAsunto = lista
                .Select(x => x.int_id_ficha_asunto)
                .ToList();

            var participaciones = (
                from fa in AsuntoService.db.tb_FichaAsunto.AsNoTracking()
                join rp in AsuntoService.db.cat_RolParticipacionAsunto.AsNoTracking()
                    on fa.int_id_rol_participacion equals rp.int_id_rol_participacion into roles
                from rp in roles.DefaultIfEmpty()
                where idsFichaAsunto.Contains(fa.int_id_ficha_asunto)
                select new
                {
                    fa.int_id_ficha_asunto,
                    fa.int_id_rol_participacion,
                    fa.nvarchar_descripcion_participacion,
                    rol_participacion = rp == null ? "Por definir" : rp.nvarchar_rol
                }
            ).ToList()
             .ToDictionary(x => x.int_id_ficha_asunto, x => x);

            var lista2 = lista.Select(x =>
            {
                var participacion = participaciones.ContainsKey(x.int_id_ficha_asunto)
                    ? participaciones[x.int_id_ficha_asunto]
                    : null;

                return new
                {
                    x.int_id_ficha_asunto,
                    x.int_id_ficha_objetivo,
                    x.int_id_asunto_relacionado,
                    x.estatus_ficha,
                    x.int_id_estatus_proceso,
                    x.nvarchar_descripcion_estatus,
                    x.nvarchar_observaciones,
                    x.int_id_objetivo,
                    x.Nombres,
                    x.Aliases,
                    x.GruposDelictivos,
                    x.FechaNacimiento,
                    x.estatus_objetivo,
                    int_id_rol_participacion = participacion == null ? null : participacion.int_id_rol_participacion,
                    rol_participacion = participacion == null ? "Por definir" : participacion.rol_participacion,
                    nvarchar_descripcion_participacion = participacion == null ? "" : participacion.nvarchar_descripcion_participacion,
                    isFoto = x.nvarchar_foto == null ? "SIN FOTO" : "CON FOTO"
                };
            }).ToList();

            return Json(lista2, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ActivateObjetivoAsunto(int int_id_ficha_asunto)
        {
            var resp = AsuntoService.ActivateObjetivoAsunto(int_id_ficha_asunto);
            return Json(resp);
        }

        [HttpPost]
        public JsonResult DisableObjetivoAsunto(int int_id_ficha_asunto)
        {
            var resp = AsuntoService.DisableObjetivoAsunto(int_id_ficha_asunto);
            return Json(resp);
        }


        #endregion

        [HttpPost]
        public JsonResult SaveVictima()
        {
            try
            {
                var file = Request.Files["foto"];
                string nombre = Request.Form["nvarchar_nombre"]?.Trim();
                string paterno = Request.Form["nvarchar_paterno"]?.Trim();
                string materno = Request.Form["nvarchar_materno"]?.Trim();
                int idllamada = Convert.ToInt32(Request.Form["llamada"]);
                int idasunto = Convert.ToInt32(Request.Form["asunto"]);
                int int_id_victima = 0;
                var sIdVictima = Request.Form["int_id_victima2"]?.ToString().Trim();
                if (!string.IsNullOrEmpty(sIdVictima))
                {
                    int.TryParse(sIdVictima, out int_id_victima);
                }


                // 🧾 Depuración opcional (para revisar en consola del servidor)
                //System.Diagnostics.Debug.WriteLine("---- DATOS RECIBIDOS ----");
                //System.Diagnostics.Debug.WriteLine($"Nombre: {nombre}");
                //System.Diagnostics.Debug.WriteLine($"Paterno: {paterno}");
                //System.Diagnostics.Debug.WriteLine($"Materno: {materno}");
                //System.Diagnostics.Debug.WriteLine($"idllamada: {idllamada}");
                //System.Diagnostics.Debug.WriteLine($"idasunto: {idasunto}");
                //System.Diagnostics.Debug.WriteLine($"Archivo: {(file != null ? file.FileName : "Ninguno")}");

                string base64Foto = null;
                if (file != null && file.ContentLength > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        file.InputStream.CopyTo(ms);
                        base64Foto = Convert.ToBase64String(ms.ToArray());
                    }
                }

                tb_Victimas victima = new tb_Victimas
                {
                    int_id_victima = int_id_victima,
                    nvarchar_nombre = nombre,
                    nvarchar_paterno = paterno,
                    nvarchar_materno = materno,
                    nvarchar_foto = base64Foto
                };

                var resultado = AsuntoService.SaveVictimaService(victima, idllamada, idasunto);

                return Json(new
                {
                    success = resultado.IsSuccess,
                    message = resultado.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error al procesar la solicitud: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public JsonResult GetFotoAjax(int id)
        {
            if (id <= 0)
                return Json(new { success = false, message = "Id inválido." });

            try
            {
                // Obtén el base64 desde base de datos
                var busqueda = FichaObjetivoService.db.tb_FichaObjetivo.FirstOrDefault(x => x.int_id_ficha_objetivo == id);
                if (busqueda == null)
                {
                    return Json(new { success = false, message = "No existe fotografía." });

                }
                else
                {

                    string fotoBase64 = busqueda.tb_Objetivo.nvarchar_foto;

                    if (string.IsNullOrEmpty(fotoBase64))
                        return Json(new { success = false, message = "No existe fotografía." });

                    return Json(new { success = true, foto = fotoBase64 });
                }
            }
            catch (Exception ex)
            {
                // Aquí puedes loguear el error
                return Json(new { success = false, message = "Error en el servidor." });
            }
        }



        [HttpPost]
        public JsonResult GetFotoAjaxVictimas(int id)
        {
            if (id <= 0)
                return Json(new { success = false, message = "Id inválido." });

            try
            {
                // Ajusta la siguiente línea al contexto/servicio que uses
                var victima = FichaObjetivoService.db.tb_Victimas.FirstOrDefault(x => x.int_id_victima == id);

                if (victima == null)
                    return Json(new { success = false, message = "No existe fotografía." });

                string fotoBase64 = victima.nvarchar_foto; // o el nombre de campo real

                if (string.IsNullOrEmpty(fotoBase64))
                    return Json(new { success = false, message = "No existe fotografía." });

                return Json(new { success = true, foto = fotoBase64 });
            }
            catch (Exception ex)
            {
                // loguea ex si quieres
                return Json(new { success = false, message = "Error en el servidor." });
            }
        }

        #region AsuntosRelacionados

        public PartialViewResult AsuntosObjetivoListPartial(bool? actives)
        {
            if (actives == null)
                actives = true;

            ViewBag.Actives = actives;
            ViewBag.RolesParticipacionAsunto = AsuntoService.GetRolesParticipacionAsunto(true).ToList();

            return SubView("AsuntosRelacionados", "AsuntosObjetivoListPartial");
        }

        [HttpPost]
        public JsonResult FillAsuntosObjetivoList(bool? active, int int_id_ficha_objetivo)
        {
            var lista = AsuntoService.GetAsuntosByFichaObjetivo(int_id_ficha_objetivo, active);

            var result = lista.Select(x => new
            {
                int_id_ficha_asunto = x.int_id_ficha_asunto,
                int_id_asunto_relacionado = x.int_id_asunto_relacionado,
                int_id_ficha_objetivo = x.int_id_ficha_objetivo,
                alias = x.tb_AsuntoRelacionado == null ? "" : x.tb_AsuntoRelacionado.nvarchar_alias,
                descripcion = x.tb_AsuntoRelacionado == null ? "" : x.tb_AsuntoRelacionado.nvarchar_descripcion,
                descripcion_corta = x.tb_AsuntoRelacionado == null || x.tb_AsuntoRelacionado.nvarchar_descripcion == null
                    ? ""
                    : (x.tb_AsuntoRelacionado.nvarchar_descripcion.Length > 180
                        ? x.tb_AsuntoRelacionado.nvarchar_descripcion.Substring(0, 180) + "..."
                        : x.tb_AsuntoRelacionado.nvarchar_descripcion),
                numavp = x.tb_AsuntoRelacionado == null ? "" : x.tb_AsuntoRelacionado.numavp,
                fecha_asunto = x.tb_AsuntoRelacionado == null ? "" : string.Format("{0:dd-MM-yyyy}", x.tb_AsuntoRelacionado.date_fecha_asunto),
                int_id_rol_participacion = x.int_id_rol_participacion,
                rol_participacion = x.cat_RolParticipacionAsunto == null ? "Por definir" : x.cat_RolParticipacionAsunto.nvarchar_rol,
                descripcion_participacion = x.nvarchar_descripcion_participacion,
                observaciones_relacion = x.nvarchar_observaciones,
                bit_estatus = x.bit_estatus
            }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult FillBusquedaAsuntosParaRelacionar(string texto, bool? active)
        {
            var lista = AsuntoService.BuscarAsuntosParaRelacionar(texto, active);

            var result = lista.Select(x => new
            {
                int_id_asunto_relacionado = x.int_id_asunto_relacionado,
                alias = x.nvarchar_alias,
                descripcion = x.nvarchar_descripcion,
                descripcion_corta = x.nvarchar_descripcion == null
                    ? ""
                    : (x.nvarchar_descripcion.Length > 160
                        ? x.nvarchar_descripcion.Substring(0, 160) + "..."
                        : x.nvarchar_descripcion),
                numavp = x.numavp,
                fecha_asunto = string.Format("{0:dd-MM-yyyy}", x.date_fecha_asunto)
            }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult RelacionarAsuntoObjetivo(
    int int_id_objetivo,
    int int_id_asunto_relacionado,
    int? int_id_rol_participacion,
    string nvarchar_descripcion_participacion,
    string observaciones)
        {
            var response = AsuntoService.RelacionarAsuntoObjetivo(
                int_id_objetivo,
                int_id_asunto_relacionado,
                int_id_rol_participacion,
                nvarchar_descripcion_participacion,
                observaciones
            );

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DisableAsuntoObjetivo(int int_id_ficha_asunto)
        {
            var resp = AsuntoService.DisableObjetivoAsunto(int_id_ficha_asunto);
            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ActivateAsuntoObjetivo(int int_id_ficha_asunto)
        {
            var resp = AsuntoService.ActivateObjetivoAsunto(int_id_ficha_asunto);
            return Json(resp, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult CrearAsuntoYRelacionarObjetivo(
            string alias,
            string descripcion,
            string numavp,
            string fechaAsunto,
            int? int_id_estatus_asunto,
            int int_id_objetivo,
            string observacionesRelacion,
            int? int_id_rol_participacion,
            string descripcionParticipacion)
        {
            DateTime fecha;

            var asunto = new tb_AsuntoRelacionado
            {
                nvarchar_alias = alias,
                nvarchar_descripcion = descripcion,
                numavp = numavp
            };

            if (!string.IsNullOrWhiteSpace(fechaAsunto) && DateTime.TryParse(fechaAsunto, out fecha))
            {
                asunto.date_fecha_asunto = fecha;
            }

            if (int_id_estatus_asunto.HasValue)
            {
                asunto.int_id_estatus_asunto = int_id_estatus_asunto.Value;
            }

            var resp = AsuntoService.CrearAsuntoYRelacionarObjetivo(
                asunto,
                int_id_objetivo,
                observacionesRelacion,
                int_id_rol_participacion,
                descripcionParticipacion
            );

            return Json(resp, JsonRequestBehavior.AllowGet);
        }



        #endregion



    }
}
