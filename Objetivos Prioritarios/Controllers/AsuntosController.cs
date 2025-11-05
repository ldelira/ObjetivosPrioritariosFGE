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

            var listaRet=lista.Select(a => new
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

            ViewBag.EstatusAsuntoList=AsuntoService.db.cat_EstatusAsunto.ToList();

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

            var model = FichaObjetivoService.GetFiliacionList(1,nombre, paterno, materno,null).ToList();

            return SubView("Victimas", "DetenidoListPartial", model);
        }
        [HttpPost]
        public JsonResult getListNombresDetenido(int clave_persona)
        {
            var result= FichaObjetivoService.getListNombresByClave(clave_persona);

            var resultRet = result.Select(r => new
            {
                CLAVE_PERSO=r.CLAVE_PERSO,
                id=r.id,
                NOMBRE = r.NOMBRE,
                AP_PATERNO=r.AP_PATERNO,
                AP_MATERNO=r.AP_MATERNO,
                NOMBRE_COMPLETO=r.NOMBRE + " " + (r.AP_PATERNO==null?"":r.AP_PATERNO) + " " + (r.AP_MATERNO==null?"": r.AP_MATERNO)
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


        public PartialViewResult ObjetivoPrioritarioListPartial(string nombre, string paterno, string materno)
        {

            var model = AsuntoService.GetVictimasNamePhotoList(nombre, paterno, materno).ToList();

          
            return SubView("Victimas", "ObjetivoPrioritarioListPartial", model);
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


        public PartialViewResult VictimasListPartial(string nombre, string paterno, string materno, int opcion,bool active)
        {

            var model = AsuntoService.getListVictimas(nombre, paterno, materno, opcion,active).ToList();
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
        public JsonResult FillVictimasRelacionadasList(bool activo,int int_id_asunto_relacionado)
        {
            var vict = AsuntoService.getListAsuntoVictima(activo,int_id_asunto_relacionado);
            var victimas = vict
                .Select(v => new
                {
                    v.int_id_asunto_victima,
                    v.tb_Victimas.int_id_victima,
                    NombreCompleto = v.tb_Victimas.nvarchar_nombre + " " + v.tb_Victimas.nvarchar_paterno + " " + v.tb_Victimas.nvarchar_materno,
                    FotoBase63=v.tb_Victimas.nvarchar_foto
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

            return SubView("Victimas", "AddEditVictimaPartial",model);
        }


        #region ObjetivosRelacionadoAsunto
        [HttpPost]
        public JsonResult addObjetivoAsunto(int int_id_objetivo, int int_id_asunto_relacionado)
        {
            return Json(AsuntoService.addObjetivoAsunto(int_id_objetivo, int_id_asunto_relacionado));


        }

        public PartialViewResult ObjetivoRelacionadoAsuntoListPartial(bool? actives)
        {
            if (actives == null) actives = true;
            ViewBag.Actives = actives;
            return PartialView();
        }


        [HttpPost]
        public JsonResult FillObjetivoRelacionadoAsuntoList(int int_id_asunto_relacionado,bool? active)
        {
            var lista = AsuntoService.getListObjetivosRelacionadoAsunto(int_id_asunto_relacionado, (bool)active).ToList();

            return Json(lista, JsonRequestBehavior.AllowGet);
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
                var resultado = AsuntoService.SaveVictimaService(nombre, paterno, materno, base64Foto, idllamada, idasunto);

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




    }
}
