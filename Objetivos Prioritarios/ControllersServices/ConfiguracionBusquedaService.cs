using System;
using System.Linq;
using Objetivos_Prioritarios.Models;

namespace Objetivos_Prioritarios.ControllersServices
{
    public class ConfiguracionBusquedaService:BaseService
    {
      
        public const string LoginConfiguracion = "JCERDAN";

        public int ObtenerTiempoEspera()
        {
            
                var usuario =
                    dbqa.tb_Usuarios
                        .AsNoTracking()
                        .FirstOrDefault(x =>
                            x.nvarchar_no_interno.ToUpper() == LoginConfiguracion
                        );

                if (
                    usuario == null ||
                    !usuario.Valor.HasValue
                )
                {
                    return 0;
                }

                int valor =
                    usuario.Valor.Value;

                if (valor < 0)
                {
                    return 0;
                }

                /*
                 * Tope de seguridad.
                 * Máximo 5 minutos.
                 */
                if (valor > 300)
                {
                    return 300;
                }

                return valor;
            
        }


        public bool ActualizarTiempoEspera(int valor)
        {
            if (
                valor < 0 ||
                valor > 300
            )
            {
                return false;
            }

            dbqa = new Objetivos_PrioritariosEntitiesQA();
                var usuario =
                    dbqa.tb_Usuarios
                        .FirstOrDefault(x =>
                            x.nvarchar_no_interno.ToUpper() == LoginConfiguracion
                        );

                if (usuario == null)
                {
                    return false;
                }

                usuario.Valor =
                    valor;

                dbqa.SaveChanges();

                return true;
            
        }
    }
}