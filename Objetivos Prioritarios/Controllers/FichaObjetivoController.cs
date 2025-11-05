using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.Controllers
{
    public class FichaObjetivoController : ABaseController
    {

        private PartialViewResult SubView(string folder, string viewName, object model = null)
        {
            string path = $"~/Views/FichaObjetivo/{folder}/{viewName}.cshtml";
            return PartialView(path, model);
        }



        #region "FichaObjetivo"
        public ActionResult Index()
        {
            ViewBag.title = "";
            return View();
        }

        [HttpPost]
        public PartialViewResult FichaObjetivosListPartial(bool? actives)
        {
            if (actives == null)
                actives = true;

            ViewBag.Actives = actives;
            return PartialView();
        }
        public JsonResult FillFichaObjetivosList(bool? active)
        {
            var general = ObjetivoService.getObjetivosList(active);
            var generalResult = general
            .Select(x => new {
                x.int_id_objetivo,
                x.Nombres,
                x.Aliases,
                x.GruposDelictivos,
                x.FechaNacimiento,
                x.bit_estatus,
                TieneFoto = (!string.IsNullOrEmpty(x.nvarchar_foto)) == true ? "CON FOTO" : "SIN FOTO"
            }).ToList();
            return Json(generalResult, JsonRequestBehavior.AllowGet);
        }


        public ActionResult EditFichaObjetivos(int int_id_objetivo)
        {
            var busquedaFicha = FichaObjetivoService.addorgetFichObjetivo(int_id_objetivo);
            return View(busquedaFicha);
        }

        #endregion


        #region "CarpetasLigadas"

        public PartialViewResult BusquedaCarpetasListPartial()
        {
            return SubView("CarpetasRelacionadas", "BusquedaCarpetasListPartial");
        }
        public JsonResult FillBusquedaCarpetasList(int movimiento, string nombre, string paterno, string materno, string numavp)
        {
            var general = FichaObjetivoService.GetCarpetasNameNumAvpList(movimiento,nombre,paterno,materno,numavp).ToList();

            
            return Json(general, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult AddCarpetaObjetivo(int int_id_ficha_objetivo, string numAvp)
        {
            var res = FichaObjetivoService.AddCarpetaObjetivo(int_id_ficha_objetivo,numAvp);
            return Json(res, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region "Carpetas Agregadad"

        public PartialViewResult CarpetasRelacionadasListPartial(bool? actives)
        {
            if (actives == null)
                actives = true;

            ViewBag.Actives = actives;
            return SubView("CarpetasRelacionadas", "CarpetasRelacionadasListPartial");
        }

        public JsonResult FillBusquedaCarpetasRelacionadasList(bool? active,int int_id_ficha_objetivo)
        {
            var general = FichaObjetivoService.GetCarpetasList(active, int_id_ficha_objetivo).ToList();

            var generalResut = general.Select(x=> new { 
                int_id_carpeta_objetivo=x.int_id_carpeta_objetivo,
                int_id_ficha_objetivo = x.int_id_ficha_objetivo,
                numavp = x.numavp,
                Delito = x.Delito,
                nvarchar_observacion = x.nvarchar_observacion==null?"": x.nvarchar_observacion,



            }).ToList();

            return Json(generalResut, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DisableCarpetaObjetivo(int int_id_carpeta_objetivo)
        {
            var res = FichaObjetivoService.DisableCarpeta(int_id_carpeta_objetivo);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ActivateCarpetaObjetivo(int int_id_carpeta_objetivo)
        {
            var res = FichaObjetivoService.ActivateCarpeta(int_id_carpeta_objetivo);
            return Json(res, JsonRequestBehavior.AllowGet);
        }


        #endregion


        #region "OrdenesLigadas"

        public PartialViewResult BusquedaOrdenesListPartial()
        {
            return SubView("OrdeneAprehension", "BusquedaOrdenesListPartial");
        }
        public JsonResult FillOrdenesList(int movimiento, string nombre, string paterno, string materno, string numavp)
        {
            var general = FichaObjetivoService.GetObjetivosNameNumAvpList(movimiento, nombre, paterno, materno, numavp).ToList();


            return Json(general, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult AddOrdenesObjetivo(int int_id_ficha_objetivo, int id_mandamiento)
        {
            var res = FichaObjetivoService.AddOrdenObjetivo(int_id_ficha_objetivo, id_mandamiento);
            return Json(res, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region "Carpetas Agregadad"

        public PartialViewResult OrdenesRelacionadasListPartial(bool? actives)
        {
            if (actives == null)
                actives = true;

            ViewBag.Actives = actives;
            return SubView("OrdeneAprehension", "OrdenesRelacionadasListPartial");
        }

        public JsonResult FillOrdenesRelacionadasList(bool? active, int int_id_ficha_objetivo)
        {
            var general = FichaObjetivoService.GetOrdenesList(active, int_id_ficha_objetivo).ToList();

            var generalResut = general.Select(x => new {
                int_id_orden_aprehension = x.int_id_orden_aprehension,
                int_id_ficha_objetivo = x.int_id_ficha_objetivo,
                id_mandamiento_judicial = x.id_mandamiento_judicial,
                delito = x.delito,
                tipo = x.tipo,



            }).ToList();

            return Json(generalResut, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DisableOrdenObjetivo(int int_id_orden_aprehension)
        {
            var res = FichaObjetivoService.DisableOrden(int_id_orden_aprehension);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ActivateOrdenObjetivo(int int_id_orden_aprehension)
        {
            var res = FichaObjetivoService.ActivateOrden(int_id_orden_aprehension);
            return Json(res, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region "Filiación Delitos"

        public PartialViewResult BusquedaFiliacionListPartial()
        {
            // Cambiamos la carpeta y la vista
            return SubView("FiliacionDelitos", "BusquedaFiliacionListPartial");
        }

        public JsonResult FillFiliacionList(int movimiento, string nombre, string paterno, string materno, string numavp)
        {
            var general = FichaObjetivoService.GetFiliacionList(movimiento, nombre, paterno, materno, numavp).ToList();
            return Json(general, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddFiliacionDelito(int int_id_ficha_objetivo, int clave_persona)
        {
            var res = FichaObjetivoService.AddDelitoFiliacion(int_id_ficha_objetivo, clave_persona);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region "Filiación Relacionadas (Delitos agregados)"

        public PartialViewResult FiliacionRelacionadasListPartial(bool? actives)
        {
            if (actives == null)
                actives = true;

            ViewBag.Actives = actives;
            return SubView("FiliacionDelitos", "FiliacionRelacionadasListPartial");
        }

        public JsonResult FillFiliacionRelacionadasList(bool active, int int_id_ficha_objetivo)
        {
            var general = FichaObjetivoService.GetDelitosList(active, int_id_ficha_objetivo).ToList();

            var generalResult = general.Select(x => new {
                int_id_detenido = x.int_id_detenido,
                numavp = x.numavp,
                fecha_detencion = x.date_fecha_detencion,
                hora_detencion = x.time_hora_detencion,
                Delito = x.Delito
            }).ToList();

            return Json(generalResult, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DisableFiliacion(int int_id_detenido)
        {
            var res = FichaObjetivoService.DisableDelito(int_id_detenido);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ActivateFiliacion(int int_id_detenido)
        {
            var res = FichaObjetivoService.ActivateDelito(int_id_detenido);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        #endregion


    }
}