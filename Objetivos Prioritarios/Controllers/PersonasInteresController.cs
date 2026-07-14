using DocumentFormat.OpenXml.Spreadsheet;
using Objetivos_Prioritarios.ControllersServices;
using Objetivos_Prioritarios.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.Controllers
{
    public class PersonasInteresController : ABaseController
    {
 
        public ActionResult Index()
        {
            ViewBag.Title = "Personas de Interés";
            return View();
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
    }
}