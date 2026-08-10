using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Objetivos_Prioritarios.Models.Extends
{
    public class CoincidenciaDetenidoFuente6Dto
    {



        public int CLAVE_PERSO { get; set; }

        public string IdsNomPerso { get; set; }

        public string IdsNomPersoOrigenAlerta { get; set; }

        public int CantidadIdsRecibidosAgrupados { get; set; }

        public int CantidadIdsUnificados { get; set; }

        public string NUM_FILIA { get; set; }

        public string NombreCompleto { get; set; }

        public string ALIAS { get; set; }

        public DateTime? FEC_NAC { get; set; }

        public string SEXO { get; set; }

        public DateTime? FechaUltimoDelito { get; set; }

        public string UltimoDelito { get; set; }



    }
}