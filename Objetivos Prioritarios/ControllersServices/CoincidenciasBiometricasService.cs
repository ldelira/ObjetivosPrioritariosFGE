using Newtonsoft.Json;
using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Models.Extends;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.ControllersServices
{
    public class CoincidenciasBiometricasService:BaseService
    {
        private const string ClaveSesionCoincidencias =
            "SIC_COINCIDENCIAS_BIOMETRICAS";


        private static readonly HttpClient ClienteHttp =
    new HttpClient
    {
        Timeout =
            TimeSpan.FromMinutes(5)
    };
        private readonly FiliacionMunicipalService
    _filiacionMunicipalService;

        private readonly string _urlApi;
        private readonly string _tokenApi;

        public CoincidenciasBiometricasService()
        {
            _urlApi =
                ConfigurationManager
                    .AppSettings["BiometriaApiUrl"];

            _tokenApi =
                ConfigurationManager
                    .AppSettings["BiometriaApiToken"];
            _filiacionMunicipalService =
        new FiliacionMunicipalService();
        }


        public BusquedaCoincidenciasViewModel CrearModeloInicial()
        {
            BusquedaCoincidenciasViewModel modelo =
                new BusquedaCoincidenciasViewModel
                {
                    NombreBusqueda =
                        string.Empty,

                    AliasBusqueda =
                        string.Empty,

                    TextoBusqueda =
                        string.Empty,

                    ModoBusquedaTexto =
                        "NOMBRE",

                    ModoCombinacion =
                        "PRIORIZAR",

                    Municipio =
                        string.Empty,

                    Sexo =
                        string.Empty,

                    EdadMinima =
                        null,

                    EdadMaxima =
                        null,

                    TipoCoincidencia =
                        string.Empty,

                    PorcentajeMinimo =
                        50,

                    Municipios =
                        ObtenerMunicipios(),

                    Sexos =
                        ObtenerSexos(),

                    TiposCoincidencia =
                        ObtenerTiposCoincidencia()
                };

            return modelo;
        }
        private List<SelectListItem> ObtenerMunicipios()
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "",
                    Text = "Todos los municipios"
                },
                new SelectListItem
                {
                    Value = "AGS",
                    Text = "Aguascalientes"
                },
                new SelectListItem
                {
                    Value = "ASI",
                    Text = "Asientos"
                },
                new SelectListItem
                {
                    Value = "CAL",
                    Text = "Calvillo"
                },
                new SelectListItem
                {
                    Value = "COS",
                    Text = "Cosío"
                },
                new SelectListItem
                {
                    Value = "JMA",
                    Text = "Jesús María"
                },
                new SelectListItem
                {
                    Value = "PAB",
                    Text = "Pabellón de Arteaga"
                },
                new SelectListItem
                {
                    Value = "RIN",
                    Text = "Rincón de Romos"
                },
                new SelectListItem
                {
                    Value = "SJG",
                    Text = "San José de Gracia"
                },
                new SelectListItem
                {
                    Value = "TEP",
                    Text = "Tepezalá"
                },
                new SelectListItem
                {
                    Value = "ELL",
                    Text = "El Llano"
                },
                new SelectListItem
                {
                    Value = "SFR",
                    Text = "San Francisco de los Romo"
                }
            };
        }

        private List<SelectListItem> ObtenerSexos()
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "",
                    Text = "Todos"
                },
                new SelectListItem
                {
                    Value = "HOMBRE",
                    Text = "Hombre"
                },
                new SelectListItem
                {
                    Value = "MUJER",
                    Text = "Mujer"
                }
            };
        }

        private List<SelectListItem> ObtenerTiposCoincidencia()
        {
            return new List<SelectListItem>
    {
        new SelectListItem
        {
            Value = "",
            Text = "Todos los resultados"
        },

        new SelectListItem
        {
            Value = "TEXTO",
            Text = "Coincidencia por nombre o alias"
        },

        new SelectListItem
        {
            Value = "FOTO",
            Text = "Coincidencia por fotografía"
        },

        new SelectListItem
        {
            Value = "HUELLA",
            Text = "Coincidencia por huella"
        },

        new SelectListItem
        {
            Value = "COMBINADA",
            Text = "Coincidencia por foto y huella"
        },

        new SelectListItem
        {
            Value = "TEXTO_BIOMETRIA",
            Text = "Coincidencia textual y biométrica"
        }
    };
        }



        public async Task<ResultadosCoincidenciasViewModel> BuscarCoincidenciasAsync(BusquedaCoincidenciasViewModel filtros)
        {
            if (filtros == null)
            {
                throw new ArgumentNullException(nameof(filtros));
            }

            ValidarArchivo(
     filtros.Fotografia,
     "fotografía"
 );


            /*
             * =========================================================
             * HUELLAS DE CONSULTA
             * =========================================================
             *
             * Nuevo formato:
             * filtros.Huellas
             *
             * Compatibilidad:
             * filtros.Huella
             * =========================================================
             */

            List<HttpPostedFileBase> huellasConsulta =
                new List<HttpPostedFileBase>();


            if (filtros.Huellas != null)
            {
                huellasConsulta.AddRange(
                    filtros.Huellas
                        .Where(x =>
                            x != null &&
                            x.ContentLength > 0
                        )
                );
            }


            /*
             * Por compatibilidad con el formato anterior.
             */
            if (
                filtros.Huella != null &&
                filtros.Huella.ContentLength > 0
            )
            {
                huellasConsulta.Add(
                    filtros.Huella
                );
            }


            /*
             * Máximo 10 huellas.
             */
            if (huellasConsulta.Count > 10)
            {
                throw new ArgumentException(
                    "Puede cargar como máximo 10 huellas por búsqueda."
                );
            }


            /*
             * Validar cada archivo individualmente.
             */
            for (
                int indiceHuella = 0;
                indiceHuella < huellasConsulta.Count;
                indiceHuella++
            )
            {
                ValidarArchivo(
                    huellasConsulta[indiceHuella],
                    "huella " +
                    (indiceHuella + 1)
                );
            }

            /* =========================================================
               NORMALIZAR MODO DE BÚSQUEDA
               ========================================================= */

            string modoBusquedaTexto =
                string.IsNullOrWhiteSpace(filtros.ModoBusquedaTexto)
                    ? "NOMBRE"
                    : filtros.ModoBusquedaTexto.Trim().ToUpperInvariant();

            if (
                modoBusquedaTexto != "NOMBRE" &&
                modoBusquedaTexto != "ALIAS" &&
                modoBusquedaTexto != "AMBOS"
            )
            {
                modoBusquedaTexto = "NOMBRE";
            }

            string nombreBusqueda =
                string.IsNullOrWhiteSpace(filtros.NombreBusqueda)
                    ? ""
                    : filtros.NombreBusqueda.Trim();

            string aliasBusqueda =
                string.IsNullOrWhiteSpace(filtros.AliasBusqueda)
                    ? ""
                    : filtros.AliasBusqueda.Trim();

            /*
             * Aunque por alguna razón el formulario conserve
             * información de otro campo, respetamos el modo seleccionado.
             */
            if (modoBusquedaTexto == "NOMBRE")
            {
                aliasBusqueda = "";
            }
            else if (modoBusquedaTexto == "ALIAS")
            {
                nombreBusqueda = "";
            }

            string modoCombinacion =
                string.Equals(
                    filtros.ModoCombinacion,
                    "ESTRICTO",
                    StringComparison.OrdinalIgnoreCase
                )
                    ? "ESTRICTO"
                    : "PRIORIZAR";

            int porcentajeMinimo =
                filtros.PorcentajeMinimo;

            if (
                porcentajeMinimo < 1 ||
                porcentajeMinimo > 100
            )
            {
                porcentajeMinimo = 50;
            }

            /* =========================================================
               DETERMINAR CRITERIOS CAPTURADOS
               ========================================================= */

            bool tieneNombre =
                !string.IsNullOrWhiteSpace(
                    nombreBusqueda
                );

            bool tieneAlias =
                !string.IsNullOrWhiteSpace(
                    aliasBusqueda
                );

            bool tieneFotografia =
    filtros.Fotografia != null &&
    filtros.Fotografia.ContentLength > 0;


            bool tieneHuella =
                huellasConsulta.Count > 0;

            /*
             * Ahora ya NO obligamos a tener biometría.
             *
             * Son válidas, por ejemplo:
             *
             * Nombre
             * Alias
             * Nombre + Alias
             * Foto
             * Huella
             * Nombre + Foto
             * Alias + Huella
             * Nombre + Alias + Foto
             * etc.
             */
            if (
                !tieneNombre &&
                !tieneAlias &&
                !tieneFotografia &&
                !tieneHuella
            )
            {
                throw new ArgumentException(
                    "Capture un nombre, un alias, una fotografía o una huella."
                );
            }

            ValidarConfiguracionApi();

            /* =========================================================
               CONSTRUIR PETICIÓN A LA API
               ========================================================= */

            using (var formulario = new MultipartFormDataContent())
            {
                /*
                 * Datos textuales.
                 *
                 * Antes el MVC no los estaba mandando a la API.
                 */

                formulario.Add(
                    new StringContent(
                        nombreBusqueda,
                        Encoding.UTF8
                    ),
                    "NombreBusqueda"
                );

                formulario.Add(
                    new StringContent(
                        aliasBusqueda,
                        Encoding.UTF8
                    ),
                    "AliasBusqueda"
                );

                formulario.Add(
                    new StringContent(
                        modoBusquedaTexto,
                        Encoding.UTF8
                    ),
                    "ModoBusquedaTexto"
                );

                formulario.Add(
                    new StringContent(
                        modoCombinacion,
                        Encoding.UTF8
                    ),
                    "ModoCombinacion"
                );

                formulario.Add(
                    new StringContent(
                        porcentajeMinimo.ToString(
                            CultureInfo.InvariantCulture
                        ),
                        Encoding.UTF8
                    ),
                    "PorcentajeMinimo"
                );

                /* =====================================================
                   FOTOGRAFÍA
                   ===================================================== */

                if (tieneFotografia)
                {
                    ByteArrayContent contenidoFoto =
                        CrearContenidoArchivo(
                            filtros.Fotografia
                        );

                    formulario.Add(
                        contenidoFoto,
                        "Fotografia",
                        ObtenerNombreArchivo(
                            filtros.Fotografia,
                            "fotografia.jpg"
                        )
                    );
                }

                /* =====================================================
                   HUELLAS
                   =====================================================
                 *
                 * Todas las imágenes se mandan con la misma clave:
                 *
                 * Huellas
                 *
                 * ASP.NET Core las enlaza en:
                 *
                 * List<IFormFile> Huellas
                 * ===================================================== */

                if (tieneHuella)
                {
                    for (
                        int indiceHuella = 0;
                        indiceHuella < huellasConsulta.Count;
                        indiceHuella++
                    )
                    {
                        HttpPostedFileBase archivoHuella =
                            huellasConsulta[
                                indiceHuella
                            ];


                        ByteArrayContent contenidoHuella =
                            CrearContenidoArchivo(
                                archivoHuella
                            );


                        formulario.Add(
                            contenidoHuella,
                            "Huellas",
                            ObtenerNombreArchivo(
                                archivoHuella,
                                "huella_" +
                                (indiceHuella + 1) +
                                ".jpg"
                            )
                        );
                    }
                }

                /* =====================================================
                   EJECUTAR PETICIÓN
                   ===================================================== */

                using (var solicitud = new HttpRequestMessage(HttpMethod.Post, _urlApi))
                {
                    solicitud.Headers.Add(
                        "X-API-TOKEN",
                        _tokenApi
                    );

                    solicitud.Content =
                        formulario;

                    HttpResponseMessage respuesta =
                        await ClienteHttp.SendAsync(
                            solicitud
                        );

                    string contenidoRespuesta =
                        await respuesta.Content.ReadAsStringAsync();


                    System.Diagnostics.Debug.WriteLine(
    "========================================"
);

                    System.Diagnostics.Debug.WriteLine(
                        "URL API MVC: " + _urlApi
                    );

                    System.Diagnostics.Debug.WriteLine(
                        "RESPUESTA API MVC:"
                    );

                    System.Diagnostics.Debug.WriteLine(
                        contenidoRespuesta
                    );

                    System.Diagnostics.Debug.WriteLine(
                        "========================================"
                    );

                    if (!respuesta.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(
                            ObtenerMensajeErrorApi(
                                contenidoRespuesta,
                                respuesta.StatusCode
                            )
                        );
                    }

                    ApiBusquedaBiometricaResponse resultadoApi =
                        JsonConvert.DeserializeObject<ApiBusquedaBiometricaResponse>(
                            contenidoRespuesta
                        );

                    if (resultadoApi == null)
                    {
                        throw new InvalidOperationException(
                            "La API biométrica regresó una respuesta vacía."
                        );
                    }

                    return await ConvertirResultadoApiAsync(
                        resultadoApi,
                        tieneFotografia,
                        tieneHuella
                    );
                }
            }
        }


        public CoincidenciaResultadoViewModel ObtenerDetalleCoincidencia(
    int idCoincidencia)
        {
            if (
                System.Web.HttpContext.Current == null ||
                System.Web.HttpContext.Current.Session == null
            )
            {
                return null;
            }

            List<CoincidenciaResultadoViewModel> coincidencias =
                System.Web.HttpContext.Current.Session[
                    ClaveSesionCoincidencias
                ]
                as List<CoincidenciaResultadoViewModel>;

            if (
                coincidencias == null ||
                coincidencias.Count == 0
            )
            {
                return null;
            }

            return coincidencias
                .FirstOrDefault(x =>
                    x.IdCoincidencia ==
                    idCoincidencia
                );
        }

        private void ValidarArchivo(
            HttpPostedFileBase archivo,
            string nombreArchivo)
        {
            if (archivo == null ||
                archivo.ContentLength <= 0)
            {
                return;
            }

            const int maximoBytes =
                10 * 1024 * 1024;

            if (archivo.ContentLength > maximoBytes)
            {
                throw new ArgumentException(
                    "El archivo de " +
                    nombreArchivo +
                    " supera el límite de 10 MB."
                );
            }

            string tipoContenido =
                string.IsNullOrWhiteSpace(
                    archivo.ContentType
                )
                    ? ""
                    : archivo.ContentType.ToLower();

            string[] tiposPermitidos =
            {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp"
    };

            if (!tiposPermitidos.Contains(tipoContenido))
            {
                throw new ArgumentException(
                    "El archivo de " +
                    nombreArchivo +
                    " no tiene un formato permitido."
                );
            }
        }

        private List<CoincidenciaResultadoViewModel>
            CrearCoincidenciasSimuladas()
        {
            return new List<CoincidenciaResultadoViewModel>
    {
        new CoincidenciaResultadoViewModel
        {
            IdCoincidencia = 1,
            NombreCompleto =
                "DIEGO ALEJANDRO LÓPEZ GÓMEZ",
            Alias = "EL DIEGO",
            Folio = "SIC-2026-015678",
            Expediente = "EXP-2026-54321",
            MunicipioClave = "AGS",
            Municipio = "AGUASCALIENTES",
            Edad = 25,
            Sexo = "HOMBRE",
            FotoUrl =
                "~/Content/imagenes/Nodisponible.jpg",
            TipoCoincidencia = "COMBINADA",
            PorcentajeNombre = 80,
            PorcentajeFoto = 94,
            PorcentajeHuella = 91,
            SimilitudGlobal = 92,
            FechaRegistro =
                new DateTime(2026, 3, 12)
        },

        new CoincidenciaResultadoViewModel
        {
            IdCoincidencia = 2,
            NombreCompleto =
                "MIGUEL ÁNGEL RAMÍREZ TORRES",
            Alias = "EL MICHO",
            Folio = "SIC-2025-009874",
            Expediente = "EXP-2025-18342",
            MunicipioClave = "JMA",
            Municipio = "JESÚS MARÍA",
            Edad = 27,
            Sexo = "HOMBRE",
            FotoUrl =
                "~/Content/imagenes/Nodisponible.jpg",
            TipoCoincidencia = "FOTO",
            PorcentajeNombre = 76,
            PorcentajeFoto = 89,
            PorcentajeHuella = 0,
            SimilitudGlobal = 85,
            FechaRegistro =
                new DateTime(2025, 11, 8)
        },

        new CoincidenciaResultadoViewModel
        {
            IdCoincidencia = 3,
            NombreCompleto =
                "JORGE LUIS HERRERA SÁNCHEZ",
            Alias = "EL JORGE",
            Folio = "SIC-2024-004532",
            Expediente = "EXP-2024-88315",
            MunicipioClave = "CAL",
            Municipio = "CALVILLO",
            Edad = 30,
            Sexo = "HOMBRE",
            FotoUrl =
                "~/Content/imagenes/Nodisponible.jpg",
            TipoCoincidencia = "HUELLA",
            PorcentajeNombre = 70,
            PorcentajeFoto = 0,
            PorcentajeHuella = 84,
            SimilitudGlobal = 78,
            FechaRegistro =
                new DateTime(2024, 6, 21)
        },

        new CoincidenciaResultadoViewModel
        {
            IdCoincidencia = 4,
            NombreCompleto =
                "CARLOS IVÁN MENDOZA RUIZ",
            Alias = "EL CHINO",
            Folio = "SIC-2023-012345",
            Expediente = "EXP-2023-72114",
            MunicipioClave = "PAB",
            Municipio = "PABELLÓN DE ARTEAGA",
            Edad = 33,
            Sexo = "HOMBRE",
            FotoUrl =
                "~/Content/imagenes/Nodisponible.jpg",
            TipoCoincidencia = "COMBINADA",
            PorcentajeNombre = 68,
            PorcentajeFoto = 74,
            PorcentajeHuella = 71,
            SimilitudGlobal = 72,
            FechaRegistro =
                new DateTime(2023, 9, 14)
        },

        new CoincidenciaResultadoViewModel
        {
            IdCoincidencia = 5,
            NombreCompleto =
                "LAURA ELENA MARTÍNEZ DÍAZ",
            Alias = "LA GÜERA",
            Folio = "SIC-2025-027544",
            Expediente = "EXP-2025-67152",
            MunicipioClave = "RIN",
            Municipio = "RINCÓN DE ROMOS",
            Edad = 29,
            Sexo = "MUJER",
            FotoUrl =
                "~/Content/imagenes/Nodisponible.jpg",
            TipoCoincidencia = "FOTO",
            PorcentajeNombre = 73,
            PorcentajeFoto = 82,
            PorcentajeHuella = 0,
            SimilitudGlobal = 79,
            FechaRegistro =
                new DateTime(2025, 7, 19)
        }
    };
        }


        private void ValidarConfiguracionApi()
        {
            if (string.IsNullOrWhiteSpace(_urlApi))
            {
                throw new InvalidOperationException(
                    "No se configuró BiometriaApiUrl en Web.config."
                );
            }

            if (string.IsNullOrWhiteSpace(_tokenApi))
            {
                throw new InvalidOperationException(
                    "No se configuró BiometriaApiToken en Web.config."
                );
            }
        }

        private ByteArrayContent CrearContenidoArchivo(
            HttpPostedFileBase archivo)
        {
            byte[] bytes =
                LeerArchivo(archivo);

            var contenido =
                new ByteArrayContent(bytes);

            string tipoContenido =
                string.IsNullOrWhiteSpace(
                    archivo.ContentType
                )
                    ? "application/octet-stream"
                    : archivo.ContentType;

            contenido.Headers.ContentType =
                MediaTypeHeaderValue.Parse(
                    tipoContenido
                );

            return contenido;
        }

        private byte[] LeerArchivo(
            HttpPostedFileBase archivo)
        {
            if (archivo == null)
            {
                return new byte[0];
            }

            if (archivo.InputStream.CanSeek)
            {
                archivo.InputStream.Position = 0;
            }

            using (var memoria =
                new MemoryStream())
            {
                archivo.InputStream.CopyTo(
                    memoria
                );

                return memoria.ToArray();
            }
        }

        private string ObtenerNombreArchivo(
            HttpPostedFileBase archivo,
            string nombrePredeterminado)
        {
            if (archivo == null ||
                string.IsNullOrWhiteSpace(
                    archivo.FileName
                ))
            {
                return nombrePredeterminado;
            }

            return Path.GetFileName(
                archivo.FileName
            );
        }

        private string ObtenerMensajeErrorApi(
            string contenido,
            System.Net.HttpStatusCode statusCode)
        {
            if (!string.IsNullOrWhiteSpace(contenido))
            {
                try
                {
                    ApiErrorBiometrico error =
                        JsonConvert.DeserializeObject
                            <ApiErrorBiometrico>(
                                contenido
                            );

                    if (error != null &&
                        !string.IsNullOrWhiteSpace(
                            error.Message
                        ))
                    {
                        return error.Message;
                    }
                }
                catch
                {
                    // La respuesta puede ser texto simple.
                }

                if (contenido.Length <= 500)
                {
                    return contenido;
                }
            }

            return
                "La API biométrica respondió con el código " +
                (int)statusCode +
                ".";
        }

        private async Task<ResultadosCoincidenciasViewModel> ConvertirResultadoApiAsync(
    ApiBusquedaBiometricaResponse resultadoApi,
    bool tieneFotografia,
    bool tieneHuella)
        {
            List<CoincidenciaResultadoViewModel> coincidencias =
                new List<CoincidenciaResultadoViewModel>();

            if (resultadoApi == null || resultadoApi.Resultados == null)
            {
                GuardarCoincidenciasEnSesion(coincidencias);

                return new ResultadosCoincidenciasViewModel
                {
                    TieneNombreConsulta = false,
                    TieneAliasConsulta = false,
                    TieneFotografiaConsulta = tieneFotografia,
                    TieneHuellaConsulta = tieneHuella,

                    Coincidencias = coincidencias,

                    TotalCoincidencias = 0,
                    TotalTexto = 0,
                    TotalFotografia = 0,
                    TotalHuella = 0,
                    TotalCombinadas = 0,
                    TotalFotoHuella = 0,

                    CoincidenciaSeleccionada = null
                };
            }
            /*
             * Por ahora habilitamos en la presentación:
             *
             * Fuente 5 = Objetivos Prioritarios
             * Fuente 6 = Detenidos FGEA
             *
             * Las demás fuentes se habilitarán conforme
             * agreguemos su enriquecimiento visual.
             */

            List<ApiCoincidenciaBiometricaDto> resultadosApi =
    resultadoApi.Resultados
        .Where(x =>
            x != null &&
            (
                x.IdTbFuente == 1 ||
                x.IdTbFuente == 2 ||
                x.IdTbFuente == 3 ||
                x.IdTbFuente == 5 ||
                x.IdTbFuente == 6 ||
                x.IdTbFuente == 7 ||
                x.IdTbFuente == 8
            )
        )
        .ToList();

            int consecutivo = 1;

            foreach (ApiCoincidenciaBiometricaDto item in resultadosApi)
            {
                decimal porcentajeNombre =
                    item.SimilitudNombre ?? 0;

                decimal porcentajeAlias =
                    item.SimilitudAlias ?? 0;

                decimal porcentajeFoto =
                    item.SimilitudFoto ?? 0;

                decimal porcentajeHuella =
                    item.SimilitudHuella ?? 0;

                decimal porcentajeTexto =
                    Math.Max(
                        porcentajeNombre,
                        porcentajeAlias
                    );

                /*
                 * No hacemos promedio.
                 *
                 * Mostramos como mejor coincidencia
                 * la evidencia más fuerte encontrada.
                 */
                decimal similitudGlobal =
                    Math.Max(
                        porcentajeTexto,
                        Math.Max(
                            porcentajeFoto,
                            porcentajeHuella
                        )
                    );

                string origenCoincidenciaTexto = "";

                if (
                    porcentajeNombre > 0 &&
                    porcentajeAlias > 0
                )
                {
                    origenCoincidenciaTexto =
                        "NOMBRE_Y_ALIAS";
                }
                else if (porcentajeNombre > 0)
                {
                    origenCoincidenciaTexto =
                        "NOMBRE";
                }
                else if (porcentajeAlias > 0)
                {
                    origenCoincidenciaTexto =
                        "ALIAS";
                }

                CoincidenciaResultadoViewModel coincidencia =
                    new CoincidenciaResultadoViewModel
                    {
                        IdCoincidencia =
                            consecutivo,

                        IdPersona =
                            item.IdPersona,

                        IdTbFuente =
                            item.IdTbFuente,

                        NombreFuente =
                            ObtenerNombreFuente(
                                item.IdTbFuente
                            ),

                        EsFuenteInformativa =
                            EsFuenteInformativa(
                                item.IdTbFuente
                            ),

                        /*
                         * Estos datos son temporales.
                         *
                         * Después EnriquecerCoincidenciasPorFuente()
                         * los sustituye con los datos reales del detenido.
                         */
                        NombreCompleto =
                            "PERSONA " +
                            item.IdPersona,

                        Alias =
                            string.IsNullOrWhiteSpace(
                                item.AliasCoincidente
                            )
                                ? "SIN INFORMACIÓN"
                                : item.AliasCoincidente,

                        Folio =
                            "ID " +
                            item.IdPersona,

                        Expediente =
                            "FUENTE " +
                            item.IdTbFuente,

                        MunicipioClave =
                            "",

                        Municipio =
                            "SIN INFORMACIÓN",

                        Edad =
                            0,

                        Sexo =
                            "SIN INFORMACIÓN",

                        FotoUrl =
                            "~/Content/imagenes/Nodisponible.jpg",

                        TipoCoincidencia =
                            item.TipoCoincidencia,

                        /* =============================================
                           EVIDENCIA TEXTUAL
                           ============================================= */

                        PorcentajeNombre =
                            porcentajeNombre,

                        PorcentajeAlias =
                            porcentajeAlias,

                        PorcentajeTexto =
                            porcentajeTexto,

                        OrigenCoincidenciaTexto =
                            origenCoincidenciaTexto,

                        TextoCoincidente =
                            item.TextoCoincidente ?? "",

                        /* =============================================
                           EVIDENCIA BIOMÉTRICA
                           ============================================= */

                        PorcentajeFoto =
                            porcentajeFoto,

                        PorcentajeHuella =
                            porcentajeHuella,

                        /* =============================================
                           DATOS DE RESULTADO
                           ============================================= */

                        CriteriosCumplidos =
                            item.CriteriosCumplidos,

                        SimilitudGlobal =
                            similitudGlobal,

                        FechaRegistro =
                            DateTime.MinValue,

                        TieneAvisoMandamientos =
    item.TieneAvisoMandamientos,

                        TotalMandamientos =
    item.TotalMandamientos,

                        MandamientosJudiciales =
    item.MandamientosJudiciales == null
        ? new List<MandamientoJudicialViewModel>()
        : item.MandamientosJudiciales
            .Select(x =>
                new MandamientoJudicialViewModel
                {
                    IdNombreMandamiento =
                        x.IdNombreMandamiento,

                    IdMandamiento =
                        x.IdMandamiento,

                    NombreCompleto =
                        x.NombreCompleto,

                    NumeroControl =
                        x.NumeroControl,

                    NumeroExpediente =
                        x.NumeroExpediente,

                    TipoMandamiento =
                        x.TipoMandamiento,

                    EstadoProceso =
                        x.EstadoProceso,

                    Delito =
                        x.Delito,

                    FechaExpedicion =
                        x.FechaExpedicion,

                    FechaAlta =
                        x.FechaAlta,

                    PorcentajeNombre =
                        x.PorcentajeNombre
                }
            )
            .ToList()
                    };

                coincidencias.Add(
                    coincidencia
                );

                consecutivo++;
            }

            /*
             * IMPORTANTE:
             *
             * NO volvemos a ordenar.
             *
             * La API ya ordenó correctamente considerando:
             *
             * - Nombre
             * - Alias
             * - Foto
             * - Huella
             * - CriteriosCumplidos
             * - PRIORIZAR / ESTRICTO
             *
             * El MVC solamente conserva ese orden.
             */



            /*
             * Fuentes 2, 7 y 8:
             *
             * 2 = CAPEA / FEMDLP
             * 7 = Alerta Amber
             * 8 = Protocolo Alba
             *
             * Se enriquecen mediante la API.
             */
            await EnriquecerCoincidenciasFiscaliaWebAsync(
                coincidencias
            );


            var mandamientoMvc =
    coincidencias
        .Where(x =>
            x.MandamientosJudiciales != null &&
            x.MandamientosJudiciales.Count > 0
        )
        .SelectMany(x =>
            x.MandamientosJudiciales
        )
        .FirstOrDefault();

            System.Diagnostics.Debug.WriteLine(
                "DELITO MAPEADO MVC: " +
                (
                    mandamientoMvc == null
                        ? "NO HAY MANDAMIENTO"
                        : mandamientoMvc.Delito ?? "NULL"
                )
            );

            /*
             * Completamos los datos reales.
             *
             * Actualmente Fuente 6:
             * Detenidos FGEA.
             */
            EnriquecerCoincidenciasPorFuente(
                coincidencias
            );


            /*
             * Fuente 3.
             */
            await EnriquecerCoincidenciasPersonasInteresAsync(
                coincidencias
            );


            /*
             * ============================================================
             * ALIAS REALES DE OBJETIVOS PRIORITARIOS
             * ============================================================
             *
             * El SP utilizado para la tarjeta no devuelve los alias
             * reales de la persona objetivo.
             *
             * Los obtenemos desde el detalle de Fuente 5.
             */
            await EnriquecerAliasObjetivosPrioritariosAsync(
                coincidencias
            );


            /*
             * Después del enriquecimiento guardamos
             * los candidatos completos en sesión.
             *
             * Así "Ver detalle" utiliza exactamente
             * los mismos resultados.
             */
            GuardarCoincidenciasEnSesion(
                coincidencias
            );


            /*
             * Como temporalmente mostramos únicamente
             * Fuente 6, calculamos los contadores sobre
             * la lista ya filtrada y no usamos directamente
             * los totales globales de la API.
             */
            int totalTexto =
                coincidencias.Count(x =>
                    x.PorcentajeNombre > 0 ||
                    x.PorcentajeAlias > 0
                );

            int totalFotografia =
                coincidencias.Count(x =>
                    x.PorcentajeFoto > 0
                );

            int totalHuella =
                coincidencias.Count(x =>
                    x.PorcentajeHuella > 0
                );

            int totalCombinadas =
                coincidencias.Count(x =>
                    x.CriteriosCumplidos >= 2
                );

            int totalFotoHuella =
                coincidencias.Count(x =>
                    x.PorcentajeFoto > 0 &&
                    x.PorcentajeHuella > 0
                );


            ResultadosCoincidenciasViewModel resultado =
                new ResultadosCoincidenciasViewModel
                {
                    TieneNombreConsulta =
                        resultadoApi.TieneNombreConsulta,

                    TieneAliasConsulta =
                        resultadoApi.TieneAliasConsulta,

                    TieneFotografiaConsulta =
                        tieneFotografia,

                    TieneHuellaConsulta =
                        tieneHuella,

                    Coincidencias =
                        coincidencias,

                    TotalCoincidencias =
                        coincidencias.Count,

                    TotalTexto =
                        totalTexto,

                    TotalFotografia =
                        totalFotografia,

                    TotalHuella =
                        totalHuella,

                    TotalCombinadas =
                        totalCombinadas,

                    TotalFotoHuella =
                        totalFotoHuella,

                    CoincidenciaSeleccionada =
                        coincidencias.FirstOrDefault()
                };

            return resultado;
        }


        //    private ResultadosCoincidenciasViewModel
        //ConvertirResultadoApi(
        //    ApiBusquedaBiometricaResponse resultadoApi,
        //    bool tieneFotografia,
        //    bool tieneHuella)
        //    {
        //        var coincidencias =
        //            new List<CoincidenciaResultadoViewModel>();

        //        int consecutivo = 1;

        //        foreach (
        //            ApiCoincidenciaBiometricaDto item
        //            in resultadoApi.Resultados
        //        )
        //        {
        //            decimal porcentajeFoto =
        //                item.SimilitudFoto ?? 0;

        //            decimal porcentajeHuella =
        //                item.SimilitudHuella ?? 0;

        //            decimal similitudGlobal =
        //                Math.Max(
        //                    porcentajeFoto,
        //                    porcentajeHuella
        //                );

        //            coincidencias.Add(
        //                new CoincidenciaResultadoViewModel
        //                {
        //                    /*
        //                     * Este consecutivo se utiliza únicamente
        //                     * para seleccionar el registro en la página.
        //                     */
        //                    IdCoincidencia =
        //                        consecutivo,

        //                    IdPersona =
        //                        item.IdPersona,

        //                    IdTbFuente =
        //                        item.IdTbFuente,

        //                    NombreCompleto =
        //                        "PERSONA " +
        //                        item.IdPersona,

        //                    Alias =
        //                        "SIN INFORMACIÓN",

        //                    Folio =
        //                        "ID " +
        //                        item.IdPersona,

        //                    Expediente =
        //                        "FUENTE " +
        //                        item.IdTbFuente,

        //                    MunicipioClave =
        //                        "",

        //                    Municipio =
        //                        "FUENTE " +
        //                        item.IdTbFuente,

        //                    Edad =
        //                        0,

        //                    Sexo =
        //                        "SIN INFORMACIÓN",

        //                    FotoUrl =
        //                        "~/Content/imagenes/Nodisponible.jpg",

        //                    TipoCoincidencia =
        //                        item.TipoCoincidencia,

        //                    PorcentajeNombre =
        //                        0,

        //                    PorcentajeFoto =
        //                        porcentajeFoto,

        //                    PorcentajeHuella =
        //                        porcentajeHuella,

        //                    SimilitudGlobal =
        //                        similitudGlobal,

        //                    FechaRegistro =
        //                        DateTime.MinValue
        //                }
        //            );

        //            consecutivo++;
        //        }

        //        coincidencias =
        //coincidencias
        //    .OrderByDescending(x =>
        //        x.SimilitudGlobal
        //    )
        //    .ToList();

        //        /*
        //         * Reasignamos el identificador visual después
        //         * de ordenar los resultados.
        //         */
        //        for (
        //            int indice = 0;
        //            indice < coincidencias.Count;
        //            indice++
        //        )
        //        {
        //            coincidencias[indice].IdCoincidencia =
        //                indice + 1;
        //        }

        //        /*
        //         * Aquí se consultan los datos reales de las
        //         * coincidencias cuya fuente sea 6.
        //         */
        //        EnriquecerCoincidenciasPorFuente(
        //            coincidencias
        //        );

        //        return new ResultadosCoincidenciasViewModel
        //        {
        //            TieneFotografiaConsulta =
        //                tieneFotografia,

        //            TieneHuellaConsulta =
        //                tieneHuella,

        //            Coincidencias =
        //                coincidencias,

        //            TotalCoincidencias =
        //                coincidencias.Count,

        //            TotalFotografia =
        //                coincidencias.Count(x =>
        //                    x.PorcentajeFoto > 0
        //                ),

        //            TotalHuella =
        //                coincidencias.Count(x =>
        //                    x.PorcentajeHuella > 0
        //                ),

        //            TotalCombinadas =
        //                coincidencias.Count(x =>
        //                    x.PorcentajeFoto > 0 &&
        //                    x.PorcentajeHuella > 0
        //                ),

        //            CoincidenciaSeleccionada =
        //                coincidencias.FirstOrDefault()
        //        };
        //    }

        private void EnriquecerCoincidenciasDetenidos(List<CoincidenciaResultadoViewModel> coincidencias)
        {
            if (coincidencias == null || coincidencias.Count == 0)
            {
                return;
            }

            /*
             * Fuente 6 = Detenidos FGEA.
             *
             * IdPersona = Filiacion.dbo.Persona.CLAVE_PERSO
             */
            List<CoincidenciaResultadoViewModel> coincidenciasFuente6 =
                coincidencias
                    .Where(x =>
                        x.IdTbFuente == 6 &&
                        x.IdPersona > 0
                    )
                    .ToList();

            if (coincidenciasFuente6.Count == 0)
            {
                return;
            }

            List<int> clavesPerso =
                coincidenciasFuente6
                    .Select(x => x.IdPersona)
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

            List<SP_SIC_getCoincidenciasDetenidos_Result> detenidos =
                _filiacionMunicipalService
                    .getCoincidenciasDetenidosPorClavePerso_Results(
                        clavesPerso
                    );

            if (detenidos == null || detenidos.Count == 0)
            {
                return;
            }

            foreach (CoincidenciaResultadoViewModel coincidencia in coincidenciasFuente6)
            {
                SP_SIC_getCoincidenciasDetenidos_Result detenido =
                    detenidos.FirstOrDefault(x =>
                        Convert.ToInt32(
                            x.CLAVE_PERSO
                        ) == coincidencia.IdPersona
                    );

                if (detenido == null)
                {
                    continue;
                }

                /* ============================================================
                   IDENTIDAD
                   ============================================================ */

                int clavePerso =
                    Convert.ToInt32(
                        detenido.CLAVE_PERSO
                    );

                coincidencia.ClavePerso =
                    clavePerso > 0
                        ? (int?)clavePerso
                        : null;

                coincidencia.NombreCompleto =
                    ValorOTextoPredeterminado(
                        detenido.NombreCompleto
                    );

                coincidencia.Alias =
                    ValorOTextoPredeterminado(
                        detenido.ALIAS
                    );

                coincidencia.Folio =
                    ValorOTextoPredeterminado(
                        detenido.NUM_FILIA
                    );

                /* ============================================================
                   FECHA NACIMIENTO / EDAD
                   ============================================================ */

                DateTime fechaNacimiento =
                    detenido.FEC_NAC;

                bool fechaNacimientoValida =
                    fechaNacimiento.Year >= 1900 &&
                    fechaNacimiento <= DateTime.Today;

                if (fechaNacimientoValida)
                {
                    coincidencia.FechaNacimiento =
                        fechaNacimiento;

                    coincidencia.Edad =
                        CalcularEdad(
                            fechaNacimiento
                        );
                }
                else
                {
                    coincidencia.FechaNacimiento =
                        null;

                    coincidencia.Edad =
                        0;
                }

                /* ============================================================
                   SEXO
                   ============================================================ */

                coincidencia.Sexo =
                    NormalizarSexo(
                        detenido.SEXO
                    );

                /* ============================================================
                   MUNICIPIO
                   Actualmente el SP no lo proporciona.
                   ============================================================ */

                coincidencia.MunicipioClave =
                    "";

                coincidencia.Municipio =
                    "SIN INFORMACIÓN";

                /* ============================================================
                   ÚLTIMO DELITO
                   ============================================================ */

                coincidencia.UltimoDelito =
                    ValorOTextoPredeterminado(
                        detenido.UltimoDelito
                    );

                coincidencia.FechaUltimoDelito =
                    detenido.FechaUltimoDelito;

                /*
                 * Ya no utilizamos Expediente para guardar el delito.
                 */
                coincidencia.Expediente =
                    "SIN INFORMACIÓN";

                /*
                 * Lo dejamos temporalmente por compatibilidad con la vista
                 * actual. Después el detalle utilizará FechaUltimoDelito.
                 */
                coincidencia.FechaRegistro =
                    detenido.FechaUltimoDelito
                    ?? DateTime.MinValue;

                /* ============================================================
                   FOTO
                   ============================================================ */

                coincidencia.FotoUrl =
                    clavePerso > 0
                        ? "~/SIC/FotoDetenidoFiliacion?clavePerso=" +
                          clavePerso
                        : "~/Content/imagenes/Nodisponible.jpg";
            }
        }


        //    private void EnriquecerCoincidenciasDetenidos(
        //List<CoincidenciaResultadoViewModel> coincidencias)
        //    {
        //        if (
        //            coincidencias == null ||
        //            coincidencias.Count == 0
        //        )
        //        {
        //            return;
        //        }

        //        List<CoincidenciaResultadoViewModel>
        //            coincidenciasFuente6 =
        //                coincidencias
        //                    .Where(x =>
        //                        x.IdTbFuente == 6 &&
        //                        x.IdPersona > 0
        //                    )
        //                    .ToList();

        //        if (coincidenciasFuente6.Count == 0)
        //        {
        //            return;
        //        }

        //        List<int> idsNomPerso =
        //            coincidenciasFuente6
        //                .Select(x =>
        //                    x.IdPersona
        //                )
        //                .Distinct()
        //                .ToList();

        //        List<SP_SIC_getCoincidenciasDetenidos_Result> detenidos =
        //            _filiacionMunicipalService
        //                .getCoincidenciasDetenidos_Results(
        //                    idsNomPerso
        //                );

        //        if (
        //            detenidos == null ||
        //            detenidos.Count == 0
        //        )
        //        {
        //            return;
        //        }

        //        foreach (
        //            CoincidenciaResultadoViewModel coincidencia
        //            in coincidenciasFuente6
        //        )
        //        {
        //            CoincidenciaDetenidoFuente6Dto detenido =
        //                detenidos.FirstOrDefault(x =>
        //                    ContieneIdNomPerso(
        //                        x.IdsNomPersoOrigenAlerta,
        //                        coincidencia.IdPersona
        //                    ) ||
        //                    ContieneIdNomPerso(
        //                        x.IdsNomPerso,
        //                        coincidencia.IdPersona
        //                    )
        //                );

        //            if (detenido == null)
        //            {
        //                continue;
        //            }

        //            coincidencia.NombreCompleto =
        //                ValorOTextoPredeterminado(
        //                    detenido.NombreCompleto
        //                );

        //            coincidencia.Alias =
        //                ValorOTextoPredeterminado(
        //                    detenido.ALIAS
        //                );

        //            coincidencia.Folio =
        //                ValorOTextoPredeterminado(
        //                    detenido.NUM_FILIA
        //                );

        //            coincidencia.Edad =
        //                detenido.FEC_NAC.HasValue
        //                    ? CalcularEdad(
        //                        detenido.FEC_NAC.Value
        //                    )
        //                    : 0;

        //            coincidencia.Sexo =
        //                NormalizarSexo(
        //                    detenido.SEXO
        //                );

        //            /*
        //             * El query no trae municipio.
        //             */
        //            coincidencia.MunicipioClave =
        //                "";

        //            coincidencia.Municipio =
        //                "SIN INFORMACIÓN";

        //            /*
        //             * Aprovechamos el campo expediente para mostrar
        //             * temporalmente el último delito.
        //             */
        //            coincidencia.Expediente =
        //                ValorOTextoPredeterminado(
        //                    detenido.UltimoDelito
        //                );

        //            coincidencia.FechaRegistro =
        //                detenido.FechaUltimoDelito ??
        //                DateTime.MinValue;

        //            coincidencia.FotoUrl =
        //                detenido.CLAVE_PERSO > 0
        //                    ? "~/SIC/FotoDetenidoFiliacion?clavePerso=" +
        //                      detenido.CLAVE_PERSO
        //                    : "~/Content/imagenes/Nodisponible.jpg";
        //        }
        //    }


        private static bool ContieneIdNomPerso(
    string idsTexto,
    int idPersona)
        {
            if (
                string.IsNullOrWhiteSpace(idsTexto) ||
                idPersona <= 0
            )
            {
                return false;
            }

            return idsTexto
                .Split(
                    new[]
                    {
                ',',
                ';',
                '|'
                    },
                    StringSplitOptions.RemoveEmptyEntries
                )
                .Select(x =>
                {
                    int id;

                    return int.TryParse(
                        x.Trim(),
                        out id
                    )
                        ? id
                        : 0;
                })
                .Any(x =>
                    x == idPersona
                );
        }


        private static string ValorOTextoPredeterminado(
            string valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? "SIN INFORMACIÓN"
                : valor.Trim();
        }


        private static int CalcularEdad(
            DateTime fechaNacimiento)
        {
            DateTime hoy =
                DateTime.Today;

            int edad =
                hoy.Year -
                fechaNacimiento.Year;

            if (
                fechaNacimiento.Date >
                hoy.AddYears(-edad)
            )
            {
                edad--;
            }

            return edad < 0
                ? 0
                : edad;
        }


        private static string NormalizarSexo(
            string sexo)
        {
            if (string.IsNullOrWhiteSpace(sexo))
            {
                return "SIN INFORMACIÓN";
            }

            string valor =
                sexo.Trim()
                    .ToUpperInvariant();

            if (
                valor == "M" ||
                valor == "MASCULINO" ||
                valor == "HOMBRE"
            )
            {
                return "HOMBRE";
            }

            if (
                valor == "F" ||
                valor == "FEMENINO" ||
                valor == "MUJER"
            )
            {
                return "MUJER";
            }

            return valor;
        }

        private void EnriquecerCoincidenciasPorFuente(
    List<CoincidenciaResultadoViewModel> coincidencias)
        {
            if (
                coincidencias == null ||
                coincidencias.Count == 0
            )
            {
                return;
            }

            foreach (
                CoincidenciaResultadoViewModel coincidencia
                in coincidencias
            )
            {
                coincidencia.NombreFuente =
                    ObtenerNombreFuente(
                        coincidencia.IdTbFuente
                    );

                coincidencia.EsFuenteInformativa =
                    EsFuenteInformativa(
                        coincidencia.IdTbFuente
                    );
            }


            /*
             * Fuente 6 - Detenidos FGEA.
             */
            EnriquecerCoincidenciasDetenidos(
                coincidencias
            );


            /*
             * Fuente 5 - Objetivos Prioritarios.
             */
            EnriquecerCoincidenciasObjetivosPrioritarios(
                coincidencias
            );


            /*
             * Fuentes 2, 7 y 8.
             *
             * Todavía falta crear su enriquecimiento
             * de tarjeta.
             */
            //EnriquecerCoincidenciasFiscaliaWeb(
            //    coincidencias
            //);


            /*
             * Fuente 1 - C5.
             */
            EnriquecerCoincidenciasC5(
                coincidencias
            );


            /*
             * Mandamientos al final.
             */
            //AgregarAvisosMandamientos(
            //    coincidencias
            //);
        }



        //  private void EnriquecerCoincidenciasPorFuente(
        //List<CoincidenciaResultadoViewModel> coincidencias)
        //  {
        //      if (
        //          coincidencias == null ||
        //          coincidencias.Count == 0
        //      )
        //      {
        //          return;
        //      }

        //      /*
        //       * Aseguramos que todas las coincidencias
        //       * tengan el nombre de su fuente.
        //       */
        //      foreach (
        //          CoincidenciaResultadoViewModel coincidencia
        //          in coincidencias
        //      )
        //      {
        //          coincidencia.NombreFuente =
        //              ObtenerNombreFuente(
        //                  coincidencia.IdTbFuente
        //              );

        //          coincidencia.EsFuenteInformativa =
        //              EsFuenteInformativa(
        //                  coincidencia.IdTbFuente
        //              );
        //      }

        //      /*
        //       * Fuente 6: Detenidos FGEA.
        //       * Este enriquecimiento ya funciona.
        //       */
        //      EnriquecerCoincidenciasDetenidos(
        //          coincidencias
        //      );

        //      /*
        //       * Próximos enriquecimientos:
        //       *
        //       * Fuente 1:*/
        //        EnriquecerCoincidenciasC5(coincidencias);
        //       /*
        //       * Fuente 5:*/
        //       EnriquecerCoincidenciasObjetivosPrioritarios(coincidencias);
        //      /*
        //      * Fuentes 2, 7 y 8:*/
        //      EnriquecerCoincidenciasCapea(coincidencias);


        //      /*
        //       * Mandamientos siempre se consulta al final,
        //       * después de obtener los nombres oficiales.
        //       */
        //      AgregarAvisosMandamientos(
        //          coincidencias
        //      );
        //  }

        private void AgregarAvisosMandamientos(
    List<CoincidenciaResultadoViewModel> coincidencias)
        {
            if (
                coincidencias == null ||
                coincidencias.Count == 0
            )
            {
                return;
            }

            /*
             * Evita consultar varias veces el mismo nombre
             * cuando aparece más de una coincidencia biométrica.
             */
            Dictionary<
                string,
                List<MandamientoJudicialViewModel>
            > cacheNombres =
                new Dictionary<
                    string,
                    List<MandamientoJudicialViewModel>
                >(
                    StringComparer.OrdinalIgnoreCase
                );

            foreach (
                CoincidenciaResultadoViewModel coincidencia
                in coincidencias
            )
            {
                coincidencia.TieneAvisoMandamientos =
                    false;

                coincidencia.TotalMandamientos =
                    0;

                coincidencia.MandamientosJudiciales =
                    new List<MandamientoJudicialViewModel>();

                string nombrePrincipal =
                    ObtenerNombrePrincipal(
                        coincidencia.NombreCompleto
                    );

                if (
                    string.IsNullOrWhiteSpace(nombrePrincipal) ||
                    nombrePrincipal.Equals(
                        "SIN INFORMACIÓN",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }

                string llaveNombre =
                    NormalizarNombreComparacion(
                        nombrePrincipal
                    );

                List<MandamientoJudicialViewModel> mandamientos;

                if (
                    !cacheNombres.TryGetValue(
                        llaveNombre,
                        out mandamientos
                    )
                )
                {
                    mandamientos =
                        BuscarMandamientosPorNombre(
                            nombrePrincipal,
                            85
                        );

                    cacheNombres.Add(
                        llaveNombre,
                        mandamientos
                    );
                }

                coincidencia.MandamientosJudiciales =
                    new List<MandamientoJudicialViewModel>(
                        mandamientos
                    );

                coincidencia.TotalMandamientos =
                    coincidencia
                        .MandamientosJudiciales
                        .Count;

                coincidencia.TieneAvisoMandamientos =
                    coincidencia.TotalMandamientos > 0;
            }
        }
        //COMENTADO LEO V2.0
        //    private List<MandamientoJudicialViewModel>
        //BuscarMandamientosPorNombre(
        //    string nombreCompleto,
        //    double umbral = 85)
        //    {
        //        List<MandamientoJudicialViewModel> resultado =
        //            new List<MandamientoJudicialViewModel>();

        //        string nombrePrincipal =
        //            ObtenerNombrePrincipal(
        //                nombreCompleto
        //            );

        //        if (
        //            string.IsNullOrWhiteSpace(nombrePrincipal) ||
        //            nombrePrincipal.Equals(
        //                "SIN INFORMACIÓN",
        //                StringComparison.OrdinalIgnoreCase
        //            )
        //        )
        //        {
        //            return resultado;
        //        }

        //        DataTable candidatos =
        //            _filiacionMunicipalService
        //                .BuscarMandamientosCandidatosPorNombre(
        //                    nombrePrincipal
        //                );

        //        if (
        //            candidatos == null ||
        //            candidatos.Rows.Count == 0
        //        )
        //        {
        //            return resultado;
        //        }

        //        foreach (DataRow fila in candidatos.Rows)
        //        {
        //            string nombreCandidato =
        //                ObtenerTextoMandamiento(
        //                    fila,
        //                    "Nombre"
        //                );

        //            if (string.IsNullOrWhiteSpace(nombreCandidato))
        //            {
        //                continue;
        //            }

        //            double similitud =
        //                CalcularSimilitudNombreMandamiento(
        //                    nombrePrincipal,
        //                    nombreCandidato
        //                );

        //            if (similitud < umbral)
        //            {
        //                continue;
        //            }

        //            MandamientoJudicialViewModel mandamiento =
        //                new MandamientoJudicialViewModel
        //                {
        //                    IdNombreMandamiento =
        //                        ObtenerEnteroMandamiento(
        //                            fila,
        //                            "IdOrigenAlerta"
        //                        ),

        //                    IdMandamiento =
        //                        ObtenerEnteroMandamiento(
        //                            fila,
        //                            "IdMandamiento"
        //                        ),

        //                    NombreCompleto =
        //                        nombreCandidato,

        //                    NumeroControl =
        //                        ValorOTextoPredeterminado(
        //                            ObtenerTextoMandamiento(
        //                                fila,
        //                                "numero_control"
        //                            )
        //                        ),

        //                    NumeroExpediente =
        //                        ValorOTextoPredeterminado(
        //                            ObtenerTextoMandamiento(
        //                                fila,
        //                                "numero_expediente"
        //                            )
        //                        ),

        //                    TipoMandamiento =
        //                        ValorOTextoPredeterminado(
        //                            ObtenerTextoMandamiento(
        //                                fila,
        //                                "mandamiento"
        //                            )
        //                        ),

        //                    EstadoProceso =
        //                        ValorOTextoPredeterminado(
        //                            ObtenerTextoMandamiento(
        //                                fila,
        //                                "EstadoProceso"
        //                            )
        //                        ),

        //                    FechaExpedicion =
        //                        ObtenerFechaNullableMandamiento(
        //                            fila,
        //                            "fecha_expedicion"
        //                        ),

        //                    FechaAlta =
        //                        ObtenerFechaNullableMandamiento(
        //                            fila,
        //                            "fecha_alta"
        //                        ),

        //                    PorcentajeNombre =
        //                        Convert.ToInt32(
        //                            Math.Round(similitud)
        //                        )
        //                };

        //            resultado.Add(mandamiento);
        //        }

        //        return resultado
        //            .GroupBy(x => new
        //            {
        //                x.IdNombreMandamiento,
        //                x.IdMandamiento
        //            })
        //            .Select(x =>
        //                x.OrderByDescending(y =>
        //                    y.PorcentajeNombre
        //                )
        //                .First()
        //            )
        //            .OrderByDescending(x =>
        //                x.FechaExpedicion ??
        //                x.FechaAlta ??
        //                DateTime.MinValue
        //            )
        //            .ToList();
        //    }
        private List<MandamientoJudicialViewModel> BuscarMandamientosPorNombre(string nombreCompleto, double umbral = 85)
        {
            List<MandamientoJudicialViewModel> resultado =
                new List<MandamientoJudicialViewModel>();


            string nombrePrincipal =
                ObtenerNombrePrincipal(
                    nombreCompleto
                );


            if (
                string.IsNullOrWhiteSpace(
                    nombrePrincipal
                ) ||
                nombrePrincipal.Equals(
                    "SIN INFORMACIÓN",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return resultado;
            }


            DataTable candidatos =
                _filiacionMunicipalService
                    .BuscarMandamientosCandidatosPorNombre(
                        nombrePrincipal
                    );


            if (
                candidatos == null ||
                candidatos.Rows.Count == 0
            )
            {
                return resultado;
            }


            /*
             * Evita consultar varias veces el delito
             * del mismo mandamiento.
             */
            Dictionary<int, string> cacheDelitos =
                new Dictionary<int, string>();


            foreach (DataRow fila in candidatos.Rows)
            {
                string nombreCandidato =
                    ObtenerTextoMandamiento(
                        fila,
                        "Nombre"
                    );


                if (
                    string.IsNullOrWhiteSpace(
                        nombreCandidato
                    )
                )
                {
                    continue;
                }


                double similitud =
                    CalcularSimilitudNombreMandamiento(
                        nombrePrincipal,
                        nombreCandidato
                    );


                if (similitud < umbral)
                {
                    continue;
                }


                int idMandamiento =
                    ObtenerEnteroMandamiento(
                        fila,
                        "IdMandamiento"
                    );


                /*
                 * ============================================================
                 * DELITO
                 * ============================================================
                 */

                string delito =
                    "";


                if (idMandamiento > 0)
                {
                    if (
                        !cacheDelitos.TryGetValue(
                            idMandamiento,
                            out delito
                        )
                    )
                    {
                        delito =
                            _filiacionMunicipalService
                                .GetDelitoMandamiento(
                                    idMandamiento
                                );

                        cacheDelitos[
                            idMandamiento
                        ] =
                            delito;
                    }
                }


                MandamientoJudicialViewModel mandamiento =
                    new MandamientoJudicialViewModel
                    {
                        IdNombreMandamiento =
                            ObtenerEnteroMandamiento(
                                fila,
                                "IdOrigenAlerta"
                            ),

                        IdMandamiento =
                            idMandamiento,

                        NombreCompleto =
                            nombreCandidato,

                        NumeroControl =
                            ValorOTextoPredeterminado(
                                ObtenerTextoMandamiento(
                                    fila,
                                    "numero_control"
                                )
                            ),

                        NumeroExpediente =
                            ValorOTextoPredeterminado(
                                ObtenerTextoMandamiento(
                                    fila,
                                    "numero_expediente"
                                )
                            ),

                        TipoMandamiento =
                            ValorOTextoPredeterminado(
                                ObtenerTextoMandamiento(
                                    fila,
                                    "mandamiento"
                                )
                            ),

                        EstadoProceso =
                            ValorOTextoPredeterminado(
                                ObtenerTextoMandamiento(
                                    fila,
                                    "EstadoProceso"
                                )
                            ),

                        Delito =
                            ValorOTextoPredeterminado(
                                delito
                            ),

                        FechaExpedicion =
                            ObtenerFechaNullableMandamiento(
                                fila,
                                "fecha_expedicion"
                            ),

                        FechaAlta =
                            ObtenerFechaNullableMandamiento(
                                fila,
                                "fecha_alta"
                            ),

                        PorcentajeNombre =
                            Convert.ToInt32(
                                Math.Round(
                                    similitud
                                )
                            )
                    };


                resultado.Add(
                    mandamiento
                );
            }


            return resultado
                .GroupBy(x => new
                {
                    x.IdNombreMandamiento,
                    x.IdMandamiento
                })
                .Select(x =>
                    x.OrderByDescending(y =>
                        y.PorcentajeNombre
                    )
                    .First()
                )
                .OrderByDescending(x =>
                    x.FechaExpedicion ??
                    x.FechaAlta ??
                    DateTime.MinValue
                )
                .ToList();
        }

        private static string ObtenerNombrePrincipal(
    string nombreCompleto)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto))
            {
                return "";
            }

            /*
             * El SP de detenidos puede devolver varios nombres
             * separados por coma. Para Mandamientos usamos el
             * nombre principal.
             */
            return nombreCompleto
                .Split(
                    new[] { ',' },
                    StringSplitOptions.RemoveEmptyEntries
                )
                .Select(x => x.Trim())
                .FirstOrDefault(x =>
                    !string.IsNullOrWhiteSpace(x)
                )
                ?? "";
        }


        private static string ObtenerTextoMandamiento(
            DataRow fila,
            string columna)
        {
            if (
                fila == null ||
                fila.Table == null ||
                !fila.Table.Columns.Contains(columna) ||
                fila[columna] == null ||
                fila[columna] == DBNull.Value
            )
            {
                return "";
            }

            return Convert
                .ToString(
                    fila[columna]
                )
                .Trim();
        }


        private static int ObtenerEnteroMandamiento(
            DataRow fila,
            string columna)
        {
            string valor =
                ObtenerTextoMandamiento(
                    fila,
                    columna
                );

            int resultado;

            return int.TryParse(
                valor,
                out resultado
            )
                ? resultado
                : 0;
        }


        private static DateTime? ObtenerFechaNullableMandamiento(
            DataRow fila,
            string columna)
        {
            if (
                fila == null ||
                fila.Table == null ||
                !fila.Table.Columns.Contains(columna) ||
                fila[columna] == null ||
                fila[columna] == DBNull.Value
            )
            {
                return null;
            }

            DateTime fecha;

            return DateTime.TryParse(
                Convert.ToString(
                    fila[columna]
                ),
                out fecha
            )
                ? fecha
                : (DateTime?)null;
        }

        private static double CalcularSimilitudNombreMandamiento(
    string nombreA,
    string nombreB)
        {
            List<string> tokensA =
                ObtenerTokensComparacionNombre(
                    nombreA
                );

            List<string> tokensB =
                ObtenerTokensComparacionNombre(
                    nombreB
                );

            if (
                tokensA.Count == 0 ||
                tokensB.Count == 0
            )
            {
                return 0;
            }

            List<string> listaMenor =
                tokensA.Count <= tokensB.Count
                    ? tokensA
                    : tokensB;

            List<string> listaMayor =
                tokensA.Count <= tokensB.Count
                    ? tokensB
                    : tokensA;

            bool[] utilizados =
                new bool[listaMayor.Count];

            double sumaMejoresCoincidencias =
                0;

            foreach (string token in listaMenor)
            {
                double mejorSimilitud =
                    0;

                int mejorIndice =
                    -1;

                for (
                    int i = 0;
                    i < listaMayor.Count;
                    i++
                )
                {
                    if (utilizados[i])
                    {
                        continue;
                    }

                    double similitudToken =
                        CalcularSimilitudTextoMandamiento(
                            token,
                            listaMayor[i]
                        );

                    if (similitudToken > mejorSimilitud)
                    {
                        mejorSimilitud =
                            similitudToken;

                        mejorIndice =
                            i;
                    }
                }

                /*
                 * Evita que una palabra completamente diferente
                 * aporte puntuación.
                 */
                if (
                    mejorIndice >= 0 &&
                    mejorSimilitud >= 60
                )
                {
                    utilizados[mejorIndice] =
                        true;

                    sumaMejoresCoincidencias +=
                        mejorSimilitud;
                }
            }

            double promedioTokens =
                sumaMejoresCoincidencias /
                listaMenor.Count;

            double factorCantidad =
                (double)listaMenor.Count /
                listaMayor.Count;

            /*
             * Permite que falte un segundo nombre sin castigar
             * demasiado, pero evita aceptar nombres incompletos.
             */
            double puntajeTokens =
                promedioTokens *
                (
                    0.75 +
                    (0.25 * factorCantidad)
                );

            string textoOrdenadoA =
                string.Join(
                    " ",
                    tokensA.OrderBy(x => x)
                );

            string textoOrdenadoB =
                string.Join(
                    " ",
                    tokensB.OrderBy(x => x)
                );

            double puntajeTextoOrdenado =
                CalcularSimilitudTextoMandamiento(
                    textoOrdenadoA,
                    textoOrdenadoB
                );

            double resultado =
                (puntajeTokens * 0.75) +
                (puntajeTextoOrdenado * 0.25);

            return Math.Max(
                0,
                Math.Min(100, resultado)
            );
        }


        private static List<string> ObtenerTokensComparacionNombre(
            string nombre)
        {
            string normalizado =
                NormalizarNombreComparacion(
                    nombre
                );

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

            return normalizado
                .Split(
                    new[] { ' ' },
                    StringSplitOptions.RemoveEmptyEntries
                )
                .Where(x =>
                    !palabrasIgnoradas.Contains(x)
                )
                .ToList();
        }


        private static string NormalizarNombreComparacion(
            string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return "";
            }

            string descompuesto =
                texto
                    .Trim()
                    .ToUpperInvariant()
                    .Normalize(
                        NormalizationForm.FormD
                    );

            StringBuilder resultado =
                new StringBuilder();

            foreach (char caracter in descompuesto)
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


        private static double CalcularSimilitudTextoMandamiento(
            string textoA,
            string textoB)
        {
            if (
                string.IsNullOrWhiteSpace(textoA) ||
                string.IsNullOrWhiteSpace(textoB)
            )
            {
                return 0;
            }

            if (
                textoA.Equals(
                    textoB,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return 100;
            }

            int distancia =
                CalcularDistanciaLevenshteinMandamiento(
                    textoA,
                    textoB
                );

            int longitudMayor =
                Math.Max(
                    textoA.Length,
                    textoB.Length
                );

            if (longitudMayor == 0)
            {
                return 100;
            }

            return
                (
                    1.0 -
                    (
                        (double)distancia /
                        longitudMayor
                    )
                ) * 100.0;
        }


        private static int CalcularDistanciaLevenshteinMandamiento(
            string textoA,
            string textoB)
        {
            int longitudA =
                textoA.Length;

            int longitudB =
                textoB.Length;

            int[,] matriz =
                new int[
                    longitudA + 1,
                    longitudB + 1
                ];

            for (int i = 0; i <= longitudA; i++)
            {
                matriz[i, 0] =
                    i;
            }

            for (int j = 0; j <= longitudB; j++)
            {
                matriz[0, j] =
                    j;
            }

            for (int i = 1; i <= longitudA; i++)
            {
                for (int j = 1; j <= longitudB; j++)
                {
                    int costo =
                        textoA[i - 1] ==
                        textoB[j - 1]
                            ? 0
                            : 1;

                    matriz[i, j] =
                        Math.Min(
                            Math.Min(
                                matriz[i - 1, j] + 1,
                                matriz[i, j - 1] + 1
                            ),
                            matriz[i - 1, j - 1] +
                            costo
                        );
                }
            }

            return matriz[
                longitudA,
                longitudB
            ];
        }

        private static string DeterminarTipoCoincidencia(
    decimal porcentajeFoto,
    decimal porcentajeHuella)
        {
            bool tieneCoincidenciaFoto =
                porcentajeFoto > 0;

            bool tieneCoincidenciaHuella =
                porcentajeHuella > 0;

            if (
                tieneCoincidenciaFoto &&
                tieneCoincidenciaHuella
            )
            {
                return "COMBINADA";
            }

            if (tieneCoincidenciaFoto)
            {
                return "FOTO";
            }

            if (tieneCoincidenciaHuella)
            {
                return "HUELLA";
            }

            return "SIN COINCIDENCIA";
        }


        private void GuardarCoincidenciasEnSesion(
            List<CoincidenciaResultadoViewModel> coincidencias)
        {
            if (
                System.Web.HttpContext.Current == null ||
                System.Web.HttpContext.Current.Session == null
            )
            {
                return;
            }

            System.Web.HttpContext.Current.Session[
                ClaveSesionCoincidencias
            ] =
                coincidencias ??
                new List<CoincidenciaResultadoViewModel>();
        }


        private static string ObtenerNombreFuente(int idTbFuente)
        {
            switch (idTbFuente)
            {
                case 1:
                    return "C5 - Detenidos";

                case 2:
                    return "FGEA - CAPEA / FEMDLP";

                case 3:
                    return "FGEA - Personas de interés";

                case 4:
                    return "FGEA - Mandamientos Judiciales";

                case 5:
                    return "FGEA - Objetivos Prioritarios";

                case 6:
                    return "FGEA - Detenidos";

                case 7:
                    return "Alerta Amber";

                case 8:
                    return "Protocolo Alba";

                default:
                    return "Fuente " + idTbFuente;
            }
        }


        private static bool EsFuenteInformativa(
            int idTbFuente)
        {
            /*
             * Mandamientos Judiciales solamente
             * agrega una alerta nominal.
             */
            return idTbFuente == 4;
        }

        private void EnriquecerCoincidenciasObjetivosPrioritarios(List<CoincidenciaResultadoViewModel> coincidencias)
        {
            if (coincidencias == null || coincidencias.Count == 0)
            {
                return;
            }

            /*
             * Fuente 5:
             *
             * IdPersona = tb_Objetivo.int_id_objetivo
             */
            List<CoincidenciaResultadoViewModel> coincidenciasFuente5 =
                coincidencias
                    .Where(x =>
                        x.IdTbFuente == 5 &&
                        x.IdPersona > 0
                    )
                    .ToList();

            if (coincidenciasFuente5.Count == 0)
            {
                return;
            }

            List<int> idsObjetivo =
                coincidenciasFuente5
                    .Select(x => x.IdPersona)
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

            DataTable objetivos =
                _filiacionMunicipalService
                    .GetInfoObjetivosPrioritariosPorIdObjetivo(
                        idsObjetivo
                    );

            if (objetivos == null || objetivos.Rows.Count == 0)
            {
                return;
            }

            foreach (CoincidenciaResultadoViewModel coincidencia in coincidenciasFuente5)
            {
                DataRow objetivo =
                    objetivos
                        .AsEnumerable()
                        .FirstOrDefault(x =>
                            x.Table.Columns.Contains(
                                "int_id_objetivo"
                            ) &&
                            x["int_id_objetivo"] != DBNull.Value &&
                            Convert.ToInt32(
                                x["int_id_objetivo"]
                            ) == coincidencia.IdPersona
                        );

                if (objetivo == null)
                {
                    continue;
                }

                /* ============================================================
                   NOMBRE PRINCIPAL
                   ============================================================ */

                string nombreOrigen =
                    ObtenerValorDataRow(
                        objetivo,
                        "NombreOrigen"
                    );

                string nombreCompleto =
                    ObtenerValorDataRow(
                        objetivo,
                        "NombreCompleto"
                    );

                coincidencia.NombreCompleto =
                    !string.IsNullOrWhiteSpace(nombreOrigen) &&
                    !nombreOrigen.Equals(
                        "SIN NOMBRE",
                        StringComparison.OrdinalIgnoreCase
                    )
                        ? nombreOrigen.Trim()
                        : ValorOTextoPredeterminado(
                            nombreCompleto
                        );

                /* ============================================================
                   OTROS NOMBRES
                   ============================================================ */

                List<string> nombres =
                    string.IsNullOrWhiteSpace(nombreCompleto)
                        ? new List<string>()
                        : nombreCompleto
                            .Split(
                                new[] { ',' },
                                StringSplitOptions.RemoveEmptyEntries
                            )
                            .Select(x => x.Trim())
                            .Where(x =>
                                !string.IsNullOrWhiteSpace(x)
                            )
                            .Distinct(
                                StringComparer.OrdinalIgnoreCase
                            )
                            .ToList();

                List<string> otrosNombres =
                    nombres
                        .Where(x =>
                            !x.Equals(
                                coincidencia.NombreCompleto,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        .ToList();

                coincidencia.OtrosNombres =
                    otrosNombres.Count > 0
                        ? string.Join(
                            ", ",
                            otrosNombres
                        )
                        : "SIN INFORMACIÓN";

                /* ============================================================
                   ALIAS
                   ============================================================ */

                /*
                  * IMPORTANTE:
                  *
                  * nvarchar_alias del SP NO representa el alias
                  * de la persona objetivo.
                  *
                  * Ese campo corresponde al alias del grupo delictivo.
                  *
                  * Los alias reales del objetivo se obtienen mediante
                  * el endpoint de detalle.
                  */
                

                /* ============================================================
                   IDENTIFICADORES
                   ============================================================ */

                coincidencia.Folio =
                    "OBJETIVO " +
                    coincidencia.IdPersona;

                /*
                 * Puede haber más de una CLAVE_PERSO relacionada
                 * con los distintos nombres del objetivo.
                 */
                string clavesPerso =
                    ObtenerValorDataRow(
                        objetivo,
                        "int_clave_perso"
                    );

                coincidencia.ClavesPersoRelacionadas =
                    string.IsNullOrWhiteSpace(clavesPerso) ||
                    clavesPerso.Equals(
                        "SIN CLAVE_PERSO",
                        StringComparison.OrdinalIgnoreCase
                    )
                        ? "SIN INFORMACIÓN"
                        : clavesPerso.Trim();

                /*
                 * Si existe una sola clave positiva,
                 * también la colocamos en ClavePerso.
                 */
                List<int> clavesValidas =
                    coincidencia.ClavesPersoRelacionadas
                        .Split(
                            new[] { ',', ';', '|' },
                            StringSplitOptions.RemoveEmptyEntries
                        )
                        .Select(x => {
                            int valor;

                            return int.TryParse(
                                x.Trim(),
                                out valor
                            )
                                ? valor
                                : 0;
                        })
                        .Where(x => x > 0)
                        .Distinct()
                        .ToList();

                coincidencia.ClavePerso =
                    clavesValidas.Count == 1
                        ? (int?)clavesValidas[0]
                        : null;

                /* ============================================================
                   GRUPO / PUESTO
                   ============================================================ */

                coincidencia.GrupoDelictivo =
                    NormalizarValorObjetivo(
                        ObtenerValorDataRow(
                            objetivo,
                            "nvarchar_grupo"
                        ),
                        "SIN GRUPO"
                    );

                coincidencia.Puesto =
                    NormalizarValorObjetivo(
                        ObtenerValorDataRow(
                            objetivo,
                            "Puesto"
                        ),
                        "SIN PUESTO"
                    );

                coincidencia.EstatusGrupo =
                    NormalizarValorObjetivo(
                        ObtenerValorDataRow(
                            objetivo,
                            "EstatusGrupo"
                        ),
                        "SIN ESTATUS DE GRUPO"
                    );

                coincidencia.EstatusObjetivo =
                    NormalizarValorObjetivo(
                        ObtenerValorDataRow(
                            objetivo,
                            "EstatusObjetivo"
                        ),
                        "SIN ESTATUS"
                    );

                /* ============================================================
                   FECHA NACIMIENTO / EDAD
                   ============================================================ */

                string fechaNacimientoTexto =
                    ObtenerValorDataRow(
                        objetivo,
                        "date_fecha_nacimiento"
                    );

                DateTime fechaNacimiento;

                bool fechaValida =
                    DateTime.TryParseExact(
                        fechaNacimientoTexto,
                        "dd/MM/yyyy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out fechaNacimiento
                    );

                if (fechaValida &&
                    fechaNacimiento.Year >= 1900 &&
                    fechaNacimiento <= DateTime.Today)
                {

                    coincidencia.FechaNacimiento =
                        fechaNacimiento;

                    coincidencia.Edad =
                        CalcularEdad(
                            fechaNacimiento
                        );
                }
                else
                {
                    coincidencia.FechaNacimiento =
                        null;

                    coincidencia.Edad =
                        0;
                }

                /* ============================================================
                   DATOS NO DISPONIBLES EN ESTE SP
                   ============================================================ */

                coincidencia.MunicipioClave =
                    "";

                coincidencia.Municipio =
                    "SIN INFORMACIÓN";

                coincidencia.Sexo =
                    "SIN INFORMACIÓN";

                coincidencia.Expediente =
                    "SIN INFORMACIÓN";

                /* ============================================================
                   FOTO
                   ============================================================ */

                string foto =
                    ObtenerValorDataRow(
                        objetivo,
                        "nvarchar_foto"
                    );

                coincidencia.FotoUrl =
                    ObtenerFotoObjetivo(
                        foto
                    );
            }
        }



        private static string ObtenerFotoObjetivo(string foto)
        {
            if (string.IsNullOrWhiteSpace(foto) ||
                foto.Equals(
                    "SIN FOTO",
                    StringComparison.OrdinalIgnoreCase
                ))
            {
                return "~/Content/imagenes/Nodisponible.jpg";
            }

            foto = foto.Trim();

            if (foto.StartsWith(
                "data:image",
                StringComparison.OrdinalIgnoreCase
            ))
            {
                return foto;
            }

            if (foto.StartsWith("~/"))
            {
                return foto;
            }

            if (foto.StartsWith(
                "/Content/",
                StringComparison.OrdinalIgnoreCase
            ))
            {
                return "~" + foto;
            }

            foto =
                foto
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Replace(" ", "");

            return
                "data:image/jpeg;base64," +
                foto;
        }
        private static string ObtenerValorDataRow(DataRow row, string columna)
        {
            if (row == null ||
                row.Table == null ||
                !row.Table.Columns.Contains(columna) ||
                row[columna] == DBNull.Value)
            {

                return "";
            }

            return Convert.ToString(
                row[columna]
            );
        }
        private static string NormalizarValorObjetivo(string valor, string valorSinInformacion)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return "SIN INFORMACIÓN";
            }

            valor = valor.Trim();

            if (valor.Equals(
                valorSinInformacion,
                StringComparison.OrdinalIgnoreCase
            ))
            {
                return "SIN INFORMACIÓN";
            }

            return valor;
        }


        private void EnriquecerCoincidenciasC5(List<CoincidenciaResultadoViewModel> coincidencias)
        {
            if (coincidencias == null || coincidencias.Count == 0)
            {
                return;
            }

            /*
             * Fuente 1:
             *
             * IdPersona =
             * Filiacion_Municipios.dbo.tb_DETENIDO_C5.IDDETENIDO
             */
            List<CoincidenciaResultadoViewModel> coincidenciasFuente1 =
                coincidencias
                    .Where(x =>
                        x.IdTbFuente == 1 &&
                        x.IdPersona > 0
                    )
                    .ToList();

            if (coincidenciasFuente1.Count == 0)
            {
                return;
            }

            foreach (CoincidenciaResultadoViewModel coincidencia in coincidenciasFuente1)
            {
                sp_BuscarDetenido_Result detenido =
                    _filiacionMunicipalService
                        .GetInfoDetenido(
                            coincidencia.IdPersona
                        );

                if (detenido == null)
                {
                    continue;
                }

                /* ============================================================
                   NOMBRE
                   ============================================================ */

                coincidencia.NombreCompleto =
                    ValorOTextoPredeterminado(
                        detenido.Nombre
                    );

                /* ============================================================
                   ALIAS
                   ============================================================ */

                coincidencia.Alias =
                    ValorOTextoPredeterminado(
                        detenido.ALIAS
                    );

                /* ============================================================
                   FOLIO
                   ============================================================ */

                coincidencia.Folio =
                    ValorOTextoPredeterminado(
                        detenido.Folio
                    );

                /* ============================================================
                   MUNICIPIO
                   ============================================================ */

                coincidencia.Municipio =
                    ValorOTextoPredeterminado(
                        detenido.MunicipioDetencion
                    );

                coincidencia.MunicipioClave =
                    string.IsNullOrWhiteSpace(
                        detenido.SiglasMunicipio
                    )
                        ? ""
                        : detenido.SiglasMunicipio.Trim();

                /* ============================================================
                   EDAD
                   ============================================================ */

                coincidencia.Edad =
                    Convert.ToInt32(
                        detenido.EDAD
                    );

                /* ============================================================
                   SEXO
                   ============================================================ */

                coincidencia.Sexo =
                    NormalizarSexo(
                        detenido.SEXO
                    );

                /* ============================================================
                   ID DETENCIÓN
                   ============================================================ */

                coincidencia.Expediente =
                    detenido.IDDETENCION > 0
                        ? "DETENCIÓN " +
                          detenido.IDDETENCION
                        : "SIN INFORMACIÓN";

                /* ============================================================
                   FOTO
                   ============================================================ */

                coincidencia.FotoUrl =
                    detenido.IDDETENIDO > 0
                        ? "~/SIC/FotoDetenidoC5?idDetenido=" +
                          detenido.IDDETENIDO
                        : "~/Content/imagenes/Nodisponible.jpg";
            }
        }


        public async Task<DetalleObjetivoApiDto> ObtenerDetalleObjetivoAsync(int idObjetivo)
        {
            if (idObjetivo <= 0)
            {
                throw new ArgumentException(
                    "El ID del objetivo no es válido."
                );
            }

            ValidarConfiguracionApi();

            string url =
                ConstruirUrlRecursoApi(
                    "objetivos/" +
                    idObjetivo +
                    "/detalle"
                );

            using (var solicitud =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    url
                ))
            {
                solicitud.Headers.Add(
                    "X-API-TOKEN",
                    _tokenApi
                );

                HttpResponseMessage respuesta =
                    await ClienteHttp.SendAsync(
                        solicitud
                    );

                string contenido =
                    await respuesta.Content
                        .ReadAsStringAsync();

                /*
                 * El objetivo no existe.
                 */
                if (respuesta.StatusCode ==
                    System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                if (!respuesta.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        ObtenerMensajeErrorApi(
                            contenido,
                            respuesta.StatusCode
                        )
                    );
                }

                DetalleObjetivoApiDto detalle =
                    JsonConvert
                        .DeserializeObject<DetalleObjetivoApiDto>(
                            contenido
                        );

                if (detalle == null)
                {
                    throw new InvalidOperationException(
                        "La API regresó un detalle vacío para el objetivo."
                    );
                }

                return detalle;
            }
        }


        private string ConstruirUrlRecursoApi(string recurso)
        {
            string urlBase =
                (_urlApi ?? "")
                    .Trim()
                    .TrimEnd('/');

            const string sufijoBuscar =
                "/buscar";

            /*
             * BiometriaApiUrl actualmente termina en:
             *
             * /api/busqueda-biometrica/buscar
             *
             * Quitamos únicamente /buscar para reutilizar
             * la misma configuración con otros recursos.
             */
            if (urlBase.EndsWith(
                sufijoBuscar,
                StringComparison.OrdinalIgnoreCase
            ))
            {
                urlBase =
                    urlBase.Substring(
                        0,
                        urlBase.Length -
                        sufijoBuscar.Length
                    );
            }

            recurso =
                (recurso ?? "")
                    .Trim()
                    .TrimStart('/');

            return
                urlBase +
                "/" +
                recurso;
        }

        public async Task<byte[]> ObtenerFotoObjetivoAsync(int idObjetivo)
        {
            if (idObjetivo <= 0)
            {
                return null;
            }

            ValidarConfiguracionApi();

            string url =
                ConstruirUrlRecursoApi(
                    "objetivos/" +
                    idObjetivo +
                    "/foto"
                );

            using (var solicitud =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    url
                ))
            {
                solicitud.Headers.Add(
                    "X-API-TOKEN",
                    _tokenApi
                );

                HttpResponseMessage respuesta =
                    await ClienteHttp.SendAsync(
                        solicitud
                    );

                if (respuesta.StatusCode ==
                    System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                if (!respuesta.IsSuccessStatusCode)
                {
                    string contenido =
                        await respuesta.Content
                            .ReadAsStringAsync();

                    throw new InvalidOperationException(
                        ObtenerMensajeErrorApi(
                            contenido,
                            respuesta.StatusCode
                        )
                    );
                }

                return await respuesta.Content
                    .ReadAsByteArrayAsync();
            }
        }

        public async Task<DetalleFiscaliaWebApiDto> ObtenerDetalleFiscaliaWebAsync(
    int idTbFuente,
    int idPersona)
        {
            if (idPersona <= 0)
            {
                throw new ArgumentException(
                    "El ID de la persona no es válido."
                );
            }

            if (
                idTbFuente != 2 &&
                idTbFuente != 7 &&
                idTbFuente != 8
            )
            {
                throw new ArgumentException(
                    "La fuente indicada no pertenece a fiscalia_web."
                );
            }

            ValidarConfiguracionApi();

            string url =
                ConstruirUrlRecursoApi(
                    "fiscalia-web/" +
                    idTbFuente +
                    "/" +
                    idPersona +
                    "/detalle"
                );

            using (var solicitud =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    url
                ))
            {
                solicitud.Headers.Add(
                    "X-API-TOKEN",
                    _tokenApi
                );

                HttpResponseMessage respuesta =
                    await ClienteHttp.SendAsync(
                        solicitud
                    );

                string contenido =
                    await respuesta.Content
                        .ReadAsStringAsync();

                if (respuesta.StatusCode ==
                    System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                if (!respuesta.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        ObtenerMensajeErrorApi(
                            contenido,
                            respuesta.StatusCode
                        )
                    );
                }

                DetalleFiscaliaWebApiDto detalle =
                    JsonConvert
                        .DeserializeObject<DetalleFiscaliaWebApiDto>(
                            contenido
                        );

                if (detalle == null)
                {
                    throw new InvalidOperationException(
                        "La API regresó un detalle vacío."
                    );
                }
                if (
                    detalle.TieneFoto &&
                    !string.IsNullOrWhiteSpace(
                        detalle.FotoUrl
                    )
                )
                {
                    detalle.FotoUrl =
                        NormalizarFotoFiscaliaWeb(
                            detalle.FotoUrl
                        );
                }
                return detalle;
            }
        }


        private async Task EnriquecerCoincidenciasFiscaliaWebAsync(
    List<CoincidenciaResultadoViewModel> coincidencias)
        {
            if (
                coincidencias == null ||
                coincidencias.Count == 0
            )
            {
                return;
            }

            /*
             * Fuentes fiscalia_web:
             *
             * 2 = CAPEA / FEMDLP
             * 7 = Alerta Amber
             * 8 = Protocolo Alba
             */
            List<CoincidenciaResultadoViewModel> coincidenciasFiscaliaWeb =
                coincidencias
                    .Where(x =>
                        x.IdPersona > 0 &&
                        (
                            x.IdTbFuente == 2 ||
                            x.IdTbFuente == 7 ||
                            x.IdTbFuente == 8
                        )
                    )
                    .ToList();

            if (coincidenciasFiscaliaWeb.Count == 0)
            {
                return;
            }

            foreach (
                CoincidenciaResultadoViewModel coincidencia
                in coincidenciasFiscaliaWeb
            )
            {
                try
                {
                    DetalleFiscaliaWebApiDto detalle =
                        await ObtenerDetalleFiscaliaWebAsync(
                            coincidencia.IdTbFuente,
                            coincidencia.IdPersona
                        );

                    if (detalle == null)
                    {
                        continue;
                    }


                    /*
                     * ============================================================
                     * NOMBRE
                     * ============================================================
                     */

                    coincidencia.NombreCompleto =
                        !string.IsNullOrWhiteSpace(
                            detalle.NombreCompleto
                        )
                            ? detalle.NombreCompleto.Trim()
                            : coincidencia.NombreCompleto;


                    /*
                     * ============================================================
                     * ALIAS
                     * ============================================================
                     *
                     * Estas fuentes actualmente no manejan un alias
                     * dentro del DTO de detalle.
                     *
                     * Si la búsqueda nominal trajo AliasCoincidente,
                     * ya quedó colocado previamente.
                     */
                    if (string.IsNullOrWhiteSpace(coincidencia.Alias))
                    {
                        coincidencia.Alias =
                            "SIN INFORMACIÓN";
                    }


                    /*
                     * ============================================================
                     * EDAD
                     * ============================================================
                     */

                    if (detalle.Edad.HasValue)
                    {
                        coincidencia.Edad =
                            detalle.Edad.Value;
                    }
                    else if (
                        detalle.FechaNacimiento.HasValue &&
                        detalle.FechaNacimiento.Value.Year >= 1900 &&
                        detalle.FechaNacimiento.Value.Date <= DateTime.Today
                    )
                    {
                        coincidencia.Edad =
                            CalcularEdad(
                                detalle.FechaNacimiento.Value
                            );
                    }
                    else
                    {
                        coincidencia.Edad =
                            0;
                    }

                    coincidencia.FechaNacimiento =
                        detalle.FechaNacimiento;


                    /*
                     * ============================================================
                     * SEXO
                     * ============================================================
                     */

                    coincidencia.Sexo =
                        !string.IsNullOrWhiteSpace(
                            detalle.Sexo
                        )
                            ? detalle.Sexo.Trim()
                            : "SIN INFORMACIÓN";


                    /*
                     * ============================================================
                     * FOTO
                     * ============================================================
                     */

                    coincidencia.FotoUrl =
                    detalle.TieneFoto
                        ? NormalizarFotoFiscaliaWeb(
                            detalle.FotoUrl
                        )
                        : "~/Content/imagenes/Nodisponible.jpg";


                    /*
                     * ============================================================
                     * IDENTIFICADOR
                     * ============================================================
                     *
                     * No inventamos un folio porque las tres fuentes
                     * manejan identificadores distintos.
                     */
                    coincidencia.Folio =
                        "ID " +
                        coincidencia.IdPersona;


                    /*
                     * ============================================================
                     * FECHA DE REGISTRO
                     * ============================================================
                     */

                    coincidencia.FechaRegistro =
                        detalle.FechaAlta
                        ?? DateTime.MinValue;


                    /*
                     * El detalle completo (lugar de ausencia,
                     * características físicas, señas particulares,
                     * resumen de hechos, etc.) NO se mete aquí.
                     *
                     * Eso se muestra mediante
                     * DetalleFiscaliaWebCoincidenciaPartial.
                     */
                }
                catch
                {
                    /*
                     * Si una fuente no puede enriquecerse,
                     * no cancelamos toda la búsqueda.
                     *
                     * Conservamos el resultado básico que ya
                     * regresó la API biométrica.
                     */
                    continue;
                }
            }
        }

        public async Task<DetalleC5ApiDto> ObtenerDetalleC5Async(int idDetenido)
        {
            if (idDetenido <= 0)
            {
                throw new ArgumentException(
                    "El ID del detenido no es válido."
                );
            }

            ValidarConfiguracionApi();

            string url =
                ConstruirUrlRecursoApi(
                    "c5/" +
                    idDetenido +
                    "/detalle"
                );

            using (var solicitud =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    url
                ))
            {
                solicitud.Headers.Add(
                    "X-API-TOKEN",
                    _tokenApi
                );

                HttpResponseMessage respuesta =
                    await ClienteHttp.SendAsync(
                        solicitud
                    );

                string contenido =
                    await respuesta.Content
                        .ReadAsStringAsync();

                if (respuesta.StatusCode ==
                    System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                if (!respuesta.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        ObtenerMensajeErrorApi(
                            contenido,
                            respuesta.StatusCode
                        )
                    );
                }

                DetalleC5ApiDto detalle =
                    JsonConvert
                        .DeserializeObject<DetalleC5ApiDto>(
                            contenido
                        );

                if (detalle == null)
                {
                    throw new InvalidOperationException(
                        "La API regresó un detalle C5 vacío."
                    );
                }

                return detalle;
            }
        }


        public async Task<DetalleFGEADetenidoApiDto>
    ObtenerDetalleFGEADetenidoAsync(int clavePerso)
        {
            if (clavePerso <= 0)
            {
                throw new ArgumentException(
                    "La CLAVE_PERSO no es válida."
                );
            }

            ValidarConfiguracionApi();

            string url =
                ConstruirUrlRecursoApi(
                    "fgea-detenidos/" +
                    clavePerso +
                    "/detalle"
                );

            using (var solicitud =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    url
                ))
            {
                solicitud.Headers.Add(
                    "X-API-TOKEN",
                    _tokenApi
                );

                HttpResponseMessage respuesta =
                    await ClienteHttp.SendAsync(
                        solicitud
                    );

                string contenido =
                    await respuesta.Content
                        .ReadAsStringAsync();

                if (
                    respuesta.StatusCode ==
                    System.Net.HttpStatusCode.NotFound
                )
                {
                    return null;
                }

                if (!respuesta.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        ObtenerMensajeErrorApi(
                            contenido,
                            respuesta.StatusCode
                        )
                    );
                }

                DetalleFGEADetenidoApiDto detalle =
                    JsonConvert
                        .DeserializeObject<DetalleFGEADetenidoApiDto>(
                            contenido
                        );

                if (detalle == null)
                {
                    throw new InvalidOperationException(
                        "La API regresó un detalle FGEA Detenidos vacío."
                    );
                }

                return detalle;
            }
        }


        private async Task EnriquecerAliasObjetivosPrioritariosAsync(
    List<CoincidenciaResultadoViewModel> coincidencias)
        {
            if (
                coincidencias == null ||
                coincidencias.Count == 0
            )
            {
                return;
            }

            List<CoincidenciaResultadoViewModel> objetivos =
                coincidencias
                    .Where(x =>
                        x.IdTbFuente == 5 &&
                        x.IdPersona > 0
                    )
                    .ToList();

            if (objetivos.Count == 0)
            {
                return;
            }

            foreach (
                CoincidenciaResultadoViewModel coincidencia
                in objetivos
            )
            {
                try
                {
                    DetalleObjetivoApiDto detalle =
                        await ObtenerDetalleObjetivoAsync(
                            coincidencia.IdPersona
                        );

                    if (
                        detalle == null ||
                        detalle.Alias == null ||
                        detalle.Alias.Count == 0
                    )
                    {
                        continue;
                    }

                    List<string> aliasReales =
                        detalle.Alias
                            .Where(x =>
                                !string.IsNullOrWhiteSpace(x)
                            )
                            .Select(x =>
                                x.Trim()
                            )
                            .Distinct(
                                StringComparer.OrdinalIgnoreCase
                            )
                            .ToList();

                    if (aliasReales.Count == 0)
                    {
                        continue;
                    }

                    coincidencia.Alias =
                        string.Join(
                            ", ",
                            aliasReales
                        );
                }
                catch
                {
                    /*
                     * Si falla el detalle del objetivo,
                     * no tumbamos toda la búsqueda.
                     *
                     * Conservamos el AliasCoincidente que ya
                     * pudiera haber regresado la API biométrica.
                     */
                    continue;
                }
            }
        }


        public async Task<DetallePersonaInteresApiDto> ObtenerDetallePersonaInteresAsync(int idPersona)
        {
            if (idPersona <= 0)
            {
                throw new ArgumentException(
                    "El ID de la persona de interés no es válido."
                );
            }


            ValidarConfiguracionApi();


            string url =
                ConstruirUrlRecursoApi(
                    "personas-interes/" +
                    idPersona +
                    "/detalle"
                );


            using (
                var solicitud =
                    new HttpRequestMessage(
                        HttpMethod.Get,
                        url
                    )
            )
            {
                solicitud.Headers.Add(
                    "X-API-TOKEN",
                    _tokenApi
                );


                HttpResponseMessage respuesta =
                    await ClienteHttp.SendAsync(
                        solicitud
                    );


                string contenido =
                    await respuesta.Content
                        .ReadAsStringAsync();


                if (
                    respuesta.StatusCode ==
                    System.Net.HttpStatusCode.NotFound
                )
                {
                    return null;
                }


                if (!respuesta.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        ObtenerMensajeErrorApi(
                            contenido,
                            respuesta.StatusCode
                        )
                    );
                }


                DetallePersonaInteresApiDto detalle =
                    JsonConvert
                        .DeserializeObject
                            <DetallePersonaInteresApiDto>(
                                contenido
                            );


                if (detalle == null)
                {
                    throw new InvalidOperationException(
                        "La API regresó un detalle vacío para la persona de interés."
                    );
                }


                return detalle;
            }
        }

        private async Task EnriquecerCoincidenciasPersonasInteresAsync(List<CoincidenciaResultadoViewModel> coincidencias)
        {
            if (
                coincidencias == null ||
                coincidencias.Count == 0
            )
            {
                return;
            }


            List<CoincidenciaResultadoViewModel> personasInteres =
                coincidencias
                    .Where(x =>
                        x.IdTbFuente == 3 &&
                        x.IdPersona > 0
                    )
                    .ToList();


            if (personasInteres.Count == 0)
            {
                return;
            }


            foreach (
                CoincidenciaResultadoViewModel coincidencia
                in personasInteres
            )
            {
                try
                {
                    DetallePersonaInteresApiDto detalle =
                        await ObtenerDetallePersonaInteresAsync(
                            coincidencia.IdPersona
                        );


                    if (detalle == null)
                    {
                        continue;
                    }


                    if (
                        !string.IsNullOrWhiteSpace(
                            detalle.NombreCompleto
                        )
                    )
                    {
                        coincidencia.NombreCompleto =
                            detalle.NombreCompleto.Trim();
                    }


                    coincidencia.Folio =
                        "ID " +
                        detalle.IdPersona;


                    coincidencia.Expediente =
                        "SIN INFORMACIÓN";


                    coincidencia.Edad =
                        detalle.Edad.HasValue
                            ? detalle.Edad.Value
                            : 0;


                    coincidencia.Sexo =
                        string.IsNullOrWhiteSpace(
                            detalle.Sexo
                        )
                            ? "SIN INFORMACIÓN"
                            : detalle.Sexo.Trim();


                    /*
                     * Personas de Interés actualmente no maneja
                     * municipio dentro de tb_Persona.
                     */
                    coincidencia.MunicipioClave =
                        "";

                    coincidencia.Municipio =
                        "SIN INFORMACIÓN";


                    coincidencia.FechaRegistro =
                        detalle.FechaRegistro
                        ??
                        DateTime.MinValue;


                    /*
                     * Ya tienes esta acción funcionando
                     * en PersonasInteresController.
                     */
                    coincidencia.FotoUrl =
                        "~/PersonasInteres/VerFotoPrincipalPersonaInteres?idPersona=" +
                        detalle.IdPersona;
                }
                catch
                {
                    /*
                     * Si falla el enriquecimiento de una persona,
                     * no tiramos toda la búsqueda biométrica.
                     */
                }
            }
        }


        private static string NormalizarFotoFiscaliaWeb(string fotoUrl)
        {
            if (string.IsNullOrWhiteSpace(fotoUrl))
            {
                return "~/Content/imagenes/Nodisponible.jpg";
            }

            string url =
                fotoUrl
                    .Trim()
                    .Replace("\\", "/");

            const string dominioSinWwwHttps =
                "https://fiscalia-aguascalientes.gob.mx/";

            const string dominioSinWwwHttp =
                "http://fiscalia-aguascalientes.gob.mx/";

            const string dominioCorrecto =
                "https://www.fiscalia-aguascalientes.gob.mx/";


            /*
             * ============================================================
             * URL HTTPS SIN WWW
             * ============================================================
             */

            if (
                url.StartsWith(
                    dominioSinWwwHttps,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return
                    dominioCorrecto +
                    url.Substring(
                        dominioSinWwwHttps.Length
                    );
            }


            /*
             * ============================================================
             * URL HTTP SIN WWW
             * ============================================================
             */

            if (
                url.StartsWith(
                    dominioSinWwwHttp,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return
                    dominioCorrecto +
                    url.Substring(
                        dominioSinWwwHttp.Length
                    );
            }


            /*
             * ============================================================
             * YA ES URL ABSOLUTA
             * ============================================================
             */

            if (
                Uri.IsWellFormedUriString(
                    url,
                    UriKind.Absolute
                )
            )
            {
                return url;
            }


            /*
             * ============================================================
             * RUTA RELATIVA
             * ============================================================
             *
             * Ejemplo:
             *
             * /images/alerta-amber/alertas/foto.jpeg
             * ============================================================
             */

            return
                dominioCorrecto +
                url.TrimStart('/');
        }

    }
}