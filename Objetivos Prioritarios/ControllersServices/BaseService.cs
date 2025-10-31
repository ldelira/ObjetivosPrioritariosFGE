using Objetivos_Prioritarios.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.ControllersServices
{
    public class BaseService : Controller
    {
        public Objetivos_PrioritariosEntities db = new Objetivos_PrioritariosEntities();
        public CatalogosEntities dbCat = new CatalogosEntities();
        public SIPJEntities dbSIPJ = new SIPJEntities();
        public Mandamientos_JudicialesEntities dbMand = new Mandamientos_JudicialesEntities();
        public FiliacionEntities dbFili = new FiliacionEntities();

    }
}