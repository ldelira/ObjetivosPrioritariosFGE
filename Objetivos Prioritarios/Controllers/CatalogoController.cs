using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.Controllers
{
    public class CatalogoController : ABaseController
    {

        public JsonResult GetEstadosList()
        {
            var data = CatalogoService.getEstadosList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetMunicipioList(int idEstado)
        {
            var data = CatalogoService.getMunicipiosListByEstado(idEstado);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetColoniaList(int idMunicipio)
        {
            var data = CatalogoService.getColoniaListByMunicipio(idMunicipio)
                 .Select(c => new
                 {
                     Cve_col = c.Cve_col,
                     Colonia1 = c.Colonia1
                 }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCallesList(int idColonia)
        {
            var data = CatalogoService.getCallesListByCalle(idColonia)
                .Select(c => new
            {
                Cve_Calle = c.Cve_Calle,
                Calle1 = c.Calle
            }).ToList() ;
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetGrupoDelictivoList()
        {
            var data = CatalogoService.getGrupoDelictivoList()
                .Select(c => new
                {
                    int_id_grupo = c.int_id_grupo,
                    nvarchar_alias = c.nvarchar_alias,
                    nvarchar_grupo = c.nvarchar_grupo

                }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        

    }
}