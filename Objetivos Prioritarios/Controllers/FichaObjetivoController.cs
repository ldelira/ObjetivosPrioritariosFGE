using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Utils;
using Antlr.Runtime.Misc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
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
            var general = ObjetivoService.getFichaObjetivosList(active);
            var generalResult = general
            .Select(x => new {
                x.int_id_objetivo,
                x.Nombres,
                x.Carpetas,
                x.Delitos
            }).ToList();
            return Json(generalResult, JsonRequestBehavior.AllowGet);
        }


        public ActionResult EditFichaObjetivos(int int_id_objetivo)
        {
            var busquedaFicha = FichaObjetivoService.addorgetFichObjetivo(int_id_objetivo);

            // 1. Obtener catálogo de estatus
            var catestatus = AsuntoService.GetEstatusProcesos(true).ToList();

            ViewBag.CatalogoEstatus = AsuntoService.GetEstatusProcesos(true).ToList();
            var idficha = ObjetivoService.GetIdFichaObjetivo(int_id_objetivo);

            ViewBag.EstatusActual = ObjetivoService.GetEstatusObjetivo(idficha);

            ViewBag.Nombre = ObjetivoService.ObtenerNombreCompletoPrincipal(int_id_objetivo);

            ViewBag.Nombress = ObjetivoService.ObtenerNombreCompletoSecundarios(int_id_objetivo);

            var alias = ObjetivoService.getAliasObjetivoList(true, int_id_objetivo);
            ViewBag.Alias = alias.Select(x => x.nvarchar_alias).ToList();

            var grupos = ObjetivoService.GetGruposDelictivosObjetivo(int_id_objetivo);
            ViewBag.GruposDelictivos = grupos;

            ViewBag.Observacion = ObjetivoService.GetObservacionObjetivo(idficha);
            ViewBag.DescripcionEstatus = ObjetivoService.GetDescripcionEstatusObjetivo(idficha);

            ViewBag.fotobase64 = ObjetivoService.GetFotoObjetivo(int_id_objetivo);

            return View(busquedaFicha);
        }


        //[HttpPost]
        //public JsonResult GetEstatusProcesos(int idficha)
        //{
        //    var catestatus = AsuntoService.GetEstatusProcesos(true).ToList();

        //    var estatusResult = catestatus.Select(x => new
        //    {
        //        x.int_id_estatus_proceso,
        //        x.nvarchar_estatus
        //    }).ToList();

        //    var estatusobjetivo = ObjetivoService.GetEstatusObjetivo(idficha);

        //    var idobjetivo = ObjetivoService.GetIdObjetivo(idficha);

        //    var nombre = ObjetivoService.ObtenerNombreCompletoPrincipal(idobjetivo);
        //    var alias = ObjetivoService.getAliasObjetivoList(true, idobjetivo);
        //    var grupodelictivo = ObjetivoService.GetObjetivoGrupoList(true, idobjetivo);
        //    var observacion = ObjetivoService.GetObservacionObjetivo(idficha);
        //    var descripcionestatus = ObjetivoService.GetDescripcionEstatusObjetivo(idficha);

        //    return Json(new
        //    {
        //        catalogo = estatusResult,
        //        estatusActual = estatusobjetivo,
        //        nombre = nombre,
        //        alias = alias,
        //        grupodelictivo = grupodelictivo,
        //        observacion = observacion,
        //        descripcionestatus = descripcionestatus
        //    }, JsonRequestBehavior.AllowGet);
        //}



        [HttpPost]
        public JsonResult AddEstatusObservaciones(int idestatus, string descripcion, string observaciones, int idficha)
        {
            var result = FichaObjetivoService.AddEstatusObservaciones(
                idficha,
                idestatus,
                descripcion,
                observaciones
            );
            return Json(result, JsonRequestBehavior.AllowGet);
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


        #region "Album Ficha Objetivo"
        public ActionResult IndexAlbum()
        {
            ViewBag.title = "";
            return SubView("AlbumFicha", "IndexAlbum");
        }

        [HttpPost]
        public PartialViewResult AlbumFichaListPartial(bool? actives)
        {
            if (actives == null)
                actives = true;

            ViewBag.Actives = actives;
            return SubView("AlbumFicha", "AlbumFichaListPartial");
        }
        public JsonResult FillAlbumFichaObjetivosList(bool? active)
        {
            var general = ObjetivoService.getAlbumFichaObjetivo((bool)active);
            var generalResult = general
            .Select(x => new {
                x.int_id_album_ficha_objetivo,
                x.nvarchar_nombre_album,
                x.nvarchar_descripcion_album,
                x.bit_estatus
            }).ToList();
            return Json(generalResult, JsonRequestBehavior.AllowGet);
        }


        public PartialViewResult AddEditAlbumFichaPartial(int? int_id_album)
        {
            if (int_id_album != null)
            {
                var busquedaFicha = FichaObjetivoService.db.tb_AlbumFichaObjetivo.Where(x=>x.int_id_album_ficha_objetivo==int_id_album).FirstOrDefault();

                return SubView("AlbumFicha", "AddEditAlbumFichaPartial", busquedaFicha);
            }
            else
            {
                return SubView("AlbumFicha", "AddEditAlbumFichaPartial");
            }
        }


        public JsonResult addEditAlbumFicha(tb_AlbumFichaObjetivo albumFicha)
        {
            var general = FichaObjetivoService.addEditAlbumFicha(albumFicha);


            return Json(general, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult DisableAlbum(int int_id_ficha)
        {
            var res = FichaObjetivoService.DisableAlbum(int_id_ficha);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ActivateAlbum(int int_id_ficha)
        {
            var res = FichaObjetivoService.ActivateAlbum(int_id_ficha);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region "Album Ficha Objetivo Grupos"
        public ActionResult IndexGrupo(int int_id_album)
        {
            ViewBag.title = "";
            ViewBag.int_id_album=int_id_album;
            return SubView("AlbumFichaGrupo", "IndexGrupo");
        }

        [HttpPost]
        public JsonResult addObjetivoAlbum(int int_id_objetivo, int int_id_album)
        {
            return Json(FichaObjetivoService.addObjetivoAlbum(int_id_objetivo, int_id_album));


        }
        public PartialViewResult GrupoFichasListPartial(bool? actives)
        {
            if (actives == null) actives = true;
            ViewBag.Actives = actives;
            return SubView("AlbumFichaGrupo", "GrupoFichasListPartial");
        }


        [HttpPost]
        public JsonResult FillGrupoFichaoList(int int_id_album_ficha_objetivo, bool? active)
        {
            var lista = FichaObjetivoService.getListObjetivosRelacionadoGrupo(int_id_album_ficha_objetivo, (bool)active).ToList();

            var lista2 = lista.Select(x => new
            {
                x.int_id_album_detalle,
                x.int_id_ficha_objetivo,
                x.int_id_album_ficha_objetivo,
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
                isFoto=x.nvarchar_foto==null?"SIN FOTO":"CON FOTO"
            }).ToList();

            return Json(lista2, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ActivateObjetivoGrupo(int int_id_album_detalle)
        {
            var resp = FichaObjetivoService.ActivateObjetivoGrupo(int_id_album_detalle);
            return Json(resp);
        }

        [HttpPost]
        public JsonResult DisableObjetivoGrupo(int int_id_album_detalle)
        {
            var resp = FichaObjetivoService.DisableObjetivoGrupo(int_id_album_detalle);
            return Json(resp);
        }



        public ActionResult GenerarPpt(int int_id_album_ficha_objetivo)
        {
            var detenidos = ReporteService.ObtenerDatosDeTuBaseDeDatos(int_id_album_ficha_objetivo); // tu lógica propia

            string plantilla = Server.MapPath("~/Plantillas/plantilla_base.pptx");
            string salida = Server.MapPath("~/Salidas/informe_detenidos.pptx");
            string defaultImg = Server.MapPath("~/Content/imagenes/Nodisponible.jpg");
            string fondo = Server.MapPath("~/Content/imagenes/FONDO.jpg");


            
            PowerPointGeneratorNoImagePartType.GenerarPresentacion(detenidos, plantilla, salida, defaultImg, fondo);

            return File(salida, "application/vnd.openxmlformats-officedocument.presentationml.presentation", "informe_detenidos.pptx");
        }
        public ActionResult GenerarPpt2(int int_id_album_ficha_objetivo)
        {
            var detenidos = ReporteService.ObtenerDatosDeTuBaseDeDatos(int_id_album_ficha_objetivo); // tu lógica propia

            string plantilla = Server.MapPath("~/Plantillas/plantilla_base_vicefiscalia.pptx");
            string salida = Server.MapPath("~/Salidas/informe_detenidos_vicefiscalia.pptx");
            string defaultImg = Server.MapPath("~/Content/imagenes/Nodisponible.jpg");
            string fondo = Server.MapPath("~/Content/imagenes/FONDO2.png");



            PowerPointGeneratorNoImagePartType.GenerarPresentacion2(detenidos, plantilla, salida, defaultImg, fondo);

            return File(salida, "application/vnd.openxmlformats-officedocument.presentationml.presentation", "informe_detenidos.pptx");
        }


        #endregion
    }
}