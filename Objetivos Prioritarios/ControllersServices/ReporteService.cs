using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.ControllersServices
{
    public class ReporteService : BaseService
    {
        public List<Detenido> ObtenerDatosDeTuBaseDeDatos(int int_id_album_ficha_objetivo)
        {
            var listaObjetivosPrioritarios = new List<Detenido>();

            try
            {
                var busqueda = db.tb_AlbumFichaObjetivoDetalle
                    .AsNoTracking()
                    .Where(x => x.int_id_album_ficha_objetivo == int_id_album_ficha_objetivo && x.bit_estatus)
                    .ToList();

                foreach (var item in busqueda)
                {
                    var ficha = item?.tb_FichaObjetivo;
                    if (ficha == null)
                    {
                        // Si no tiene ficha, agrega un objeto vacío para mantener estructura
                        listaObjetivosPrioritarios.Add(new Detenido());
                        continue;
                    }

                    int int_id_ficha = ficha.int_id_ficha_objetivo;
                    int? int_id_objetivo = ficha.int_id_objetivo;

                    // 🧩 Nombre principal
                    var nombrePrincipal = db.tb_NombreObjetivo
                        .AsNoTracking()
                        .FirstOrDefault(x => x.int_id_objetivo == int_id_objetivo && x.bit_principal==true);

                    string nombreCompleto = nombrePrincipal?.NombreCompleto ?? "";

                    // 🧩 Carteles
                    var carteles = db.tb_ObjetivoGrupo
                        .AsNoTracking()
                        .Where(x => x.int_id_objetivo == int_id_objetivo)
                        .Select(x => x.tb_Grupo_Delictivo != null ? x.tb_Grupo_Delictivo.nvarchar_alias : "")
                        .ToList();

                    string cartelTexto = carteles != null && carteles.Any()
                        ? string.Join(" ", carteles)
                        : "";

                    // 🧩 Detenido
                    var detenido = db.tb_Detenido
                        .AsNoTracking()
                        .Where(x => x.int_id_ficha_objetivo == int_id_ficha).ToList();

                    //List<string> detenidoDelitosFinal = new List<string>() { ""};
                    //if (detenido.Count > 0)
                    //{

                    //    foreach (var dete in detenido)
                    //    {
                    //        int int_id_detenido = dete?.int_id_detenido ?? 0;

                    //        // 🧩 Delitos
                    //        var detenidoDelitos = (int_id_detenido > 0)
                    //            ? db.tb_Detenidos_DelitoIngreso
                    //                .AsNoTracking()
                    //                .Where(x => x.int_id_detenido == int_id_detenido)
                    //                .Select(x => x.Delito ?? "")
                    //                .Distinct()
                    //                .ToList()
                    //            : new List<string>();
                    //        if(detenidoDelitos != null && detenidoDelitos.Count > 0)
                    //        {
                    //            detenidoDelitosFinal.AddRange(detenidoDelitos);
                    //        }
                    //    }
                    //}

                    List<string> detenidoDelitosFinal = new List<string>();

                    if (detenido.Count > 0)
                    {
                        foreach (var dete in detenido)
                        {
                            int int_id_detenido = dete?.int_id_detenido ?? 0;

                            // 🧩 Delitos del detenido
                            var detenidoDelitos = (int_id_detenido > 0)
                                ? db.tb_Detenidos_DelitoIngreso
                                    .AsNoTracking()
                                    .Where(x => x.int_id_detenido == int_id_detenido)
                                    .Select(x => x.Delito ?? "")
                                    .Distinct()
                                    .ToList()
                                : new List<string>();

                            if (detenidoDelitos != null && detenidoDelitos.Count > 0)
                            {
                                // 🕓 Combinar cada delito con la fecha y hora de detención
                                string fechaDetencion = dete.date_fecha_detencion?.ToString("dd/MM/yyyy") ?? "";
                                string horaDetencion = dete.time_hora_detencion?.ToString(@"hh\:mm") ?? "";

                                string fechaHora = (!string.IsNullOrEmpty(fechaDetencion) || !string.IsNullOrEmpty(horaDetencion))
                                    ? $"{fechaDetencion} {horaDetencion}".Trim()
                                    : "";

                                var delitosConcatenados = detenidoDelitos
                                    .Where(d => !string.IsNullOrWhiteSpace(d))
                                    .Select(d => string.IsNullOrEmpty(fechaHora) ? d : $"{d} — {fechaHora}")
                                    .ToList();

                                detenidoDelitosFinal.AddRange(delitosConcatenados);
                            }
                        }
                    }

                    // 🔹 Eliminar duplicados y vacíos
                    detenidoDelitosFinal = detenidoDelitosFinal
                        .Where(d => !string.IsNullOrWhiteSpace(d))
                        .Distinct()
                        .ToList();

                    // 🔹 Agregar un registro vacío al inicio
                    detenidoDelitosFinal.Insert(0, "");


                    // 🧩 Carpetas
                    //var detenidoCarpetas = db.tb_CarpetasObjetivo
                    //    .AsNoTracking()
                    //    .Where(x => x.int_id_ficha_objetivo == int_id_ficha)
                    //    .Select(x => ((x.numavp==null?"":x.numavp) +" "+ (x.Delito==null?"":x.Delito)))
                    //    .ToList();

                    // 🧩 Carpetas
                    List<string> detenidoCarpetasFinal = new List<string>();

                    var detenidoCarpetas = db.tb_CarpetasObjetivo
                        .AsNoTracking()
                        .Where(x => x.int_id_ficha_objetivo == int_id_ficha)
                        .Select(x => new
                        {
                            NumAvp = x.numavp ?? "",
                            Delito = x.Delito ?? ""
                        })
                        .ToList();

                    if (detenidoCarpetas != null && detenidoCarpetas.Count > 0)
                    {
                        foreach (var c in detenidoCarpetas)
                        {
                            string combinado = $"{c.NumAvp.Trim()} {c.Delito.Trim()}".Trim();

                            if (!string.IsNullOrWhiteSpace(combinado))
                                detenidoCarpetasFinal.Add(combinado);
                        }
                    }

                    // 🔹 Quitar duplicados y vacíos
                    detenidoCarpetasFinal = detenidoCarpetasFinal
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Distinct()
                        .ToList();

                    // 🔹 Agregar un registro vacío al principio
                    detenidoCarpetasFinal.Insert(0, "");


                    // 🧩 Órdenes
                    //var detenidoOrdenes = db.tb_OrdenAprehension
                    //    .AsNoTracking()
                    //    .Where(x => x.int_id_ficha_objetivo == int_id_ficha)
                    //    .Select(x => x.delito==null?"":x.delito )
                    //    .Distinct()
                    //    .ToList();
                    // 🧩 Órdenes
                    List<string> detenidoOrdenesFinal = new List<string>();

                    var detenidoOrdenes = db.tb_OrdenAprehension
                        .AsNoTracking()
                        .Where(x => x.int_id_ficha_objetivo == int_id_ficha)
                        .Select(x => new
                        {
                            Delito = x.delito ?? "",
                            Tipo = x.tipo ?? ""
                        })
                        .ToList();

                    if (detenidoOrdenes != null && detenidoOrdenes.Count > 0)
                    {
                        foreach (var o in detenidoOrdenes)
                        {
                            // Construir la cadena según disponibilidad de valores
                            string combinado;
                            if (!string.IsNullOrWhiteSpace(o.Delito))
                            {
                                combinado = string.IsNullOrWhiteSpace(o.Tipo)
                                    ? o.Delito.Trim()
                                    : $"{o.Delito.Trim()} — {o.Tipo.Trim()}";
                            }
                            else if (!string.IsNullOrWhiteSpace(o.Tipo))
                            {
                                combinado = o.Tipo.Trim();
                            }
                            else
                            {
                                // Ambos vacíos: saltar
                                continue;
                            }

                            detenidoOrdenesFinal.Add(combinado);
                        }
                    }

                    // Quitar duplicados y vacíos por si acaso
                    detenidoOrdenesFinal = detenidoOrdenesFinal
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct()
                        .ToList();

                    // Agregar un registro vacío al principio
                    detenidoOrdenesFinal.Insert(0, "");




                    // 🧩 Asuntos
                    var Asuntos = db.tb_FichaAsunto
                        .AsNoTracking()
                        .Where(x => x.int_id_ficha_objetivo == int_id_ficha)
                        .ToList();

                    var detenidoAsuntos = Asuntos
                        .Where(x => x.tb_AsuntoRelacionado != null)
                        .Select(x => ((x.tb_AsuntoRelacionado.nvarchar_alias==null?"": x.tb_AsuntoRelacionado.nvarchar_alias) +" "+(x.tb_AsuntoRelacionado.nvarchar_descripcion==null?"": x.tb_AsuntoRelacionado.nvarchar_descripcion)))
                        .ToList();

                    // 🧩 Víctimas
                    var listaVictimas = new List<Victima>();
                    foreach (var asunto in Asuntos)
                    {
                        int? int_id_asunto_relacionado = asunto?.int_id_asunto_relacionado;
                        if (int_id_asunto_relacionado == null) continue;

                        var victimas = db.tb_AsuntoVictimas
                            .AsNoTracking()
                            .Where(x => x.int_id_asunto_relacionado == int_id_asunto_relacionado)
                            .Select(x => new Victima
                            {
                                Nombre = x.tb_Victimas==null?"": (x.tb_Victimas.nvarchar_nombre+" "+(x.tb_Victimas.nvarchar_paterno==null?"": x.tb_Victimas.nvarchar_paterno)+ " "+(x.tb_Victimas.nvarchar_materno==null?"":x.tb_Victimas.nvarchar_materno)),
                                Foto = x.tb_Victimas==null? "":x.tb_Victimas.nvarchar_foto
                            })
                            .ToList();

                        if (victimas != null && victimas.Count > 0)
                            listaVictimas.AddRange(victimas);
                    }

                    var fechaNacimiento = ficha.tb_Objetivo?.date_fecha_nacimiento;
                    string edadTexto = "";

                    if (fechaNacimiento != null)
                    {
                        var hoy = DateTime.Today;
                        int edad = hoy.Year - fechaNacimiento.Value.Year;

                        // Si aún no cumple años este año, restar 1
                        if (fechaNacimiento.Value.Date > hoy.AddYears(-edad))
                            edad--;

                        edadTexto = $"{edad} año{(edad == 1 ? "" : "s")}";
                    }
                    else
                    {
                        edadTexto = "";
                    }

                    string aliasPrincipalTexto = "";
                    var alias=db.tb_AliasObjetivo.Where(x=>x.int_id_objetivo==int_id_objetivo).FirstOrDefault();
                    if (alias != null)
                    {
                        aliasPrincipalTexto ="(a) '"+ (alias.nvarchar_alias ?? "")+" '";
                    }

                    // 🧩 Crear objeto final
                    var objDetenido = new Detenido
                    {
                        Nombre = nombreCompleto,
                        Cartel = cartelTexto,
                        Ocupacion = "EJECUTOR",
                        Delitos = detenidoDelitosFinal.Any() ? detenidoDelitosFinal : new List<string> { "" },
                        Carpetas = detenidoCarpetas.Any() ? detenidoCarpetasFinal : new List<string> { "" },
                        Ordenes = detenidoOrdenes.Any() ? detenidoOrdenesFinal : new List<string> { "" },
                        Estatus = ficha.int_id_estatus_proceso==null?"":ficha.cat_EstatusProceso.nvarchar_estatus,
                        DescripcionEstatus = ficha.nvarchar_descripcion_estatus ?? "",
                        Foto = ficha.tb_Objetivo?.nvarchar_foto ?? "",
                        Asunto = detenidoAsuntos.Any() ? detenidoAsuntos : new List<string> { "" },
                        Victimas = listaVictimas.Any() ? listaVictimas : new List<Victima> { new Victima { Nombre = "", Foto = "" } },
                        FechaNacimiento=ficha.tb_Objetivo?.date_fecha_nacimiento==null?"SIN FECHA NACIMIENTO": ficha.tb_Objetivo.date_fecha_nacimiento.Value.ToString("dd/MM/yyyy"),
                        Edad= edadTexto,
                        Alias= aliasPrincipalTexto
                    };

                    listaObjetivosPrioritarios.Add(objDetenido);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error en ObtenerDatosDeTuBaseDeDatos: {ex.Message}");
                // Devuelve al menos un objeto vacío para evitar que el PDF truene
                listaObjetivosPrioritarios.Add(new Detenido());
            }

            return listaObjetivosPrioritarios;
        


        //return new List<Detenido>
        //    {
        //        new Detenido
        //        {
        //            Nombre = "JAVIER ESQUIVEL PADILLA  ALIAS 'LA QUETA'",
        //            Cartel = "CJNG",
        //            Ocupacion = "EJECUTOR",
        //            Delitos = new List<string> {
        //                "Contra la salud",
        //                "Robo simple",
        //                "Lesiones dolosas",
        //                "Violación"
        //            },
        //            Carpetas = new List<string> {
        //                "R-06/00011 Robo calificado",
        //                "R-06/00418 Robo calificado",
        //                "R-07/00017 Robo en las cosas dolosas",
        //                "R-07/00019 Robo en las cosas dolosas",
        //                "R-07/00031 Daño en las cosas dolosas"
        //            },
        //            Ordenes = new List<string> {
        //                "Cumplimentada. Por homicidio doloso calificado con premeditación y ventaja"
        //            },
        //            Estatus = "Sentenciado",
        //            DescripcionEstatus = "15 años de prisión.",
        //            Asunto = "Caso Jassiel: homicidio doloso cometido en Rincón de Romos el 30 de mayo de 2023.",
        //            Foto = Server.MapPath("~/Content/imagenes/principal.jpg"),
        //            Victimas = new List<Victima>
        //            {
        //                new Victima {
        //                    Nombre = "Jassiel Díaz Cardona",
        //                    Foto = Server.MapPath("~/Content/imagenes/victima1.jpg")
        //                },
        //                new Victima {
        //                    Nombre = "Ricardo Navarro",
        //                    Foto = Server.MapPath("~/Content/imagenes/victima2.jpg")
        //                }
        //            }
        //        },

        //        new Detenido
        //        {
        //            Nombre = "MARIO ALONSO GARCÍA ALIAS 'EL CHUCHO'",
        //            Cartel = "Los Viagras",
        //            Ocupacion = "SICARIO",
        //            Delitos = new List<string> {
        //                "Homicidio doloso",
        //                "Portación ilegal de arma de fuego",
        //                "Asociación delictuosa"
        //            },
        //            Carpetas = new List<string> {
        //                "R-08/00123 Homicidio calificado",
        //                "R-09/00512 Portación de arma de fuego sin licencia"
        //            },
        //            Ordenes = new List<string> {
        //                "Orden vigente. Homicidio en grado de tentativa."
        //            },
        //            Estatus = "Prófugo",
        //            DescripcionEstatus = "En proceso de localización.",
        //            Asunto = "Relacionados con enfrentamientos registrados en Tepatitlán en abril de 2024.",
        //            Foto = Server.MapPath("~/Content/imagenes/principal.jpg"),
        //            Victimas = new List<Victima>
        //            {
        //                new Victima {
        //                    Nombre = "Juan Pérez",
        //                    Foto = Server.MapPath("~/Content/imagenes/victima2.jpg")
        //                }
        //            }
        //        }
        //    };
    }
}
}