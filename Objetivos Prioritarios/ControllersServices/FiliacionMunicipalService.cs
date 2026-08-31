using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Ajax.Utilities;
using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Utils;
using Objetivos_Prioritarios.Models.Extends;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.UI.WebControls;

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

        public List<Tuple<int, int, int, int, int, string>> GetAlertaTipo(
    int idDetenido
)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                var lista = db.tb_Alerta
                    .Where(x =>
                        x.idDetenidoC5 == idDetenido &&
                        x.idPersonaFGEA != null
                    )
                    .Select(x => new
                    {
                        IdPersonaFGEA = x.idPersonaFGEA.Value,
                        IdTbFuente = x.IdTbFuente,
                        Estatus = x.Estatus,
                        Porcentaje = x.Porcentaje,
                        IdTipoAlerta = x.idTipoAlerta,

                        NombreTipoAlerta =
                            x.cat_TipoAlerta != null
                                ? x.cat_TipoAlerta.Alerta
                                : null
                    })
                    .ToList();

                var resultado = lista
                    .Select(x =>
                    {
                        int idPersonaFGEA =
                            x.IdPersonaFGEA;

                        int idTbFuente =
                            Convert.ToInt32(x.IdTbFuente);

                        int estatus =
                            x.Estatus == null
                                ? 0
                                : Convert.ToInt32(x.Estatus);

                        int porcentaje =
                            x.Porcentaje == null
                                ? 0
                                : Convert.ToInt32(x.Porcentaje);

                        int idTipoAlerta =
                            Convert.ToInt32(x.IdTipoAlerta);

                        string nombreTipoAlerta =
                            string.IsNullOrWhiteSpace(x.NombreTipoAlerta)
                                ? "SIN TIPO DE ALERTA"
                                : x.NombreTipoAlerta.Trim();

                        return Tuple.Create(
                            idPersonaFGEA,       // Item1
                            idTbFuente,          // Item2
                            estatus,             // Item3
                            porcentaje,          // Item4
                            idTipoAlerta,        // Item5
                            nombreTipoAlerta     // Item6
                        );
                    })
                    .ToList();

                return resultado;
            }
        }

        public List<Capea_boletin_busqueda> GetInfoCapeas(
    List<int> idsCapea,
    List<int> idsAmber,
    List<int> idsAlba
)
        {
            using (var db = new fiscalia_webEntities())
            {
                var resultado =
                    new List<Capea_boletin_busqueda>();

                idsCapea = idsCapea == null
                    ? new List<int>()
                    : idsCapea
                        .Where(x => x > 0)
                        .Distinct()
                        .ToList();

                idsAmber = idsAmber == null
                    ? new List<int>()
                    : idsAmber
                        .Where(x => x > 0)
                        .Distinct()
                        .ToList();

                idsAlba = idsAlba == null
                    ? new List<int>()
                    : idsAlba
                        .Where(x => x > 0)
                        .Distinct()
                        .ToList();

                if (
                    idsCapea.Count == 0 &&
                    idsAmber.Count == 0 &&
                    idsAlba.Count == 0
                )
                {
                    return resultado;
                }


                /* ============================================================
                   FUENTE 2: CAPEA
                   ============================================================ */

                if (idsCapea.Count > 0)
                {
                    var registrosCapea =
                        db.Capea_boletin_busqueda
                            .Where(x =>
                                idsCapea.Contains(
                                    x.id_boletin_busqueda
                                )
                            )
                            .ToList();

                    resultado.AddRange(
                        registrosCapea
                    );
                }


                /* ============================================================
                   FUENTE 7: ALERTA AMBER
                   ============================================================ */

                if (idsAmber.Count > 0)
                {
                    var registrosAmber =
                        db.A_Amber_alertas_amber
                            .Where(x =>
                                idsAmber.Contains(
                                    x.id_alerta_amber
                                )
                            )
                            .ToList();

                    foreach (var item in registrosAmber)
                    {
                        var registro =
                            new Capea_boletin_busqueda();

                        AsignarValorCompatible(
                            registro,
                            "id_boletin_busqueda",
                            item.id_alerta_amber
                        );

                        AsignarValorCompatible(
                            registro,
                            "nombre",
                            item.nombre
                        );

                        AsignarValorCompatible(
                            registro,
                            "a_paterno",
                            item.a_paterno
                        );

                        AsignarValorCompatible(
                            registro,
                            "a_materno",
                            item.a_materno
                        );

                        AsignarValorCompatible(
                            registro,
                            "edad",
                            item.edad
                        );

                        AsignarValorCompatible(
                            registro,
                            "sexo",
                            item.genero
                        );

                        AsignarValorCompatible(
                            registro,
                            "fecha_nacimiento",
                            item.fecha_nacimiento
                        );

                        AsignarValorCompatible(
                            registro,
                            "fecha_ausencia",
                            item.fecha_hechos
                        );

                        AsignarValorCompatible(
                            registro,
                            "lugar_ausencia",
                            item.lugar_hechos
                        );

                        AsignarValorCompatible(
                            registro,
                            "estatura",
                            item.estatura
                        );

                        AsignarValorCompatible(
                            registro,
                            "peso",
                            item.peso
                        );

                        AsignarValorCompatible(
                            registro,
                            "tipo_color_cabello",
                            item.cabello
                        );

                        AsignarValorCompatible(
                            registro,
                            "tipo_color_ojos",
                            item.ojos
                        );

                        AsignarValorCompatible(
                            registro,
                            "senas_particulares",
                            item.senas_particulares
                        );

                        AsignarValorCompatible(
                            registro,
                            "observaciones",
                            item.resumen_hechos
                        );

                        AsignarValorCompatible(
                            registro,
                            "url_imagen",
                            item.url_imagen
                        );

                        AsignarValorCompatible(
                            registro,
                            "fecha_alta",
                            item.fecha_alta
                        );

                        AsignarValorCompatible(
                            registro,
                            "prioridad",
                            item.prioridad
                        );

                        resultado.Add(
                            registro
                        );
                    }
                }


                /* ============================================================
                   FUENTE 8: ALERTA ALBA
                   ============================================================ */

                if (idsAlba.Count > 0)
                {
                    var registrosAlba =
                        db.A_Alba_alertas_alba
                            .Where(x =>
                                idsAlba.Contains(
                                    x.id_alerta_alba
                                )
                            )
                            .ToList();

                    foreach (var item in registrosAlba)
                    {
                        var registro =
                            new Capea_boletin_busqueda();

                        AsignarValorCompatible(
                            registro,
                            "id_boletin_busqueda",
                            item.id_alerta_alba
                        );

                        AsignarValorCompatible(
                            registro,
                            "nombre",
                            item.nombre
                        );

                        AsignarValorCompatible(
                            registro,
                            "a_paterno",
                            item.a_paterno
                        );

                        AsignarValorCompatible(
                            registro,
                            "a_materno",
                            item.a_materno
                        );

                        AsignarValorCompatible(
                            registro,
                            "edad",
                            item.edad
                        );

                        /*
                         * ALBA no contiene fecha de nacimiento ni sexo.
                         * No se asignan para evitar intentar enviar NULL
                         * a propiedades int o DateTime no anulables.
                         */

                        AsignarValorCompatible(
                            registro,
                            "fecha_ausencia",
                            item.fecha_desaparicion
                        );

                        AsignarValorCompatible(
                            registro,
                            "lugar_ausencia",
                            item.lugar_desaparicion
                        );

                        AsignarValorCompatible(
                            registro,
                            "estatura",
                            item.estatura
                        );

                        AsignarValorCompatible(
                            registro,
                            "peso",
                            item.peso
                        );

                        AsignarValorCompatible(
                            registro,
                            "complexion",
                            item.complexion
                        );

                        AsignarValorCompatible(
                            registro,
                            "tez",
                            item.tez
                        );

                        AsignarValorCompatible(
                            registro,
                            "tipo_color_cabello",
                            item.cabello
                        );

                        AsignarValorCompatible(
                            registro,
                            "tipo_color_ojos",
                            item.ojos
                        );

                        AsignarValorCompatible(
                            registro,
                            "nariz",
                            item.nariz
                        );

                        AsignarValorCompatible(
                            registro,
                            "boca",
                            item.boca
                        );

                        AsignarValorCompatible(
                            registro,
                            "senas_particulares",
                            item.senas_particulares
                        );

                        AsignarValorCompatible(
                            registro,
                            "vestimenta",
                            item.vestimenta
                        );

                        AsignarValorCompatible(
                            registro,
                            "observaciones",
                            item.resumen_hechos
                        );

                        AsignarValorCompatible(
                            registro,
                            "url_imagen",
                            item.url_imagen
                        );

                        AsignarValorCompatible(
                            registro,
                            "fecha_alta",
                            item.fecha_alta
                        );

                        AsignarValorCompatible(
                            registro,
                            "prioridad",
                            item.prioridad
                        );

                        resultado.Add(
                            registro
                        );
                    }
                }

                return resultado;
            }
        }

        private static void AsignarValorCompatible(
    object destino,
    string nombrePropiedad,
    object valor
)
        {
            if (
                destino == null ||
                string.IsNullOrWhiteSpace(nombrePropiedad) ||
                valor == null ||
                valor == DBNull.Value
            )
            {
                return;
            }

            var propiedad =
                destino
                    .GetType()
                    .GetProperty(
                        nombrePropiedad
                    );

            if (
                propiedad == null ||
                !propiedad.CanWrite
            )
            {
                return;
            }

            try
            {
                Type tipoPropiedad =
                    Nullable.GetUnderlyingType(
                        propiedad.PropertyType
                    )
                    ?? propiedad.PropertyType;

                object valorConvertido;

                if (tipoPropiedad == typeof(string))
                {
                    valorConvertido =
                        Convert.ToString(valor);
                }
                else if (tipoPropiedad == typeof(Guid))
                {
                    valorConvertido =
                        valor is Guid
                            ? valor
                            : Guid.Parse(
                                Convert.ToString(valor)
                            );
                }
                else if (tipoPropiedad.IsEnum)
                {
                    valorConvertido =
                        Enum.ToObject(
                            tipoPropiedad,
                            valor
                        );
                }
                else
                {
                    valorConvertido =
                        Convert.ChangeType(
                            valor,
                            tipoPropiedad
                        );
                }

                propiedad.SetValue(
                    destino,
                    valorConvertido,
                    null
                );
            }
            catch
            {
                /*
                 * Si un campo no es compatible con el tipo generado por EF,
                 * se omite sin impedir que se muestre el resto del registro.
                 */
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

                if (conexion.TrimStart().StartsWith("metadata=", StringComparison.OrdinalIgnoreCase))
                {
                    var builder = new EntityConnectionStringBuilder(conexion);
                    conexion = builder.ProviderConnectionString;
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

        private string ConvertirIdsATexto(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return "";
            }

            return string.Join(",", ids.Distinct());
        }

        public DataTable GetInfoObjetivosPrioritarios(List<int> idsNombreObjetivo)
        {
            DataTable tabla = new DataTable();

            if (idsNombreObjetivo == null || idsNombreObjetivo.Count == 0)
            {
                return tabla;
            }

            idsNombreObjetivo = idsNombreObjetivo
                .Distinct()
                .ToList();

            string idsTexto = ConvertirIdsATexto(idsNombreObjetivo);

            using (var db = new Objetivos_PrioritariosEntities())
            {
                string conexion = db.Database.Connection.ConnectionString;

                if (conexion.TrimStart().StartsWith("metadata=", StringComparison.OrdinalIgnoreCase))
                {
                    var builder = new EntityConnectionStringBuilder(conexion);
                    conexion = builder.ProviderConnectionString;
                }

                using (SqlConnection cn = new SqlConnection(conexion))
                {
                    cn.Open();

                    using (SqlCommand cmd = new SqlCommand("dbo.SP_SIC_getCoincidenciasDetenidos", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@ids_nombre_objetivo", SqlDbType.NVarChar).Value = idsTexto;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(tabla);
                        }
                    }
                }
            }

            return tabla;
        }



        public DataTable GetInfoDetenidos(List<int> idsNomPerso, List<int> clavesPerso)
        {
            DataTable tabla = new DataTable();

            idsNomPerso = idsNomPerso == null
                ? new List<int>()
                : idsNomPerso.Where(x => x > 0).Distinct().ToList();

            clavesPerso = clavesPerso == null
                ? new List<int>()
                : clavesPerso.Where(x => x > 0).Distinct().ToList();

            if (idsNomPerso.Count == 0 && clavesPerso.Count == 0)
            {
                return tabla;
            }

            string idsNomPersoTexto = ConvertirIdsATexto(idsNomPerso);
            string clavesPersoTexto = ConvertirIdsATexto(clavesPerso);

            using (var db = new FiliacionEntities())
            {
                string conexion = db.Database.Connection.ConnectionString;

                if (conexion.TrimStart().StartsWith("metadata=", StringComparison.OrdinalIgnoreCase))
                {
                    var builder = new EntityConnectionStringBuilder(conexion);
                    conexion = builder.ProviderConnectionString;
                }

                using (SqlConnection cn = new SqlConnection(conexion))
                {
                    cn.Open();

                    using (SqlCommand cmd = new SqlCommand("dbo.SP_SIC_getCoincidenciasDetenidos", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 300;

                        cmd.Parameters.Add("@ids_nom_perso", SqlDbType.NVarChar).Value =
                            idsNomPerso.Count > 0
                                ? (object)idsNomPersoTexto
                                : DBNull.Value;

                        cmd.Parameters.Add("@claves_perso", SqlDbType.NVarChar).Value =
                            clavesPerso.Count > 0
                                ? (object)clavesPersoTexto
                                : DBNull.Value;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(tabla);
                        }
                    }
                }
            }

            return tabla;
        }


     

        public List<SP_SIC_getCoincidenciasDetenidos_Result>
    getCoincidenciasDetenidos_Results(
        List<int> idsNomPerso)
        {
            if (
                idsNomPerso == null ||
                idsNomPerso.Count == 0
            )
            {
                return new List<
                    SP_SIC_getCoincidenciasDetenidos_Result
                >();
            }

            idsNomPerso =
                idsNomPerso
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

            string clavesTexto =
                ConvertirIdsATexto(
                    idsNomPerso
                );

            using (
                var db =
                    new FiliacionEntities()
            )
            {
                return db
                    .SP_SIC_getCoincidenciasDetenidos(
                        null,clavesTexto
                    )
                    .ToList();
            }
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
                bool bandera = !db.tb_Alerta .Any(x =>
                                 x.idDetenidoC5 == idDetenido &&
                                 x.IdTbFuente == idFuente &&
                                 (x.Estatus == 1 || x.Estatus == 2));

                if (bandera)
                {
                    ActualizarEstatusDetenido(2, idDetenido);
                }



                return alertas.Count;
            }
        }

        public bool ActualizarEstatusDetenido( int origen, int idDetenido)
        {

            if (origen == 2)
            {
                using (var db = new SICEntities())
                {
                    var registro = db.DETENIDO
                        .FirstOrDefault(x =>
                            x.IDDETENIDO == idDetenido);

                    if (registro == null)
                    {
                        return false;
                    }

                    registro.Situacion = "R";

                    db.SaveChanges();

                    return true;
                }
            }
            else if (origen == 1)
            {
                using (var db = new SICEntities())
                {
                    var registro = db.DETENIDO
                        .FirstOrDefault(x =>
                            x.IDDETENIDO == idDetenido);

                    if (registro == null)
                    {
                        return false;
                    }

                    registro.Situacion = "C";

                    db.SaveChanges();

                    return true;
                }
            }
            else if (origen == 3)
            {
                using (var db = new SICEntities())
                {
                    var registro = db.DETENIDO
                       .FirstOrDefault(x =>
                           x.IDDETENIDO == idDetenido);

                    if (registro == null)
                    {
                        return false;
                    }

                    registro.Situacion = "I";

                    db.SaveChanges();

                    return true;
                }
            }
            else if (origen == 4)
            {
                using (var db = new SICEntities())
                {
                    var registro = db.DETENIDO
                       .FirstOrDefault(x =>
                           x.IDDETENIDO == idDetenido);
                    if (registro == null)
                    {
                        return false;
                    }
                    registro.Situacion = "D";
                    db.SaveChanges();
                    return true;
                }
            }

            return false;
        }

        public int ApagarNotificacionDetenidos(int idDetenido, List<int> idsNomPerso)
        {
            if (idsNomPerso == null || idsNomPerso.Count == 0)
            {
                return 0;
            }

            idsNomPerso = idsNomPerso
                .Distinct()
                .ToList();

            using (var db = new Filiacion_MunicipiosEntities())
            {
                var alertas = db.tb_Alerta
                    .Where(x => x.idDetenidoC5 == idDetenido
                             && x.IdTbFuente == 6
                             && x.idPersonaFGEA != null
                             && idsNomPerso.Contains(x.idPersonaFGEA.Value)
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


        public int ReactivarNotificacionDetenidos(int idDetenido, List<int> idsNomPerso)
        {
            if (idsNomPerso == null || idsNomPerso.Count == 0)
            {
                return 0;
            }

            idsNomPerso = idsNomPerso
                .Distinct()
                .ToList();

            using (var db = new Filiacion_MunicipiosEntities())
            {
                var alertas = db.tb_Alerta
                    .Where(x => x.idDetenidoC5 == idDetenido
                             && x.IdTbFuente == 6
                             && x.idPersonaFGEA != null
                             && idsNomPerso.Contains(x.idPersonaFGEA.Value)
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

                if (db.tb_Alerta.Any(x =>
                                      x.idDetenidoC5 == idDetenido &&
                                      x.IdTbFuente != 6 &&
                                       (x.Estatus != 1 || x.Estatus != 2)))
                {
                    ActualizarEstatusDetenido(2, idDetenido);
                }

                return alertas.Count;
            }
        }

        public Tuple<int, int, int> GetConteoAlertasPorEstatus(int idDetenido)
{
    using (var db = new Filiacion_MunicipiosEntities())
    {
        int totalActivas = db.tb_Alerta.Count(x =>
            x.idDetenidoC5 == idDetenido &&
            x.Estatus == 1
        );

        int totalRevisadas = db.tb_Alerta.Count(x =>
            x.idDetenidoC5 == idDetenido &&
            x.Estatus == 0
        );

        int totalConfirmadas = db.tb_Alerta.Count(x =>
            x.idDetenidoC5 == idDetenido &&
            (x.Estatus == 2 || x.Estatus == 3)
        );

        return Tuple.Create(
            totalActivas,
            totalRevisadas,
            totalConfirmadas
        );
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

        public int ActualizarEstatusNotificacion( int idDetenido, int idOrigen, int idFuente, int nuevoEstatus) {
            if (nuevoEstatus != 0 &&
                nuevoEstatus != 1 &&
                nuevoEstatus != 2 &&
                nuevoEstatus != 3)
            {
                throw new ArgumentException("El estatus recibido no es válido.");
            }

            using (var db = new Filiacion_MunicipiosEntities())
            {
                var alertas = db.tb_Alerta
                    .Where(x =>
                        x.idDetenidoC5 == idDetenido &&
                        x.idPersonaFGEA == idOrigen &&
                        x.IdTbFuente == idFuente)
                    .ToList();

                foreach (var alerta in alertas)
                {
                    alerta.Estatus = nuevoEstatus;
                }

                db.SaveChanges();

                if(nuevoEstatus == 2)
                {
                    if (db.tb_Alerta.Any(x =>
                                     x.idDetenidoC5 == idDetenido &&
                                     x.IdTbFuente != 6 &&
                                     x.Estatus == 2))
                    {
                        ActualizarEstatusDetenido(1, idDetenido);
                    }
                }
                else if(nuevoEstatus == 1)
                {
                    if (db.tb_Alerta.Any(x =>
                                     x.idDetenidoC5 == idDetenido &&
                                     x.IdTbFuente != 6 &&
                                     x.Estatus == 1 &&
                                     x.Estatus != 2))
                    {
                        ActualizarEstatusDetenido(3, idDetenido);
                    }
                }else if(nuevoEstatus == 0)
                {
                    if (db.tb_Alerta.Any(x =>
                                     x.idDetenidoC5 == idDetenido &&
                                     x.IdTbFuente != 6 &&
                                      (x.Estatus != 1 || x.Estatus != 2)))
                    {
                        ActualizarEstatusDetenido(2, idDetenido);
                    }
                }else if(nuevoEstatus == 3)
                {
                    if (db.tb_Alerta.Any(x =>
                                     x.idDetenidoC5 == idDetenido &&
                                     x.IdTbFuente != 6 &&
                                      (x.Estatus == 3))) 
                    {
                        ActualizarEstatusDetenido(4, idDetenido);
                    }
                }


                    return alertas.Count;
            }
        }

        public int ActualizarEstatusNotificacionDetenidos(
    int idDetenido,
    List<int> idsNomPerso,
    int nuevoEstatus)
        {
            if (idsNomPerso == null || idsNomPerso.Count == 0)
            {
                return 0;
            }

            if (nuevoEstatus != 0 &&
                nuevoEstatus != 1 &&
                nuevoEstatus != 2)
            {
                throw new ArgumentException("El estatus recibido no es válido.");
            }

            idsNomPerso = idsNomPerso
                .Distinct()
                .ToList();

            using (var db = new Filiacion_MunicipiosEntities())
            {

                if (nuevoEstatus == 2)
                {
                    var desactivar = db.tb_Alerta
                        .Where(x =>
                            x.idDetenidoC5 == idDetenido &&
                            x.IdTbFuente == 6);

                    foreach (var alerta in desactivar)
                    {
                        alerta.Estatus = 0;
                    }

                }

                var alertas = db.tb_Alerta
                    .Where(x =>
                        x.idDetenidoC5 == idDetenido &&
                        x.IdTbFuente == 6 &&
                        x.idPersonaFGEA.HasValue &&
                        idsNomPerso.Contains(x.idPersonaFGEA.Value))
                    .ToList();

                foreach (var alerta in alertas)
                {
                    alerta.Estatus = nuevoEstatus;
                }

                db.SaveChanges();

                return alertas.Count;
            }
        }


        public DataTable BuscarMandamientosCandidatosPorNombre(
    string nombreCompleto)
        {
            DataTable tabla =
                new DataTable();

            if (string.IsNullOrWhiteSpace(nombreCompleto))
            {
                return tabla;
            }

            List<string> tokens =
                ObtenerTokensMandamientos(
                    nombreCompleto
                )
                .Where(x => x.Length >= 3)
                .OrderByDescending(x => x.Length)
                .Take(4)
                .ToList();

            if (tokens.Count == 0)
            {
                return tabla;
            }

            using (
                var db =
                    new Mandamientos_JudicialesEntities()
            )
            {
                string conexion =
                    db.Database.Connection.ConnectionString;

                if (
                    conexion
                        .TrimStart()
                        .StartsWith(
                            "metadata=",
                            StringComparison.OrdinalIgnoreCase
                        )
                )
                {
                    EntityConnectionStringBuilder builder =
                        new EntityConnectionStringBuilder(
                            conexion
                        );

                    conexion =
                        builder.ProviderConnectionString;
                }

                using (
                    SqlConnection cn =
                        new SqlConnection(conexion)
                )
                {
                    cn.Open();

                    StringBuilder sql =
                        new StringBuilder();

                    sql.AppendLine(@"
SELECT
    NM.id AS IdOrigenAlerta,
    MJ.id AS IdMandamiento,
    MJ.numero_control,
    MJ.numero_expediente,

    LTRIM(
        RTRIM(
            CONCAT(
                ISNULL(NM.nombre, ''),
                ' ',
                ISNULL(NM.paterno, ''),
                ' ',
                ISNULL(NM.materno, '')
            )
        )
    ) AS Nombre,

    TM.mandamiento,
    MJ.fecha_expedicion,
    MJ.fecha_alta,
    EP.tipo AS EstadoProceso

FROM Mandamientos_Judiciales.dbo.mandamiento_judicial MJ

INNER JOIN Mandamientos_Judiciales.dbo.nombres_mandamiento NM
    ON MJ.id = NM.id_mandamiento_judicial

INNER JOIN Mandamientos_Judiciales.dbo.tipo_mandamiento_judicial TM
    ON MJ.id_tipo_mandato = TM.id_tipo_mandato

INNER JOIN Mandamientos_Judiciales.dbo.catalogo_estado_proceso EP
    ON MJ.id_estado_proceso = EP.id_estado_proceso

CROSS APPLY
(
    SELECT
        UPPER(
            LTRIM(
                RTRIM(
                    CONCAT(
                        ISNULL(NM.nombre, ''),
                        ' ',
                        ISNULL(NM.paterno, ''),
                        ' ',
                        ISNULL(NM.materno, '')
                    )
                )
            )
        ) COLLATE Modern_Spanish_CI_AI AS NombreBusqueda
) NB

WHERE
(
");

                    for (int i = 0; i < tokens.Count; i++)
                    {
                        if (i > 0)
                        {
                            sql.AppendLine(" + ");
                        }

                        sql.Append(
                            "CASE WHEN NB.NombreBusqueda LIKE @token" +
                            i +
                            " THEN 1 ELSE 0 END"
                        );
                    }

                    sql.AppendLine();
                    sql.AppendLine(") >= @minimoTokens");
                    sql.AppendLine("ORDER BY MJ.fecha_alta DESC;");

                    using (
                        SqlCommand cmd =
                            new SqlCommand(
                                sql.ToString(),
                                cn
                            )
                    )
                    {
                        cmd.CommandType =
                            CommandType.Text;

                        cmd.CommandTimeout =
                            120;

                        for (int i = 0; i < tokens.Count; i++)
                        {
                            cmd.Parameters.Add(
                                "@token" + i,
                                SqlDbType.NVarChar,
                                100
                            ).Value =
                                "%" + tokens[i] + "%";
                        }

                        /*
                         * Con dos o más tokens pedimos que coincidan
                         * al menos dos. Esto evita consultar únicamente
                         * por un apellido demasiado común.
                         */
                        int minimoTokens =
                            tokens.Count >= 2
                                ? 2
                                : 1;

                        cmd.Parameters.Add(
                            "@minimoTokens",
                            SqlDbType.Int
                        ).Value =
                            minimoTokens;

                        using (
                            SqlDataAdapter da =
                                new SqlDataAdapter(cmd)
                        )
                        {
                            da.Fill(tabla);
                        }
                    }
                }
            }

            return tabla;
        }


        private static List<string> ObtenerTokensMandamientos(
    string nombreCompleto)
        {
            string nombreNormalizado =
                NormalizarNombreMandamientos(
                    nombreCompleto
                );

            if (string.IsNullOrWhiteSpace(nombreNormalizado))
            {
                return new List<string>();
            }

            HashSet<string> palabrasIgnoradas =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                )
                {
            "DE",
            "DEL",
            "LA",
            "LAS",
            "LOS",
            "Y"
                };

            return nombreNormalizado
                .Split(
                    new[] { ' ' },
                    StringSplitOptions.RemoveEmptyEntries
                )
                .Select(x => x.Trim())
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x) &&
                    !palabrasIgnoradas.Contains(x)
                )
                .Distinct(
                    StringComparer.OrdinalIgnoreCase
                )
                .ToList();
        }


        private static string NormalizarNombreMandamientos(
            string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return "";
            }

            string textoDescompuesto =
                texto
                    .Trim()
                    .ToUpperInvariant()
                    .Normalize(
                        NormalizationForm.FormD
                    );

            StringBuilder resultado =
                new StringBuilder();

            foreach (char caracter in textoDescompuesto)
            {
                UnicodeCategory categoria =
                    CharUnicodeInfo.GetUnicodeCategory(
                        caracter
                    );

                if (
                    categoria ==
                    UnicodeCategory.NonSpacingMark
                )
                {
                    continue;
                }

                if (
                    char.IsLetterOrDigit(caracter) ||
                    char.IsWhiteSpace(caracter)
                )
                {
                    resultado.Append(caracter);
                }
                else
                {
                    resultado.Append(' ');
                }
            }

            return string.Join(
                " ",
                resultado
                    .ToString()
                    .Split(
                        new[] { ' ' },
                        StringSplitOptions.RemoveEmptyEntries
                    )
            );
        }


        public List<Tuple<int, int, string, string, string>> GetContactosMunicipios()
        {
            using (var dbFiliacion = new Filiacion_MunicipiosEntities())
            using (var dbCatalogos = new CatalogosEntities())
            {
                /*
                 * Base Filiacion_Municipios
                 */
                var contactos =
                    dbFiliacion.cat_Contactos_Municipios
                        .AsNoTracking()
                        .ToList();


                /*
                 * Base Catalogos
                 */
                var municipios =
                    dbCatalogos.Municipio
                        .AsNoTracking()
                        .ToList();


                /*
                 * Como ya hicimos ToList(),
                 * este JOIN se realiza en memoria.
                 */
                var resultado =
                    (
                        from contacto in contactos

                        join municipio in municipios
                            on Convert.ToInt32(contacto.Municipio)
                            equals Convert.ToInt32(municipio.Cve_mun)

                        select Tuple.Create(
                            Convert.ToInt32(contacto.ID_Mun),       // Item1 = Id contacto
                            Convert.ToInt32(municipio.Cve_mun),     // Item2 = Id municipio
                            Convert.ToString(municipio.Municipio1),  // Item3 = Municipio
                            Convert.ToString(contacto.Telefono),    // Item4 = Teléfono
                            Convert.ToString(contacto.Contacto)     // Item5 = Contacto
                        )
                    )
                    .OrderBy(x => x.Item3)
                    .ThenBy(x => x.Item5)
                    .ToList();


                return resultado;
            }
        }


        public DataTable GetInfoPersonasFiliacion( List<int> idsPersona)
        {
            DataTable tabla =
                new DataTable();

            /* ============================================================
               VALIDAR IDS
               ============================================================ */

            if (idsPersona == null ||
                idsPersona.Count == 0)
            {
                return tabla;
            }


            /* ============================================================
               LIMPIAR IDS

               Evitamos:
               - IDs repetidos
               - IDs inválidos
               ============================================================ */

            idsPersona =
                idsPersona
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();


            if (idsPersona.Count == 0)
            {
                return tabla;
            }


            /* ============================================================
               CONVERTIR:

               List<int>
                   19
                   20
                   25

               A:

               "19,20,25"
               ============================================================ */

            string idsTexto =
                ConvertirIdsATexto(
                    idsPersona
                );


            /* ============================================================
               BASE DE DATOS:
               Filiacion_Municipios
               ============================================================ */

            using (var db =
                new Filiacion_MunicipiosEntities())
            {
                string conexion =
                    db.Database
                        .Connection
                        .ConnectionString;


                /*
                 * Si la conexión viene del EDMX como EntityConnection,
                 * obtenemos solamente la conexión real de SQL Server.
                 */

                if (
                    conexion
                        .TrimStart()
                        .StartsWith(
                            "metadata=",
                            StringComparison.OrdinalIgnoreCase
                        )
                )
                {
                    var builder =
                        new EntityConnectionStringBuilder(
                            conexion
                        );

                    conexion =
                        builder.ProviderConnectionString;
                }


                /* ========================================================
                   EJECUTAR SP
                   ======================================================== */

                using (SqlConnection cn =
                    new SqlConnection(conexion))
                {
                    cn.Open();


                    using (SqlCommand cmd =
                        new SqlCommand(
                            "dbo.SP_SIC_ObtenerPersonas",
                            cn
                        ))
                    {
                        cmd.CommandType =
                            CommandType.StoredProcedure;


                        /*
                         * Le damos tiempo suficiente porque el resultado
                         * puede incluir fotografías Base64.
                         */

                        cmd.CommandTimeout =
                            300;


                        /* =================================================
                           PARÁMETRO DEL SP

                           @IdsPersona = "19,20,25"
                           ================================================= */

                        cmd.Parameters
                            .Add(
                                "@IdsPersona",
                                SqlDbType.NVarChar,
                                -1
                            )
                            .Value =
                                idsTexto;


                        /* =================================================
                           LLENAR DATATABLE
                           ================================================= */

                        using (SqlDataAdapter da =
                            new SqlDataAdapter(cmd))
                        {
                            da.Fill(tabla);
                        }
                    }
                }
            }


            return tabla;
        }


        public List<SP_SIC_getCoincidenciasDetenidos_Result> getCoincidenciasDetenidosPorClavePerso_Results(List<int> clavesPerso)
        {
            if (clavesPerso == null || clavesPerso.Count == 0)
            {
                return new List<SP_SIC_getCoincidenciasDetenidos_Result>();
            }

            clavesPerso = clavesPerso
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (clavesPerso.Count == 0)
            {
                return new List<SP_SIC_getCoincidenciasDetenidos_Result>();
            }

            string clavesTexto =
                string.Join(
                    ",",
                    clavesPerso
                );

            using (var db = new FiliacionEntities())
            {
                SqlParameter parametroIdsNomPerso =
                    new SqlParameter(
                        "@ids_nom_perso",
                        SqlDbType.NVarChar
                    );

                parametroIdsNomPerso.Value =
                    DBNull.Value;

                SqlParameter parametroClavesPerso =
                    new SqlParameter(
                        "@claves_perso",
                        SqlDbType.NVarChar
                    );

                parametroClavesPerso.Value =
                    clavesTexto;

                return db.Database
                    .SqlQuery<SP_SIC_getCoincidenciasDetenidos_Result>(
                        @"EXEC dbo.SP_SIC_getCoincidenciasDetenidos
                    @ids_nom_perso,
                    @claves_perso",
                        parametroIdsNomPerso,
                        parametroClavesPerso
                    )
                    .ToList();
            }
        }


        public DataTable GetInfoObjetivosPrioritariosPorIdObjetivo(List<int> idsObjetivo)
        {
            DataTable tabla = new DataTable();

            if (idsObjetivo == null || idsObjetivo.Count == 0)
            {
                return tabla;
            }

            idsObjetivo = idsObjetivo
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (idsObjetivo.Count == 0)
            {
                return tabla;
            }

            string idsTexto = ConvertirIdsATexto(idsObjetivo);

            using (var db = new Objetivos_PrioritariosEntities())
            {
                string conexion = db.Database.Connection.ConnectionString;

                if (conexion.TrimStart().StartsWith("metadata=", StringComparison.OrdinalIgnoreCase))
                {
                    var builder = new EntityConnectionStringBuilder(conexion);
                    conexion = builder.ProviderConnectionString;
                }

                using (SqlConnection cn = new SqlConnection(conexion))
                {
                    cn.Open();

                    using (SqlCommand cmd = new SqlCommand("dbo.SP_SIC_getCoincidenciasDetenidos", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 300;

                        cmd.Parameters.Add(
                            "@ids_nombre_objetivo",
                            SqlDbType.NVarChar
                        ).Value = DBNull.Value;

                        cmd.Parameters.Add(
                            "@ids_objetivo",
                            SqlDbType.NVarChar
                        ).Value = idsTexto;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(tabla);
                        }
                    }
                }
            }

            return tabla;
        }


        public string GetDelitoMandamiento(int idMandamiento)
        {
            try
            {
                if (idMandamiento <= 0)
                {
                    return "";
                }

                var datos =
                    dbMand.sp_ObjPri_getObjetivoInfo(
                        3,
                        "",
                        "",
                        "",
                        "",
                        idMandamiento
                    )
                    .ToList();

                if (
                    datos == null ||
                    datos.Count == 0
                )
                {
                    return "";
                }

                List<string> delitos =
                    datos
                        .Where(x =>
                            x != null &&
                            !string.IsNullOrWhiteSpace(
                                x.delito
                            )
                        )
                        .Select(x =>
                            x.delito.Trim()
                        )
                        .Distinct(
                            StringComparer.OrdinalIgnoreCase
                        )
                        .ToList();

                if (delitos.Count == 0)
                {
                    return "";
                }

                return string.Join(
                    ", ",
                    delitos
                );
            }
            catch
            {
                return "";
            }
        }

    }
}