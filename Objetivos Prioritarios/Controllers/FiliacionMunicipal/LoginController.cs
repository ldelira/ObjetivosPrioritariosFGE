using FiliacionMunicipal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web_SIPAEIC.Controllers;
using Web_SIPAEIC.Utils;

namespace FiliacionMunicipal.Controllers
{
    public class LoginController : ABaseController
    {
        public ActionResult Index()
        {
            ViewBag.Error = false;
            return View(new LoginUser());
        }

        public ActionResult LogOut()
        {
            Session["UserName"] = null;
            return RedirectToAction("index", "Login");
        }

        [HttpPost]
        public JsonResult ValidateCredentials(LoginUser user)
        {

            var data = LoginService.validateCredentialsToaccesss(user.UserName, user.Password);
            if (data.IsSuccess == true)
            {
                Session["UserName"] = data.user;
                Session["IdSedeM"] = data.IdSede;
                Session["idUsuario"] = data.user.idUsuario;
                ViewBag.Municipio = data.Mun;
                Session["Mun"] = data.Mun;

                data.user = null;
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(data, JsonRequestBehavior.AllowGet);

            }


        }
    }
}
