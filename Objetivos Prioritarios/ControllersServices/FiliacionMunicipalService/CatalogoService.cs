using FiliacionMunicipal.Models;
using FiliacionMunicipal.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Web_SIPAEIC.ControllerServices;

namespace FiliacionMunicipal.ControllerServices
{
    public class CatalogoService : BaseService
    {
       public List<Estudios> List_Estudios() 
        {
            return bdCatalogos.Estudios.OrderBy(x => x.Estudio).ToList();
        }
        public List<Ocupacion> List_Ocupacion()
        {
            return bdCatalogos.Ocupacion.OrderBy(x=>x.Ocupacion1).ToList();
        }

        public List<Estado> List_Estado()
        {
            return bdCatalogos.Estado.ToList();
        }
        public List<cat_FaltaAdministrativa> List_FaltaAdministrativa()
        {
            return db.cat_FaltaAdministrativa.OrderBy(x => x.FaltaAdministrativa).ToList();
        }
        public List<Corporaciones> List_Corporaciones()
        {
            return bdCatalogos.Corporaciones.OrderBy(x => x.Corporacion).ToList();
        }
        public List<Municipio> List_Municipio_ByEstado(int idEstado)
        {
            return bdCatalogos.Municipio.Where(m => m.FK_Estado == idEstado).OrderBy(x=>x.Municipio1).ToList();
        }

        public List<Colonia> List_Colonia_ByMunicipio(int idMunicipio)
        {
            return bdCatalogos.Colonia.Where(c => c.Cve_mun == idMunicipio).OrderBy(x=>x.Colonia_Doctos).ToList();
        }
        public List<Calles> List_Calle_ByColonia(int idColonia)
        {
            return bdCatalogos.Calles.Where(c => c.Cve_Col == idColonia).OrderBy(x=>x.Calle).ToList();
        }
        public List<dynamic> BuscarCallesFull(string texto)
        {
            var query = (from c in bdCatalogos.Calles
                         join col in bdCatalogos.Colonia on c.Cve_Col equals col.Cve_col
                         join mun in bdCatalogos.Municipio on col.Cve_mun equals mun.Cve_mun
                         join est in bdCatalogos.Estado on mun.FK_Estado equals est.ID_Estado
                         where c.Calle.Contains(texto)
                         select new
                         {
                             idCalle = c.Cve_Calle,
                             calle = c.Calle,
                             idColonia = col.Cve_col,
                             colonia = col.Colonia_Doctos,
                             idMunicipio = mun.Cve_mun,
                             municipio = mun.Municipio1,
                             idEstado = est.ID_Estado,
                             estado = est.Estado1
                         }).ToList();

            return query.Cast<dynamic>().ToList();
        }
        public List<Municipio> List_Muni()
        {
            return bdCatalogos.Municipio.ToList();
        }
        public List<TipoFotoVM> List_TipoFoto()
        {
            return db.cat_TipoFoto
                .Select(x => new TipoFotoVM
                {
                    idTipoFoto = x.idTipoFoto,
                    TipoFoto = x.TipoFoto
                })
                .ToList();
        }

    }
}