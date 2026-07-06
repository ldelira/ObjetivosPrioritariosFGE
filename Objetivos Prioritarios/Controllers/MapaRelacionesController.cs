using Objetivos_Prioritarios.ControllersServices;
using System;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.Controllers
{
    public class MapaRelacionesController : ABaseController
    {
        public ActionResult Index()
        {
            ViewBag.title = "RED DE INTELIGENCIA";
            return View();
        }

        [HttpPost]
        public JsonResult GetGruposActivos()
        {
            var grupos = MapaRelacionesService.GetGruposActivos();
            return Json(grupos, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetRedPorGrupo(int int_id_grupo)
        {
            string urlFoto = Url.Action(
                "FotoObjetivoMapa",
                "MapaRelaciones",
                null,
                Request.Url.Scheme
            );

            var red = MapaRelacionesService.GetRedPorGrupo(int_id_grupo, urlFoto);

            var json = Json(red, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;

            return json;
        }


        //[HttpGet]
        //public ActionResult FotoObjetivoMapa(int int_id_objetivo)
        //{
        //    try
        //    {
        //        var foto = MapaRelacionesService.GetFotoObjetivoMapa(int_id_objetivo);

        //        if (string.IsNullOrWhiteSpace(foto))
        //        {
        //            return File(Server.MapPath("~/images/NoDisponible.png"), "image/png");
        //        }

        //        if (foto.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
        //        {
        //            var partes = foto.Split(',');
        //            if (partes.Length > 1)
        //            {
        //                foto = partes[1];
        //            }
        //        }

        //        if (foto.StartsWith("/", StringComparison.OrdinalIgnoreCase))
        //        {
        //            return Redirect(foto);
        //        }

        //        byte[] bytes = Convert.FromBase64String(foto);

        //        return File(bytes, "image/jpeg");
        //    }
        //    catch
        //    {
        //        return File(Server.MapPath("~/images/NoDisponible.png"), "image/png");
        //    }
        //}


        [HttpGet]
        public ActionResult FotoObjetivoMapa(int int_id_objetivo)
        {
            try
            {
                string foto = MapaRelacionesService.GetFotoObjetivoMapa(int_id_objetivo);

                if (string.IsNullOrWhiteSpace(foto))
                {
                    return ImagenNoDisponible();
                }

                foto = foto.Trim();

                // Si ya viene con encabezado data:image/png;base64,...
                if (foto.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
                {
                    int coma = foto.IndexOf(",");
                    if (coma >= 0)
                    {
                        foto = foto.Substring(coma + 1);
                    }
                }

                // Limpiar saltos o espacios
                foto = foto
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Replace(" ", "");

                byte[] bytes = Convert.FromBase64String(foto);

                string mime = "image/jpeg";

                // Detectar PNG
                if (bytes.Length > 4 && bytes[0] == 0x89 && bytes[1] == 0x50)
                {
                    mime = "image/png";
                }

                return File(bytes, mime);
            }
            catch
            {
                return ImagenNoDisponible();
            }
        }

        private ActionResult ImagenNoDisponible()
        {
            string[] rutas =
            {
        "~/images/NoDisponible.png",
        "~/images/Nodisponible.png",
        "~/Content/imagenes/Nodisponible.jpg",
        "~/Content/image/Nodisponible.png"
    };

            foreach (var ruta in rutas)
            {
                string path = Server.MapPath(ruta);

                if (System.IO.File.Exists(path))
                {
                    return File(path, System.Web.MimeMapping.GetMimeMapping(path));
                }
            }

            return new HttpStatusCodeResult(404, "Imagen no disponible");
        }









        #region "Diagrama por Asuntos"
        [HttpPost]
        public JsonResult GetAsuntosActivos()
        {
            var asuntos = MapaRelacionesService.GetAsuntosActivos();
            return Json(asuntos, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetRedPorAsunto(int int_id_asunto_relacionado)
        {
            string urlFotoObjetivo = Url.Action(
                "FotoObjetivoMapa",
                "MapaRelaciones",
                null,
                Request.Url.Scheme
            );

            string urlFotoVictima = Url.Action(
                "FotoVictimaMapa",
                "MapaRelaciones",
                null,
                Request.Url.Scheme
            );

            var red = MapaRelacionesService.GetRedPorAsunto(
                int_id_asunto_relacionado,
                urlFotoObjetivo,
                urlFotoVictima
            );

            var json = Json(red, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;

            return json;
        }

        [HttpGet]
        public ActionResult FotoVictimaMapa(int int_id_victima)
        {
            try
            {
                string foto = MapaRelacionesService.GetFotoVictimaMapa(int_id_victima);

                if (string.IsNullOrWhiteSpace(foto))
                {
                    return ImagenNoDisponible();
                }

                foto = foto.Trim();

                if (foto.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
                {
                    int coma = foto.IndexOf(",");
                    if (coma >= 0)
                    {
                        foto = foto.Substring(coma + 1);
                    }
                }

                foto = foto
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Replace(" ", "");

                byte[] bytes = Convert.FromBase64String(foto);

                string mime = "image/jpeg";

                if (bytes.Length > 4 && bytes[0] == 0x89 && bytes[1] == 0x50)
                {
                    mime = "image/png";
                }

                return File(bytes, mime);
            }
            catch
            {
                return ImagenNoDisponible();
            }
        }


        #endregion

        #region "Mapa por Objetivo"

        [HttpPost]
        public JsonResult GetObjetivosActivos()
        {
            var objetivos = MapaRelacionesService.GetObjetivosActivos();
            return Json(objetivos, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetRedPorObjetivo(int int_id_objetivo)
        {
            string urlFotoObjetivo = Url.Action(
                "FotoObjetivoMapa",
                "MapaRelaciones",
                null,
                Request.Url.Scheme
            );

            string urlFotoVictima = Url.Action(
                "FotoVictimaMapa",
                "MapaRelaciones",
                null,
                Request.Url.Scheme
            );

            var red = MapaRelacionesService.GetRedPorObjetivo(
                int_id_objetivo,
                urlFotoObjetivo,
                urlFotoVictima
            );

            var json = Json(red, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;

            return json;
        }

        #endregion


        #region "Album"

        [HttpPost]
        public JsonResult GetAlbumesActivos()
        {
            var albumes = MapaRelacionesService.GetAlbumesActivos();
            return Json(albumes, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetRedPorAlbum(int int_id_album_ficha_objetivo)
        {
            string urlFotoObjetivo = Url.Action(
                "FotoObjetivoMapa",
                "MapaRelaciones",
                null,
                Request.Url.Scheme
            );

            var red = MapaRelacionesService.GetRedPorAlbum(
                int_id_album_ficha_objetivo,
                urlFotoObjetivo
            );

            var json = Json(red, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;

            return json;
        }

        #endregion

    }
}