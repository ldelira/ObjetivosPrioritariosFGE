using Objetivos_Prioritarios.ControllersServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.Controllers
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
        private ObjetivoService objetivoService = null;
        public ObjetivoService ObjetivoService
        {
            get
            {
                if (objetivoService == null)
                    objetivoService = new ObjetivoService();
                return objetivoService;
            }
        }
        private CatalogoService catalogoService = null;
        public CatalogoService CatalogoService
        {
            get
            {
                if (catalogoService == null)
                    catalogoService = new CatalogoService();
                return catalogoService;
            }
        }

        private AsuntoService asuntoService = null;
        public AsuntoService AsuntoService
        {
            get
            {
                if (asuntoService == null)
                    asuntoService = new AsuntoService();
                return asuntoService;
            }
        }
        private FichaObjetivoService fichaObjetivoService = null;
        public FichaObjetivoService FichaObjetivoService
        {
            get
            {
                if (fichaObjetivoService == null)
                    fichaObjetivoService = new FichaObjetivoService();
                return fichaObjetivoService;
            }
        }

    }
}