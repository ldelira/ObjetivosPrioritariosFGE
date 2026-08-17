using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Objetivos_Prioritarios.Helpers
{
    public class ResultadoEmbeddingApi
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Operacion { get; set; }
        public int IdPersona { get; set; }
        public int IdTbFuente { get; set; }
        public int Dimensiones { get; set; }
    }


    public static class BiometriaApiHelper
    {
        public static ResultadoEmbeddingApi GuardarOActualizarEmbedding(
            int idPersona,
            int idTbFuente,
            byte[] fotografia,
            string nombreArchivo,
            string contentType)
        {
            try
            {
                if (idPersona <= 0)
                {
                    return new ResultadoEmbeddingApi
                    {
                        Success = false,
                        Message = "El IdPersona no es válido."
                    };
                }


                if (fotografia == null || fotografia.Length == 0)
                {
                    return new ResultadoEmbeddingApi
                    {
                        Success = false,
                        Message = "La fotografía está vacía."
                    };
                }


                string url =
                    ConfigurationManager.AppSettings[
                        "BiometriaApiUrlEmbedding"
                    ];

                string token =
                    ConfigurationManager.AppSettings[
                        "BiometriaApiToken"
                    ];


                if (string.IsNullOrWhiteSpace(url))
                {
                    return new ResultadoEmbeddingApi
                    {
                        Success = false,
                        Message = "No está configurado Biometria.ApiEmbeddingUrl."
                    };
                }


                if (string.IsNullOrWhiteSpace(token))
                {
                    return new ResultadoEmbeddingApi
                    {
                        Success = false,
                        Message = "No está configurado Biometria.ApiToken."
                    };
                }


                if (string.IsNullOrWhiteSpace(nombreArchivo))
                {
                    nombreArchivo = "fotografia.jpg";
                }


                if (string.IsNullOrWhiteSpace(contentType))
                {
                    contentType = "image/jpeg";
                }


                using (var cliente = new HttpClient())
                {
                    cliente.Timeout =
                        TimeSpan.FromSeconds(60);

                    cliente
                        .DefaultRequestHeaders
                        .Add(
                            "X-API-TOKEN",
                            token
                        );


                    using (
                        var contenido =
                            new MultipartFormDataContent()
                    )
                    {
                        contenido.Add(
                            new StringContent(
                                idPersona.ToString()
                            ),
                            "idPersona"
                        );


                        contenido.Add(
                            new StringContent(
                                idTbFuente.ToString()
                            ),
                            "idTbFuente"
                        );


                        var contenidoFoto =
                            new ByteArrayContent(
                                fotografia
                            );


                        contenidoFoto
                            .Headers
                            .ContentType =
                                new MediaTypeHeaderValue(
                                    contentType
                                );


                        contenido.Add(
                            contenidoFoto,
                            "fotografia",
                            Path.GetFileName(
                                nombreArchivo
                            )
                        );


                        HttpResponseMessage respuesta =
                            cliente
                                .PostAsync(
                                    url,
                                    contenido
                                )
                                .GetAwaiter()
                                .GetResult();


                        string json =
                            respuesta
                                .Content
                                .ReadAsStringAsync()
                                .GetAwaiter()
                                .GetResult();


                        ResultadoEmbeddingApi resultado = null;


                        if (!string.IsNullOrWhiteSpace(json))
                        {
                            try
                            {
                                resultado =
                                    JsonConvert
                                        .DeserializeObject<
                                            ResultadoEmbeddingApi
                                        >(
                                            json
                                        );
                            }
                            catch
                            {
                                // Si la API regresó algo que no es JSON,
                                // se maneja abajo.
                            }
                        }


                        if (!respuesta.IsSuccessStatusCode)
                        {
                            return new ResultadoEmbeddingApi
                            {
                                Success = false,

                                Message =
                                    resultado != null &&
                                    !string.IsNullOrWhiteSpace(
                                        resultado.Message
                                    )
                                        ? resultado.Message
                                        : "La API biométrica respondió con error: "
                                          + respuesta.StatusCode
                            };
                        }


                        return
                            resultado
                            ??
                            new ResultadoEmbeddingApi
                            {
                                Success = true,
                                Message = "Embedding procesado correctamente."
                            };
                    }
                }
            }
            catch (Exception ex)
            {
                Exception errorReal =
                    ex.GetBaseException();


                return new ResultadoEmbeddingApi
                {
                    Success = false,

                    Message =
                        "No fue posible comunicarse con la API biométrica: "
                        + errorReal.Message
                };
            }
        }


        public static ResultadoEmbeddingApi GuardarOActualizarEmbeddingDesdeBase64(
            int idPersona,
            int idTbFuente,
            string fotoBase64)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fotoBase64))
                {
                    return new ResultadoEmbeddingApi
                    {
                        Success = false,
                        Message = "La fotografía Base64 está vacía."
                    };
                }


                string contenidoBase64 =
                    fotoBase64.Trim();

                string contentType =
                    "image/jpeg";

                string nombreArchivo =
                    "fotografia.jpg";


                /*
                 * También soportamos:
                 *
                 * data:image/png;base64,AAAA...
                 */
                if (
                    contenidoBase64.StartsWith(
                        "data:",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    int posicionComa =
                        contenidoBase64.IndexOf(',');


                    if (posicionComa <= 0)
                    {
                        return new ResultadoEmbeddingApi
                        {
                            Success = false,
                            Message = "El formato Base64 de la fotografía no es válido."
                        };
                    }


                    string encabezado =
                        contenidoBase64.Substring(
                            5,
                            posicionComa - 5
                        );


                    int posicionPuntoComa =
                        encabezado.IndexOf(';');


                    if (posicionPuntoComa > 0)
                    {
                        contentType =
                            encabezado.Substring(
                                0,
                                posicionPuntoComa
                            );
                    }


                    contenidoBase64 =
                        contenidoBase64.Substring(
                            posicionComa + 1
                        );


                    switch (
                        contentType.ToLowerInvariant()
                    )
                    {
                        case "image/png":
                            nombreArchivo =
                                "fotografia.png";
                            break;

                        case "image/bmp":
                            nombreArchivo =
                                "fotografia.bmp";
                            break;

                        case "image/webp":
                            nombreArchivo =
                                "fotografia.webp";
                            break;

                        default:
                            nombreArchivo =
                                "fotografia.jpg";
                            break;
                    }
                }


                byte[] bytes =
                    Convert.FromBase64String(
                        contenidoBase64
                    );


                return GuardarOActualizarEmbedding(
                    idPersona,
                    idTbFuente,
                    bytes,
                    nombreArchivo,
                    contentType
                );
            }
            catch (FormatException)
            {
                return new ResultadoEmbeddingApi
                {
                    Success = false,
                    Message = "La cadena de fotografía no contiene un Base64 válido."
                };
            }
            catch (Exception ex)
            {
                return new ResultadoEmbeddingApi
                {
                    Success = false,
                    Message = ex.GetBaseException().Message
                };
            }
        }
    }
}