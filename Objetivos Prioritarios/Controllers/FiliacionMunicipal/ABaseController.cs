using Objetivos_Prioritarios.ControllerServices;
using Objetivos_Prioritarios.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Objetivos_Prioritarios.ControllersServices;

namespace Web_SIPAEIC.Controllers
{
    public class ABaseController : Controller
    {
        private LoginService loginService = null;
        public LoginService LoginService
        {
            get
            {
                if (loginService == null)
                    loginService = new LoginService();
                return loginService;
            }
        }
        private CatalogoService catalogoservice = null;
        public CatalogoService CatalogoService
        {
            get
            {
                if (catalogoservice == null)
                    catalogoservice = new CatalogoService();
                return catalogoservice;
            }
        }

    }
}