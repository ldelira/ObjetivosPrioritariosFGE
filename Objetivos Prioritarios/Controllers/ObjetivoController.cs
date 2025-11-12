using Antlr.Runtime.Misc;
using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace Objetivos_Prioritarios.Controllers
{
    public class ObjetivoController : ABaseController
    {
        // GET: Objetivo

        private PartialViewResult SubView(string folder, string viewName, object model = null)
        {
            string path = $"~/Views/Objetivo/{folder}/{viewName}.cshtml";
            return PartialView(path, model);
        }

        public ActionResult Index()
        {
            ViewBag.title = "";
            return View();
        }
        public PartialViewResult ObjetivosListPartial(bool? actives)
        {
            if (actives == null)
                actives = true;

            ViewBag.Actives = actives;

            return PartialView();
        }


        [HttpPost]
        public JsonResult FillObjetivosList(bool? active)
        {
            var general = ObjetivoService.getObjetivosList(active);

            var generalResult = general
            .Select(x => new {
                x.int_id_objetivo,
                x.Nombres,
                x.Aliases,
                x.GruposDelictivos,
                x.FechaNacimiento,
                x.bit_estatus,
                TieneFoto = (!string.IsNullOrEmpty(x.nvarchar_foto)) == true ? "CON FOTO" : "SIN FOTO"
            }).ToList();

            return Json(generalResult, JsonRequestBehavior.AllowGet);
        }


        public ActionResult AddObjetivo()
        {
            var result = ObjetivoService.addObjetivoFicha();

            return RedirectToAction("AddEditObjetivos", new { int_id_objetivo = result.int_id_objetivo });
        }

        public ActionResult AddEditObjetivos(int? int_id_objetivo)
        {
            ViewBag.title = "OBJETIVOS PRIORITARIOS";

            ViewBag.nombreprincipal = ObjetivoService.ObtenerNombreCompletoPrincipal(int_id_objetivo);
            var busqueda = ObjetivoService.getObjetivoById((int)int_id_objetivo);
            return View(busqueda);
            
        }


        [HttpPost]
        public JsonResult AddOrEditFoto(HttpPostedFileBase foto, DateTime fechaNacimiento, int int_id_objetivo)
        {
            var res = ObjetivoService.addEditPhoto(foto, fechaNacimiento, int_id_objetivo);
            return Json(res, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult DisableObjetivo(int int_id_objetivo)
        {
            var res = ObjetivoService.DisableObjetivo(int_id_objetivo);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ActivateObjetivo(int int_id_objetivo)
        {
            var res = ObjetivoService.ActivateObjetivo(int_id_objetivo);
            return Json(res, JsonRequestBehavior.AllowGet);
        }



        #region "ObjetivoNombre"
        public PartialViewResult ObjetivoNombreListPartial(bool? actives)
        {
            if (actives == null)
                actives = true;

            ViewBag.Actives = actives;
            return SubView("Nombre", "ObjetivoNombreListPartial");
        }

        [HttpPost]
        public JsonResult FillObjetivoNombreList(bool? active, int? int_id_objetivo)

        {
            var objetivosNombre = ObjetivoService.getNombreObetivoList(active, int_id_objetivo);

            var objetivosNombreList = objetivosNombre.Select(u => new
            {
                id_nombre = u.int_id_nombre,
                nombre_competo = u.nvarchar_nombre + ' ' + (u.nvarchar_paterno == null ? "" : u.nvarchar_paterno) + ' ' + (u.nvarchar_materno == null ? "" : u.nvarchar_materno),
                paterno = u.nvarchar_paterno,
                materno = u.nvarchar_materno,
                nombre = u.nvarchar_nombre,
                bit_principal = u.bit_principal

            }).ToList();

            return Json(objetivosNombreList, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult AddObjetivoNombrePartial()

        {
            return SubView("Nombre", "AddObjetivoNombrePartial");
        }

        [HttpPost]
        public JsonResult AddObjetivoNombre(tb_NombreObjetivo objetivoNombre)
        {
            var res = ObjetivoService.addNombreObjetivo(objetivoNombre);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult EditObjetivoNombrePartial(int int_id_nombre)
        {
            var objetivoNombre = ObjetivoService.getNombreById(int_id_nombre);

            return SubView("Nombre", "EditObjetivoNombrePartial", objetivoNombre);
        }

        [HttpPost]
        public JsonResult EditObjetivoNombre(tb_NombreObjetivo objetivoNombre)
        {
            var res = ObjetivoService.editNombreObjetivo(objetivoNombre);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DesactivarObjetivoNombre(int int_id_nombre)
        {
            var res = ObjetivoService.diasbleNombre(int_id_nombre);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ReactivarObjetivoNombre(int int_id_nombre)
        {
            var res = ObjetivoService.activatNombre(int_id_nombre);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region "AliasNombre"

        public PartialViewResult ObjetivoAliasListPartial(bool? actives)
        {
            if (actives == null)
                actives = true;

            ViewBag.Actives = actives;
            return SubView("Alias", "ObjetivoAliasListPartial");
        }

        [HttpPost]
        public JsonResult FillObjetivoAliasList(bool? active, int? int_id_objetivo)
        {
            var objetivosAlias = ObjetivoService.getAliasObjetivoList(active, int_id_objetivo);

            var objetivosAliasList = objetivosAlias.Select(u => new
            {
                id_alias = u.int_id_alias,
                alias = u.nvarchar_alias
            }).ToList();

            return Json(objetivosAliasList, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult AddObjetivoAliasPartial()
        {
            return SubView("Alias", "AddObjetivoAliasPartial");
        }

        [HttpPost]
        public JsonResult AddObjetivoAlias(tb_AliasObjetivo objetivoAlias)
        {
            var res = ObjetivoService.addAliasObjetivo(objetivoAlias);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult EditObjetivoAliasPartial(int int_id_alias)
        {
            var objetivoAlias = ObjetivoService.getAliasById(int_id_alias);

            return SubView("Alias", "EditObjetivoAliasPartial", objetivoAlias);
        }

        [HttpPost]
        public JsonResult EditObjetivoAlias(tb_AliasObjetivo objetivoAlias)
        {
            var res = ObjetivoService.editAliasObjetivo(objetivoAlias);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DesactivarObjetivoAlias(int int_id_alias)
        {
            var res = ObjetivoService.disableAlias(int_id_alias);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ReactivarObjetivoAlias(int int_id_alias)
        {
            var res = ObjetivoService.activateAlias(int_id_alias);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region "Domicilios"

        public PartialViewResult ObjetivoDomicilioListPartial(bool? actives)
        {
            if (actives == null)
                actives = true;

            ViewBag.Actives = actives;
            return SubView("Domicilio", "ObjetivoDomicilioListPartial");
        }

        [HttpPost]
        public JsonResult FillObjetivoDomicilioList(bool? active, int? int_id_objetivo)
        {
            var objetivosDomicilio = ObjetivoService.getInfoObjetivoList(active, int_id_objetivo);

            return Json(objetivosDomicilio, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult AddObjetivoDomicilioPartial()
        {
            return SubView("Domicilio", "AddObjetivoDomicilioPartial");
        }

        [HttpPost]
        public JsonResult AddObjetivoDomicilio(tb_InformacionObjetivo objetivoDomicilio)
        {
            var res = ObjetivoService.addDomicilioObjetivo(objetivoDomicilio);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult EditObjetivoDomicilioPartial(int int_id_informacion)
        {
            var objetivoDomicilio = ObjetivoService.getInfoObjetivoListById(int_id_informacion);

            return SubView("Domicilio", "EditObjetivoDomicilioPartial", objetivoDomicilio);
        }

        [HttpPost]
        public JsonResult EditObjetivoDomicilio(tb_InformacionObjetivo objetivoDomicilio)
        {
            var res = ObjetivoService.editDomicilioObjetivo(objetivoDomicilio);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DesactivarObjetivoDomicilio(int int_id_domicilio)
        {
            var res = ObjetivoService.disableDomicilio(int_id_domicilio);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ReactivarObjetivoDomicilio(int int_id_domicilio)
        {
            var res = ObjetivoService.activateDomicilio(int_id_domicilio);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region "ObjetivoGrupo"

        public PartialViewResult ObjetivoGrupoListPartial(bool? actives)
        {
            if (actives == null)
                actives = true;

            ViewBag.Actives = actives;
            return SubView("Grupo", "ObjetivoGrupoListPartial");
        }

        [HttpPost]
        public JsonResult FillObjetivoGrupoList(bool? active, int? int_id_objetivo)
        {
            var objetivosGrupo = ObjetivoService.GetObjetivoGrupoList(active, int_id_objetivo);

            var objetivosGrupoList = objetivosGrupo.Select(u => new
            {
                id_objetivo_grupo = u.int_id_objetivo_grupo,
                nvarchar_alias = u.tb_Grupo_Delictivo.nvarchar_alias,
                fecha_ingreso = u.date_fecha_ingreso?.ToString("yyyy-MM-dd"),
                fecha_salida = u.date_fecha_salida?.ToString("yyyy-MM-dd"),
                observaciones = u.nvarchar_observaciones,
                estatus = u.bit_estatus
            }).ToList();

            return Json(objetivosGrupoList, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult AddObjetivoGrupoPartial()
        {
            return SubView("Grupo", "AddObjetivoGrupoPartial");
        }

        [HttpPost]
        public JsonResult AddObjetivoGrupo(tb_ObjetivoGrupo objetivoGrupo)
        {
            var res = ObjetivoService.AddObjetivoGrupo(objetivoGrupo);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult EditObjetivoGrupoPartial(int int_id_objetivo_grupo)
        {
            var objetivoGrupo = ObjetivoService.GetObjetivoGrupoById(int_id_objetivo_grupo);
            return SubView("Grupo", "EditObjetivoGrupoPartial", objetivoGrupo);
        }

        [HttpPost]
        public JsonResult EditObjetivoGrupo(tb_ObjetivoGrupo objetivoGrupo)
        {
            var res = ObjetivoService.EditObjetivoGrupo(objetivoGrupo);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DesactivarObjetivoGrupo(int int_id_objetivo_grupo)
        {
            var res = ObjetivoService.DisableObjetivoGrupo(int_id_objetivo_grupo);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ReactivarObjetivoGrupo(int int_id_objetivo_grupo)
        {
            var res = ObjetivoService.ActivateObjetivoGrupo(int_id_objetivo_grupo);
            return Json(res, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetGrupoDelictivoNotinObjetivoList(int int_id_objetivo)
        {

            var gruposAsignados = ObjetivoService.db.tb_ObjetivoGrupo
                                .Where(x => x.int_id_objetivo == int_id_objetivo && x.bit_estatus == true)
                                .Select(x => x.int_id_grupo)
                                .ToList();

            var gruposDisponibles = ObjetivoService.db.tb_Grupo_Delictivo
                .Where(g => g.bit_estatus == true && !gruposAsignados.Contains(g.int_id_grupo))
                .Select(c => new
                {
                    int_id_grupo = c.int_id_grupo,
                    nvarchar_alias = c.nvarchar_alias,
                    nvarchar_grupo = c.nvarchar_grupo

                }).ToList();


            return Json(gruposDisponibles, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetGrupoDelictivoNotinObjetivoListEdit(int int_id_objetivo, int int_grupo_actual)
        {

            // 1. Grupos que ya están asignados al objetivo
            var gruposAsignados = ObjetivoService.db.tb_ObjetivoGrupo
                .Where(x => x.int_id_objetivo == int_id_objetivo && x.bit_estatus == true)
                .Select(x => x.int_id_grupo)
                .ToList();

            // 2. Traemos los grupos activos NO asignados, o el que está actualmente seleccionado
            var gruposDisponibles = ObjetivoService.db.tb_Grupo_Delictivo
                .Where(g => g.bit_estatus == true &&
                           (!gruposAsignados.Contains(g.int_id_grupo) || g.int_id_grupo == int_grupo_actual))
                .ToList();


            return Json(gruposDisponibles, JsonRequestBehavior.AllowGet);
        }


        #endregion


        public PartialViewResult InfoGeneralListPartial(string nombre, string paterno, string materno)
        {
            System.Diagnostics.Debug.WriteLine($"Nombre: {nombre}, Paterno: {paterno}, Materno: {materno}");

            var general = ObjetivoService.GetInfoGeneralList(nombre, paterno, materno).ToList();

            return PartialView("BusquedaInfoGeneralListPartial", general);
        }

        [HttpPost]
        public JsonResult FillInfoGeneralList(string nombre, string paterno, string materno)
        {
            var general = ObjetivoService.GetInfoGeneralList(nombre, paterno, materno).ToList();
            return Json(general, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddInfoGeneral()
        {
            try
            {

                Request.InputStream.Position = 0;
                using (var reader = new StreamReader(Request.InputStream))
                {
                    var json = reader.ReadToEnd();
                    dynamic datos = Newtonsoft.Json.JsonConvert.DeserializeObject(json);


                    int id_objetivo = Convert.ToInt32(datos.id_objetivo);
                    int Clave_Persona = Convert.ToInt32(datos.CLAVE_PERSO);


                    var nombresArray = string.IsNullOrEmpty((string)datos.nombre) ? new string[0] :
                        ((string)datos.nombre).Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                    var paternosArray = string.IsNullOrEmpty((string)datos.paterno) ? new string[0] :
                        ((string)datos.paterno).Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                    var maternosArray = string.IsNullOrEmpty((string)datos.materno) ? new string[0] :
                        ((string)datos.materno).Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                    var idsArray = string.IsNullOrEmpty((string)datos.id_concat) ? new string[0] :
                        ((string)datos.id_concat).Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                    var aliasArray = string.IsNullOrEmpty((string)datos.alias) ? new string[0] :
                        ((string)datos.alias).Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                    var callesArray = string.IsNullOrEmpty((string)datos.Cla_Calle_concat) ? new string[0] :
                        ((string)datos.Cla_Calle_concat).Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                    var casasArray = string.IsNullOrEmpty((string)datos.Num_Casa) ? new string[0] :
                        ((string)datos.Num_Casa).Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);



                    bool ExisteClavePersona = ObjetivoService.ExisteClavePerso(Clave_Persona);
                    if (ExisteClavePersona)
                    {
                        return Json(new
                        {
                            IsSuccess = false,
                            Message = $"La CLAVE_PERSO {Clave_Persona} ya existe en la base de datos."
                        });
                    }

                    var responseInfo = new List<object>();
                    for (int i = 0; i < callesArray.Length; i++)
                    {
                        var resultado = ObjetivoService.addInfoGeneralObjetivo(new tb_InformacionObjetivo
                        {
                            int_id_objetivo = id_objetivo,
                            Cve_Calle = int.TryParse(callesArray[i], out int claveCalle) ? claveCalle : 0,
                            nvarchar_no_casa = casasArray[i],
                            int_clave_perso = Clave_Persona
                        });

                        responseInfo.Add(resultado);
                    }


                    var responseNombres = new List<object>();
                    for (int i = 0; i < idsArray.Length; i++)
                    {
                        var resultado = ObjetivoService.addInfoGeneralObjetivoNombre(new tb_NombreObjetivo
                        {
                            int_id_nombre_filiacion = int.TryParse(idsArray[i], out int idNombre) ? idNombre : 0,
                            int_id_objetivo = id_objetivo,
                            nvarchar_nombre = nombresArray[i],
                            nvarchar_paterno = paternosArray[i],
                            nvarchar_materno = maternosArray[i],
                            int_clave_perso = Clave_Persona
                        });

                        responseNombres.Add(resultado);
                    }


                    var responseAlias = new List<object>();
                    foreach (var alias in aliasArray)
                    {
                        var resultado = ObjetivoService.addInfoGeneralObjetivoAlias(new tb_AliasObjetivo
                        {
                            int_id_objetivo = id_objetivo,
                            nvarchar_alias = alias,
                            int_clave_perso = Clave_Persona
                        });

                        responseAlias.Add(resultado);
                    }


                    return Json(new
                    {
                        IsSuccess = true,
                        InfoGeneral = responseInfo,
                        Nombres = responseNombres,
                        Alias = responseAlias
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    IsSuccess = false,
                    Message = $"Error al procesar la información: {ex.Message}"
                });
            }
        }

        [HttpPost]
        public JsonResult MarcarComoPrincipal(int int_id_nombre)
        {
            try
            {
                var resultado = ObjetivoService.MarcarNombreComoPrincipal(int_id_nombre);

                return Json(resultado);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    IsSuccess = false,
                    Message = $"Error al procesar la información: {ex.Message}"
                });
            }
        }


        [HttpPost]
        public ActionResult FotoPrincipal()
        {
            try
            {
                // Leer todo el rowData enviado desde JS
                Request.InputStream.Position = 0;
                string json = new StreamReader(Request.InputStream).ReadToEnd();
                var rowData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);

                // Opcional: extraer campos que quieras usar en la vista parcial
                string id_objetivo = rowData.id_objetivo;
                string nombre = rowData.nombre;
                string paterno = rowData.paterno;
                string materno = rowData.materno;
                int clavePerso = rowData.CLAVE_PERSO;

                string foto = AsuntoService.getFotosDetenidosBase(clavePerso);
                if (string.IsNullOrEmpty(foto))
                {
                    return Json(new
                    {
                        IsSuccess = false,
                        Message = "No existe una fotografía del objetivo."
                    });
                }


                ViewBag.FotoBase64 = string.IsNullOrEmpty(foto) ? null : foto;
                ViewBag.RowData = rowData;

                // Retornar la partial view
                return PartialView("BusquedaFotosDetenidosListPartial"); // nombre de tu partial
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    IsSuccess = false,
                    Message = $"Error al procesar la información: {ex.Message}"
                });
            }
        }


        [HttpPost]
        public JsonResult GuardarFotoDetenido()
        {
            try
            {
                Request.InputStream.Position = 0;
                string json = new StreamReader(Request.InputStream).ReadToEnd();
                dynamic datos = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);

                int idObjetivo = datos.IdObjetivo;
                string fotoBase64 = datos.FotoBase64;

                if (string.IsNullOrEmpty(fotoBase64))
                    return Json(new { IsSuccess = false, Message = "La foto está vacía." });

                var foto = ObjetivoService.GuardarNuevaFoto(idObjetivo, fotoBase64);

                return Json(new { IsSuccess = foto.IsSuccess, Message = foto.Message });
            }
            catch (Exception ex)
            {
                return Json(new { IsSuccess = false, Message = $"Error: {ex.Message}" });
            }
        }


    }
}