using Objetivos_Prioritarios.ControllersServices;
using Objetivos_Prioritarios.Helpers;
using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Models.Extends;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.Controllers
{
    public class SICController : ABaseController
    {
        #region Configuración del controlador

        private readonly FiliacionMunicipalService _filiacionService;
        private const string SessionResultadosCoincidencias = "SIC_RESULTADOS_COINCIDENCIAS";
        private const string RutaBaseFotosC5 = @"\\10.13.1.232\detenidos";
        private const string RutaBaseFotosCapea = @"https://www.fiscalia-aguascalientes.gob.mx/";
        //private const string RutaBaseFotosCapea = @"\\234fgea\temporalfiliacion$\CAPEA\";
        public SICController()
        {
            _filiacionService = new FiliacionMunicipalService();
        }

        #endregion

        #region Pantalla principal y listado SIC

        public ActionResult Index()
        {
            ViewBag.Title = "SIC";
            ViewBag.ContactosMunicipios = _filiacionService.GetContactosMunicipios();
            return View();
        }

        [HttpGet]
        public JsonResult ObtenerSIC()
        {
            try
            {
                var registros = _filiacionService.GetAlertas() ?? new List<sp_Alertas_Result>();
                var idsDetenidos = registros
                    .Select(x => Convert.ToInt32(x.IdDetenidoC5))
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();
                using (var db = new Filiacion_MunicipiosEntities())
                {
                    // Se consultan todas las alertas reales porque un detenido puede tener varias.
                    var alertasDetalle = db.tb_Alerta
                        .Where(x => idsDetenidos.Contains((int)x.idDetenidoC5))
                        .Select(x => new
                        {
                            x.idDetenidoC5,
                            x.IdTbFuente,
                            x.Estatus
                        })
                        .ToList();
                    var resumenPorDetenido = alertasDetalle
                        .GroupBy(x => Convert.ToInt32(x.idDetenidoC5))
                        .ToDictionary(
                            grupo => grupo.Key,
                            grupo => new
                            {
                                TieneConfirmacionDetenidos = grupo.Any(x => Convert.ToInt32(x.IdTbFuente) == 6 && (Convert.ToInt32(x.Estatus) == 2)),
                                TieneObjetivoConfirmado = grupo.Any(x => Convert.ToInt32(x.IdTbFuente) != 6 && (Convert.ToInt32(x.Estatus) == 2)),
                                TieneDetenidoResguardo = grupo.Any(x => Convert.ToInt32(x.IdTbFuente) != 6 && Convert.ToInt32(x.Estatus) == 3),
                                TieneFuente6Activa = grupo.Any(x => Convert.ToInt32(x.IdTbFuente) == 6 && Convert.ToInt32(x.Estatus) != 0),
                                TieneOtraFuenteActiva = grupo.Any(x => Convert.ToInt32(x.IdTbFuente) != 6 && Convert.ToInt32(x.Estatus) != 0),
                                TienePendiente = grupo.Any(x => Convert.ToInt32(x.Estatus) == 1),
                                FuentesActivas = string.Join(
                                    ",",
                                    grupo
                                        .Where(x => Convert.ToInt32(x.Estatus) != 0)
                                        .Select(x => Convert.ToInt32(x.IdTbFuente))
                                        .Distinct()
                                        .OrderBy(x => x)
                                )
                            }
                        );
                    var resultado = registros
                        .Select(registro =>
                        {
                            int idDetenidoC5 = Convert.ToInt32(registro.IdDetenidoC5);
                            var resumen = resumenPorDetenido.ContainsKey(idDetenidoC5) ? resumenPorDetenido[idDetenidoC5] : null;

                            return new
                            {
                                registro.IdDetenidoC5,
                                registro.NombrePersona,
                                registro.FechaDetencion,
                                registro.MunicipioDetencion,
                                registro.TotalAlertas,

                                // Campos anteriores conservados para no romper el JavaScript actual.
                                Fuente = resumen != null ? resumen.FuentesActivas : Convert.ToString(registro.Fuente),
                                Estatus = resumen == null ? Convert.ToInt32(registro.Estatus) : resumen.TieneConfirmacionDetenidos ? 2 : resumen.TienePendiente ? 1 : 0,

                                // Indicadores calculados con todas las alertas reales del detenido.
                                TieneConfirmacionDetenidos = resumen != null && resumen.TieneConfirmacionDetenidos,
                                TieneFuente6Activa = resumen != null && resumen.TieneFuente6Activa,
                                TieneOtraFuenteActiva = resumen != null && resumen.TieneOtraFuenteActiva,
                                TieneObjetivoConfirmado = resumen != null && resumen.TieneObjetivoConfirmado,
                                TieneDetenidoResguardo = resumen != null && resumen.TieneDetenidoResguardo
                            };
                        })
                        .ToList();
                    return Json(
                        new
                        {
                            success = true,
                            data = resultado
                        },
                        JsonRequestBehavior.AllowGet
                    );
                }
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        success = false,
                        message = "Ocurrió un error al obtener las alertas del SIC: " + ex.Message,
                        data = new object[] { }
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
        }

        #endregion

        #region Detalle del detenido C5

        public ActionResult DetalleDetenidoC5(int idDetenido)
        {
            var detenido = _filiacionService.GetInfoDetenido(idDetenido);
            var detenidoraiz = detenido.IDDETENIDO;

            var idsrelacionados = _filiacionService.GetIdsDetenidosRelacionados(detenidoraiz);

            if (detenido == null)
                return HttpNotFound("No se encontró información del detenido.");
            var idsDetencionesC5 = _filiacionService.GetInfoDetencionC5(idsrelacionados);
            var detencionesC5 = _filiacionService.GetDatosDetencionesC5(idsDetencionesC5);
            var fotosC5 = _filiacionService.GetFotosDetenidoC5(idsrelacionados);
            var huellasC5 = _filiacionService.GetHuellasDetenidoC5(detenidoraiz);
            var rasgosC5 = _filiacionService.GetRasgosDetenidoC5(idsrelacionados);
            var conteoAlertas = _filiacionService.GetConteoAlertasPorEstatus(idsrelacionados);
            var tiposAlertas = _filiacionService.GetTiposAlertas(idsrelacionados);
            var datosDetenidoC5 = _filiacionService.GetDatosDetenidoC5(idsrelacionados);
            var alertasTipo = _filiacionService.GetAlertaTipo(idsrelacionados);

            ViewBag.DetencionC5 = detencionesC5;
            ViewBag.FotosC5 = fotosC5;
            ViewBag.HuellasC5 = huellasC5;
            ViewBag.RasgosC5 = rasgosC5;
            ViewBag.TotalAlertasActivas = conteoAlertas.Item1;
            ViewBag.TotalAlertasRevisadas = conteoAlertas.Item2;
            ViewBag.TotalCoincidenciasConfirmadas = conteoAlertas.Item3;
            ViewBag.TiposAlertas = tiposAlertas;
            ViewBag.DatosDetenidoC5 = datosDetenidoC5;
            ViewBag.IdDetenidoRaiz = detenidoraiz;
            ViewBag.AlertasTipo = alertasTipo;
            ViewBag.idsrelacionados = idsrelacionados;
            bool identidadConfirmada = false;
            bool esDeInteres = false;

            if (alertasTipo != null && alertasTipo.Count > 0)
            {
                identidadConfirmada = alertasTipo.Any(x => x.Item3 == 2 || x.Item3 == 3);
                esDeInteres = alertasTipo.Any(x => (x.Item3 == 2 || x.Item3 == 3) && x.Item2 != 6);
            }
            ViewBag.IdentidadConfirmada = identidadConfirmada;
            ViewBag.EsDeInteres = esDeInteres;
            var estadoIdentidad = CalcularEstadoIdentidad(alertasTipo);
            ViewBag.EstadoIdentidadCodigo = estadoIdentidad.Item1;
            ViewBag.EstadoIdentidadNombre = estadoIdentidad.Item2;
            ViewBag.EstadoIdentidadDetalle = estadoIdentidad.Item3;

            // Se separan los IDs por fuente para consultar únicamente los registros relacionados.
            var idsCapea = alertasTipo.Where(x => x.Item2 == 2)
                                        .Select(x => x.Item1)
                                        .Where(x => x > 0).Distinct().ToList();
            var idsPersonaInteres = alertasTipo.Where(x => x.Item2 == 3)
                                                .Select(x => x.Item1)
                                                .Where(x => x > 0).Distinct().ToList();
            var idsMandamientos = alertasTipo.Where(x => x.Item2 == 4)
                                               .Select(x => x.Item1)
                                               .Where(x => x > 0).Distinct().ToList();
            var idsNombreObjetivo = alertasTipo.Where(x => x.Item2 == 5)
                                                 .Select(x => x.Item1)
                                                 .Where(x => x > 0).Distinct().ToList();
            // Detenidos FGEA - Tipo alerta 1: Item1 corresponde a Nom_perso.id.
            var idsNomPersoDetenidos = alertasTipo
                .Where(x => x.Item2 == 6 && x.Item5 == 1)
                .Select(x => x.Item1)
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            // Detenidos FGEA - Tipo alerta 2 o 3: Item1 corresponde a CLAVE_PERSO.
            var clavesPersoDetenidos = alertasTipo
                .Where(x => x.Item2 == 6 && (x.Item5 == 2 || x.Item5 == 3))
                .Select(x => x.Item1)
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            var idsAmber = alertasTipo.Where(x => x.Item2 == 7)
                                       .Select(x => x.Item1)
                                       .Where(x => x > 0).Distinct().ToList();
            var idsAlba = alertasTipo.Where(x => x.Item2 == 8)
                                      .Select(x => x.Item1)
                                      .Where(x => x > 0).Distinct().ToList();
            var idsDetenidosMunicipiosFusionados = alertasTipo
    .Where(x => x.Item2 == 1 && x.Item3 == 2)
    .Select(x => x.Item1)
    .Where(x => x > 0)
    .Distinct()
    .ToList();

            var idsDetenidosMunicipios = alertasTipo
                .Where(x =>
                    x.Item2 == 1 &&
                    x.Item3 != 2 &&
                    !idsDetenidosMunicipiosFusionados.Contains(x.Item1))
                .Select(x => x.Item1)
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            // CAPEA / AMBER / ALBA (fuentes 2, 7 y 8).
            var capeas = _filiacionService.GetInfoCapeas(idsCapea, idsAmber, idsAlba);

            capeas = capeas
                .Select(x => new
                {
                    Registro = x,
                    DatosAlerta = ObtenerDatosAlerta(
                        alertasTipo,
                        new int[] { x.id_boletin_busqueda },
                        new int[] { 2, 7, 8 }
                    )
                })
                .OrderByDescending(x =>
                    x.DatosAlerta.Item2 == 2 || x.DatosAlerta.Item2 == 3
                        ? 3
                        : x.DatosAlerta.Item2 == 1
                            ? 2
                            : x.DatosAlerta.Item2 == 0
                                ? 1
                                : 0
                )
                .ThenByDescending(x => x.DatosAlerta.Item3)
                .Select(x => x.Registro)
                .ToList();

            ViewBag.Capeas = capeas;

            // Personas de interés (fuente 3).
            var personasInteres = _filiacionService.GetInfoPersonasFiliacion(idsPersonaInteres);
            AgregarDatosAlertaADataTable(personasInteres, alertasTipo, new int[] { 3 }, "idPersona");
            personasInteres = OrdenarPorPorcentaje(personasInteres);
            ViewBag.PersonasInteres = personasInteres;

            // Mandamientos judiciales (fuente 4).
            var mandamientos = _filiacionService.GetInfoMandamientos(idsMandamientos);
            AgregarDatosAlertaADataTable(mandamientos, alertasTipo, new int[] { 4 }, "IdOrigenAlerta");
            mandamientos = OrdenarPorPorcentaje(mandamientos);
            ViewBag.Mandamientos = mandamientos;

            // Detenidos FGEA (fuente 6).
            var detenidos = _filiacionService.GetInfoDetenidos(idsNomPersoDetenidos, clavesPersoDetenidos);
            AgregarFotoUrlADetenidos(detenidos);
            AgregarDatosAlertaADataTable(
    detenidos,
    alertasTipo,
    new int[] { 6 },
    "IdsNomPerso",
    "CLAVE_PERSO"
);
            detenidos = OrdenarPorPorcentaje(detenidos);
            ViewBag.Detenidos = detenidos;

            // Detenidos de municipios: por ahora, sin ID se consultan todos.
            var detenidosMunicipios = _filiacionService.GetInfoDetenidoMunicipio(idsDetenidosMunicipios);
            AgregarDatosAlertaADataTable(detenidosMunicipios, alertasTipo, new int[] { 1 }, "IDDETENIDO");
            detenidosMunicipios = OrdenarPorPorcentaje(detenidosMunicipios);
            ViewBag.DetenidosMunicipios = detenidosMunicipios;

            // Objetivos prioritarios (fuente 5).
            var objetivosPrioritarios = _filiacionService.GetInfoObjetivosPrioritarios(idsNombreObjetivo);
            AgregarDatosAlertaADataTable(objetivosPrioritarios, alertasTipo, new int[] { 5 }, "IdsNombreObjetivoOrigenAlerta");
            objetivosPrioritarios = OrdenarPorPorcentaje(objetivosPrioritarios);
            ViewBag.ObjetivosPrioritarios = objetivosPrioritarios;

            // Se preparan funciones y valores auxiliares para la vista.
            PrepararViewBagDetalleDetenido( detencionesC5, alertasTipo, tiposAlertas );
            return View(detenido);
        }

        #endregion

        #region Procesamiento de alertas y coincidencias

        private void AgregarDatosAlertaADataTable(System.Data.DataTable tabla, List<Tuple<int, int, int, int, int, string>> alertasTipo, int[] idsFuentes, string columnaIdsOrigen, string columnaClavePerso = null)
        {
            if (tabla == null)
            {
                return;
            }

            if (alertasTipo == null)
            {
                alertasTipo = new List<Tuple<int, int, int, int, int, string>>();
            }

            if (idsFuentes == null)
            {
                idsFuentes = new int[0];
            }

            if (!tabla.Columns.Contains("IdFuenteAlerta"))
            {
                tabla.Columns.Add("IdFuenteAlerta", typeof(int));
            }

            if (!tabla.Columns.Contains("EstatusAlerta"))
            {
                tabla.Columns.Add("EstatusAlerta", typeof(int));
            }

            if (!tabla.Columns.Contains("PorcentajeCoincidencia"))
            {
                tabla.Columns.Add("PorcentajeCoincidencia", typeof(int));
            }

            if (!tabla.Columns.Contains("PorcentajeCoincidenciaTexto"))
            {
                tabla.Columns.Add("PorcentajeCoincidenciaTexto", typeof(string));
            }

            if (!tabla.Columns.Contains("IdTipoAlerta"))
            {
                tabla.Columns.Add("IdTipoAlerta", typeof(int));
            }

            if (!tabla.Columns.Contains("NombreTipoAlerta"))
            {
                tabla.Columns.Add("NombreTipoAlerta", typeof(string));
            }

            foreach (System.Data.DataRow row in tabla.Rows)
            {
                string idsTexto = "";

                if (row.Table.Columns.Contains(columnaIdsOrigen) &&
                    row[columnaIdsOrigen] != DBNull.Value)
                {
                    idsTexto = Convert.ToString(row[columnaIdsOrigen]);
                }

                var idsOrigen = ConvertirTextoAListaEnteros(idsTexto);

                int clavePerso = 0;

                if (!string.IsNullOrWhiteSpace(columnaClavePerso) &&
                    row.Table.Columns.Contains(columnaClavePerso) &&
                    row[columnaClavePerso] != DBNull.Value)
                {
                    int.TryParse(
                        Convert.ToString(row[columnaClavePerso]),
                        out clavePerso
                    );
                }

                var datosAlerta = ObtenerDatosAlerta(
                    alertasTipo,
                    idsOrigen,
                    idsFuentes,
                    clavePerso
                );

                int idFuenteAlerta = datosAlerta.Item1;
                int estatusAlerta = datosAlerta.Item2;
                int porcentaje = datosAlerta.Item3;
                int idTipoAlerta = datosAlerta.Item4;
                string nombreTipoAlerta = datosAlerta.Item5;

                if (idFuenteAlerta <= 0)
                {
                    row["IdFuenteAlerta"] = DBNull.Value;
                    row["EstatusAlerta"] = DBNull.Value;
                    row["PorcentajeCoincidencia"] = DBNull.Value;
                    row["PorcentajeCoincidenciaTexto"] = "";
                    row["IdTipoAlerta"] = DBNull.Value;
                    row["NombreTipoAlerta"] = "";

                    continue;
                }

                row["IdFuenteAlerta"] = idFuenteAlerta;
                row["EstatusAlerta"] = estatusAlerta;

                if (porcentaje > 0)
                {
                    row["PorcentajeCoincidencia"] = porcentaje;
                    row["PorcentajeCoincidenciaTexto"] = FormatearPorcentaje(porcentaje);
                }
                else
                {
                    row["PorcentajeCoincidencia"] = DBNull.Value;
                    row["PorcentajeCoincidenciaTexto"] = "";
                }

                if (idTipoAlerta > 0)
                {
                    row["IdTipoAlerta"] = idTipoAlerta;
                }
                else
                {
                    row["IdTipoAlerta"] = DBNull.Value;
                }

                row["NombreTipoAlerta"] = string.IsNullOrWhiteSpace(nombreTipoAlerta)
                    ? "SIN TIPO DE ALERTA"
                    : nombreTipoAlerta.Trim();
            }
        }

        private List<Tuple<int, int, int, int, int, string>> ObtenerAlertasDetenido(List<Tuple<int, int, int, int, int, string>> alertasTipo, IEnumerable<int> idsNomPerso, int clavePerso)
        {
            List<int> listaIdsNomPerso = idsNomPerso == null
                ? new List<int>()
                : idsNomPerso.Where(x => x > 0).Distinct().ToList();

            if (alertasTipo == null)
            {
                return new List<Tuple<int, int, int, int, int, string>>();
            }

            return alertasTipo
                .Where(x =>
                    x.Item2 == 6 &&
                    (
                        (x.Item5 == 1 && listaIdsNomPerso.Contains(x.Item1))
                        ||
                        ((x.Item5 == 2 || x.Item5 == 3) && clavePerso > 0 && x.Item1 == clavePerso)
                    )
                )
                .ToList();
        }

        private System.Collections.ArrayList ObtenerCoincidenciasAlertaDetenido(List<Tuple<int, int, int, int, int, string>> alertasTipo, IEnumerable<int> idsNomPerso, int clavePerso)
        {
            var resultado = new System.Collections.ArrayList();

            var alertas = ObtenerAlertasDetenido(
                alertasTipo,
                idsNomPerso,
                clavePerso
            );

            var coincidencias = alertas
                .GroupBy(x => x.Item5)
                .Select(grupo =>
                    grupo
                        .OrderByDescending(x => x.Item4)
                        .ThenByDescending(x =>
                            x.Item3 == 3
                                ? 4
                                : x.Item3 == 2
                                    ? 3
                                    : x.Item3 == 1
                                        ? 2
                                        : 1
                        )
                        .First()
                )
                .OrderByDescending(x => x.Item4)
                .ThenBy(x => x.Item5)
                .ToList();

            foreach (var coincidencia in coincidencias)
            {
                string nombreTipoAlerta = string.IsNullOrWhiteSpace(coincidencia.Item6)
                    ? "SIN TIPO DE ALERTA"
                    : coincidencia.Item6.Trim();

                resultado.Add(
                    new object[]
                    {
                coincidencia.Item2, // Fuente
                coincidencia.Item3, // Estatus
                coincidencia.Item4, // Porcentaje
                coincidencia.Item5, // Tipo alerta
                nombreTipoAlerta
                    }
                );
            }

            return resultado;
        }

        private Tuple<int, int, int, int, string> ObtenerDatosAlerta(List<Tuple<int, int, int, int, int, string>> alertasTipo, IEnumerable<int> idsOrigen, IEnumerable<int> idsFuentes, int clavePerso = 0)
        {
            if (alertasTipo == null)
            {
                return Tuple.Create(0, 0, 0, 0, "");
            }

            var listaIdsOrigen = idsOrigen == null
                ? new List<int>()
                : idsOrigen
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

            var listaFuentes = idsFuentes == null
                ? new List<int>()
                : idsFuentes
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

            /*
             * NO validamos listaIdsOrigen.Count.
             *
             * Foto y huella pueden llegar únicamente mediante CLAVE_PERSO.
             */
            if (listaFuentes.Count == 0)
            {
                return Tuple.Create(0, 0, 0, 0, "");
            }

            var alertaSeleccionada = alertasTipo
                .Where(x =>
                    listaFuentes.Contains(x.Item2) &&
                    (
                        x.Item2 == 6
                            ?
                            (
                                /*
                                 * TIPO 1
                                 * Item1 = Nom_perso.id
                                 */
                                (
                                    x.Item5 == 1 &&
                                    listaIdsOrigen.Contains(x.Item1)
                                )
                                ||
                                /*
                                 * TIPOS 2 Y 3
                                 * Item1 = CLAVE_PERSO
                                 */
                                (
                                    (x.Item5 == 2 || x.Item5 == 3) &&
                                    clavePerso > 0 &&
                                    x.Item1 == clavePerso
                                )
                            )
                            :
                            listaIdsOrigen.Contains(x.Item1)
                    )
                )
                .OrderByDescending(x =>
                    x.Item3 == 3
                        ? 4
                        : x.Item3 == 2
                            ? 3
                            : x.Item3 == 1
                                ? 2
                                : 1
                )
                .ThenByDescending(x => x.Item4)
                .FirstOrDefault();

            if (alertaSeleccionada == null)
            {
                return Tuple.Create(0, 0, 0, 0, "");
            }

            string nombreTipoAlerta = string.IsNullOrWhiteSpace(alertaSeleccionada.Item6)
                ? "SIN TIPO DE ALERTA"
                : alertaSeleccionada.Item6.Trim();

            return Tuple.Create(
                alertaSeleccionada.Item2,
                alertaSeleccionada.Item3,
                alertaSeleccionada.Item4,
                alertaSeleccionada.Item5,
                nombreTipoAlerta
            );
        }

        private System.Collections.ArrayList ObtenerCoincidenciasAlerta(List<Tuple<int, int, int, int, int, string>> alertasTipo, IEnumerable<int> idsOrigen, IEnumerable<int> idsFuentes)
        {
            var resultado = new System.Collections.ArrayList();

            if (alertasTipo == null)
            {
                return resultado;
            }
            var listaIdsOrigen =
                idsOrigen == null
                    ? new List<int>()
                    : idsOrigen
                        .Where(x => x > 0)
                        .Distinct()
                        .ToList();
            var listaIdsFuentes =
                idsFuentes == null
                    ? new List<int>()
                    : idsFuentes
                        .Where(x => x > 0)
                        .Distinct()
                        .ToList();

            if (listaIdsOrigen.Count == 0 || listaIdsFuentes.Count == 0)
            {
                return resultado;
            }

            // Conserva una coincidencia por origen, fuente y tipo; si se repite, usa el mayor porcentaje.
            var coincidencias =
                alertasTipo
                    .Where(x => listaIdsOrigen.Contains(x.Item1) && listaIdsFuentes.Contains(x.Item2))
                    .GroupBy(x => new
                    {
                        IdOrigen = x.Item1,
                        IdFuente = x.Item2,
                        IdTipoAlerta = x.Item5
                    })
                    .Select(grupo =>
                        grupo
                            .OrderByDescending(x => x.Item4)
                            .ThenByDescending(x => x.Item3 == 3 ? 4 : x.Item3 == 1 ? 3 : x.Item3 == 2 ? 2 : 1)
                            .First()
                    )
                    .OrderByDescending(x => x.Item4)
                    .ThenBy(x => x.Item5)
                    .ToList();

            foreach (var coincidencia in coincidencias)
            {
                string nombreTipoAlerta = string.IsNullOrWhiteSpace(coincidencia.Item6) ? "SIN TIPO DE ALERTA" : coincidencia.Item6.Trim();
                resultado.Add(
                    new object[]
                    {
                        coincidencia.Item2, // Posición 0: fuente
                        coincidencia.Item3, // Posición 1: estatus
                        coincidencia.Item4, // Posición 2: porcentaje
                        coincidencia.Item5, // Posición 3: ID tipo de alerta
                        nombreTipoAlerta    // Posición 4: nombre del tipo
                    }
                );
            }
            return resultado;
        }
        [HttpGet]
        public JsonResult ObtenerEstadoIdentidad(int idDetenido)
        {
            try
            {
                // El estado se recalcula desde base de datos y no desde los valores actuales de la vista.
                var alertasTipo = _filiacionService.GetAlertaTipo(new List<int> { idDetenido });
                var estadoIdentidad = CalcularEstadoIdentidad(alertasTipo);
                return Json(
                    new
                    {
                        success = true,
                        codigo = estadoIdentidad.Item1,
                        nombre = estadoIdentidad.Item2,
                        detalle = estadoIdentidad.Item3
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        success = false,
                        message = "No se pudo obtener el estado de identidad: " + ex.Message
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
        }

        private Tuple<int, string, string> CalcularEstadoIdentidad(List<Tuple<int, int, int, int, int, string>> alertasTipo)
        {
            if (alertasTipo == null)
            {
                alertasTipo = new List<Tuple<int, int, int, int, int, string>>();
            }
            // Prioridad 1: identidad confirmada en una fuente de interés (estatus 2 o 3, excepto fuente 6).
            var confirmadasDeInteres = alertasTipo
                .Where(x => (x.Item3 == 2 || x.Item3 == 3) && x.Item2 != 6)
                .ToList();

            if (confirmadasDeInteres.Count > 0)
            {
                string detalle = ConstruirDetalleEstadoIdentidad(confirmadasDeInteres);
                return Tuple.Create(2, "Identidad confirmada - De interés", detalle);
            }
            // Prioridad 2: identidad confirmada únicamente contra Detenidos FGEA (fuente 6).
            var confirmadasDetenidos = alertasTipo
                .Where(x => x.Item3 == 2 && x.Item2 == 6)
                .ToList();

            if (confirmadasDetenidos.Count > 0)
            {
                string detalle = ConstruirDetalleEstadoIdentidad(confirmadasDetenidos);
                return Tuple.Create(1, "Identidad confirmada", detalle);
            }
            // Prioridad 3: no existe una identidad confirmada.
            string detalleSinConfirmar = alertasTipo.Count == 0 ? "No existen alertas asociadas." : "Ningún registro de alerta ha confirmado la identidad.";
            return Tuple.Create(0, "Sin confirmar", detalleSinConfirmar);
        }

        private string ConstruirDetalleEstadoIdentidad(List<Tuple<int, int, int, int, int, string>> alertas)
        {
            if (alertas == null || alertas.Count == 0)
            {
                return "";
            }
            var nombresFuentes = alertas
                .Select(x => ObtenerNombreFuenteAlerta(x.Item2))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
            var nombresTiposAlerta = alertas
                .Select(x => x.Item6)
                .Where(x => !string.IsNullOrWhiteSpace(x) && !string.Equals(x, "SIN TIPO DE ALERTA", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Trim())
                .Distinct()
                .ToList();
            var partesDetalle = new List<string>();

            if (nombresFuentes.Count > 0)
            {
                partesDetalle.Add("Fuente: " + string.Join(", ", nombresFuentes));
            }

            if (nombresTiposAlerta.Count > 0)
            {
                partesDetalle.Add("Tipo de alerta: " + string.Join(", ", nombresTiposAlerta));
            }
            return partesDetalle.Count > 0
                ? string.Join(" | ", partesDetalle)
                : "";
        }

        private string FormatearPorcentaje(int porcentaje)
        {
            return porcentaje.ToString() + "%";
        }

        #endregion

        #region Fotografías, huellas y archivos

        private void AgregarFotoUrlADetenidos(System.Data.DataTable detenidos)
        {
            if (detenidos == null)
            {
                return;
            }

            if (!detenidos.Columns.Contains("FotoDetenidoUrl"))
            {
                detenidos.Columns.Add("FotoDetenidoUrl", typeof(string));
            }

            foreach (System.Data.DataRow row in detenidos.Rows)
            {
                int clavePerso = 0;

                if (row.Table.Columns.Contains("CLAVE_PERSO") && row["CLAVE_PERSO"] != DBNull.Value)
                {
                    int.TryParse(Convert.ToString(row["CLAVE_PERSO"]), out clavePerso);
                }

                if (clavePerso > 0)
                {
                    row["FotoDetenidoUrl"] = Url.Action("FotoDetenidoFiliacion", "SIC", new
                    {
                        clavePerso = clavePerso
                    });
                }
                else
                {
                    row["FotoDetenidoUrl"] = Url.Content("~/Content/imagenes/Nodisponible.jpg");
                }
            }
        }

        public ActionResult FotoDetenidoFiliacion(int clavePerso)
        {
            try
            {
                var _AsuntoService = new AsuntoService();
                string foto = _AsuntoService.getFotosDetenidosBase(clavePerso);

                if (string.IsNullOrWhiteSpace(foto))
                {
                    return Redirect(Url.Content("~/Content/imagenes/Nodisponible.jpg"));
                }
                foto = foto.Trim();

                if (foto.StartsWith("~/"))
                {
                    return Redirect(Url.Content(foto));
                }

                if (foto.StartsWith("/Content/", StringComparison.OrdinalIgnoreCase))
                {
                    return Redirect(Url.Content("~" + foto));
                }

                if (foto.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
                {
                    int indiceComa = foto.IndexOf(',');

                    if (indiceComa >= 0)
                    {
                        foto = foto.Substring(indiceComa + 1);
                    }
                }
                foto = foto
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Replace(" ", "");
                byte[] bytesFoto = Convert.FromBase64String(foto);
                return File(bytesFoto, "image/jpeg");
            }
            catch
            {
                return Redirect(Url.Content("~/Content/imagenes/Nodisponible.jpg"));
            }
        }

        public ActionResult FotoDetenidoC5(int idDetenido)
        {
            var detenido = _filiacionService.GetInfoDetenido(idDetenido);

            if (detenido == null)
            {
                return HttpNotFound();
            }
            string rutaFotoBD = "";
            var propiedadFoto = detenido.GetType().GetProperty("Foto");

            if (propiedadFoto != null)
            {
                var valorFoto = propiedadFoto.GetValue(detenido, null);
                rutaFotoBD = valorFoto == null ? "" : valorFoto.ToString();
            }

            if (string.IsNullOrWhiteSpace(rutaFotoBD))
            {
                return HttpNotFound();
            }
            rutaFotoBD = rutaFotoBD
                .Trim()
                .Trim('"')
                .Trim()
                .TrimStart('/', '\\')
                .Replace("/", "\\");
            string rutaCompleta = Path.Combine(RutaBaseFotosC5, rutaFotoBD);
            string extension = Path.GetExtension(rutaCompleta).ToLower();
            string contentType = ObtenerContentType(extension);

            try
            {
                byte[] fileBytes;
                // Se abre la conexión con credenciales
                using (new NetworkConnection(RutaBaseFotosC5, @"Fiscalia1", "A1b2c3d4"))
                {
                    if (!System.IO.File.Exists(rutaCompleta))
                    {
                        return HttpNotFound();
                    }
                    // Se lee el archivo mientras la conexión está activa
                    fileBytes = System.IO.File.ReadAllBytes(rutaCompleta);
                }
                return File(fileBytes, contentType);
            }
            catch (Exception)
            {
                return HttpNotFound("Error al acceder al servidor de imágenes C5.");
            }
        }

        public ActionResult FotoGaleriaC5(int idFoto)
        {
            var foto = _filiacionService.GetFotoC5PorId(idFoto);

            if (foto == null)
            {
                return HttpNotFound();
            }
            string rutaFotoBD = foto.FOTO;

            if (string.IsNullOrWhiteSpace(rutaFotoBD))
            {
                return HttpNotFound();
            }
            rutaFotoBD = rutaFotoBD
                .Trim()
                .Trim('"')
                .Trim()
                .TrimStart('/', '\\')
                .Replace("/", "\\");
            string rutaCompleta = Path.Combine(RutaBaseFotosC5, rutaFotoBD);
            string extension = Path.GetExtension(rutaCompleta).ToLower();
            string contentType = ObtenerContentType(extension);

            try
            {
                byte[] fileBytes;
                // Se abre la conexión con credenciales
                using (new NetworkConnection(RutaBaseFotosC5, @"Fiscalia1", "A1b2c3d4"))
                {
                    if (!System.IO.File.Exists(rutaCompleta))
                    {
                        return HttpNotFound();
                    }
                    // Se lee el archivo mientras la conexión está activa
                    fileBytes = System.IO.File.ReadAllBytes(rutaCompleta);
                }
                return File(fileBytes, contentType);
            }
            catch (Exception)
            {
                return HttpNotFound("Error al acceder al servidor de imágenes C5.");
            }
        }

        public ActionResult HuellaGaleriaC5(int idHuella)
        {
            var huella = _filiacionService.GetHuellaC5PorId(idHuella);

            if (huella == null)
            {
                return HttpNotFound();
            }
            string rutaHuellaBD = huella.Huellas;

            if (string.IsNullOrWhiteSpace(rutaHuellaBD))
            {
                return HttpNotFound();
            }
            rutaHuellaBD = rutaHuellaBD
                .Trim()
                .Trim('"')
                .Trim()
                .TrimStart('/', '\\')
                .Replace("/", "\\");
            string rutaCompleta = Path.Combine(RutaBaseFotosC5, rutaHuellaBD);
            string extension = Path.GetExtension(rutaCompleta).ToLower();
            string contentType = ObtenerContentType(extension);

            try
            {
                byte[] fileBytes;
                // Se abre la conexión con credenciales
                using (new NetworkConnection(RutaBaseFotosC5, @"Fiscalia1", "A1b2c3d4"))
                {
                    if (!System.IO.File.Exists(rutaCompleta))
                    {
                        return HttpNotFound();
                    }
                    // Se lee el archivo mientras la conexión está activa
                    fileBytes = System.IO.File.ReadAllBytes(rutaCompleta);
                }
                return File(fileBytes, contentType);
            }
            catch (Exception)
            {
                return HttpNotFound("Error al acceder al servidor de imágenes C5.");
            }
        }

        #endregion

        #region Acción básica de notificación

        [HttpPost]
        public JsonResult ApagarNotificacion(int idDetenido, int idOrigen, int idFuente, int idTipoAlerta)
        {
            try
            {
                if (idTipoAlerta <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se recibió el tipo de alerta."
                    });
                }

                int totalApagadas = _filiacionService.ApagarNotificacion(
                    idDetenido,
                    idOrigen,
                    idFuente,
                    idTipoAlerta
                );

                return Json(new
                {
                    success = true,
                    message = totalApagadas > 0
                        ? "Notificación apagada correctamente."
                        : "No se encontró una notificación activa para apagar.",
                    totalApagadas = totalApagadas
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Ocurrió un error al apagar la notificación: " + ex.Message
                });
            }
        }

        private string ObtenerContentType(string extension)
        {
            switch (extension)
            {
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                case ".bmp": return "image/bmp";
                case ".webp": return "image/webp";
                default: return "image/jpeg";
            }
        }

        #endregion

        #region Fotografía CAPEA

        public ActionResult FotoCapea(int idCapea)
        {
            var capea = _filiacionService.GetCapeaPorId(idCapea);

            if (capea == null)
            {
                return HttpNotFound();
            }
            string rutaFotoBD = capea.url_imagen;

            if (string.IsNullOrWhiteSpace(rutaFotoBD))
            {
                return HttpNotFound();
            }
            rutaFotoBD = rutaFotoBD
                .Trim()
                .Trim('"')
                .Trim('\'')
                .Trim()
                .TrimStart('/', '\\')
                .Replace("\\", "/");

            //rutaFotoBD = rutaFotoBD
            //   .Trim()
            //   .Trim('"')
            //   .Trim()
            //   .TrimStart('/', '\\')
            //   .Replace("/", "\\");
            string rutaCompleta = Path.Combine(RutaBaseFotosCapea, rutaFotoBD);

            //if (!System.IO.File.Exists(rutaCompleta))
            //{
            //    return HttpNotFound();
            //}

            //string extension = Path.GetExtension(rutaCompleta).ToLower();
            //string contentType = "image/jpeg";

            //if (extension == ".png")
            //{
            //    contentType = "image/png";
            //}
            //else if (extension == ".gif")
            //{
            //    contentType = "image/gif";
            //}
            //else if (extension == ".bmp")
            //{
            //    contentType = "image/bmp";
            //}
            //else if (extension == ".webp")
            //{
            //    contentType = "image/webp";
            //}
            return Redirect(rutaCompleta);
        }

        public ContentResult DiagnosticoFotoCapea(int idCapea)
        {
            try
            {
                var capea = _filiacionService.GetCapeaPorId(idCapea);

                if (capea == null)
                {
                    return Content("No se encontró CAPEA con id: " + idCapea);
                }
                string rutaFotoBD = capea.url_imagen;

                if (string.IsNullOrWhiteSpace(rutaFotoBD))
                {
                    return Content("El campo url_imagen viene vacío.");
                }
                string rutaLimpia = rutaFotoBD
                    .Trim()
                    .Trim('"')
                    .Trim()
                    .TrimStart('/', '\\')
                    .Replace("/", "\\");
                string rutaCompleta = Path.Combine(RutaBaseFotosCapea, rutaLimpia);
                bool existe = System.IO.File.Exists(rutaCompleta);
                string texto =
                    "ID CAPEA: " + idCapea + "\n" +
                    "url_imagen BD: " + rutaFotoBD + "\n" +
                    "Ruta limpia: " + rutaLimpia + "\n" +
                    "Ruta base: " + RutaBaseFotosCapea + "\n" +
                    "Ruta completa: " + rutaCompleta + "\n" +
                    "File.Exists: " + existe;
                return Content(texto, "text/plain");
            }
            catch (Exception ex)
            {
                return Content("ERROR:\n" + ex.ToString(), "text/plain");
            }
        }

        #endregion

        #region Preparación de la vista y estado general

        private void PrepararViewBagDetalleDetenido(
    List<tb_DETENCION_C5> detencionesC5,
    List<Tuple<int, int, int, int, int, string>> alertasTipo,
    List<Tuple<int, int>> tiposAlertas)
        {
            ViewBag.Texto = new Func<object, string>(Texto);
            ViewBag.ParteNombre = new Func<object, string>(ParteNombre);
            ViewBag.Fecha = new Func<object, string>(Fecha);
            ViewBag.TextoDetencion = new Func<object, string>(TextoDetencion);
            ViewBag.FechaHoraDetencion = new Func<object, string>(FechaHoraDetencion);

            ViewBag.ObtenerEstatusAlerta =
                new Func<int, int, int>((idOrigen, idFuente) =>
                {
                    return ObtenerEstatusAlerta(
                        alertasTipo,
                        idOrigen,
                        idFuente
                    );
                });

            ViewBag.ObtenerDatosAlerta =
                new Func<int, int[],
                    Tuple<int, int, int, int, string>>(
                    (idOrigen, idsFuentes) =>
                    {
                        return ObtenerDatosAlerta(
                            alertasTipo,
                            new int[] { idOrigen },
                            idsFuentes
                        );
                    });

            ViewBag.ObtenerCoincidenciasAlerta =
                new Func<int[], int[],
                    System.Collections.ArrayList>(
                    (idsOrigen, idsFuentes) =>
                    {
                        return ObtenerCoincidenciasAlerta(
                            alertasTipo,
                            idsOrigen,
                            idsFuentes
                        );
                    });

            ViewBag.ObtenerCoincidenciasAlertaDetenido =
                new Func<int[], int,
                    System.Collections.ArrayList>(
                    (idsNomPerso, clavePerso) =>
                    {
                        return ObtenerCoincidenciasAlertaDetenido(
                            alertasTipo,
                            idsNomPerso,
                            clavePerso
                        );
                    });

            /*
             * Primera detención = la más reciente.
             * Se utiliza inicialmente para el mapa.
             */
            var detencionSeleccionada =
                detencionesC5 != null
                    ? detencionesC5.FirstOrDefault()
                    : null;

            string mapaLatitud =
                detencionSeleccionada != null
                    ? TextoDetencion(
                        detencionSeleccionada.Latitud
                    )
                    : "SIN INFORMACIÓN";

            string mapaLongitud =
                detencionSeleccionada != null
                    ? TextoDetencion(
                        detencionSeleccionada.Longitud
                    )
                    : "SIN INFORMACIÓN";

            ViewBag.MapaLatitud = mapaLatitud;
            ViewBag.MapaLongitud = mapaLongitud;

            ViewBag.TieneCoordenadasMapa =
                mapaLatitud != "SIN INFORMACIÓN" &&
                mapaLongitud != "SIN INFORMACIÓN";

            ViewBag.TotalCoincidenciaNombre =
                TotalPorTipoAlerta(tiposAlertas, 1);

            ViewBag.TotalCoincidenciaFoto =
                TotalPorTipoAlerta(tiposAlertas, 2);

            ViewBag.TotalCoincidenciaHuella =
                TotalPorTipoAlerta(tiposAlertas, 3);

            PrepararEstadoGeneralIdentidad(alertasTipo);
        }

        private void PrepararEstadoGeneralIdentidad(List<Tuple<int, int, int, int, int, string>> alertasTipo)
        {
            if (alertasTipo == null)
            {
                alertasTipo = new List<Tuple<int, int, int, int, int, string>>();
            }
            // Prioridad 1: identidad confirmada en una fuente de interés.
            var alertasConfirmadasInteres = alertasTipo
                .Where(x => (x.Item3 == 2 || x.Item3 == 3) && x.Item2 != 6)
                .ToList();

            if (alertasConfirmadasInteres.Count > 0)
            {
                var alertaPrincipal = alertasConfirmadasInteres
                    .OrderByDescending(x => x.Item4)
                    .First();
                var nombresFuentes = alertasConfirmadasInteres
                    .Select(x => ObtenerNombreFuenteAlerta(x.Item2))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();
                var nombresTipos = alertasConfirmadasInteres
                    .Select(x => x.Item6)
                    .Where(x => !string.IsNullOrWhiteSpace(x) && x != "SIN TIPO DE ALERTA")
                    .Distinct()
                    .ToList();
                ViewBag.EstadoIdentidadCodigo = 2;
                ViewBag.EstadoIdentidadClave = "IDENTIDAD_CONFIRMADA_INTERES";
                ViewBag.EstadoIdentidadNombre = "Identidad confirmada - De interés";
                ViewBag.EstadoIdentidadDescripcion = "La identidad fue confirmada en una fuente de interés.";
                ViewBag.EstadoIdentidadFuentes = nombresFuentes.Count > 0 ? string.Join(", ", nombresFuentes) : ObtenerNombreFuenteAlerta(alertaPrincipal.Item2);
                ViewBag.EstadoIdentidadTipos = nombresTipos.Count > 0 ? string.Join(", ", nombresTipos) : alertaPrincipal.Item6;
                ViewBag.EstadoIdentidadIdFuente = alertaPrincipal.Item2;
                ViewBag.EstadoIdentidadPorcentaje = alertaPrincipal.Item4;
                return;
            }
            // Prioridad 2: identidad confirmada únicamente en Detenidos FGEA.
            var alertasConfirmadasDetenidos = alertasTipo
                .Where(x => x.Item3 == 2 && x.Item2 == 6)
                .ToList();

            if (alertasConfirmadasDetenidos.Count > 0)
            {
                var alertaPrincipal = alertasConfirmadasDetenidos
                    .OrderByDescending(x => x.Item4)
                    .First();
                var nombresTipos = alertasConfirmadasDetenidos
                    .Select(x => x.Item6)
                    .Where(x => !string.IsNullOrWhiteSpace(x) && x != "SIN TIPO DE ALERTA")
                    .Distinct()
                    .ToList();
                ViewBag.EstadoIdentidadCodigo = 1;
                ViewBag.EstadoIdentidadClave = "IDENTIDAD_CONFIRMADA";
                ViewBag.EstadoIdentidadNombre = "Identidad confirmada";
                ViewBag.EstadoIdentidadDescripcion = "La identidad fue confirmada únicamente contra registros de detenidos.";
                ViewBag.EstadoIdentidadFuentes = "Detenidos FGEA";
                ViewBag.EstadoIdentidadTipos = nombresTipos.Count > 0 ? string.Join(", ", nombresTipos) : alertaPrincipal.Item6;
                ViewBag.EstadoIdentidadIdFuente = alertaPrincipal.Item2;
                ViewBag.EstadoIdentidadPorcentaje = alertaPrincipal.Item4;
                return;
            }
            // Prioridad 3: ninguna coincidencia ha confirmado la identidad.
            ViewBag.EstadoIdentidadCodigo = 0;
            ViewBag.EstadoIdentidadClave = "SIN_CONFIRMAR";
            ViewBag.EstadoIdentidadNombre = "Sin confirmar";
            ViewBag.EstadoIdentidadDescripcion = alertasTipo.Count == 0 ? "No existen coincidencias registradas." : "Ninguna de las coincidencias ha confirmado la identidad.";
            ViewBag.EstadoIdentidadFuentes = "";
            ViewBag.EstadoIdentidadTipos = "";
            ViewBag.EstadoIdentidadIdFuente = 0;
            ViewBag.EstadoIdentidadPorcentaje = 0;
        }

        private string ObtenerNombreFuenteAlerta(int idFuente)
        {
            switch (idFuente)
            {
                case 2:
                    return "FEMDLP / CAPEA";
                case 3:
                    return "Personas de interés";
                case 4:
                    return "Mandamientos judiciales";
                case 5:
                    return "Objetivos prioritarios";
                case 6:
                    return "Detenidos FGEA";
                case 7:
                    return "FEMDLP / CAPEA";
                case 8:
                    return "FEMDLP / CAPEA";
                default:
                    return "Fuente " + idFuente;
            }
        }

        #endregion

        #region Utilidades de formato y conversión

        private string Texto(object valor)
        {
            if (valor == null)
                return "SIN INFORMACIÓN";
            string texto = Convert.ToString(valor);
            return string.IsNullOrWhiteSpace(texto)
                ? "SIN INFORMACIÓN"
                : texto;
        }

        private string ParteNombre(object valor)
        {
            if (valor == null)
                return "";
            string texto = Convert.ToString(valor);
            return string.IsNullOrWhiteSpace(texto)
                ? ""
                : texto.Trim();
        }

        private string Fecha(object valor)
        {
            if (valor == null)
                return "SIN INFORMACIÓN";
            DateTime fecha;

            if (DateTime.TryParse(Convert.ToString(valor), out fecha))
                return fecha.ToString("dd/MM/yyyy");
            return "SIN INFORMACIÓN";
        }

        private string TextoDetencion(object valor)
        {
            if (valor == null)
                return "SIN INFORMACIÓN";
            string texto = Convert.ToString(valor);
            return string.IsNullOrWhiteSpace(texto)
                ? "SIN INFORMACIÓN"
                : texto.Trim();
        }

        private string FechaHoraDetencion(object valor)
        {
            if (valor == null)
                return "SIN INFORMACIÓN";
            DateTime fecha;

            if (DateTime.TryParse(Convert.ToString(valor), out fecha))
                return fecha.ToString("dd/MM/yyyy HH:mm");
            return "SIN INFORMACIÓN";
        }

        private int ObtenerEstatusAlerta(List<Tuple<int, int, int, int, int, string>> alertasTipo, int idOrigen, int idFuente)
        {
            var datosAlerta = ObtenerDatosAlerta(alertasTipo, new int[] { idOrigen }, new int[] { idFuente });
            return datosAlerta.Item2;
        }

        private int TotalPorTipoAlerta(List<Tuple<int, int>> tiposAlertas, int idTipoAlerta)
        {
            if (tiposAlertas == null)
                return 0;
            var tipo = tiposAlertas
                .FirstOrDefault(x => x.Item1 == idTipoAlerta);

            if (tipo == null)
                return 0;
            return tipo.Item2;
        }

        private List<int> ConvertirTextoAListaEnteros(string idsTexto)
        {
            if (string.IsNullOrWhiteSpace(idsTexto))
            {
                return new List<int>();
            }
            return idsTexto
                .Split(',')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x =>
                {
                    int id = 0;
                    int.TryParse(x, out id);
                    return id;
                })
                .Where(x => x > 0)
                .Distinct()
                .ToList();
        }

        #endregion

        #region Acciones de alertas y resguardo

        [HttpPost]
        public JsonResult ApagarNotificacionDetenidos(int idDetenido, int idOrigen, int idTipoAlerta)
        {
            try
            {
                if (idOrigen <= 0 ||
                    idTipoAlerta <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se recibieron identificadores válidos."
                    });
                }

                int totalActualizadas = _filiacionService.ApagarNotificacionDetenidos(
                    idDetenido,
                    idOrigen,
                    idTipoAlerta
                );

                return Json(new
                {
                    success = true,
                    totalActualizadas = totalActualizadas,
                    message = "Notificación apagada correctamente."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al apagar la notificación de detenidos: " + ex.Message
                });
            }
        }

        [HttpPost]
        public JsonResult DescartarNotificacion(int idDetenido, int idOrigen, int idFuente, int idTipoAlerta)
        {
            try
            {
                if (idTipoAlerta <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se recibió el tipo de alerta."
                    });
                }

                int totalActualizadas = _filiacionService.ActualizarEstatusNotificacion(
                    idDetenido,
                    idOrigen,
                    idFuente,
                    idTipoAlerta,
                    0
                );

                return Json(new
                {
                    success = true,
                    estatus = 0,
                    totalActualizadas = totalActualizadas,
                    message = "Coincidencia descartada correctamente."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al descartar la coincidencia: " + ex.Message
                });
            }
        }

        [HttpPost]
        public JsonResult ConfirmarIdentidad(int idDetenido, int idOrigen, int idFuente, int idTipoAlerta)
        {
            try
            {
                if (idTipoAlerta <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se recibió el tipo de alerta."
                    });
                }

                int totalActualizadas = _filiacionService.ActualizarEstatusNotificacion(
                    idDetenido,
                    idOrigen,
                    idFuente,
                    idTipoAlerta,
                    2
                );

                return Json(new
                {
                    success = true,
                    estatus = 2,
                    totalActualizadas = totalActualizadas,
                    message = "Identidad confirmada correctamente."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al confirmar la identidad: " + ex.Message
                });
            }
        }

        [HttpPost]
        public JsonResult ReactivarNotificacion(int idDetenido, int idOrigen, int idFuente, int idTipoAlerta)
        {
            try
            {
                if (idTipoAlerta <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se recibió el tipo de alerta."
                    });
                }

                int totalActualizadas = _filiacionService.ActualizarEstatusNotificacion(
                    idDetenido,
                    idOrigen,
                    idFuente,
                    idTipoAlerta,
                    1
                );

                return Json(new
                {
                    success = true,
                    estatus = 1,
                    totalActualizadas = totalActualizadas,
                    message = "Coincidencia reabierta correctamente."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al reabrir la coincidencia: " + ex.Message
                });
            }
        }

        [HttpPost]
        public JsonResult DescartarNotificacionDetenidos(int idDetenido, int idOrigen, int idTipoAlerta)
        {
            try
            {
                if (idOrigen <= 0 ||
                    idTipoAlerta <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se recibieron identificadores válidos."
                    });
                }

                int totalActualizadas = _filiacionService.ActualizarEstatusNotificacionDetenidos(
                    idDetenido,
                    idOrigen,
                    idTipoAlerta,
                    0
                );

                return Json(new
                {
                    success = true,
                    estatus = 0,
                    totalActualizadas = totalActualizadas,
                    message = "Coincidencia descartada correctamente."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al descartar la coincidencia: " + ex.Message
                });
            }
        }

        [HttpPost]
        public JsonResult ConfirmarIdentidadDetenidos(int idDetenido, int idOrigen, int idTipoAlerta)
        {
            try
            {
                if (idOrigen <= 0 ||
                    idTipoAlerta <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se recibieron identificadores válidos."
                    });
                }

                int totalActualizadas = _filiacionService.ActualizarEstatusNotificacionDetenidos(
                    idDetenido,
                    idOrigen,
                    idTipoAlerta,
                    2
                );

                return Json(new
                {
                    success = true,
                    estatus = 2,
                    totalActualizadas = totalActualizadas,
                    message = "Identidad confirmada correctamente."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al confirmar la identidad: " + ex.Message
                });
            }
        }

        [HttpPost]
        public JsonResult ReactivarNotificacionDetenidos(int idDetenido, int idOrigen, int idTipoAlerta)
        {
            try
            {
                if (idOrigen <= 0 ||
                    idTipoAlerta <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se recibieron identificadores válidos."
                    });
                }

                int totalActualizadas = _filiacionService.ActualizarEstatusNotificacionDetenidos(
                    idDetenido,
                    idOrigen,
                    idTipoAlerta,
                    1
                );

                return Json(new
                {
                    success = true,
                    estatus = 1,
                    totalActualizadas = totalActualizadas,
                    message = "Coincidencia reabierta correctamente."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al reabrir la coincidencia: " + ex.Message
                });
            }
        }

        [HttpPost]
        public JsonResult ResguardoFGEA(int idDetenido, int idOrigen, int idFuente, int idTipoAlerta)
        {
            try
            {
                int totalActualizadas = _filiacionService.ActualizarEstatusNotificacion(
                    idDetenido,
                    idOrigen,
                    idFuente,
                    idTipoAlerta,
                    3
                );

                return Json(new
                {
                    success = totalActualizadas > 0,
                    estatus = 3,
                    totalActualizadas = totalActualizadas,
                    message = totalActualizadas > 0
                        ? "Registro puesto en Resguardo FGEA correctamente."
                        : "No se encontró la coincidencia para actualizar."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al poner el registro en Resguardo FGEA: " + ex.Message
                });
            }
        }

        [HttpPost]
        public JsonResult CancelarResguardoFGEA(int idDetenido, int idOrigen, int idFuente, int idTipoAlerta)
        {
            try
            {
                int totalActualizadas = _filiacionService.ActualizarEstatusNotificacion(
                    idDetenido,
                    idOrigen,
                    idFuente,
                    idTipoAlerta,
                    2
                );

                return Json(new
                {
                    success = totalActualizadas > 0,
                    estatus = 2,
                    totalActualizadas = totalActualizadas,
                    message = totalActualizadas > 0
                        ? "Resguardo FGEA cancelado correctamente."
                        : "No se encontró la coincidencia para actualizar."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al cancelar el Resguardo FGEA: " + ex.Message
                });
            }
        }

        #endregion

        #region Código legado comentado

        //private Dictionary< int, Tuple<int, int, int, int, string> > ConstruirDatosAlertaPorOrigen(  List<Tuple<int, int, int, int, int, string>> alertasTipo, IEnumerable<int> idsFuentes )
        //{
        //    var resultado =
        //        new Dictionary<
        //            int,
        //            Tuple<int, int, int, int, string>
        //        >();

        //    if (alertasTipo == null || alertasTipo.Count == 0)
        //    {
        //        return resultado;
        //    }

        //    var fuentesPermitidas = idsFuentes == null
        //        ? new List<int>()
        //        : idsFuentes
        //            .Distinct()
        //            .ToList();

        //    var grupos = alertasTipo
        //        .Where(x =>
        //            fuentesPermitidas.Contains(x.Item2)
        //        )
        //        .GroupBy(x => x.Item1)
        //        .ToList();

        //    foreach (var grupo in grupos)
        //    {
        //        /*
        //         * Item1 = idPersonaFGEA / idOrigen
        //         * Item2 = IdTbFuente
        //         * Item3 = Estatus
        //         * Item4 = Porcentaje
        //         * Item5 = IdTipoAlerta
        //         * Item6 = NombreTipoAlerta
        //         */

        //        var alertaSeleccionada = grupo
        //            .OrderByDescending(x =>
        //                x.Item3 == 1
        //                    ? 3
        //                    : x.Item3 == 2
        //                        ? 2
        //                        : 1
        //            )
        //            .ThenByDescending(x => x.Item4)
        //            .FirstOrDefault();

        //        if (alertaSeleccionada == null)
        //        {
        //            continue;
        //        }

        //        int idOrigen =
        //            alertaSeleccionada.Item1;

        //        int idFuente =
        //            alertaSeleccionada.Item2;

        //        int estatus =
        //            alertaSeleccionada.Item3;

        //        int porcentaje =
        //            alertaSeleccionada.Item4;

        //        int idTipoAlerta =
        //            alertaSeleccionada.Item5;

        //        string nombreTipoAlerta =
        //            string.IsNullOrWhiteSpace(
        //                alertaSeleccionada.Item6
        //            )
        //                ? "SIN TIPO DE ALERTA"
        //                : alertaSeleccionada.Item6.Trim();

        //        resultado[idOrigen] = Tuple.Create(
        //            idFuente,
        //            estatus,
        //            porcentaje,
        //            idTipoAlerta,
        //            nombreTipoAlerta
        //        );
        //    }

        //    return resultado;
        //}

        #endregion

        #region Modal de detenidos relacionados
        [HttpGet]
        public ActionResult ObtenerDetenidosObjetivoModal(int idNomPerso)
        {
            try
            {
                if (idNomPerso <= 0)
                {
                    return Content(
                        "<div class=\"sic-modal-sin-detenido\">" +
                            "<i class=\"fa fa-exclamation-circle\"></i>" +
                            "<strong>Sin información disponible</strong>" +
                            "<span>" +
                                "El nombre seleccionado no tiene un ID válido." +
                            "</span>" +
                        "</div>",
                        "text/html"
                    );
                }

                List<int> listaIdsNomPerso = new List<int>
        {
            idNomPerso
        };

                List<int> listaClavesPerso = new List<int>();

                System.Data.DataTable detenidos = _filiacionService.GetInfoDetenidos(
                    listaIdsNomPerso,
                    listaClavesPerso
                );

                if (detenidos == null || detenidos.Rows.Count == 0)
                {
                    return Content(
                        "<div class=\"sic-modal-sin-detenido\">" +
                            "<i class=\"fa fa-exclamation-circle\"></i>" +
                            "<strong>Sin ficha disponible</strong>" +
                            "<span>" +
                                "El nombre seleccionado no tiene información " +
                                "completa en la base de detenidos." +
                            "</span>" +
                        "</div>",
                        "text/html"
                    );
                }

                AgregarFotoUrlADetenidos(detenidos);

                ViewBag.Texto = new Func<object, string>(Texto);
                ViewBag.Fecha = new Func<object, string>(Fecha);

                ViewData["EsModal"] = true;
                ViewData["MostrarAcciones"] = false;
                ViewData["IdDetenidoC5"] = 0;

                return PartialView(
                    "DetenidosObjetivoModal",
                    detenidos.Rows[0]
                );
            }
            catch (Exception ex)
            {
                return Content(
                    "<div class=\"sic-modal-sin-detenido\">" +
                        "<i class=\"fa fa-times-circle\"></i>" +
                        "<strong>No se pudo consultar la información</strong>" +
                        "<span>" +
                            Server.HtmlEncode(ex.Message) +
                        "</span>" +
                    "</div>",
                    "text/html"
                );
            }
        }

        #endregion

        #region Búsqueda intencionada de coincidencias biométricas

        [HttpGet]
        public ActionResult BusquedaCoincidencias()
        {
            ViewBag.Title = "Búsqueda Intencionada";

            // Al entrar se eliminan resultados de una búsqueda anterior.
            Session.Remove(SessionResultadosCoincidencias);
            BusquedaCoincidenciasViewModel modelo = CoincidenciasBiometricasService.CrearModeloInicial();
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BuscarCoincidencias(BusquedaCoincidenciasViewModel modelo)
        {
            try
            {
                ResultadosCoincidenciasViewModel resultado = await CoincidenciasBiometricasService.BuscarCoincidenciasAsync(modelo);

                // Conserva los resultados reales para que Ver detalle use la misma búsqueda.
                Session[SessionResultadosCoincidencias] = resultado;
                return PartialView(
                    "Coincidencias/ResultadosCoincidenciasPartial",
                    resultado
                );
            }
            catch (ArgumentException ex)
            {
                Response.StatusCode = 400;
                return Content(
                    ex.Message,
                    "text/plain"
                );
            }
            catch (InvalidOperationException ex)
            {
                // Errores devueltos por la API o por su configuración.
                Response.StatusCode = 502;
                return Content(
                    ex.Message,
                    "text/plain"
                );
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                Response.StatusCode = 503;
                return Content(
                    "No fue posible comunicarse con la API biométrica. " +
                    ex.Message,
                    "text/plain"
                );
            }
            catch (TaskCanceledException)
            {
                Response.StatusCode = 504;
                return Content(
                    "La API biométrica superó el tiempo máximo de espera.",
                    "text/plain"
                );
            }
            catch (Exception)
            {
                Response.StatusCode = 500;
                return Content(
                    "Ocurrió un error al buscar las coincidencias biométricas.",
                    "text/plain"
                );
            }
        }

        [HttpGet]
        public ActionResult DetalleCoincidenciaPartial(int idCoincidencia, bool tieneFotografiaConsulta = false, bool tieneHuellaConsulta = false)
        {
            ResultadosCoincidenciasViewModel resultados = Session[SessionResultadosCoincidencias] as ResultadosCoincidenciasViewModel;

            if (resultados == null)
            {
                Response.StatusCode = 409;
                return Content(
                    "La búsqueda ya no está disponible. Realice nuevamente la consulta biométrica.",
                    "text/plain"
                );
            }
            CoincidenciaResultadoViewModel coincidencia =
                resultados.Coincidencias == null
                    ? null
                    : resultados.Coincidencias
                        .FirstOrDefault(x => x.IdCoincidencia == idCoincidencia);

            if (coincidencia == null)
            {
                return HttpNotFound("No se encontró la coincidencia solicitada.");
            }

            // Usa los indicadores de la búsqueda real; los parámetros se conservan por compatibilidad con el JavaScript.
            DetalleCoincidenciaViewModel modelo =
                new DetalleCoincidenciaViewModel
                {
                    Coincidencia = coincidencia,
                    TieneFotografiaConsulta = resultados.TieneFotografiaConsulta,
                    TieneHuellaConsulta = resultados.TieneHuellaConsulta
                };
            return PartialView(
                "Coincidencias/DetalleCoincidenciaPartial",
                modelo
            );
        }

        #endregion

        #region Ordenamiento de resultados

        private System.Data.DataTable OrdenarPorPorcentaje(System.Data.DataTable tabla)
        {
            if (tabla == null ||
                !tabla.Columns.Contains("PorcentajeCoincidencia") ||
                tabla.Rows.Count == 0)
            {
                return tabla;
            }

            var filasOrdenadas = tabla
                .AsEnumerable()
                .OrderByDescending(row =>
                {
                    if (!tabla.Columns.Contains("EstatusAlerta") ||
                        row["EstatusAlerta"] == DBNull.Value)
                    {
                        return 0;
                    }

                    int estatus = 0;

                    int.TryParse(
                        Convert.ToString(row["EstatusAlerta"]),
                        out estatus
                    );

                    switch (estatus)
                    {
                        case 3:
                            return 4;

                        case 2:
                            return 3;

                        case 1:
                            return 2;

                        case 0:
                            return 1;

                        default:
                            return 0;
                    }
                })
                .ThenByDescending(row =>
                {
                    if (row["PorcentajeCoincidencia"] == DBNull.Value)
                    {
                        return 0;
                    }

                    int porcentaje = 0;

                    int.TryParse(
                        Convert.ToString(row["PorcentajeCoincidencia"]),
                        out porcentaje
                    );

                    return porcentaje;
                })
                .ToList();

            var tablaOrdenada = tabla.Clone();

            foreach (var fila in filasOrdenadas)
            {
                tablaOrdenada.ImportRow(fila);
            }

            return tablaOrdenada;
        }

        #endregion

    }
}