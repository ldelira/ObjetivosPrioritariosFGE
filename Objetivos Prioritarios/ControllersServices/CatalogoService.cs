using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.ControllersServices
{
    public class CatalogoService : BaseService
    {
        public List<Estado> getEstadosList()
        {
            return dbCat.Estado.AsNoTracking().ToList();
        }

        public List<Municipio> getMunicipiosListByEstado(int int_id_estado)
        {
            return dbCat.Municipio.AsNoTracking().Where(x=>x.FK_Estado==int_id_estado).ToList();
        }
        public List<Colonia> getColoniaListByMunicipio( int int_id_municipio)
        {
            return dbCat.Colonia.AsNoTracking().Where(x=>x.Cve_mun==int_id_municipio).ToList();
        }
        public List<Calles> getCallesListByCalle(int int_id_colonia)
        {
            return dbCat.Calles.AsNoTracking().Where(x=>x.Cve_Col==int_id_colonia).ToList();
        }


        public List<tb_Grupo_Delictivo> getGrupoDelictivoList()
        {
            return db.tb_Grupo_Delictivo.AsNoTracking().Where(x=>x.bit_estatus==true).ToList();
        }

    }
}