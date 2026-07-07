using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Objetivos_Prioritarios.Models;

namespace Objetivos_Prioritarios.Controllers
{
    public class SICController : Controller
    {
        #region "Vista principal SIC"

        public ActionResult Index()
        {
            ViewBag.Title = "SIC";
            return View();
        }

        #endregion


        #region "Obtener información SIC"

        [HttpGet]
        public JsonResult ObtenerSIC()
        {
            try
            {
                var data = new List<object>
                {
                    new
                    {
                        Id = 1,
                        Nombre = "JUAN PÉREZ LÓPEZ",
                        Fuente = "SIC",
                        Estatus = "ACTIVO",
                        Detenciones = 2
                    },
                    new
                    {
                        Id = 2,
                        Nombre = "PEDRO RAMÍREZ GARCÍA",
                        Fuente = "SIC",
                        Estatus = "INACTIVO",
                        Detenciones = 0
                    }
                };

                return Json(new { data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    data = new List<object>(),
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}