using Objetivos_Prioritarios.ControllersServices;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Objetivos_Prioritarios.Helpers;

namespace Objetivos_Prioritarios.Controllers
{
    public class SICController : Controller
    {
        private readonly FiliacionMunicipalService _filiacionService;


        private const string RutaBaseFotosC5 = @"\\10.13.1.232\detenidos";
        private const string RutaBaseFotosCapea = @"\\221fgea.pgj.gob\www\FGE";

        public SICController()
        {
            _filiacionService = new FiliacionMunicipalService();
        }

        public ActionResult Index()
        {
            ViewBag.Title = "SIC";

            return View();
        }

        [HttpGet]
        public JsonResult ObtenerSIC()
        {
            try
            {
                var alertas = _filiacionService.GetAlertas();

                return Json(new
                {
                    success = true,
                    data = alertas
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Ocurrió un error al obtener las alertas del SIC: " + ex.Message,
                    data = new object[] { }
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DetalleDetenidoC5(int idDetenido)
        {
            var detenido = _filiacionService.GetInfoDetenido(idDetenido);

            var detencionC5 = _filiacionService.GetInfoDetencionC5(Convert.ToInt32(detenido.IDDETENCION));

            ViewBag.DetencionC5 = detencionC5;

            var fotosC5 = _filiacionService.GetFotosDetenidoC5(idDetenido);
            ViewBag.FotosC5 = fotosC5;

            var huellasC5 = _filiacionService.GetHuellasDetenidoC5(idDetenido);
            ViewBag.HuellasC5 = huellasC5;

            var rasgosC5 = _filiacionService.GetRasgosDetenidoC5(idDetenido);
            ViewBag.RasgosC5 = rasgosC5;

            var conteoAlertas = _filiacionService.GetConteoAlertasPorEstatus(idDetenido);

            ViewBag.TotalAlertasActivas = conteoAlertas.Item1;
            ViewBag.TotalAlertasRevisadas = conteoAlertas.Item2;

            var tiposAlertas = _filiacionService.GetTiposAlertas(idDetenido);
            ViewBag.TiposAlertas = tiposAlertas;

            if (detenido == null)
            {
                return HttpNotFound("No se encontró información del detenido.");
            }

            var datosDetenidoC5 = _filiacionService.GetDatosDetenidoC5(idDetenido);
            ViewBag.DatosDetenidoC5 = datosDetenidoC5;

            var alertasTipo = _filiacionService.GetAlertaTipo(idDetenido);

            var idsCapea = alertasTipo
                .Where(x => x.Item2 == 2)
                .Select(x => x.Item1)
                .Distinct()
                .ToList();

            var idsPersonaInteres = alertasTipo
                .Where(x => x.Item2 == 3)
                .Select(x => x.Item1)
                .Distinct()
                .ToList();

            var idsMandamientos = alertasTipo
                .Where(x => x.Item2 == 4)
                .Select(x => x.Item1)
                .Distinct()
                .ToList();

            var idsObjetivosPrioritarios = alertasTipo
                .Where(x => x.Item2 == 5)
                .Select(x => x.Item1)
                .Distinct()
                .ToList();

            var idsFgeaDetenidos = alertasTipo
                .Where(x => x.Item2 == 6)
                .Select(x => x.Item1)
                .Distinct()
                .ToList();

            // Aquí ya llamas tus métodos del servicio
            ViewBag.Capeas = _filiacionService.GetInfoCapeas(idsCapea);
            ViewBag.Mandamientos = _filiacionService.GetInfoMandamientos(idsMandamientos);

            // Después harías lo mismo con las demás fuentes cuando tengas sus funciones:
            // ViewBag.PersonasInteres = idsPersonaInteres.Select(id => _filiacionService.GetInfoPersonaInteres(id)).Where(x => x != null).ToList();
            // ViewBag.Mandamientos = idsMandamientos.Select(id => _filiacionService.GetInfoMandamiento(id)).Where(x => x != null).ToList();
            // ViewBag.ObjetivosPrioritarios = idsObjetivosPrioritarios.Select(id => _filiacionService.GetInfoObjetivoPrioritario(id)).Where(x => x != null).ToList();
            // ViewBag.FgeaDetenidos = idsFgeaDetenidos.Select(id => _filiacionService.GetInfoFgeaDetenido(id)).Where(x => x != null).ToList();

            ViewBag.AlertasTipo = alertasTipo;

            return View(detenido);
        }

        public ActionResult FotoDetenidoC5(int idDetenido)
        {
            var detenido = _filiacionService.GetInfoDetenido(idDetenido);

            if (detenido == null)
            {
                return HttpNotFound();
            }

            string rutaFotoBD = "";
            var propiedadFoto = detenido.GetType().GetProperty("Foto");

            if (propiedadFoto != null)
            {
                var valorFoto = propiedadFoto.GetValue(detenido, null);
                rutaFotoBD = valorFoto == null ? "" : valorFoto.ToString();
            }

            if (string.IsNullOrWhiteSpace(rutaFotoBD))
            {
                return HttpNotFound();
            }

            rutaFotoBD = rutaFotoBD
                .Trim()
                .Trim('"')
                .Trim()
                .TrimStart('/', '\\')
                .Replace("/", "\\");

            string rutaCompleta = Path.Combine(RutaBaseFotosC5, rutaFotoBD);
            string extension = Path.GetExtension(rutaCompleta).ToLower();
            string contentType = ObtenerContentType(extension);

            try
            {
                byte[] fileBytes;
                // Se abre la conexión con credenciales
                using (new NetworkConnection(RutaBaseFotosC5, @"Fiscalia1", "A1b2c3d4"))
                {
                    if (!System.IO.File.Exists(rutaCompleta))
                    {
                        return HttpNotFound();
                    }
                    // Se lee el archivo mientras la conexión está activa
                    fileBytes = System.IO.File.ReadAllBytes(rutaCompleta);
                }

                return File(fileBytes, contentType);
            }
            catch (Exception)
            {
                return HttpNotFound("Error al acceder al servidor de imágenes C5.");
            }
        }


        public ActionResult FotoGaleriaC5(int idFoto)
        {
            var foto = _filiacionService.GetFotoC5PorId(idFoto);

            if (foto == null)
            {
                return HttpNotFound();
            }

            string rutaFotoBD = foto.FOTO;

            if (string.IsNullOrWhiteSpace(rutaFotoBD))
            {
                return HttpNotFound();
            }

            rutaFotoBD = rutaFotoBD
                .Trim()
                .Trim('"')
                .Trim()
                .TrimStart('/', '\\')
                .Replace("/", "\\");

            string rutaCompleta = Path.Combine(RutaBaseFotosC5, rutaFotoBD);
            string extension = Path.GetExtension(rutaCompleta).ToLower();
            string contentType = ObtenerContentType(extension);

            try
            {
                byte[] fileBytes;
                // Se abre la conexión con credenciales
                using (new NetworkConnection(RutaBaseFotosC5, @"Fiscalia1", "A1b2c3d4"))
                {
                    if (!System.IO.File.Exists(rutaCompleta))
                    {
                        return HttpNotFound();
                    }
                    // Se lee el archivo mientras la conexión está activa
                    fileBytes = System.IO.File.ReadAllBytes(rutaCompleta);
                }

                return File(fileBytes, contentType);
            }
            catch (Exception)
            {
                return HttpNotFound("Error al acceder al servidor de imágenes C5.");
            }
        }


        public ActionResult HuellaGaleriaC5(int idHuella)
        {
            var huella = _filiacionService.GetHuellaC5PorId(idHuella);

            if (huella == null)
            {
                return HttpNotFound();
            }

            string rutaHuellaBD = huella.Huellas;

            if (string.IsNullOrWhiteSpace(rutaHuellaBD))
            {
                return HttpNotFound();
            }

            rutaHuellaBD = rutaHuellaBD
                .Trim()
                .Trim('"')
                .Trim()
                .TrimStart('/', '\\')
                .Replace("/", "\\");

            string rutaCompleta = Path.Combine(RutaBaseFotosC5, rutaHuellaBD);
            string extension = Path.GetExtension(rutaCompleta).ToLower();
            string contentType = ObtenerContentType(extension);

            try
            {
                byte[] fileBytes;
                // Se abre la conexión con credenciales
                using (new NetworkConnection(RutaBaseFotosC5, @"Fiscalia1", "A1b2c3d4"))
                {
                    if (!System.IO.File.Exists(rutaCompleta))
                    {
                        return HttpNotFound();
                    }
                    // Se lee el archivo mientras la conexión está activa
                    fileBytes = System.IO.File.ReadAllBytes(rutaCompleta);
                }

                return File(fileBytes, contentType);
            }
            catch (Exception)
            {
                return HttpNotFound("Error al acceder al servidor de imágenes C5.");
            }
        }

        [HttpPost]
        public JsonResult ApagarNotificacion(int idDetenido, int idOrigen, int idFuente)
        {
            try
            {
                int totalApagadas = _filiacionService.ApagarNotificacion(idDetenido, idOrigen, idFuente);

                return Json(new
                {
                    success = true,
                    message = totalApagadas > 0
                        ? "Notificación apagada correctamente."
                        : "No se encontró una notificación activa para apagar.",
                    totalApagadas = totalApagadas
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Ocurrió un error al apagar la notificación: " + ex.Message
                });
            }
        }

        private string ObtenerContentType(string extension)
        {
            switch (extension)
            {
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                case ".bmp": return "image/bmp";
                case ".webp": return "image/webp";
                default: return "image/jpeg";
            }
        }


        public ActionResult FotoCapea(int idCapea)
        {
            var capea = _filiacionService.GetCapeaPorId(idCapea);

            if (capea == null)
            {
                return HttpNotFound();
            }

            string rutaFotoBD = capea.url_imagen;

            if (string.IsNullOrWhiteSpace(rutaFotoBD))
            {
                return HttpNotFound();
            }

            rutaFotoBD = rutaFotoBD
                .Trim()
                .Trim('"')
                .Trim()
                .TrimStart('/', '\\')
                .Replace("/", "\\");

            string rutaCompleta = Path.Combine(RutaBaseFotosCapea, rutaFotoBD);

            if (!System.IO.File.Exists(rutaCompleta))
            {
                return HttpNotFound();
            }

            string extension = Path.GetExtension(rutaCompleta).ToLower();
            string contentType = "image/jpeg";

            if (extension == ".png")
            {
                contentType = "image/png";
            }
            else if (extension == ".gif")
            {
                contentType = "image/gif";
            }
            else if (extension == ".bmp")
            {
                contentType = "image/bmp";
            }
            else if (extension == ".webp")
            {
                contentType = "image/webp";
            }

            return File(rutaCompleta, contentType);
        }


        [HttpPost]
        public JsonResult ReactivarNotificacion(int idDetenido, int idOrigen, int idFuente)
        {
            try
            {
                int totalReactivadas = _filiacionService.ReactivarNotificacion(idDetenido, idOrigen, idFuente);

                return Json(new
                {
                    success = true,
                    message = totalReactivadas > 0
                        ? "Notificación reactivada correctamente."
                        : "No se encontró una notificación desactivada para reactivar.",
                    totalReactivadas = totalReactivadas
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Ocurrió un error al reactivar la notificación: " + ex.Message
                });
            }
        }


        public ContentResult DiagnosticoFotoCapea(int idCapea)
        {
            try
            {
                var capea = _filiacionService.GetCapeaPorId(idCapea);

                if (capea == null)
                {
                    return Content("No se encontró CAPEA con id: " + idCapea);
                }

                string rutaFotoBD = capea.url_imagen;

                if (string.IsNullOrWhiteSpace(rutaFotoBD))
                {
                    return Content("El campo url_imagen viene vacío.");
                }

                string rutaLimpia = rutaFotoBD
                    .Trim()
                    .Trim('"')
                    .Trim()
                    .TrimStart('/', '\\')
                    .Replace("/", "\\");

                string rutaCompleta = Path.Combine(RutaBaseFotosCapea, rutaLimpia);

                bool existe = System.IO.File.Exists(rutaCompleta);

                string texto =
                    "ID CAPEA: " + idCapea + "\n" +
                    "url_imagen BD: " + rutaFotoBD + "\n" +
                    "Ruta limpia: " + rutaLimpia + "\n" +
                    "Ruta base: " + RutaBaseFotosCapea + "\n" +
                    "Ruta completa: " + rutaCompleta + "\n" +
                    "File.Exists: " + existe;

                return Content(texto, "text/plain");
            }
            catch (Exception ex)
            {
                return Content("ERROR:\n" + ex.ToString(), "text/plain");
            }
        }



    }

}