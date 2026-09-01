using Objetivos_Prioritarios.ControllersServices;
using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Models.Extends;
using Objetivos_Prioritarios.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.Controllers
{
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Error = false;
            return View(new LoginUser());
        }

        //[HttpPost] poncho LEO 2
        //[ValidateAntiForgeryToken]
        //public ActionResult Index([Bind(Include = "UserName,Password")] LoginUser user)
        //{
        //    if (user.UserName.ToUpper() == "ADMIN" && user.Password == "12345")
        //    {
        //        Session["User"] = new tb_Usuarios()
        //        {
        //            int_id_usuario=0,
        //            nvarchar_nombre_usuario="Usuario Administrador",
        //            nvarchar_no_interno=user.UserName,
        //            nvarchar_puesto="Administrador Sistema",
        //            Int_id_organismo=1,
        //            date_fecha_alta=DateTime.Now
        //        };
        //        return RedirectToAction("index", "Unidades");
        //    }
        //    else
        //    {
        //        var data = LoginService.validateCredentialsToaccesss(user.UserName, user.Password);

        //        if (!data.IsSuccess)
        //        {
        //            ViewBag.Error = true;
        //            ViewBag.Message = data.Message;
        //            return View();
        //        }

        //        Session["User"] = data.user;
        //        return RedirectToAction("index", "AEIC");
        //    }
        //}

        //  [HttpPost]
        public ActionResult LogOut()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction(
                "Index",
                "Login"
            );
        }

        //[HttpPost]
        //public JsonResult ValidateCredentials(LoginUser user)
        //{
        //    LoginService LoginService = new LoginService();
        //    var data = LoginService.validateCredentialsToaccesss(user.UserName, user.Password);
        //    if (data.IsSuccess == true)
        //    {
        //        data.user.UnidadId = data.Id;

        //        Session["User"] = data.user;
        //        data.user = null;
        //        return Json(data, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        return Json(data, JsonRequestBehavior.AllowGet);

        //    }


        //}


        private readonly AccesoService _accesoService = new AccesoService();

        [HttpPost]
        public JsonResult ValidateCredentials(LoginUser user)
        {
            LoginService loginService =
                new LoginService();

            var data =
                loginService
                    .validateCredentialsToaccesss(
                        user.UserName,
                        user.Password
                    );

            if (!data.IsSuccess)
            {
                return Json(
                    data,
                    JsonRequestBehavior.AllowGet
                );
            }


            data.user.UnidadId =
                data.Id;


            string login =
                data.user.nvarchar_no_interno;


            PermisosUsuarioDto permisos =
                _accesoService
                    .ObtenerPermisosUsuario(
                        login
                    );


            if (permisos == null)
            {
                return Json(
                    new
                    {
                        IsSuccess = false,
                        Message = "El usuario está autenticado correctamente, pero no tiene permisos asignados para ingresar al sistema."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }


            Session["User"] =
                data.user;

            Session["PermisosUsuario"] =
                permisos;


            string urlInicio =
                ObtenerUrlInicio(
                    permisos
                );


            return Json(
                new
                {
                    IsSuccess = true,
                    Message = data.Message,
                    UrlInicio = urlInicio
                },
                JsonRequestBehavior.AllowGet
            );
        }

        //[HttpPost]
        //public JsonResult ValidateCredentials(LoginUser user)
        //{
        //    LoginService loginService = new LoginService();

        //    var data =
        //        loginService.validateCredentialsToaccesss(
        //            user.UserName,
        //            user.Password
        //        );

        //    if (!data.IsSuccess)
        //    {
        //        return Json(
        //            data,
        //            JsonRequestBehavior.AllowGet
        //        );
        //    }

        //    data.user.UnidadId =
        //        data.Id;

        //    string login =
        //        data.user.nvarchar_no_interno;

        //    PermisosUsuarioDto permisos =
        //        _accesoService.ObtenerPermisosUsuario(
        //            login
        //        );

        //    if (permisos == null)
        //    {
        //        return Json(
        //            new
        //            {
        //                IsSuccess = false,
        //                Message = "Usuario autenticado correctamente, pero no tiene permisos asignados para ingresar al sistema."
        //            },
        //            JsonRequestBehavior.AllowGet
        //        );
        //    }

        //    Session["User"] =
        //        data.user;

        //    Session["PermisosUsuario"] =
        //        permisos;

        //    data.user =
        //        null;

        //    return Json(
        //        data,
        //        JsonRequestBehavior.AllowGet
        //    );
        //}

        public ActionResult CerrarSesion()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction(
                "Index",
                "Login"
            );
        }


        private string ObtenerUrlInicio(PermisosUsuarioDto permisos)
        {
            if (permisos == null)
            {
                return Url.Action("Index", "Login");
            }

            if (
                permisos.EsAdministrador ||
                permisos.Modulos.Any(x =>
                    string.Equals(
                        x,
                        "OBJETIVOS",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
            )
            {
                return Url.Action(
                    "Index",
                    "Objetivo"
                );
            }

            if (
                permisos.Modulos.Any(x =>
                    string.Equals(
                        x,
                        "SIC",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
            )
            {
                return Url.Action(
                    "Index",
                    "SIC"
                );
            }

            if (
                permisos.Modulos.Any(x =>
                    string.Equals(
                        x,
                        "BUSQUEDA_INTENCIONADA",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
            )
            {
                return Url.Action(
                    "BusquedaCoincidencias",
                    "SIC"
                );
            }

            if (
                permisos.Modulos.Any(x =>
                    string.Equals(
                        x,
                        "PERSONAS_INTERES",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
            )
            {
                return Url.Action(
                    "Index",
                    "PersonasInteres"
                );
            }

            if (
                permisos.Modulos.Any(x =>
                    string.Equals(
                        x,
                        "ADMIN_ACCESOS",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
            )
            {
                return Url.Action(
                    "Index",
                    "Accesos"
                );
            }

            return Url.Action(
                "CerrarSesion",
                "Login"
            );
        }




    }
}