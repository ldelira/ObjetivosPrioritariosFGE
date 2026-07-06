using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using FiliacionMunicipal.Models;

namespace Web_SIPAEIC.ControllerServices
{
    public class BaseService
    {
        public Filiacion_MunicipiosEntities1 db = new Filiacion_MunicipiosEntities1();
        public CatalogosEntities bdCatalogos = new CatalogosEntities();
    }
}