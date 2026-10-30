using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Models.Extends;
using Objetivos_Prioritarios.Utils;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.Controllers
{
    public class AccesosController : ABaseController
    {
        public ActionResult Index()
        {
            if (!PuedeAdministrarAccesos())
            {
                return new HttpStatusCodeResult(403);
            }

            return View();
        }

        [HttpGet]
        public JsonResult ListarUsuarios()
        {
            if (!PuedeAdministrarAccesos())
            {
                Response.StatusCode = 403;
                return Json(new { success = false, message = "No tiene permiso para administrar accesos." }, JsonRequestBehavior.AllowGet);
            }

            return Json(
                new
                {
                    success = true,
                    usuarios = AccesoService.ObtenerUsuariosRegistrados()
                },
                JsonRequestBehavior.AllowGet
            );
        }

        [HttpGet]
        public JsonResult ListarBitacora(string usuario = "", int limite = 200)
        {
            if (!PuedeAdministrarAccesos())
            {
                Response.StatusCode = 403;
                return Json(new { success = false, message = "No tiene permiso para consultar la bitácora." }, JsonRequestBehavior.AllowGet);
            }

            return Json(
                new
                {
                    success = true,
                    registros = BitacoraBusquedaService.ObtenerBitacora(usuario, limite)
                },
                JsonRequestBehavior.AllowGet
            );
        }

        [HttpGet]
        public JsonResult DetalleBitacora(long idBitacoraBusqueda)
        {
            if (!PuedeAdministrarAccesos())
            {
                Response.StatusCode = 403;
                return Json(new { success = false, message = "No tiene permiso para consultar la bitácora." }, JsonRequestBehavior.AllowGet);
            }

            BitacoraBusquedaDetalleViewModel detalle =
                BitacoraBusquedaService.ObtenerDetalle(
                    idBitacoraBusqueda
                );

            if (detalle == null)
            {
                return Json(
                    new
                    {
                        success = false,
                        message = "No se encontró la búsqueda solicitada."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }

            return Json(
                new
                {
                    success = true,
                    detalle
                },
                JsonRequestBehavior.AllowGet
            );
        }

        [HttpGet]
        public ActionResult ArchivoBitacora(long idBitacoraArchivo)
        {
            if (!PuedeAdministrarAccesos())
            {
                return new HttpStatusCodeResult(403);
            }

            BitacoraArchivoContenidoViewModel archivo =
                BitacoraBusquedaService.ObtenerArchivoContenido(
                    idBitacoraArchivo
                );

            if (
                archivo == null ||
                archivo.Contenido == null ||
                archivo.Contenido.Length == 0
            )
            {
                return HttpNotFound();
            }

            return File(
                archivo.Contenido,
                archivo.ContentType
            );
        }

        [HttpGet]
        public JsonResult BuscarUsuario(string login)
        {
            try
            {
                AdministracionUsuarioViewModel resultado =
                    AccesoService.ObtenerUsuarioAdministracion(
                        login
                    );

                if (resultado == null)
                {
                    return Json(
                        new
                        {
                            success = false,
                            message = "No se encontró el usuario en Accesos."
                        },
                        JsonRequestBehavior.AllowGet
                    );
                }

                return Json(
                    new
                    {
                        success = true,

                        usuario = new
                        {
                            login =
                                resultado.Usuario.Login,

                            nombre =
                                resultado.Usuario.NombreCompleto,

                            puesto =
                                resultado.Usuario.Puesto,

                            area =
                                resultado.Usuario.Area,

                            agencia =
                                resultado.Usuario.Agencia,

                            idUnidad =
                                resultado.Usuario.IdUnidad,

                            unidad =
                                resultado.Usuario.Unidad,

                            existeEnObjetivos =
                                resultado.Usuario.ExisteEnObjetivos,

                            activoEnObjetivos =
                                resultado.Usuario.ActivoEnObjetivos
                        },

                        perfiles =
                            resultado.Perfiles
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        success = false,
                        message = ex.Message
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
        }

        [HttpPost]
        public JsonResult GuardarUsuario(string login, List<int> idsPerfiles)
        {
            tb_Usuarios usuarioActual =
                Session["User"] as tb_Usuarios;

            if (usuarioActual == null)
            {
                Response.StatusCode = 440;

                return Json(
                    new
                    {
                        success = false,
                        message = "La sesión ha expirado."
                    }
                );
            }

            string usuarioModificacion =
                usuarioActual.nvarchar_no_interno ?? "";

            BasicOperationResponse resultado =
                AccesoService.GuardarUsuarioSistema(
                    login,
                    idsPerfiles,
                    usuarioModificacion
                );

            return Json(
                new
                {
                    success =
                        resultado.IsSuccess,

                    message =
                        resultado.Message
                }
            );
        }

        [HttpPost]
        public JsonResult CambiarEstatus(string login, bool activo)
        {
            tb_Usuarios usuarioActual =
                Session["User"] as tb_Usuarios;

            if (usuarioActual == null)
            {
                Response.StatusCode = 440;

                return Json(
                    new
                    {
                        success = false,
                        message = "La sesión ha expirado."
                    }
                );
            }

            BasicOperationResponse resultado =
                AccesoService.CambiarEstatusUsuario(
                    login,
                    activo,
                    usuarioActual.nvarchar_no_interno ?? ""
                );

            return Json(
                new
                {
                    success =
                        resultado.IsSuccess,

                    message =
                        resultado.Message
                }
            );
        }

        private bool PuedeAdministrarAccesos()
        {
            return
                EsAdministrador() ||
                TieneModulo("ADMIN_ACCESOS");
        }


    }
}