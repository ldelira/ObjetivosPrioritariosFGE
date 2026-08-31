using Objetivos_Prioritarios.ControllersServices;
using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Models.Extends;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Objetivos_Prioritarios.Controllers
{
    public class ABaseController : Controller
    {

        public tb_Usuarios UsuarioActual
        {
            get
            {
                return Session["User"] as tb_Usuarios;
            }
        }

        public PermisosUsuarioDto PermisosUsuario
        {
            get
            {
                return Session["PermisosUsuario"] as PermisosUsuarioDto;
            }
        }

        public bool EsAdministrador()
        {
            return
                PermisosUsuario != null &&
                PermisosUsuario.EsAdministrador;
        }

        public bool TieneModulo(string modulo)
        {
            if (
                PermisosUsuario == null ||
                string.IsNullOrWhiteSpace(modulo)
            )
            {
                return false;
            }

            return PermisosUsuario.Modulos.Any(x =>
                string.Equals(
                    x,
                    modulo,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }

        public bool TienePermiso(string permiso)
        {
            if (
                PermisosUsuario == null ||
                string.IsNullOrWhiteSpace(permiso)
            )
            {
                return false;
            }

            return PermisosUsuario.Permisos.Any(x =>
                string.Equals(
                    x,
                    permiso,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }

        public bool TieneFuenteBusqueda(int idFuente)
        {
            if (
                PermisosUsuario == null ||
                idFuente <= 0
            )
            {
                return false;
            }

            return PermisosUsuario
                .FuentesBusqueda
                .Contains(
                    idFuente
                );
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            /*
             * ============================================================
             * EVITAR CACHE DE PÁGINAS PROTEGIDAS
             * ============================================================
             */

            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddYears(-1));


            /*
             * ============================================================
             * VALIDAR SESIÓN
             * ============================================================
             */

            if (Session["User"] == null)
            {
                /*
                 * Si la llamada viene mediante AJAX, no regresamos
                 * la página completa del Login dentro de un PartialView.
                 */
                if (Request.IsAjaxRequest())
                {
                    filterContext.Result =
                        new HttpStatusCodeResult(
                            440,
                            "Sesión expirada"
                        );

                    return;
                }


                /*
                 * Petición normal:
                 *
                 * /SIC
                 * /PersonasInteres
                 * /Objetivo
                 * etc.
                 *
                 * Se manda directamente al Login.
                 */

                filterContext.Result =
                    new RedirectToRouteResult(
                        new RouteValueDictionary(
                            new
                            {
                                controller = "Login",
                                action = "Index"
                            }
                        )
                    );

                return;
            }


            base.OnActionExecuting(
                filterContext
            );
        }

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

        private ReporteService reporteService = null;
        public ReporteService ReporteService
        {
            get
            {
                if (reporteService == null)
                    reporteService = new ReporteService();
                return reporteService;
            }
        }

        private MapaRelacionesService mapaRelacionesService = null;
        public MapaRelacionesService MapaRelacionesService
        {
            get
            {
                if (mapaRelacionesService == null)
                    mapaRelacionesService = new MapaRelacionesService();
                return mapaRelacionesService;
            }
        }
        private PersonaInteresService personaInteresService = null;
        public PersonaInteresService PersonaInteresService
        {
            get
            {
                if (personaInteresService == null)
                    personaInteresService = new PersonaInteresService();
                return personaInteresService;
            }
        }


        private CoincidenciasBiometricasService coincidenciasBiometricasService = null;
        public CoincidenciasBiometricasService CoincidenciasBiometricasService
        {
            get
            {
                if (coincidenciasBiometricasService == null)
                    coincidenciasBiometricasService = new CoincidenciasBiometricasService();
                return coincidenciasBiometricasService;
            }
        }
        private ConfiguracionBusquedaService configuracionBusquedaService = null;
        public ConfiguracionBusquedaService ConfiguracionBusquedaService
        {
            get
            {
                if (configuracionBusquedaService == null)
                    configuracionBusquedaService = new ConfiguracionBusquedaService();
                return configuracionBusquedaService;
            }
        }

        private AccesoService accesoService = null;
        public AccesoService AccesoService
        {
            get
            {
                if (accesoService == null)
                    accesoService = new AccesoService();
                return accesoService;
            }
        }

    }
}