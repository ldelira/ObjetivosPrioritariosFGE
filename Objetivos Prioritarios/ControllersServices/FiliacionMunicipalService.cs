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

        public List<Tuple<int, int, int, int, int, string>> GetAlertaTipo(List<int> idsDetenidos)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                if (idsDetenidos == null || idsDetenidos.Count == 0)
                {
                    return new List<Tuple<int, int, int, int, int, string>>();
                }

                idsDetenidos = idsDetenidos
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

                var lista = db.tb_Alerta
                    .Where(x =>
                        x.idDetenidoC5.HasValue &&
                        idsDetenidos.Contains(x.idDetenidoC5.Value) &&
                        x.idPersonaFGEA != null
                    )
                    .Select(x => new
                    {
                        IdPersonaFGEA = x.idPersonaFGEA.Value,
                        IdTbFuente = x.IdTbFuente,
                        Estatus = x.Estatus,
                        Porcentaje = x.Porcentaje,
                        IdTipoAlerta = x.idTipoAlerta,
                        NombreTipoAlerta = x.cat_TipoAlerta != null
                            ? x.cat_TipoAlerta.Alerta
                            : null
                    })
                    .ToList();

                var resultado = lista
                    .Select(x =>
                    {
                        int idPersonaFGEA = x.IdPersonaFGEA;

                        int idTbFuente = x.IdTbFuente == null
                            ? 0
                            : Convert.ToInt32(x.IdTbFuente);

                        int estatus = x.Estatus == null
                            ? 0
                            : Convert.ToInt32(x.Estatus);

                        int porcentaje = x.Porcentaje == null
                            ? 0
                            : Convert.ToInt32(x.Porcentaje);

                        int idTipoAlerta = x.IdTipoAlerta == null
                            ? 0
                            : Convert.ToInt32(x.IdTipoAlerta);

                        string nombreTipoAlerta = string.IsNullOrWhiteSpace(x.NombreTipoAlerta)
                            ? "SIN TIPO DE ALERTA"
                            : x.NombreTipoAlerta.Trim();

                        return Tuple.Create(
                            idPersonaFGEA,   // Item1
                            idTbFuente,      // Item2
                            estatus,         // Item3
                            porcentaje,      // Item4
                            idTipoAlerta,    // Item5
                            nombreTipoAlerta // Item6
                        );
                    })
                    .ToList();

                return resultado;
            }
        }

        public List<Capea_boletin_busqueda> GetInfoCapeas( List<int> idsCapea, List<int> idsAmber, List<int> idsAlba){
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

        private static void AsignarValorCompatible(object destino,string nombrePropiedad,object valor)
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

        public DataTable GetInfoDetenidoMunicipio(List<int> idsDetenidos)
        {
            DataTable tabla = new DataTable();

            tabla.Columns.Add("IDDETENIDO", typeof(int));
            tabla.Columns.Add("NOMBRE", typeof(string));
            tabla.Columns.Add("DOMICILIO", typeof(string));
            tabla.Columns.Add("EDAD", typeof(string));
            tabla.Columns.Add("SEXO", typeof(string));
            tabla.Columns.Add("Folio", typeof(string));
            tabla.Columns.Add("Ocupacion", typeof(string));
            tabla.Columns.Add("Situacion", typeof(string));
            tabla.Columns.Add("FOTO", typeof(string));

            if (idsDetenidos == null || idsDetenidos.Count == 0)
                return tabla;

            idsDetenidos = idsDetenidos
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (idsDetenidos.Count == 0)
                return tabla;

            using (var db = new Filiacion_MunicipiosEntities())
            {
                var query =
                            from d in db.tb_DETENIDO_C5.AsNoTracking()
                            join f in db.tb_FOTO_C5.AsNoTracking()
                                .Where(x => x.FOTO.Contains("1.jpg")
                                         && !x.FOTO.Contains("pertenencia")
                                         && !x.FOTO.Contains("rasgo")
                                         && !x.FOTO.Contains("Evidencia"))
                                on d.IDDETENIDO equals f.IDDETENIDO into fotos
                            from f in fotos.DefaultIfEmpty()
                            where idsDetenidos.Contains(d.IDDETENIDO)
                            select new
                            {
                                Detenido = d,
                                FOTO = f == null ? null : f.FOTO
                            };

                var detenidos = query
                    .OrderBy(x => x.Detenido.IDDETENIDO)
                    .ToList();

                foreach (var item in detenidos)
                {
                    var detenido = item.Detenido;

                    string nombre = string.Join(" ", new[]
                    {
                detenido.NOMBRE,
                detenido.Ap_Paterno,
                detenido.Ap_Materno
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

                    string domicilio = "";

                    if (!string.IsNullOrWhiteSpace(detenido.CALLE))
                        domicilio += detenido.CALLE.Trim();

                    if (!string.IsNullOrWhiteSpace(Convert.ToString(detenido.NUMEXT)))
                        domicilio += " #" + detenido.NUMEXT;

                    if (!string.IsNullOrWhiteSpace(Convert.ToString(detenido.NUMINT)))
                        domicilio += ", INTERIOR: #" + detenido.NUMINT;

                    if (!string.IsNullOrWhiteSpace(detenido.COLONIA))
                        domicilio += ", " + detenido.COLONIA.Trim();

                    if (!string.IsNullOrWhiteSpace(Convert.ToString(detenido.CP)))
                        domicilio += " CP: " + detenido.CP;

                    if (!string.IsNullOrWhiteSpace(detenido.MUNICIPIO))
                        domicilio += ", " + detenido.MUNICIPIO.Trim();

                    if (!string.IsNullOrWhiteSpace(detenido.Estado))
                        domicilio += ", " + detenido.Estado.Trim();

                    tabla.Rows.Add(
                        Convert.ToInt32(detenido.IDDETENIDO),
                        nombre,
                        domicilio,
                        Convert.ToString(detenido.EDAD),
                        Convert.ToString(detenido.SEXO),
                        Convert.ToString(detenido.Folio),
                        Convert.ToString(detenido.Ocupacion),
                        Convert.ToString(detenido.Situacion),
                        Convert.ToString(item.FOTO)
                    );
                }
            }

            return tabla;
        }

        public List<SP_SIC_getCoincidenciasDetenidos_Result> getCoincidenciasDetenidos_Results(List<int> idsNomPerso)
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

        public int ApagarNotificacion(int idDetenido, int idOrigen, int idFuente, int idTipoAlerta)
        {
            if (idDetenido <= 0 ||
                idOrigen <= 0 ||
                idFuente <= 0)
            {
                return 0;
            }

            if (idFuente == 6)
            {
                return ApagarNotificacionDetenidos(
                    idDetenido,
                    idOrigen,
                    idTipoAlerta
                );
            }

            using (var db = new Filiacion_MunicipiosEntities())
            {
                var alertas = db.tb_Alerta
                    .Where(x =>
                        x.idDetenidoC5 == idDetenido &&
                        x.idPersonaFGEA == idOrigen &&
                        x.IdTbFuente == idFuente &&
                        x.Estatus == 1)
                    .ToList();

                DateTime fechaModificacion = DateTime.Now;
                foreach (var alerta in alertas)
                {
                    alerta.Estatus = 0;
                    alerta.FechaModificacion = fechaModificacion;
                }

                db.SaveChanges();

                bool bandera = !db.tb_Alerta.Any(x =>
                    x.idDetenidoC5 == idDetenido &&
                    x.IdTbFuente == idFuente &&
                    (x.Estatus == 1 || x.Estatus == 2));

                if (bandera)
                {
                    ActualizarEstatusDetenido(
                        2,
                        idDetenido
                    );
                }

                return alertas.Count;
            }
        }

        public bool ActualizarEstatusDetenido(int origen, int idDetenido)
        {
            if (idDetenido <= 0)
            {
                return false;
            }

            string situacion;

            switch (origen)
            {
                case 1:
                    situacion = "C";
                    break;

                case 2:
                    situacion = "R";
                    break;

                case 3:
                    situacion = "I";
                    break;

                case 4:
                    situacion = "D";
                    break;

                default:
                    return false;
            }

            var idsRelacionados = GetIdsDetenidosRelacionados(idDetenido);

            using (var db = new SICEntities())
            {
                var registros = db.DETENIDO
                    .Where(x => idsRelacionados.Contains(x.IDDETENIDO))
                    .ToList();

                if (registros.Count == 0)
                {
                    return false;
                }

                foreach (var registro in registros)
                {
                    registro.Situacion = situacion;
                }

                db.SaveChanges();

                return true;
            }
        }

        public int ApagarNotificacionDetenidos(int idDetenido, int idOrigen, int idTipoAlerta)
        {
            if (idDetenido <= 0 ||
                idOrigen <= 0)
            {
                return 0;
            }

            using (var db = new Filiacion_MunicipiosEntities())
            {
                var alertas = ObtenerAlertasRelacionadasDetenidosFGEA(
                    db,
                    idDetenido,
                    idOrigen,
                    idTipoAlerta
                )
                .Where(x => x.Estatus == 1)
                .ToList();

                DateTime fechaModificacion = DateTime.Now;
                foreach (var alerta in alertas)
                {
                    alerta.Estatus = 0;
                    alerta.FechaModificacion = fechaModificacion;
                }

                db.SaveChanges();

                return alertas.Count;
            }
        }

        public int ReactivarNotificacionDetenidos(int idDetenido, int idOrigen, int idTipoAlerta)
        {
            if (idDetenido <= 0 ||
                idOrigen <= 0)
            {
                return 0;
            }

            using (var db = new Filiacion_MunicipiosEntities())
            {
                var alertas = ObtenerAlertasRelacionadasDetenidosFGEA(
                    db,
                    idDetenido,
                    idOrigen,
                    idTipoAlerta
                )
                .Where(x => x.Estatus == 0)
                .ToList();

                DateTime fechaModificacion = DateTime.Now;
                foreach (var alerta in alertas)
                {
                    alerta.Estatus = 1;
                    alerta.FechaModificacion = fechaModificacion;
                }

                db.SaveChanges();

                return alertas.Count;
            }
        }

        public List<int> GetIdsDetenidosRelacionados(int idDetenido)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                var idsDetenidos = db.tb_CoincidenciasNormalizadas
                    .Where(x => x.DetenidoCoincidenciaId == idDetenido)
                    .Select(x => x.DetenidoId)
                    .Distinct()
                    .ToList();

                if (!idsDetenidos.Contains(idDetenido))
                {
                    idsDetenidos.Insert(0, idDetenido);
                }

                return idsDetenidos;
            }
        }

        public List<int> GetInfoDetencionC5(List<int> idsDetenidos)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                if (idsDetenidos == null || idsDetenidos.Count == 0)
                {
                    return new List<int>();
                }

                idsDetenidos = idsDetenidos
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

                var resultado = db.tb_DETENIDO_C5
                    .Where(x => idsDetenidos.Contains(x.IDDETENIDO))
                    .Where(x => x.IDDETENCION.HasValue)
                    .Select(x => x.IDDETENCION.Value)
                    .Where(x => x > 0)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                return resultado;
            }
        }


        public List<tb_DETENCION_C5> GetDatosDetencionesC5(List<int> idsDetenciones)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                if (idsDetenciones == null || idsDetenciones.Count == 0)
                {
                    return new List<tb_DETENCION_C5>();
                }

                idsDetenciones = idsDetenciones
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

                var resultado = db.tb_DETENCION_C5
                    .Where(x => idsDetenciones.Contains(x.IDDETENCION))
                    .OrderByDescending(x => x.FECHA_DETENCION)
                    .ToList();

                return resultado;
            }
        }



        public List<tb_DETENIDO_C5> GetDatosDetenidoC5(List<int> idsDetenidos)
{
    using (var db = new Filiacion_MunicipiosEntities())
    {
        if (idsDetenidos == null || idsDetenidos.Count == 0)
        {
            return new List<tb_DETENIDO_C5>();
        }

        idsDetenidos = idsDetenidos
            .Where(x => x > 0)
            .Distinct()
            .ToList();

        var resultado = db.tb_DETENIDO_C5
            .Where(x => idsDetenidos.Contains(x.IDDETENIDO))
            .OrderBy(x => x.IDDETENIDO)
            .ToList();

        return resultado;
    }
}


        public List<tb_FOTO_C5> GetFotosDetenidoC5(List<int> idsDetenidos)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                if (idsDetenidos == null || idsDetenidos.Count == 0)
                {
                    return new List<tb_FOTO_C5>();
                }

                idsDetenidos = idsDetenidos
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

                var resultado = db.tb_FOTO_C5
                    .Where(x =>
                        x.IDDETENIDO.HasValue &&
                        idsDetenidos.Contains(x.IDDETENIDO.Value) &&
                        x.TIPO != null &&
                        x.TIPO.Contains("Foto") &&
                        x.FOTO != null &&
                        x.FOTO != "")
                    .OrderBy(x => x.IDDETENIDO)
                    .ThenBy(x => x.IDFOTO)
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

        public List<tb_FOTO_C5> GetRasgosDetenidoC5(List<int> idsDetenidos)
        {

            using (var db = new Filiacion_MunicipiosEntities())
            {
                if (idsDetenidos == null || idsDetenidos.Count == 0)
                {
                    return new List<tb_FOTO_C5>();
                }

                idsDetenidos = idsDetenidos
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

                var resultado = db.tb_FOTO_C5
                    .Where(x =>
                        x.IDDETENIDO.HasValue &&
                        idsDetenidos.Contains(x.IDDETENIDO.Value) &&
                        x.TIPO != null &&
                        x.TIPO.Contains("Rasgo") &&
                        x.FOTO != null &&
                        x.FOTO != "")
                    .OrderBy(x => x.IDDETENIDO)
                    .ThenBy(x => x.IDFOTO)
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

        public int ReactivarNotificacion(int idDetenido, int idOrigen, int idFuente, int idTipoAlerta)
        {
            if (idDetenido <= 0 ||
                idOrigen <= 0 ||
                idFuente <= 0)
            {
                return 0;
            }

            if (idFuente == 6)
            {
                return ReactivarNotificacionDetenidos(
                    idDetenido,
                    idOrigen,
                    idTipoAlerta
                );
            }

            using (var db = new Filiacion_MunicipiosEntities())
            {
                var alertas = db.tb_Alerta
                    .Where(x =>
                        x.idDetenidoC5 == idDetenido &&
                        x.idPersonaFGEA == idOrigen &&
                        x.IdTbFuente == idFuente &&
                        x.Estatus == 0)
                    .ToList();

                DateTime fechaModificacion = DateTime.Now;
                foreach (var alerta in alertas)
                {
                    alerta.Estatus = 1;
                    alerta.FechaModificacion = fechaModificacion;
                }

                db.SaveChanges();

                /*
                 * Se conserva tu lógica actual.
                 */
                if (db.tb_Alerta.Any(x =>
                    x.idDetenidoC5 == idDetenido &&
                    x.IdTbFuente != 6 &&
                    (x.Estatus != 1 || x.Estatus != 2)))
                {
                    ActualizarEstatusDetenido(
                        2,
                        idDetenido
                    );
                }

                return alertas.Count;
            }
        }

        public Tuple<int, int, int> GetConteoAlertasPorEstatus(List<int> idsDetenidos)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                if (idsDetenidos == null || idsDetenidos.Count == 0)
                {
                    return Tuple.Create(0, 0, 0);
                }

                var alertas = db.tb_Alerta
                    .Where(x =>
                        x.idDetenidoC5.HasValue &&
                        idsDetenidos.Contains(x.idDetenidoC5.Value))
                    .ToList();

                var alertasUnicas = alertas
                    .GroupBy(x => new
                    {
                        x.idPersonaFGEA,
                        x.idTipoAlerta,
                        x.IdTbFuente
                    })
                    .Select(g => g
                        .OrderByDescending(x => x.Porcentaje ?? 0)
                        .ThenByDescending(x => x.FechaAlerta)
                        .ThenByDescending(x => x.idAlerta)
                        .First())
                    .ToList();

                int totalActivas = alertasUnicas.Count(x =>
                    x.Estatus == 1
                );

                int totalRevisadas = alertasUnicas.Count(x =>
                    x.Estatus == 0
                );

                int totalConfirmadas = alertasUnicas.Count(x =>
                    x.Estatus == 2 ||
                    x.Estatus == 3
                );

                return Tuple.Create(
                    totalActivas,
                    totalRevisadas,
                    totalConfirmadas
                );
            }
        }

        public List<Tuple<int, int>> GetTiposAlertas(List<int> idsDetenidos)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                if (idsDetenidos == null || idsDetenidos.Count == 0)
                {
                    return new List<Tuple<int, int>>();
                }

                idsDetenidos = idsDetenidos
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

                var alertas = db.tb_Alerta
                    .Where(x =>
                        x.idDetenidoC5.HasValue &&
                        idsDetenidos.Contains(x.idDetenidoC5.Value))
                    .ToList();

                /*
                 * Si la misma alerta existe en varios IDDETENIDO relacionados,
                 * conservamos solamente la de mayor porcentaje.
                 *
                 * La identidad de una alerta se determina por:
                 *
                 * idPersonaFGEA
                 * idTipoAlerta
                 * IdTbFuente
                 */
                var alertasUnicas = alertas
                    .GroupBy(x => new
                    {
                        x.idPersonaFGEA,
                        x.idTipoAlerta,
                        x.IdTbFuente
                    })
                    .Select(grupo => grupo
                        .OrderByDescending(x => x.Porcentaje ?? 0)
                        .ThenByDescending(x => x.FechaAlerta)
                        .ThenByDescending(x => x.idAlerta)
                        .First())
                    .ToList();

                var resultado = alertasUnicas
                    .GroupBy(x => x.idTipoAlerta)
                    .Select(grupo => Tuple.Create(
                        grupo.Key == null
                            ? 0
                            : Convert.ToInt32(grupo.Key),
                        grupo.Count()
                    ))
                    .OrderByDescending(x => x.Item2)
                    .ToList();

                return resultado;
            }
        }

        public int ActualizarEstatusNotificacion(int idDetenido, int idOrigen, int idFuente, int idTipoAlerta, int nuevoEstatus)
        {
            var idsRelacionados = GetIdsDetenidosRelacionados(idDetenido);

            if (nuevoEstatus != 0 &&
                nuevoEstatus != 1 &&
                nuevoEstatus != 2 &&
                nuevoEstatus != 3)
            {
                throw new ArgumentException("El estatus recibido no es válido.");
            }

            if (idDetenido <= 0 ||
                idOrigen <= 0 ||
                idFuente <= 0)
            {
                return 0;
            }

            /*
             * FUENTE 6 tiene la particularidad de que:
             *
             * Tipo 1 = Nom_perso.id
             * Tipo 2 = CLAVE_PERSO
             * Tipo 3 = CLAVE_PERSO
             *
             * Se manda al método especializado.
             */
            if (idFuente == 6)
            {
                return ActualizarEstatusNotificacionDetenidos(
                    idDetenido,
                    idOrigen,
                    idTipoAlerta,
                    nuevoEstatus
                );
            }

            using (var db = new Filiacion_MunicipiosEntities())
            {
                /*
                 * Se buscan las alertas de cualquiera de los IDDETENIDO
                 * relacionados con la raíz.
                 *
                 * Una alerta se identifica por:
                 * idPersonaFGEA
                 * idTipoAlerta
                 * IdTbFuente
                 */
                var alertas = db.tb_Alerta
                    .Where(x =>
                        x.idDetenidoC5.HasValue &&
                        idsRelacionados.Contains(x.idDetenidoC5.Value) &&
                        x.idPersonaFGEA == idOrigen &&
                        x.IdTbFuente == idFuente &&
                        x.idTipoAlerta == idTipoAlerta)
                    .ToList();

                DateTime fechaModificacion = DateTime.Now;
                foreach (var alerta in alertas)
                {
                    alerta.Estatus = nuevoEstatus;
                    alerta.FechaModificacion = fechaModificacion;
                }

                /*
                 * FUENTE 1 - DETENIDOS MUNICIPIOS
                 *
                 * Al confirmar identidad se genera la relación
                 * entre el detenido coincidente y el detenido raíz.
                 */
                if (idFuente == 1)
                {
                    if (nuevoEstatus == 2)
                    {
                        bool yaExisteRelacion = db.tb_CoincidenciasNormalizadas
                            .Any(x => x.DetenidoId == idOrigen);

                        if (!yaExisteRelacion)
                        {
                            var nuevaCoincidencia = new tb_CoincidenciasNormalizadas
                            {
                                DetenidoId = idOrigen,
                                DetenidoCoincidenciaId = idDetenido,
                                TipoCoincidencia = "visual",
                                FechaRegistro = DateTime.Now
                            };

                            db.tb_CoincidenciasNormalizadas.Add(
                                nuevaCoincidencia
                            );
                        }
                    }

                    db.SaveChanges();

                    return alertas.Count;
                }

                /*
                 * Primero se guardan los nuevos estatus
                 * de las alertas encontradas.
                 */
                db.SaveChanges();

                /*
                 * Se revisan las alertas de todos los IDDETENIDO
                 * relacionados para determinar el estado general
                 * del detenido raíz.
                 */
                if (nuevoEstatus == 2)
                {
                    bool tieneConfirmada = db.tb_Alerta.Any(x =>
                        x.idDetenidoC5.HasValue &&
                        idsRelacionados.Contains(x.idDetenidoC5.Value) &&
                        x.IdTbFuente != 6 &&
                        x.IdTbFuente != 1 &&
                        x.Estatus == 2);

                    if (tieneConfirmada)
                    {
                        ActualizarEstatusDetenido(
                            1,
                            idDetenido
                        );
                    }
                }
                else if (nuevoEstatus == 1)
                {
                    bool tienePendiente = db.tb_Alerta.Any(x =>
                        x.idDetenidoC5.HasValue &&
                        idsRelacionados.Contains(x.idDetenidoC5.Value) &&
                        x.IdTbFuente != 6 &&
                        x.IdTbFuente != 1 &&
                        x.Estatus == 1);

                    if (tienePendiente)
                    {
                        ActualizarEstatusDetenido(
                            3,
                            idDetenido
                        );
                    }
                }
                else if (nuevoEstatus == 0)
                {
                    bool tieneDescartada = db.tb_Alerta.Any(x =>
                        x.idDetenidoC5.HasValue &&
                        idsRelacionados.Contains(x.idDetenidoC5.Value) &&
                        x.IdTbFuente != 6 &&
                        x.IdTbFuente != 1 &&
                        x.Estatus != 1 &&
                        x.Estatus != 2);

                    if (tieneDescartada)
                    {
                        ActualizarEstatusDetenido(
                            2,
                            idDetenido
                        );
                    }
                }
                else if (nuevoEstatus == 3)
                {
                    bool tieneResguardo = db.tb_Alerta.Any(x =>
                        x.idDetenidoC5.HasValue &&
                        idsRelacionados.Contains(x.idDetenidoC5.Value) &&
                        x.IdTbFuente != 6 &&
                        x.IdTbFuente != 1 &&
                        x.Estatus == 3);

                    if (tieneResguardo)
                    {
                        ActualizarEstatusDetenido(
                            4,
                            idDetenido
                        );
                    }
                }

                return alertas.Count;
            }
        }

        public int ActualizarEstatusNotificacionDetenidos(int idDetenido, int idOrigen, int idTipoAlerta, int nuevoEstatus)
        {
            if (nuevoEstatus != 0 &&
                nuevoEstatus != 1 &&
                nuevoEstatus != 2)
            {
                throw new ArgumentException("El estatus recibido no es válido.");
            }

            if (idDetenido <= 0 ||
                idOrigen <= 0)
            {
                return 0;
            }

            var idsRelacionados = GetIdsDetenidosRelacionados(idDetenido);

            using (var db = new Filiacion_MunicipiosEntities())
            {
                /*
                 * Se obtienen las alertas de la misma identidad
                 * para TODOS los IDDETENIDO relacionados.
                 */
                var alertas = new List<tb_Alerta>();

                foreach (var idDetenidoRelacionado in idsRelacionados)
                {
                    var alertasRelacionado = ObtenerAlertasRelacionadasDetenidosFGEA(
                        db,
                        idDetenidoRelacionado,
                        idOrigen,
                        idTipoAlerta
                    );

                    if (alertasRelacionado != null &&
                        alertasRelacionado.Count > 0)
                    {
                        alertas.AddRange(alertasRelacionado);
                    }
                }

                /*
                 * Evitamos repetir una misma alerta por si llegara
                 * a obtenerse más de una vez.
                 */
                alertas = alertas
                    .GroupBy(x => x.idAlerta)
                    .Select(x => x.First())
                    .ToList();

                /*
                 * Al confirmar una ficha de Detenidos FGEA:
                 *
                 * primero se descartan TODAS las coincidencias
                 * de Fuente 6 pertenecientes a cualquiera de los
                 * IDDETENIDO relacionados.
                 */
                DateTime fechaModificacion = DateTime.Now;

                if (nuevoEstatus == 2)
                {
                    var desactivar = db.tb_Alerta.Where(x => x.idDetenidoC5.HasValue && idsRelacionados.Contains(x.idDetenidoC5.Value) && x.IdTbFuente == 6).ToList();

                    foreach (var alerta in desactivar)
                    {
                        alerta.Estatus = 0;
                        alerta.FechaModificacion = fechaModificacion;
                    }
                }

                foreach (var alerta in alertas)
                {
                    alerta.Estatus = nuevoEstatus;
                    alerta.FechaModificacion = fechaModificacion;
                }

                db.SaveChanges();

                return alertas.Count;
            }
        }


        public DataTable BuscarMandamientosCandidatosPorNombre( string nombreCompleto) {
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


        private static List<string> ObtenerTokensMandamientos(string nombreCompleto)
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

        private static string NormalizarNombreMandamientos(string texto){
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

        public int GetIdDetenidoRaiz(int idDetenido)
        {
            using (var db = new Filiacion_MunicipiosEntities())
            {
                var relacion = db.tb_CoincidenciasNormalizadas
                    .AsNoTracking()
                    .FirstOrDefault(x => x.DetenidoId == idDetenido);

                if (relacion == null)
                {
                    return idDetenido;
                }

                return Convert.ToInt32(relacion.DetenidoCoincidenciaId);
            }
        }

        private Tuple<int, List<int>> ObtenerIdentidadFiliacion(int idReferencia, int idTipoAlerta)
        {
            int clavePerso = 0;
            List<int> idsNomPerso = new List<int>();

            if (idReferencia <= 0)
            {
                return Tuple.Create(clavePerso, idsNomPerso);
            }

            /*
             * Tipo 2 = Foto
             * Tipo 3 = Huella
             *
             * En estos casos idReferencia YA ES CLAVE_PERSO.
             */
            if (idTipoAlerta == 2 || idTipoAlerta == 3)
            {
                clavePerso = idReferencia;
            }

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

                    /*
                     * Tipo 1 = Nombre
                     *
                     * idReferencia es Nom_perso.id.
                     * Primero obtenemos su CLAVE_PERSO.
                     */
                    if (idTipoAlerta == 1)
                    {
                        using (SqlCommand cmd = new SqlCommand(@"
SELECT TOP 1
    CLAVE_PERSO
FROM dbo.Nom_perso
WHERE id = @id;", cn))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idReferencia;

                            object resultado = cmd.ExecuteScalar();

                            if (resultado != null && resultado != DBNull.Value)
                            {
                                int.TryParse(
                                    Convert.ToString(resultado),
                                    out clavePerso
                                );
                            }
                        }
                    }

                    /*
                     * Si encontramos CLAVE_PERSO, recuperamos TODOS
                     * los Nom_perso.id asociados a esa persona.
                     */
                    if (clavePerso > 0)
                    {
                        using (SqlCommand cmd = new SqlCommand(@"
SELECT
    id
FROM dbo.Nom_perso
WHERE CLAVE_PERSO = @clavePerso;", cn))
                        {
                            cmd.Parameters.Add("@clavePerso", SqlDbType.Int).Value = clavePerso;

                            using (SqlDataReader dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    int idNomPerso = 0;

                                    int.TryParse(
                                        Convert.ToString(dr["id"]),
                                        out idNomPerso
                                    );

                                    if (idNomPerso > 0 &&
                                        !idsNomPerso.Contains(idNomPerso))
                                    {
                                        idsNomPerso.Add(idNomPerso);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            /*
             * Si por alguna razón no encontramos CLAVE_PERSO,
             * conservamos el ID recibido para no romper la lógica
             * anterior de coincidencia por nombre.
             */
            if (idTipoAlerta == 1 &&
                idReferencia > 0 &&
                !idsNomPerso.Contains(idReferencia))
            {
                idsNomPerso.Add(idReferencia);
            }

            return Tuple.Create(
                clavePerso,
                idsNomPerso
            );
        }

        private List<tb_Alerta> ObtenerAlertasRelacionadasDetenidosFGEA(Filiacion_MunicipiosEntities db, int idDetenido, int idOrigen, int idTipoAlerta)
        {
            if (db == null ||
                idDetenido <= 0 ||
                idOrigen <= 0)
            {
                return new List<tb_Alerta>();
            }

            /*
             * Si por algún motivo llega otro tipo de alerta,
             * conservamos el comportamiento anterior.
             */
            if (idTipoAlerta != 1 &&
                idTipoAlerta != 2 &&
                idTipoAlerta != 3)
            {
                return db.tb_Alerta
                    .Where(x =>
                        x.idDetenidoC5 == idDetenido &&
                        x.IdTbFuente == 6 &&
                        x.idPersonaFGEA.HasValue &&
                        x.idPersonaFGEA.Value == idOrigen)
                    .ToList();
            }

            var identidad = ObtenerIdentidadFiliacion(
                idOrigen,
                idTipoAlerta
            );

            int clavePerso = identidad.Item1;
            List<int> idsNomPerso = identidad.Item2;

            return db.tb_Alerta
                .Where(x =>
                    x.idDetenidoC5 == idDetenido &&
                    x.IdTbFuente == 6 &&
                    x.idPersonaFGEA.HasValue &&
                    (
                        /*
                         * Tipo 1:
                         * idPersonaFGEA contiene Nom_perso.id.
                         */
                        (
                            x.idTipoAlerta == 1 &&
                            idsNomPerso.Contains(x.idPersonaFGEA.Value)
                        )
                        ||
                        /*
                         * Tipo 2 / 3:
                         * idPersonaFGEA contiene CLAVE_PERSO.
                         */
                        (
                            (x.idTipoAlerta == 2 || x.idTipoAlerta == 3) &&
                            clavePerso > 0 &&
                            x.idPersonaFGEA.Value == clavePerso
                        )
                    ))
                .ToList();
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