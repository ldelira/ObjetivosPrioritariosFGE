using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult AdminDashboard()
        {
            // ViewBag.JsonDataSet = JsonConvert.SerializeObject(DashboardService.getMonthlySurvey(UserInfo.CustomerId));
            ViewBag.Module = "Panel";
            ViewBag.Page = "Principal";
            ViewBag.TitlePage = "Dashboard";

            return View();
            //return View();
        }
    }
}