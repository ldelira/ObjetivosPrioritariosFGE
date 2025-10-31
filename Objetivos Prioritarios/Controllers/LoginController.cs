using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.Controllers
{
    public class LoginController : ABaseController
    {
        public ActionResult Index()
        {
            ViewBag.Error = false;
            return View(new LoginUser());
        }

        //[HttpPost]
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
            Session["User"] = null;
            return RedirectToAction("index", "Login");
        }

        [HttpPost]
        public JsonResult ValidateCredentials(LoginUser user)
        {

            var data = LoginService.validateCredentialsToaccesss(user.UserName, user.Password);
            if (data.IsSuccess == true)
            {
                data.user.UnidadId = data.Id;

                Session["User"] = data.user;
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