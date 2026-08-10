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
                        70,

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

        public async Task<ResultadosCoincidenciasViewModel>
    BuscarCoincidenciasAsync(
        BusquedaCoincidenciasViewModel filtros)
        {
            if (filtros == null)
            {
                throw new ArgumentNullException(
                    nameof(filtros)
                );
            }

            ValidarArchivo(
                filtros.Fotografia,
                "fotografía"
            );

            ValidarArchivo(
                filtros.Huella,
                "huella"
            );

            bool tieneNombre =
            !string.IsNullOrWhiteSpace(
                filtros.NombreBusqueda
            );

            bool tieneAlias =
                !string.IsNullOrWhiteSpace(
                    filtros.AliasBusqueda
                );

            bool tieneFotografia =
                filtros.Fotografia != null &&
                filtros.Fotografia.ContentLength > 0;

            bool tieneHuella =
                filtros.Huella != null &&
                filtros.Huella.ContentLength > 0;

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

            using (var formulario =
                new MultipartFormDataContent())
            {
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

                if (tieneHuella)
                {
                    ByteArrayContent contenidoHuella =
                        CrearContenidoArchivo(
                            filtros.Huella
                        );

                    formulario.Add(
                        contenidoHuella,
                        "Huella",
                        ObtenerNombreArchivo(
                            filtros.Huella,
                            "huella.jpg"
                        )
                    );
                }

                using (var solicitud =
                    new HttpRequestMessage(
                        HttpMethod.Post,
                        _urlApi
                    ))
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
                        await respuesta.Content
                            .ReadAsStringAsync();

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
                        JsonConvert.DeserializeObject
                            <ApiBusquedaBiometricaResponse>(
                                contenidoRespuesta
                            );

                    if (resultadoApi == null)
                    {
                        throw new InvalidOperationException(
                            "La API biométrica regresó una respuesta vacía."
                        );
                    }

                    return ConvertirResultadoApi(
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

        private ResultadosCoincidenciasViewModel ConvertirResultadoApi(
    ApiBusquedaBiometricaResponse resultadoApi,
    bool tieneFotografia,
    bool tieneHuella)
        {
            List<CoincidenciaResultadoViewModel> coincidencias =
                new List<CoincidenciaResultadoViewModel>();

            /*
             * Evitamos errores cuando la API no devuelve
             * una respuesta o la colección viene nula.
             */
            if (
                resultadoApi == null ||
                resultadoApi.Resultados == null
            )
            {
                GuardarCoincidenciasEnSesion(
                    coincidencias
                );

                return new ResultadosCoincidenciasViewModel
                {
                    TieneFotografiaConsulta =
                        tieneFotografia,

                    TieneHuellaConsulta =
                        tieneHuella,

                    Coincidencias =
                        coincidencias,

                    TotalCoincidencias =
                        0,

                    TotalFotografia =
                        0,

                    TotalHuella =
                        0,

                    TotalCombinadas =
                        0,

                    CoincidenciaSeleccionada =
                        null
                };
            }

            int consecutivo =
                1;

            foreach (
                ApiCoincidenciaBiometricaDto item
                in resultadoApi.Resultados
            )
            {
                if (item == null)
                {
                    continue;
                }

                decimal porcentajeFoto =
                    item.SimilitudFoto ?? 0;

                decimal porcentajeHuella =
                    item.SimilitudHuella ?? 0;

                decimal similitudGlobal =
                    Math.Max(
                        porcentajeFoto,
                        porcentajeHuella
                    );

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

         NombreCompleto =
             "PERSONA " +
             item.IdPersona,

         Alias =
             "SIN INFORMACIÓN",

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

         PorcentajeNombre =
             0,

         PorcentajeAlias =
             0,

         PorcentajeTexto =
             0,

         OrigenCoincidenciaTexto =
             "",

         TextoCoincidente =
             "",

         PorcentajeFoto =
             porcentajeFoto,

         PorcentajeHuella =
             porcentajeHuella,

         SimilitudGlobal =
             similitudGlobal,

         FechaRegistro =
             DateTime.MinValue,

         TieneAvisoMandamientos =
             false,

         TotalMandamientos =
             0,

         MandamientosJudiciales =
             new List<
                 MandamientoJudicialViewModel
             >()
     };

                coincidencias.Add(
                    coincidencia
                );

                consecutivo++;
            }

            /*
             * Primero ordenamos por la similitud biométrica.
             */
            coincidencias =
                coincidencias
                    .OrderByDescending(x =>
                        x.SimilitudGlobal
                    )
                    .ThenByDescending(x =>
                        x.PorcentajeFoto
                    )
                    .ThenByDescending(x =>
                        x.PorcentajeHuella
                    )
                    .ToList();

            /*
             * Reasignamos el identificador visual después
             * de ordenar los resultados.
             */
            for (
                int indice = 0;
                indice < coincidencias.Count;
                indice++
            )
            {
                coincidencias[indice].IdCoincidencia =
                    indice + 1;
            }

            /*
             * Aquí se completan los datos reales según
             * la fuente de cada coincidencia.
             *
             * El coordinador debe enriquecer primero la
             * información biométrica y consultar
             * Mandamientos Judiciales al final.
             */
            EnriquecerCoincidenciasPorFuente(
                coincidencias
            );

            /*
             * Guardamos la lista ya enriquecida.
             *
             * Esto permite que al presionar "Ver detalle"
             * se recupere el mismo candidato con su foto,
             * nombre y avisos de Mandamientos.
             */
            GuardarCoincidenciasEnSesion(
                coincidencias
            );

            ResultadosCoincidenciasViewModel resultado =
                new ResultadosCoincidenciasViewModel
                {
                    TieneFotografiaConsulta =
                        tieneFotografia,

                    TieneHuellaConsulta =
                        tieneHuella,

                    Coincidencias =
                        coincidencias,

                    TotalCoincidencias =
                        coincidencias.Count,

                    TotalFotografia =
                        tieneFotografia
                            ? coincidencias.Count(x =>
                                x.PorcentajeFoto > 0
                            )
                            : 0,

                    TotalHuella =
                        tieneHuella
                            ? coincidencias.Count(x =>
                                x.PorcentajeHuella > 0
                            )
                            : 0,

                    TotalCombinadas =
                        tieneFotografia &&
                        tieneHuella
                            ? coincidencias.Count(x =>
                                x.PorcentajeFoto > 0 &&
                                x.PorcentajeHuella > 0
                            )
                            : 0,

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

        private void EnriquecerCoincidenciasDetenidos(
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
             * Fuente 6 = Detenidos FGEA.
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

            /*
             * IdPersona corresponde al ID de Nom_perso
             * almacenado en las tablas biométricas.
             */
            List<int> idsNomPerso =
                coincidenciasFuente6
                    .Select(x => x.IdPersona)
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

            /*
             * Ejecuta el procedimiento almacenado:
             * dbo.SP_SIC_getCoincidenciasDetenidos
             */
            List<SP_SIC_getCoincidenciasDetenidos_Result> detenidos =
                _filiacionMunicipalService
                    .getCoincidenciasDetenidos_Results(
                        idsNomPerso
                    );

            if (
                detenidos == null ||
                detenidos.Count == 0
            )
            {
                return;
            }

            foreach (
                CoincidenciaResultadoViewModel coincidencia
                in coincidenciasFuente6
            )
            {
                /*
                 * El procedimiento puede agrupar varios IDs
                 * de Nom_perso dentro de la misma CLAVE_PERSO.
                 */
                SP_SIC_getCoincidenciasDetenidos_Result detenido =
                    detenidos.FirstOrDefault(x =>
                        ContieneIdNomPerso(
                            x.IdsNomPersoOrigenAlerta,
                            coincidencia.IdPersona
                        )
                        ||
                        ContieneIdNomPerso(
                            x.IdsNomPerso,
                            coincidencia.IdPersona
                        )
                    );

                if (detenido == null)
                {
                    continue;
                }

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

                /*
                 * FEC_NAC es DateTime, no DateTime?.
                 */
                DateTime fechaNacimiento =
                    detenido.FEC_NAC;

                coincidencia.Edad =
                    fechaNacimiento.Year >= 1900 &&
                    fechaNacimiento <= DateTime.Today
                        ? CalcularEdad(
                            fechaNacimiento
                        )
                        : 0;

                coincidencia.Sexo =
                    NormalizarSexo(
                        detenido.SEXO
                    );

                /*
                 * Actualmente el SP no devuelve municipio.
                 */
                coincidencia.MunicipioClave =
                    "";

                coincidencia.Municipio =
                    "SIN INFORMACIÓN";

                /*
                 * Temporalmente utilizamos Expediente para
                 * mostrar el último delito registrado.
                 */
                coincidencia.Expediente =
                    ValorOTextoPredeterminado(
                        detenido.UltimoDelito
                    );

                coincidencia.FechaRegistro =
                    detenido.FechaUltimoDelito
                    ?? DateTime.MinValue;

                int clavePerso =
                    Convert.ToInt32(
                        detenido.CLAVE_PERSO
                    );

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

            /*
             * Aseguramos que todas las coincidencias
             * tengan el nombre de su fuente.
             */
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
             * Fuente 6: Detenidos FGEA.
             * Este enriquecimiento ya funciona.
             */
            EnriquecerCoincidenciasDetenidos(
                coincidencias
            );

            /*
             * Próximos enriquecimientos:
             *
             * Fuente 1:
             * EnriquecerCoincidenciasC5(coincidencias);
             *
             * Fuente 5:
             * EnriquecerCoincidenciasObjetivosPrioritarios(coincidencias);
             *
             * Fuentes 2, 7 y 8:
             * EnriquecerCoincidenciasCapea(coincidencias);
             */

            /*
             * Mandamientos siempre se consulta al final,
             * después de obtener los nombres oficiales.
             */
            AgregarAvisosMandamientos(
                coincidencias
            );
        }

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

        private List<MandamientoJudicialViewModel>
    BuscarMandamientosPorNombre(
        string nombreCompleto,
        double umbral = 85)
        {
            List<MandamientoJudicialViewModel> resultado =
                new List<MandamientoJudicialViewModel>();

            string nombrePrincipal =
                ObtenerNombrePrincipal(
                    nombreCompleto
                );

            if (
                string.IsNullOrWhiteSpace(nombrePrincipal) ||
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

            foreach (DataRow fila in candidatos.Rows)
            {
                string nombreCandidato =
                    ObtenerTextoMandamiento(
                        fila,
                        "Nombre"
                    );

                if (string.IsNullOrWhiteSpace(nombreCandidato))
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

                MandamientoJudicialViewModel mandamiento =
                    new MandamientoJudicialViewModel
                    {
                        IdNombreMandamiento =
                            ObtenerEnteroMandamiento(
                                fila,
                                "IdOrigenAlerta"
                            ),

                        IdMandamiento =
                            ObtenerEnteroMandamiento(
                                fila,
                                "IdMandamiento"
                            ),

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
                                Math.Round(similitud)
                            )
                    };

                resultado.Add(mandamiento);
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


        private static string ObtenerNombreFuente(
    int idTbFuente)
        {
            switch (idTbFuente)
            {
                case 1:
                    return "C5 - Detenidos";

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
    }
}