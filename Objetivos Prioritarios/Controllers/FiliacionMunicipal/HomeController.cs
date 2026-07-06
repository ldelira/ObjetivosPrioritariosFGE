using FiliacionMunicipal.ControllerServices;
using FiliacionMunicipal.Models.ViewModels;
using FiliacionMunicipal.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using Web_SIPAEIC.Controllers;
using static FiliacionMunicipal.ControllerServices.CatalogoService;

namespace FiliacionMunicipal.Controllers
{
    [VerificarSesion]
    public class HomeController : ABaseController
    {
        public ActionResult Index()
        {

            return View();
        }
        //Captura Detenido
        public ActionResult CapturaDetenido()
        {
            ViewBag.List_Estudios = CatalogoService.List_Estudios();   
            ViewBag.List_Ocupacion = CatalogoService.List_Ocupacion();   
            ViewBag.List_Estado = CatalogoService.List_Estado();   
            ViewBag.List_Muni = CatalogoService.List_Muni();
            ViewBag.List_TipoFoto = CatalogoService.List_TipoFoto();
            ViewBag.List_FaltaAdministrativa = CatalogoService.List_FaltaAdministrativa();
            ViewBag.List_Corporaciones = CatalogoService.List_Corporaciones();
            ViewBag.Message = "Captura de Detenido.";
            return View();
        }
        public JsonResult GetMunicipios(int idEstado)
        {
            var lista = CatalogoService.List_Municipio_ByEstado(idEstado)
                .Select(m => new
                {
                    id = m.Cve_mun,
                    nombre = m.Municipio1
                }).ToList();

            return Json(lista, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetColonias(int idMunicipio)
        {
            var lista = CatalogoService.List_Colonia_ByMunicipio(idMunicipio)
                .Select(c => new
                {
                    id = c.Cve_col,
                    nombre = c.Colonia_Doctos
                }).ToList();

            return Json(lista, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCalle(int idColonia)
        {
            var lista = CatalogoService.List_Calle_ByColonia(idColonia)
                .Select(c => new
                {
                    id = c.Cve_Calle,
                    nombre = c.Calle
                }).ToList();

            return Json(lista, JsonRequestBehavior.AllowGet);
        }
        public JsonResult BuscarCalles(string texto)
        {
            var lista = CatalogoService.BuscarCallesFull(texto);
            return Json(lista, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GuardarDetenido(DetenidoVM model)
        {
            try
            {
                var service = new DetenidoService();
                // ARCHIVOS
                var fotos = Request.Files;
                var tiposFoto = Request.Form.GetValues("TiposFoto");
                var pdf = Request.Files["Documento"];


                service.Guardar(model, fotos, pdf,tiposFoto);

                return Json(new { IsSuccess = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    Inner = ex.InnerException?.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult BuscarDetenido(string nombre)
        {
            try
            {
                var service = new DetenidoService();
                var data = service.Buscar(nombre);

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    Inner = ex.InnerException?.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult VerificarFGEA(string nombre, string paterno, string materno)
        {
            try
            {
                var service = new DetenidoService();
                var ids = service.VerificarFGEA(nombre, paterno, materno);
                return Json(new { encontrado = ids.Count > 0, ids = ids });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    Inner = ex.InnerException?.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        //Historial
        public ActionResult Historial()
        {
            return View();
        }

        [HttpPost]
        public JsonResult ObtenerHistorial(string nombre,int? mun, string fechaInicio, string fechaFin)
        {
            try
            {
                var service = new HistorialService();

                var lista = service.ObtenerHistorial(nombre, mun, fechaInicio, fechaFin);

                return Json(new { data = lista });
            }
            catch (Exception ex)
            {
                return Json(new { data = new List<object>(), error = ex.Message });
            }
        }

        //FichaDetenido
        public ActionResult FichaDetenido(int id)
        {
            ViewBag.idPersona = id;
            return View();
        }

        [HttpPost]
        public JsonResult ObtenerFicha(int idPersona)
        {
            try
            {
                var service = new HistorialService();
                var data = service.ObtenerFichaCompleta(idPersona);

                // 🔥 aquí transformas las rutas
                data.Fotos = data.Fotos.Select(f => new FotoVM
                {
                    Ruta = Url.Action("ObtenerFoto", "Home", new { id = f.idFoto }),
                    TipoFoto = f.TipoFoto
                }).ToList();

                // Foto principal
                var principal = data.Fotos.FirstOrDefault(f => f.TipoFoto.Contains("Frontal"));

                if (principal != null)
                    data.FotoPrincipal = principal.Ruta;

                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = true,
                    mensaje = ex.Message,
                    detalle = ex.InnerException?.Message
                });
            }
        }

        public ActionResult ObtenerFoto(int id)
        {
            var service = new HistorialService();
            var foto = service.ObtenerFoto(id); // 🔥 usa el service

            if (foto == null || string.IsNullOrEmpty(foto.Base64))
                return File("~/images/sin-foto.png", "image/png");

            byte[] bytes = Convert.FromBase64String(foto.Base64);

            return File(bytes, "image/jpeg");
        }
    }
}