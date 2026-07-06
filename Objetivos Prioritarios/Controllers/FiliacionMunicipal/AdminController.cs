using FiliacionMunicipal.ControllerServices;
using FiliacionMunicipal.Models;
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
    public class AdminController : ABaseController
    {
        public ActionResult AdminIndex()
        {
            return View();
        }


        public ActionResult Usuarios()
        {
            return View();
        }
        public JsonResult ListarUsuarios(bool ban)
        {
            try
            {
                var service = new AdminService();
                var lista = service.ListarUsuarios(ban);


                return Json(new { data = lista }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw; // 👈 temporal para ver el error real
            }
        }

        [HttpPost]
        public JsonResult GuardarUsuario(UsuarioVM model)
        {
            try
            {
                var service = new AdminService();
                service.Guardar(model);

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

        [HttpPost]
        public JsonResult EstatusUsuario(int id, bool ban)
        { 
            try
            {
                var service = new AdminService();
                service.Estatus(id,ban,"tb_Usuario");
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


        public ActionResult Alertas()
        {
            return View();
        }
        public JsonResult ListarAlertas()
        {
            try
            {
                var service = new AdminService();
                var lista = service.ListarAlertas();

                return Json(new { data = lista }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw; 
            }
        }


        public ActionResult SedesPoliciales()
        {
            ViewBag.List_Muni = CatalogoService.List_Muni();
            return View();
        }
        public JsonResult ListarSedes()
        {
            try
            {
                var service = new AdminService();
                var lista = service.ListarSedes();

                return Json(new { data = lista }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult GuardarSede(SedesVM model)
        {
            try
            {
                var service = new AdminService();
                service.GuardarSedes(model);

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

        [HttpPost]
        public JsonResult EstatusSede(int id, bool ban)
        {
            try
            {
                var service = new AdminService();
                service.Estatus(id, ban, "cat_SedesPoliciales");
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

        public JsonResult UsuariosSede(int IDsede)
        {
            try
            {
                var service = new AdminService();
                var lista = service.UsuariosSede(IDsede);

                return Json(new { data = lista }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult AgregarUsuarioSede(UsuarioSedeVM model)
        {
            try
            {
                var service = new AdminService();
                service.AgregarUsuarioSede(model);

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
        [HttpPost]
        public JsonResult EstatusSedeU(int id, bool ban)
        {
            try
            {
                var service = new AdminService();
                service.Estatus(id, ban, "tb_UsuarioSede");
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

        [HttpPost]
        public JsonResult DesP(string password)
        {
            try
            {
                string texto = AdminService.Des(password); // 👈 TU FUNCIÓN

                return Json(new { ok = true, password = texto });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
        }

    }
}