using Antlr.Runtime.Misc;
using FiliacionMunicipal.Models;
using FiliacionMunicipal.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Net.NetworkInformation;
using System.Web;
using System.Xml.Linq;
using Web_SIPAEIC.ControllerServices;

namespace FiliacionMunicipal.ControllerServices
{
    public class HistorialService : BaseService
    {
        public List<HistorialVM> ObtenerHistorial(string nombre,int? mun, string fechaInicio, string fechaFin)
        {
            if (mun == 1)
            {
                return db.Database.SqlQuery<HistorialVM>(
                    "EXEC sp_HistorialDetenido @Nombre, @Mun, @FechaInicio, @FechaFin",
                    new SqlParameter("@Nombre", (object)nombre ?? DBNull.Value),
                    new SqlParameter("@Mun", DBNull.Value),
                    new SqlParameter("@FechaInicio", string.IsNullOrEmpty(fechaInicio) ? (object)DBNull.Value : fechaInicio),
                    new SqlParameter("@FechaFin", string.IsNullOrEmpty(fechaFin) ? (object)DBNull.Value : fechaFin)
                ).ToList();
            }else
            {
                return db.Database.SqlQuery<HistorialVM>(
                        "EXEC sp_HistorialDetenido @Nombre, @Mun, @FechaInicio, @FechaFin",
                        new SqlParameter("@Nombre", (object)nombre ?? DBNull.Value),
                        new SqlParameter("@Mun", (object)mun ?? DBNull.Value),
                        new SqlParameter("@FechaInicio", string.IsNullOrEmpty(fechaInicio) ? (object)DBNull.Value : fechaInicio),
                        new SqlParameter("@FechaFin", string.IsNullOrEmpty(fechaFin) ? (object)DBNull.Value : fechaFin)
                    ).ToList();
            }
        }
        public FichaVM ObtenerFichaCompleta(int idPersona)
        {
            string rutaBase = ConfigurationManager.AppSettings["RutaArchivos"];
            var result = new FichaVM();

            using (var conn = db.Database.Connection)
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_FichaDetenido";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@idPersona", idPersona));

                    using (var reader = cmd.ExecuteReader())
                    {
                        /* 1️ DATOS */
                        if (reader.Read())
                        {
                            result.NombreCompleto = reader["NombreCompleto"]?.ToString();
                            result.Nombre = reader["Nombre"]?.ToString();
                            result.Paterno = reader["Paterno"]?.ToString();
                            result.Materno = reader["Materno"]?.ToString();
                            result.CURP = reader["CURP"]?.ToString();
                            result.FechaNacimiento = reader["FechaNacimiento"]?.ToString();
                            result.Sexo = reader["Sexo"]?.ToString();
                            result.Telefono = reader["Telefono"]?.ToString();
                            result.Estatura = reader["Estatura"]?.ToString();
                            result.Observaciones = reader["Observaciones"]?.ToString();
                            result.Escolaridad = reader["Escolaridad"]?.ToString();
                            result.Ocupacion = reader["Ocupacion"]?.ToString();
                        }

                        /* 2️ ALIAS */
                        if (reader.NextResult())
                        {
                            // IMPORTANTE: Debemos llamar a Read() para posicionarnos en la fila de datos
                            if (reader.Read())
                            {
                                // Usamos una forma segura por si el valor es NULL en la BD
                                result.Alias = reader["Alias"] != DBNull.Value ? reader["Alias"].ToString() : "";
                            }
                        }


                        /* 3️ DOMICILIOS */
                        reader.NextResult();
                        result.Domicilios = new List<DomicilioVM>();
                        while (reader.Read())
                        {
                            result.Domicilios.Add(new DomicilioVM
                            {
                                Calle = reader["Calle"].ToString(),
                                Colonia = reader["Colonia"].ToString(),
                                Municipio = reader["Municipio"].ToString(),
                                Estado = reader["Estado"].ToString(),
                                NumExt = reader["NumExt"]?.ToString(),
                                NumInt = reader["NumInt"]?.ToString(),
                                Tipo = Convert.ToInt32(reader["Tipo"])
                            });
                        }

                        /* 4️ DETENCIONES */
                        reader.NextResult();
                        result.Detenciones = new List<DetencionVM>();
                        var ban = true;
                        while (reader.Read())
                        {
                            if(ban)
                            {
                                ban = false;
                                result.Edad = reader["Edad"]?.ToString();
                                result.ActivoD = DateTime.Now < Convert.ToDateTime(reader["FechaLiberacion"]);
                            }
                            result.Detenciones.Add(new DetencionVM
                            {
                                FechaDetencion = Convert.ToDateTime(reader["FechaDetencion"].ToString()),
                                FechaLiberacion = Convert.ToDateTime(reader["FechaLiberacion"].ToString()),
                                FaltaAdministrativa = reader["Falta"].ToString(),
                                Corporacion = reader["Corporacion"]?.ToString(),
                                Activo = DateTime.Now < Convert.ToDateTime(reader["FechaLiberacion"]),
                                Observaciones=reader["Observaciones"].ToString()
                            });
                        }

                        /* 5️ FOTOS */
                        reader.NextResult();
                        result.Fotos = new List<FotoVM>();
                        bool esLaPrimera = true; 
                        while (reader.Read())
                        {
                            int idFoto = Convert.ToInt32(reader["idFoto"]);
                            string tipoFoto = reader["TipoFoto"]?.ToString();

                            // 🔥 solo mandamos ID (no base64)
                            result.Fotos.Add(new FotoVM
                            {
                                idFoto = idFoto,
                                Ruta = "/Home/ObtenerFoto?id=" + idFoto, // 🔥 URL REAL
                                TipoFoto = tipoFoto
                            });

                            // 🔥 FOTO PRINCIPAL (también URL, NO base64)
                            if (esLaPrimera && !string.IsNullOrEmpty(tipoFoto) && tipoFoto.ToUpper().Contains("FRONTAL"))
                            {
                                result.FotoPrincipal = "/Home/ObtenerFoto?id=" + idFoto;
                                esLaPrimera = false;
                            }
                            //string base64 = reader["ArchivoB64"]?.ToString(); // 🔥 CAMBIO
                            //string tipoFoto = reader["TipoFoto"]?.ToString();
                            //string extension = reader["Ruta"].ToString().ToLower().Replace(".", "");
                            //string mime = "";

                            //switch (extension)
                            //{
                            //    case "jpg":
                            //    case "jpeg":
                            //        mime = "image/jpeg";
                            //        break;
                            //    case "png":
                            //        mime = "image/png";
                            //        break;
                            //}

                            //// 🔥 armar data URL completa
                            //string dataUrl = "";

                            //if (!string.IsNullOrEmpty(base64))
                            //{
                            //    mime = string.IsNullOrEmpty(mime) ? "image/jpeg" : mime;
                            //    dataUrl = $"data:{mime};base64,{base64}";
                            //}

                            //result.Fotos.Add(new FotoVM
                            //{
                            //    Ruta = dataUrl, // 🔥 YA VA LISTO PARA FRONT
                            //    TipoFoto = tipoFoto
                            //}); 

                            //if (esLaPrimera && tipoFoto.ToUpper().Contains("FRONTAL"))
                            //{
                            //    result.FotoPrincipal = dataUrl;
                            //    esLaPrimera = false; // Ya encontramos la principal, no entrará más aquí
                            //}
                        }

                        /* 6️ PDF */
                        reader.NextResult();
                        if (reader.Read())
                        {
                            string rutaDocs =  reader["ArchivoPDF"]?.ToString();
                            result.PDF = rutaDocs;
                        }
                    }
                }
            }

            return result;
        }
        public FotoVM ObtenerFoto(int id)
        {
            return db.tb_Fotografia
                .AsNoTracking()
                .Where(x => x.idFoto == id)
                .Select(x => new FotoVM
                {
                    idFoto = x.idFoto,
                    Base64 = x.ArchivoB64
                })
                .FirstOrDefault();
        }


    }
}

