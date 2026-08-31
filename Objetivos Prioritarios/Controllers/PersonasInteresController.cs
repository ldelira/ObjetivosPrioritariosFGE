using DocumentFormat.OpenXml.Spreadsheet;
using Objetivos_Prioritarios.ControllersServices;
using Objetivos_Prioritarios.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.Controllers
{
    public class PersonasInteresController : ABaseController
    {
 
        public ActionResult Index()
        {
            ViewBag.TiempoEsperaBusqueda =ConfiguracionBusquedaService.ObtenerTiempoEspera();
            ViewBag.Title = "Personas de Interés";
            return View();
        }

        [HttpPost]
        public JsonResult ActualizarTiempoEsperaBusqueda(int valor)
        {
            try
            {
                if (
                    valor < 0 ||
                    valor > 300
                )
                {
                    return Json(
                        new
                        {
                            success = false,
                            message = "El valor debe estar entre 0 y 300 segundos."
                        }
                    );
                }

                bool actualizado =
                    ConfiguracionBusquedaService
                        .ActualizarTiempoEspera(
                            valor
                        );

                if (!actualizado)
                {
                    return Json(
                        new
                        {
                            success = false,
                            message = "No fue posible actualizar el valor de JCerdan."
                        }
                    );
                }

                return Json(
                    new
                    {
                        success = true,
                        valor = valor,
                        message = "Tiempo actualizado correctamente."
                    }
                );
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        success = false,
                        message = ex.Message
                    }
                );
            }
        }

        public ActionResult AddEditPersonaInteres(int? idPersona)
        {
            ViewBag.Title = "Ficha de Persona de Interés";

            tb_Persona model = new tb_Persona
            {
                idPersona = 0,
                Nombre = "",
                Paterno = "",
                Materno = "",
                FechaRegistro = DateTime.Now,
                UsuarioRegistro = 0
            };

            if (idPersona.HasValue && idPersona.Value > 0)
            {
                var persona = PersonaInteresService.GetPersonaInteresById(idPersona.Value);

                if (persona != null)
                {
                    model = persona;
                }
            }

            ViewBag.EdadAproximada = PersonaInteresService.GetEdadAproximada(model.FechaNacimiento);
            ViewBag.NombreMostrar = PersonaInteresService.GetNombreMostrar(model);

            return View(model);
        }

        [HttpPost]
        public JsonResult FillPersonasInteresList()
        {
            var lista = PersonaInteresService.GetPersonasInteresList();

            var result = lista.Select(x =>
            {
                var nombreMostrar = PersonaInteresService.GetNombreMostrar(x);
                var edad = PersonaInteresService.GetEdadAproximada(x.FechaNacimiento);

                return new
                {
                    idPersona = x.idPersona,
                    nombreMostrar = nombreMostrar,
                    nombre = x.Nombre,
                    paterno = x.Paterno,
                    materno = x.Materno,
                    edadAproximada = edad.HasValue ? edad.Value + " años" : "Sin edad",
                    sexo = string.IsNullOrWhiteSpace(x.Sexo) ? "Sin dato" : x.Sexo,
                    estatura = string.IsNullOrWhiteSpace(x.Estatura) ? "Sin dato" : x.Estatura,
                    observaciones = x.Observaciones,
                    observacionesCorta = string.IsNullOrWhiteSpace(x.Observaciones)
                        ? ""
                        : (x.Observaciones.Length > 120 ? x.Observaciones.Substring(0, 120) + "..." : x.Observaciones),
                    fechaRegistro = x.FechaRegistro.HasValue ? x.FechaRegistro.Value.ToString("dd-MM-yyyy HH:mm") : ""
                };
            }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SavePersonaInteres(
            int idPersona,
            string nombre,
            string paterno,
            string materno,
            int? edadAproximada,
            DateTime? fechaNacimientoExacta,
            string estatura,
            string sexo,
            string observaciones)
        {
            int usuarioRegistro = 0;

            var resp = PersonaInteresService.SavePersonaInteres(
                idPersona,
                nombre,
                paterno,
                materno,
                edadAproximada,
                fechaNacimientoExacta,
                estatura,
                sexo,
                observaciones,
                usuarioRegistro
            );

            return Json(resp, JsonRequestBehavior.AllowGet);
        }


        // [HttpPost]
        //public JsonResult FillFotografiasPersonaInteres(int idPersona)
        //{
        //    var lista = PersonaInteresService.GetFotografiasPersonaInteres(idPersona);

        //    var result = lista.Select(x => new
        //    {
        //        idFoto = x.idFoto,
        //        idPersona = x.idPersona,
        //        idTipoFoto = x.idTipoFoto,
        //        tipoFoto = PersonaInteresService.GetTipoFotoTexto(x.idTipoFoto),
        //        rutaArchivo = x.RutaArchivo,
        //        archivoB64 = string.IsNullOrWhiteSpace(x.ArchivoB64)
        //            ? ""
        //            : "data:image/jpeg;base64," + x.ArchivoB64,
        //        fechaRegistro = x.FechaRegistro.HasValue
        //            ? x.FechaRegistro.Value.ToString("dd-MM-yyyy HH:mm")
        //            : ""
        //    }).ToList();

        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public JsonResult FillFotografiasPersonaInteres(int idPersona)
        {
            var lista =
                PersonaInteresService
                    .GetFotografiasPersonaInteres(
                        idPersona
                    );

            var result =
                lista
                    .Select(x => new
                    {
                        idFoto =
                            x.idFoto,

                        idPersona =
                            x.idPersona,

                        idTipoFoto =
                            x.idTipoFoto,

                        tipoFoto =
                            PersonaInteresService
                                .GetTipoFotoTexto(
                                    x.idTipoFoto
                                ),

                        fotoUrl =
                            Url.Action(
                                "VerFotoPersonaInteres",
                                "PersonasInteres",
                                new
                                {
                                    idFoto = x.idFoto,
                                    v = x.FechaRegistro.HasValue
                                        ? x.FechaRegistro.Value.Ticks
                                        : DateTime.Now.Ticks
                                }
                            ),

                        fechaRegistro =
                            x.FechaRegistro.HasValue
                                ? x.FechaRegistro.Value
                                    .ToString(
                                        "dd-MM-yyyy HH:mm"
                                    )
                                : ""
                    })
                    .ToList();

            return Json(
                result,
                JsonRequestBehavior.AllowGet
            );
        }


        [HttpPost]
        public JsonResult SaveFotografiaPersonaInteres()
        {
            int idPersona = 0;
            int idTipoFoto = 1;
            int usuarioRegistro = 0;

            int.TryParse(Request.Form["idPersona"], out idPersona);
            int.TryParse(Request.Form["idTipoFoto"], out idTipoFoto);

            var archivo = Request.Files["foto"];

            var resp = PersonaInteresService.SaveFotografiaPersonaInteres(
                idPersona,
                idTipoFoto,
                archivo,
                usuarioRegistro
            );

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult FotografiasPersonaInteresPartial()
        {
            return PartialView();
        }


        public PartialViewResult HuellasPersonaInteresPartial()
        {
            return PartialView();
        }

        //[HttpPost]
        //public JsonResult FillHuellasPersonaInteres(int idPersona)
        //{
        //    var lista = PersonaInteresService.GetHuellasPersonaInteres(idPersona);

        //    var result = lista.Select(x => new
        //    {
        //        idFicha = x.idFicha,
        //        idPersona = x.idPersona,
        //        rutaHuella = x.RutaHuella,
        //        huellaB64 = PersonaInteresService.GetHuellaBase64DesdeRuta(x.RutaHuella),
        //        fechaRegistro = x.FechaRegistro.HasValue
        //            ? x.FechaRegistro.Value.ToString("dd-MM-yyyy HH:mm")
        //            : "",
        //        activo = x.Activo
        //    }).ToList();

        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public JsonResult FillHuellasPersonaInteres(int idPersona)
        {
            var lista = PersonaInteresService.GetHuellasPersonaInteres(idPersona);

            var result = lista.Select(x => new
            {
                idFicha = x.idFicha,
                idPersona = x.idPersona,
                rutaHuella = x.RutaHuella,
                fechaRegistro = x.FechaRegistro.HasValue
                    ? x.FechaRegistro.Value.ToString("dd-MM-yyyy HH:mm")
                    : "",
                activo = x.Activo
            }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult SaveHuellasPersonaInteres()
        {
            int idPersona = 0;
            int usuarioRegistro = 0;

            int.TryParse(Request.Form["idPersona"], out idPersona);

            var archivos = new List<HttpPostedFileBase>();

            for (int i = 0; i < Request.Files.Count; i++)
            {
                var archivo = Request.Files[i];

                if (archivo != null && archivo.ContentLength > 0)
                {
                    archivos.Add(archivo);
                }
            }

            var resp = PersonaInteresService.SaveHuellasPersonaInteres(
                idPersona,
                archivos,
                usuarioRegistro
            );

            return Json(resp, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult VerHuellaPersonaInteres(int idFicha)
        {
            var resp = PersonaInteresService.GetHuellaArchivoPersonaInteres(idFicha);

            if (resp == null || !resp.IsSuccess || resp.Bytes == null || resp.Bytes.Length == 0)
            {
                return HttpNotFound(resp == null ? "No se encontró la imagen." : resp.Message);
            }

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            return File(resp.Bytes, resp.MimeType);
        }


        [HttpGet]
        public ActionResult VerFotoPrincipalPersonaInteres(int idPersona)
        {
            var resp = PersonaInteresService.GetFotoPrincipalPersonaInteres(idPersona);

            if (resp == null || !resp.IsSuccess || resp.Bytes == null || resp.Bytes.Length == 0)
            {
                return HttpNotFound(resp == null ? "No se encontró la fotografía." : resp.Message);
            }

            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            return File(resp.Bytes, resp.MimeType);
        }


        public PartialViewResult PeriodosBusquedaPersonaInteresPartial()
        {
            return PartialView();
        }

        [HttpPost]
        public JsonResult FillPeriodosBusquedaPersonaInteres(int idPersona)
        {
            var lista = PersonaInteresService.GetHistorialPeriodosBusqueda(idPersona);

            var result = lista.Select(x =>
            {
                int diasTotales = PersonaInteresService.CalcularDiasTotales(
                    x.FechaInicioBusqueda,
                    x.FechaFinBusqueda
                );

                int diasRestantes = PersonaInteresService.CalcularDiasRestantes(
                    x.FechaFinBusqueda
                );

                string estatusVisual = PersonaInteresService.GetEstatusVisualPeriodo(x);

                string estatusCatalogo = "";

                if (x.cat_EstatusPeriodoBusqueda != null)
                {
                    estatusCatalogo = x.cat_EstatusPeriodoBusqueda.Nombre;
                }

                return new
                {
                    idPeriodoBusqueda = x.idPeriodoBusqueda,
                    idPersona = x.idPersona,

                    fechaInicioBusqueda = x.FechaInicioBusqueda.ToString("dd-MM-yyyy"),
                    fechaFinBusqueda = x.FechaFinBusqueda.HasValue
                        ? x.FechaFinBusqueda.Value.ToString("dd-MM-yyyy")
                        : "",

                    fechaInicioInput = x.FechaInicioBusqueda.ToString("yyyy-MM-dd"),
                    fechaFinInput = x.FechaFinBusqueda.HasValue
                        ? x.FechaFinBusqueda.Value.ToString("yyyy-MM-dd")
                        : "",

                    diasTotales = diasTotales,
                    diasRestantes = diasRestantes,

                    activo = x.Activo,

                    idEstatusPeriodoBusqueda = x.idEstatusPeriodoBusqueda,
                    estatusCatalogo = estatusCatalogo,
                    estatusVisual = estatusVisual,

                    observaciones = x.Observaciones,
                    motivoCancelacion = x.MotivoCancelacion,

                    fechaRegistro = x.FechaRegistro.ToString("dd-MM-yyyy HH:mm")
                };
            }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SavePeriodoBusquedaPersonaInteres(
            int idPersona,
            DateTime fechaInicioBusqueda,
            DateTime? fechaFinBusqueda,
            string observaciones)
        {
            int usuarioRegistro = 0;

            var resp = PersonaInteresService.SavePeriodoBusquedaPersonaInteres(
                idPersona,
                fechaInicioBusqueda,
                fechaFinBusqueda,
                observaciones,
                usuarioRegistro
            );

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CancelarPeriodoBusquedaPersonaInteres(
            int idPeriodoBusqueda,
            string motivoCancelacion)
        {
            int usuarioCancelacion = 0;

            var resp = PersonaInteresService.CancelarPeriodoBusquedaPersonaInteres(
                idPeriodoBusqueda,
                motivoCancelacion,
                usuarioCancelacion
            );

            return Json(resp, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult VerFotoPersonaInteres(int idFoto)
        {
            var resp =
                PersonaInteresService
                    .GetFotoArchivoPersonaInteres(
                        idFoto
                    );

            if (
                resp == null ||
                !resp.IsSuccess ||
                resp.Bytes == null ||
                resp.Bytes.Length == 0
            )
            {
                return HttpNotFound(
                    resp == null
                        ? "No se encontró la fotografía."
                        : resp.Message
                );
            }

            Response.Cache.SetCacheability(
                HttpCacheability.NoCache
            );

            Response.Cache.SetNoStore();

            return File(
                resp.Bytes,
                resp.MimeType
            );
        }




    }
}