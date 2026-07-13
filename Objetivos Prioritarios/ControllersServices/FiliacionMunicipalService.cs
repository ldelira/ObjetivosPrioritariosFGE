using Objetivos_Prioritarios.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity.Core.EntityClient;
using System.Linq;

namespace Objetivos_Prioritarios.ControllersServices
{
    public class FiliacionMunicipalService : BaseService
    {
        public List<sp_Alertas_Result> GetAlertas()
        {
            return dbFiliMuni.sp_Alertas().ToList();
        }

        public sp_BuscarDetenido_Result GetInfoDetenido(int idDetenido)
        {
            return dbFiliMuni.sp_BuscarDetenido(idDetenido).FirstOrDefault();
        }

        public List<Tuple<int, int, int>> GetAlertaTipo(int ID_DETENIDO)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                var lista = db.tb_Alerta
                    .Where(x => x.idDetenidoC5 == ID_DETENIDO
                             && x.idPersonaFGEA != null)
                    .Select(x => new
                    {
                        IdPersonaFGEA = x.idPersonaFGEA.Value,
                        IdTbFuente = x.IdTbFuente,
                        Estatus = x.Estatus
                    })
                    .ToList();

                var resultado = lista
                    .Select(x =>
                    {
                        int estatus = 0;

                        if (x.Estatus != null)
                        {
                            estatus = Convert.ToInt32(x.Estatus);
                        }

                        return Tuple.Create(
                            x.IdPersonaFGEA,
                            x.IdTbFuente,
                            estatus
                        );
                    })
                    .ToList();

                return resultado;
            }
        }

        public List<Capea_boletin_busqueda> GetInfoCapeas(List<int> idsCapea)
        {
            using (var db = new fiscalia_webEntities())
            {
                if (idsCapea == null || idsCapea.Count == 0)
                {
                    return new List<Capea_boletin_busqueda>();
                }

                idsCapea = idsCapea
                    .Distinct()
                    .ToList();

                var resultado = db.Capea_boletin_busqueda
                    .Where(x => idsCapea.Contains(x.id_boletin_busqueda))
                    .ToList();

                return resultado;
            }
        }

        public DataTable GetInfoMandamientos(List<int> idsNombresMandamiento)
        {
            DataTable tabla = new DataTable();

            if (idsNombresMandamiento == null || idsNombresMandamiento.Count == 0)
            {
                return tabla;
            }

            idsNombresMandamiento = idsNombresMandamiento
                .Distinct()
                .ToList();

            using (var db = new Mandamientos_JudicialesEntities())
            {
                string conexion = db.Database.Connection.ConnectionString;

                try
                {
                    var builder = new EntityConnectionStringBuilder(conexion);
                    conexion = builder.ProviderConnectionString;
                }
                catch
                {
                    // Si la conexión ya viene como SqlConnection normal, se queda igual.
                }

                using (SqlConnection cn = new SqlConnection(conexion))
                {
                    cn.Open();

                    List<string> parametros = new List<string>();

                    for (int i = 0; i < idsNombresMandamiento.Count; i++)
                    {
                        parametros.Add("@id" + i);
                    }

                    string sql = @"
                SELECT
                    NM.id AS IdOrigenAlerta,
                    MJ.id,
                    MJ.numero_control,
                    MJ.numero_expediente,
                    CONCAT(NM.nombre, ' ', NM.paterno, ' ', NM.materno) AS Nombre,
                    TM.mandamiento,
                    MJ.fecha_expedicion,
                    MJ.fecha_alta,
                    EP.tipo
                FROM Mandamientos_Judiciales.dbo.mandamiento_judicial MJ
                INNER JOIN Mandamientos_Judiciales.dbo.nombres_mandamiento NM
                    ON MJ.id = NM.id_mandamiento_judicial
                INNER JOIN Mandamientos_Judiciales.dbo.tipo_mandamiento_judicial TM
                    ON MJ.id_tipo_mandato = TM.id_tipo_mandato
                INNER JOIN Mandamientos_Judiciales.dbo.catalogo_estado_proceso EP
                    ON MJ.id_estado_proceso = EP.id_estado_proceso
                WHERE NM.id IN (" + string.Join(",", parametros) + @")
                ORDER BY MJ.fecha_alta DESC;
            ";

                    using (SqlCommand cmd = new SqlCommand(sql, cn))
                    {
                        for (int i = 0; i < idsNombresMandamiento.Count; i++)
                        {
                            cmd.Parameters.AddWithValue("@id" + i, idsNombresMandamiento[i]);
                        }

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(tabla);
                        }
                    }
                }
            }

            return tabla;
        }

        public int ApagarNotificacion(int idDetenido, int idOrigen, int idFuente)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                var alertas = db.tb_Alerta
                    .Where(x => x.idDetenidoC5 == idDetenido
                             && x.idPersonaFGEA == idOrigen
                             && x.IdTbFuente == idFuente
                             && x.Estatus == 1)
                    .ToList();

                foreach (var alerta in alertas)
                {
                    alerta.Estatus = 0;
                }

                db.SaveChanges();

                return alertas.Count;
            }
        }
        public tb_DETENCION_C5 GetInfoDetencionC5(int idDetencion)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                var resultado = db.tb_DETENCION_C5
                    .FirstOrDefault(x => x.IDDETENCION == idDetencion);

                return resultado;
            }
        }

        public tb_DETENIDO_C5 GetDatosDetenidoC5(int idDetenido)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                var resultado = db.tb_DETENIDO_C5
                    .FirstOrDefault(x => x.IDDETENIDO == idDetenido);

                return resultado;
            }
        }


        public List<tb_FOTO_C5> GetFotosDetenidoC5(int idDetenido)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                var resultado = db.tb_FOTO_C5
                    .Where(x => x.IDDETENIDO == idDetenido
                             && x.TIPO != null
                             && x.TIPO.Contains("Foto")
                             && x.FOTO != null
                             && x.FOTO != "")
                    .OrderBy(x => x.IDFOTO)
                    .ToList();

                return resultado;
            }
        }

        public tb_FOTO_C5 GetFotoC5PorId(int idFoto)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                var resultado = db.tb_FOTO_C5
                    .FirstOrDefault(x => x.IDFOTO == idFoto);

                return resultado;
            }
        }

        public List<tb_HUELLA_C5> GetHuellasDetenidoC5(int idDetenido)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                var resultado = db.tb_HUELLA_C5
                    .Where(x => x.IdDetenido == idDetenido
                             && x.Huellas != null)
                    .OrderBy(x => x.IdHuella)
                    .ToList()
                    .Where(x => !string.IsNullOrWhiteSpace(x.Huellas))
                    .ToList();

                return resultado;
            }
        }

        public tb_HUELLA_C5 GetHuellaC5PorId(int idHuella)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                var resultado = db.tb_HUELLA_C5
                    .FirstOrDefault(x => x.IdHuella == idHuella);

                return resultado;
            }
        }

        public List<tb_FOTO_C5> GetRasgosDetenidoC5(int idDetenido)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                var resultado = db.tb_FOTO_C5
                    .Where(x => x.IDDETENIDO == idDetenido
                             && x.TIPO != null
                             && x.TIPO.Contains("Rasgo")
                             && x.FOTO != null)
                    .OrderBy(x => x.IDFOTO)
                    .ToList()
                    .Where(x => !string.IsNullOrWhiteSpace(x.FOTO))
                    .ToList();

                return resultado;
            }
        }

        public Capea_boletin_busqueda GetCapeaPorId(int idCapea)
        {
            using (var db = new fiscalia_webEntities())
            {
                var resultado = db.Capea_boletin_busqueda
                    .FirstOrDefault(x => x.id_boletin_busqueda == idCapea);

                return resultado;
            }
        }

        public int ReactivarNotificacion(int idDetenido, int idOrigen, int idFuente)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                var alertas = db.tb_Alerta
                    .Where(x => x.idDetenidoC5 == idDetenido
                             && x.idPersonaFGEA == idOrigen
                             && x.IdTbFuente == idFuente
                             && x.Estatus == 0)
                    .ToList();

                foreach (var alerta in alertas)
                {
                    alerta.Estatus = 1;
                }

                db.SaveChanges();

                return alertas.Count;
            }
        }

        public Tuple<int, int> GetConteoAlertasPorEstatus(int idDetenido)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                int activas = db.tb_Alerta
                    .Count(x => x.idDetenidoC5 == idDetenido
                             && x.Estatus == 1);

                int revisadas = db.tb_Alerta
                    .Count(x => x.idDetenidoC5 == idDetenido
                             && x.Estatus == 0);

                return Tuple.Create(activas, revisadas);
            }
        }


        public List<Tuple<int, int>> GetTiposAlertas(int idDetenido)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                var resultado = db.tb_Alerta
                    .Where(x => x.idDetenidoC5 == idDetenido)
                    .GroupBy(x => x.idTipoAlerta)
                    .Select(grupo => new
                    {
                        IdTipoAlerta = grupo.Key,
                        TotalAlertas = grupo.Count()
                    })
                    .OrderByDescending(x => x.TotalAlertas)
                    .ToList()
                    .Select(x => Tuple.Create(
                        x.IdTipoAlerta == null ? 0 : Convert.ToInt32(x.IdTipoAlerta),
                        x.TotalAlertas
                    ))
                    .ToList();

                return resultado;
            }
        }

    }
}