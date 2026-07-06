using Objetivos_Prioritarios.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Objetivos_Prioritarios.ControllersServices
{
    public class MapaRelacionesService : BaseService
    {
        public List<object> GetGruposActivos()
        {
            return db.tb_Grupo_Delictivo
                .AsNoTracking()
                .Where(x => x.bit_estatus == true)
                .OrderBy(x => x.nvarchar_grupo)
                .Select(x => new
                {
                    int_id_grupo = x.int_id_grupo,
                    grupo = x.nvarchar_grupo,
                    alias = x.nvarchar_alias
                })
                .ToList<object>();
        }

        public object GetRedPorGrupo(int int_id_grupo,string urlFoto)
        {
            var grupo = db.tb_Grupo_Delictivo
                .AsNoTracking()
                .FirstOrDefault(x => x.int_id_grupo == int_id_grupo);

            if (grupo == null)
            {
                return new
                {
                    success = false,
                    message = "No se encontró el grupo delictivo.",
                    nodes = new List<object>(),
                    edges = new List<object>()
                };
            }

            var integrantes = db.tb_ObjetivoGrupo
                .AsNoTracking()
                .Where(x => x.int_id_grupo == int_id_grupo && x.bit_estatus == true && x.tb_Objetivo.bit_estatus)
                .ToList();

            var nodes = new List<object>();
            var edges = new List<object>();

            string idGrupoNode = "grupo_" + grupo.int_id_grupo;

            nodes.Add(new
            {
                id = idGrupoNode,
                label = grupo.nvarchar_grupo,
                title = grupo.nvarchar_alias,
                tipo = "grupo",
                shape = "box",
                color = new
                {
                    background = "#0b2364",
                    border = "#061742",
                    highlight = new
                    {
                        background = "#123a9c",
                        border = "#061742"
                    }
                },
                font = new
                {
                    color = "#ffffff",
                    size = 18,
                    face = "Arial",
                    bold = true
                },
                margin = 18
            });

            foreach (var item in integrantes)
            {
                var objetivo = db.tb_Objetivo
                    .AsNoTracking()
                    .FirstOrDefault(x => x.int_id_objetivo == item.int_id_objetivo);

                if (objetivo == null)
                    continue;

                string nombre = ObtenerNombrePrincipal(item.int_id_objetivo);
                string alias = ObtenerAliasObjetivo(item.int_id_objetivo);
                string puesto = ObtenerPuesto(item.ID_Nivel_Organizacion);
                //string foto = FormatearFoto(objetivo.nvarchar_foto);
                string foto = urlFoto + "?int_id_objetivo=" + item.int_id_objetivo;

                string idObjetivoNode = "obj_" + item.int_id_objetivo;

                nodes.Add(new
                {
                    id = idObjetivoNode,
                    label = nombre + "\n" + alias,
                    title = puesto,
                    tipo = "objetivo",
                    int_id_objetivo = item.int_id_objetivo,
                    nombre = nombre,
                    alias = alias,
                    puesto = puesto,
                    funcion = item.nvarchar_funcion_grupo,
                    observaciones = item.nvarchar_observaciones,
                    fechaIngreso = item.date_fecha_ingreso.HasValue ? item.date_fecha_ingreso.Value.ToString("dd-MM-yyyy") : "",
                    fechaSalida = item.date_fecha_salida.HasValue ? item.date_fecha_salida.Value.ToString("dd-MM-yyyy") : "",
                    image = foto,
                    shape = "circularImage",
                    size = 32,
                    font = new
                    {
                        color = "#0f172a",
                        size = 13,
                        face = "Arial"
                    },
                    borderWidth = 3,
                    color = new
                    {
                        border = "#4758fd",
                        background = "#eff4ff",
                        highlight = new
                        {
                            border = "#2f5bff",
                            background = "#dce8ff"
                        }
                    }
                });

                edges.Add(new
                {
                    from = idGrupoNode,
                    to = idObjetivoNode,
                    label = string.IsNullOrWhiteSpace(puesto) ? "Por definir" : puesto,
                    arrows = "to",
                    color = new
                    {
                        color = "#94a3b8",
                        highlight = "#2f5bff"
                    },
                    font = new
                    {
                        size = 11,
                        color = "#334155",
                        background = "#ffffff"
                    }
                });
            }

            return new
            {
                success = true,
                message = "Red generada correctamente.",
                grupo = grupo.nvarchar_grupo,
                total = integrantes.Count,
                nodes = nodes,
                edges = edges
            };
        }

        private string ObtenerNombrePrincipal(int int_id_objetivo)
        {
            var nombre = db.tb_NombreObjetivo
                .AsNoTracking()
                .Where(x => x.int_id_objetivo == int_id_objetivo && x.bit_estatus == true)
                .OrderByDescending(x => x.bit_principal)
                .ThenBy(x => x.int_id_nombre)
                .Select(x =>
                    ((x.nvarchar_nombre ?? "") + " " +
                     (x.nvarchar_paterno ?? "") + " " +
                     (x.nvarchar_materno ?? "")).Trim()
                )
                .FirstOrDefault();

            return string.IsNullOrWhiteSpace(nombre) ? "SIN NOMBRE" : nombre.ToUpper();
        }

        private string ObtenerAliasObjetivo(int int_id_objetivo)
        {
            var alias = db.tb_AliasObjetivo
                .AsNoTracking()
                .Where(x => x.int_id_objetivo == int_id_objetivo && x.bit_estatus == true)
                .OrderBy(x => x.int_id_alias)
                .Select(x => x.nvarchar_alias)
                .ToList();

            if (alias == null || alias.Count == 0)
                return "SIN ALIAS";

            return string.Join(", ", alias);
        }

        private string ObtenerPuesto(int? idNivel)
        {
            if (!idNivel.HasValue)
                return "Por definir";

            var puesto = db.cat_Nivel_Organizacion
                .AsNoTracking()
                .Where(x => x.ID == idNivel.Value)
                .Select(x => x.Puesto)
                .FirstOrDefault();

            return string.IsNullOrWhiteSpace(puesto) ? "Por definir" : puesto;
        }

        private string FormatearFoto(string foto)
        {
            if (string.IsNullOrWhiteSpace(foto))
            {
                return "/images/NoDisponible.png";
            }

            if (foto.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
            {
                return foto;
            }

            if (foto.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                return foto;
            }

            return "data:image/png;base64," + foto;
        }

        public string GetFotoObjetivoMapa(int int_id_objetivo)
        {
            return db.tb_Objetivo
                     .AsNoTracking()
                     .Where(x => x.int_id_objetivo == int_id_objetivo)
                     .Select(x => x.nvarchar_foto)
                     .FirstOrDefault();
        }


        public List<object> GetAsuntosActivos()
        {
            return db.tb_AsuntoRelacionado
                .AsNoTracking()
                .Where(x => x.bit_estatus == true)
                .OrderByDescending(x => x.date_fecha_creacion)
                .Select(x => new
                {
                    int_id_asunto_relacionado = x.int_id_asunto_relacionado,
                    alias = x.nvarchar_alias,
                    descripcion = x.nvarchar_descripcion,
                    numavp = x.numavp
                })
                .ToList<object>();
        }

        public object GetRedPorAsunto(
     int int_id_asunto_relacionado,
     string urlFotoObjetivo,
     string urlFotoVictima)
        {
            var asunto = db.tb_AsuntoRelacionado
                .AsNoTracking()
                .FirstOrDefault(x => x.int_id_asunto_relacionado == int_id_asunto_relacionado);

            if (asunto == null)
            {
                return new
                {
                    success = false,
                    message = "No se encontró el asunto.",
                    nodes = new List<object>(),
                    edges = new List<object>()
                };
            }

            var nodes = new List<object>();
            var edges = new List<object>();

            var nodosAgregados = new HashSet<string>();
            var edgesAgregados = new HashSet<string>();

            string idAsuntoNode = "asunto_" + asunto.int_id_asunto_relacionado;

            string tituloAsunto = string.IsNullOrWhiteSpace(asunto.nvarchar_alias)
                ? "ASUNTO " + asunto.int_id_asunto_relacionado
                : asunto.nvarchar_alias;

            // ================================
            // NODO CENTRAL: ASUNTO
            // ================================
            nodosAgregados.Add(idAsuntoNode);

            nodes.Add(new
            {
                id = idAsuntoNode,
                label = tituloAsunto,
                title = asunto.nvarchar_descripcion,
                tipo = "asunto",
                int_id_asunto_relacionado = asunto.int_id_asunto_relacionado,
                descripcion = asunto.nvarchar_descripcion,
                numavp = asunto.numavp,
                fecha = asunto.date_fecha_asunto.HasValue ? asunto.date_fecha_asunto.Value.ToString("dd-MM-yyyy") : "",
                shape = "box",
                color = new
                {
                    background = "#7f1d1d",
                    border = "#450a0a",
                    highlight = new
                    {
                        background = "#991b1b",
                        border = "#450a0a"
                    }
                },
                font = new
                {
                    color = "#ffffff",
                    size = 18,
                    face = "Arial",
                    bold = true
                },
                margin = 18
            });

            int totalObjetivos = 0;
            int totalGrupos = 0;
            int totalVictimas = 0;

            // ================================
            // OBJETIVOS DEL ASUNTO
            // ================================
            var objetivos = db.tb_FichaAsunto
                .AsNoTracking()
                .Where(x => x.int_id_asunto_relacionado == int_id_asunto_relacionado
                         && x.bit_estatus == true)
                .ToList();

            foreach (var item in objetivos)
            {
                var ficha = db.tb_FichaObjetivo
                    .AsNoTracking()
                    .FirstOrDefault(x => x.int_id_ficha_objetivo == item.int_id_ficha_objetivo);

                if (ficha == null)
                    continue;

                int idObjetivo = ficha.int_id_objetivo;

                string idObjetivoNode = "obj_" + idObjetivo;
                string nombre = ObtenerNombrePrincipal(idObjetivo);
                string alias = ObtenerAliasObjetivo(idObjetivo);
                string rol = ObtenerRolParticipacion(item.int_id_rol_participacion);
                string foto = urlFotoObjetivo + "?int_id_objetivo=" + idObjetivo;

                if (!nodosAgregados.Contains(idObjetivoNode))
                {
                    nodosAgregados.Add(idObjetivoNode);
                    totalObjetivos++;

                    nodes.Add(new
                    {
                        id = idObjetivoNode,
                        label = nombre + "\n" + alias,
                        title = rol,
                        tipo = "objetivo",
                        int_id_objetivo = idObjetivo,
                        nombre = nombre,
                        alias = alias,
                        rol = rol,
                        participacion = item.nvarchar_descripcion_participacion,
                        observaciones = item.nvarchar_observaciones,
                        image = foto,
                        shape = "circularImage",
                        size = 34,
                        font = new
                        {
                            color = "#0f172a",
                            size = 13,
                            face = "Arial"
                        },
                        borderWidth = 3,
                        color = new
                        {
                            border = "#4758fd",
                            background = "#eff4ff",
                            highlight = new
                            {
                                border = "#2f5bff",
                                background = "#dce8ff"
                            }
                        }
                    });
                }

                // ================================
                // GRUPOS / CÁRTELES DEL OBJETIVO
                // ================================
                var gruposObjetivo = db.tb_ObjetivoGrupo
                    .AsNoTracking()
                    .Where(x => x.int_id_objetivo == idObjetivo
                             && x.bit_estatus == true)
                    .ToList();

                // Si no tiene grupo, lo conectamos directo al asunto
                if (gruposObjetivo == null || gruposObjetivo.Count == 0)
                {
                    AgregarEdgeMapa(
                        edges,
                        edgesAgregados,
                        idAsuntoNode,
                        idObjetivoNode,
                        string.IsNullOrWhiteSpace(rol) ? "Por definir" : rol,
                        "#64748b"
                    );

                    continue;
                }

                foreach (var relacionGrupo in gruposObjetivo)
                {
                    var grupo = db.tb_Grupo_Delictivo
                        .AsNoTracking()
                        .FirstOrDefault(x => x.int_id_grupo == relacionGrupo.int_id_grupo);

                    if (grupo == null)
                        continue;

                    string idGrupoNode = "grupo_" + grupo.int_id_grupo;

                    string nombreGrupo = !string.IsNullOrWhiteSpace(grupo.nvarchar_alias)
                        ? grupo.nvarchar_alias
                        : grupo.nvarchar_grupo;

                    if (string.IsNullOrWhiteSpace(nombreGrupo))
                        nombreGrupo = "GRUPO " + grupo.int_id_grupo;

                    if (!nodosAgregados.Contains(idGrupoNode))
                    {
                        nodosAgregados.Add(idGrupoNode);
                        totalGrupos++;

                        nodes.Add(new
                        {
                            id = idGrupoNode,
                            label = nombreGrupo,
                            title = grupo.nvarchar_grupo,
                            tipo = "grupo",
                            int_id_grupo = grupo.int_id_grupo,
                            nombre = grupo.nvarchar_grupo,
                            alias = grupo.nvarchar_alias,
                            shape = "box",
                            color = new
                            {
                                background = "#0b2364",
                                border = "#061742",
                                highlight = new
                                {
                                    background = "#123a9c",
                                    border = "#061742"
                                }
                            },
                            font = new
                            {
                                color = "#ffffff",
                                size = 16,
                                face = "Arial",
                                bold = true
                            },
                            margin = 16
                        });
                    }

                    // ASUNTO -> GRUPO
                    AgregarEdgeMapa(
                        edges,
                        edgesAgregados,
                        idAsuntoNode,
                        idGrupoNode,
                        "Grupo",
                        "#2563eb"
                    );

                    // GRUPO -> OBJETIVO
                    AgregarEdgeMapa(
                        edges,
                        edgesAgregados,
                        idGrupoNode,
                        idObjetivoNode,
                        string.IsNullOrWhiteSpace(rol) ? "Por definir" : rol,
                        "#64748b"
                    );
                }
            }

            // ================================
            // VÍCTIMAS DEL ASUNTO
            // ================================
            var victimas = db.tb_AsuntoVictimas
                .AsNoTracking()
                .Where(x => x.int_id_asunto_relacionado == int_id_asunto_relacionado
                         && x.bit_estatus == true)
                .ToList();

            foreach (var item in victimas)
            {
                var victima = db.tb_Victimas
                    .AsNoTracking()
                    .FirstOrDefault(x => x.int_id_victima == item.int_id_victima);

                if (victima == null)
                    continue;

                string nombreVictima = (
                    (victima.nvarchar_nombre ?? "") + " " +
                    (victima.nvarchar_paterno ?? "") + " " +
                    (victima.nvarchar_materno ?? "")
                ).Trim();

                if (string.IsNullOrWhiteSpace(nombreVictima))
                    nombreVictima = "VÍCTIMA SIN NOMBRE";

                string idVictimaNode = "vic_" + victima.int_id_victima;
                string fotoVictima = urlFotoVictima + "?int_id_victima=" + victima.int_id_victima;

                if (!nodosAgregados.Contains(idVictimaNode))
                {
                    nodosAgregados.Add(idVictimaNode);
                    totalVictimas++;

                    nodes.Add(new
                    {
                        id = idVictimaNode,
                        label = nombreVictima.ToUpper(),
                        title = "Víctima relacionada",
                        tipo = "victima",
                        int_id_victima = victima.int_id_victima,
                        nombre = nombreVictima.ToUpper(),
                        image = fotoVictima,
                        shape = "circularImage",
                        size = 30,
                        font = new
                        {
                            color = "#7f1d1d",
                            size = 13,
                            face = "Arial"
                        },
                        borderWidth = 3,
                        color = new
                        {
                            border = "#dc2626",
                            background = "#fee2e2",
                            highlight = new
                            {
                                border = "#b91c1c",
                                background = "#fecaca"
                            }
                        }
                    });
                }

                AgregarEdgeMapa(
                    edges,
                    edgesAgregados,
                    idAsuntoNode,
                    idVictimaNode,
                    "Víctima",
                    "#dc2626"
                );
            }

            return new
            {
                success = true,
                message = "Red generada correctamente.",
                asunto = tituloAsunto,
                totalObjetivos = totalObjetivos,
                totalGrupos = totalGrupos,
                totalVictimas = totalVictimas,
                nodes = nodes,
                edges = edges
            };
        }

        private void AgregarEdgeMapa(
    List<object> edges,
    HashSet<string> edgesAgregados,
    string from,
    string to,
    string label,
    string colorHex)
        {
            string key = from + "_" + to + "_" + label;

            if (edgesAgregados.Contains(key))
                return;

            edgesAgregados.Add(key);

            edges.Add(new
            {
                from = from,
                to = to,
                label = string.IsNullOrWhiteSpace(label) ? "" : label,
                arrows = "to",
                color = new
                {
                    color = colorHex,
                    highlight = "#2f5bff"
                },
                font = new
                {
                    size = 11,
                    color = "#334155",
                    background = "#ffffff"
                }
            });
        }

        public string GetFotoVictimaMapa(int int_id_victima)
        {
            return db.tb_Victimas
                     .AsNoTracking()
                     .Where(x => x.int_id_victima == int_id_victima)
                     .Select(x => x.nvarchar_foto)
                     .FirstOrDefault();
        }

        private string ObtenerRolParticipacion(int? idRol)
        {
            if (!idRol.HasValue)
                return "Por definir";

            var rol = db.cat_RolParticipacionAsunto
                .AsNoTracking()
                .Where(x => x.int_id_rol_participacion == idRol.Value)
                .Select(x => x.nvarchar_rol)
                .FirstOrDefault();

            return string.IsNullOrWhiteSpace(rol) ? "Por definir" : rol;
        }

        public List<object> GetObjetivosActivos()
        {
            var objetivos = db.tb_Objetivo
                .AsNoTracking()
                .Where(x => x.bit_estatus == true)
                .OrderByDescending(x => x.int_id_objetivo)
                .ToList();

            var lista = objetivos.Select(x => new
            {
                int_id_objetivo = x.int_id_objetivo,
                nombre = ObtenerNombrePrincipal(x.int_id_objetivo),
                alias = ObtenerAliasObjetivo(x.int_id_objetivo)
            })
            .OrderBy(x => x.nombre)
            .Cast<object>()
            .ToList();

            return lista;
        }

        public object GetRedPorObjetivo(
     int int_id_objetivo,
     string urlFotoObjetivo,
     string urlFotoVictima)
        {
            var objetivoPrincipal = db.tb_Objetivo
                .AsNoTracking()
                .FirstOrDefault(x => x.int_id_objetivo == int_id_objetivo);

            if (objetivoPrincipal == null)
            {
                return new
                {
                    success = false,
                    message = "No se encontró el objetivo seleccionado.",
                    nodes = new List<object>(),
                    edges = new List<object>()
                };
            }

            var nodes = new List<object>();
            var edges = new List<object>();

            var nodosAgregados = new HashSet<string>();
            var edgesAgregados = new HashSet<string>();

            string idObjetivoPrincipalNode = "obj_" + int_id_objetivo;

            string nombrePrincipal = ObtenerNombrePrincipal(int_id_objetivo);
            string aliasPrincipal = ObtenerAliasObjetivo(int_id_objetivo);
            string fotoPrincipal = urlFotoObjetivo + "?int_id_objetivo=" + int_id_objetivo;

            string gruposPrincipal = ObtenerGruposObjetivoTexto(int_id_objetivo);
            string puestosPrincipal = ObtenerPuestosObjetivoTexto(int_id_objetivo);

            nodes.Add(new
            {
                id = idObjetivoPrincipalNode,
                label = nombrePrincipal + "\n" + aliasPrincipal,
                title = "Objetivo seleccionado",
                tipo = "objetivo",
                esPrincipal = true,
                int_id_objetivo = int_id_objetivo,
                nombre = nombrePrincipal,
                alias = aliasPrincipal,
                grupos = gruposPrincipal,
                puestos = puestosPrincipal,
                image = fotoPrincipal,
                shape = "circularImage",
                size = 48,
                level = 0,
                font = new
                {
                    color = "#0b2364",
                    size = 15,
                    face = "Arial",
                    bold = true
                },
                borderWidth = 5,
                color = new
                {
                    border = "#f59e0b",
                    background = "#fff7ed",
                    highlight = new
                    {
                        border = "#d97706",
                        background = "#ffedd5"
                    }
                }
            });

            nodosAgregados.Add(idObjetivoPrincipalNode);

            int totalAsuntos = 0;
            int totalGrupos = 0;
            int totalObjetivosRelacionados = 0;
            int totalVictimas = 0;

            var objetivosRelacionadosUnicos = new HashSet<int>();

            var fichaObjetivo = db.tb_FichaObjetivo
                .AsNoTracking()
                .FirstOrDefault(x => x.int_id_objetivo == int_id_objetivo);

            if (fichaObjetivo == null)
            {
                return new
                {
                    success = true,
                    message = "El objetivo no tiene ficha relacionada.",
                    objetivo = nombrePrincipal,
                    totalGrupos = 0,
                    totalAsuntos = 0,
                    totalObjetivosRelacionados = 0,
                    totalVictimas = 0,
                    nodes = nodes,
                    edges = edges
                };
            }

            var asuntosObjetivo = db.tb_FichaAsunto
                .AsNoTracking()
                .Where(x => x.int_id_ficha_objetivo == fichaObjetivo.int_id_ficha_objetivo
                         && x.bit_estatus == true)
                .ToList();

            foreach (var relacionAsuntoPrincipal in asuntosObjetivo)
            {
                var asunto = db.tb_AsuntoRelacionado
                    .AsNoTracking()
                    .FirstOrDefault(x => x.int_id_asunto_relacionado == relacionAsuntoPrincipal.int_id_asunto_relacionado);

                if (asunto == null)
                    continue;

                totalAsuntos++;

                string idAsuntoNode = "asunto_" + asunto.int_id_asunto_relacionado;

                string tituloAsunto = string.IsNullOrWhiteSpace(asunto.nvarchar_alias)
                    ? "ASUNTO " + asunto.int_id_asunto_relacionado
                    : asunto.nvarchar_alias;

                string rolPrincipal = ObtenerRolParticipacion(relacionAsuntoPrincipal.int_id_rol_participacion);

                if (!nodosAgregados.Contains(idAsuntoNode))
                {
                    nodosAgregados.Add(idAsuntoNode);

                    nodes.Add(new
                    {
                        id = idAsuntoNode,
                        label = tituloAsunto,
                        title = asunto.nvarchar_descripcion,
                        tipo = "asunto",
                        int_id_asunto_relacionado = asunto.int_id_asunto_relacionado,
                        descripcion = asunto.nvarchar_descripcion,
                        numavp = asunto.numavp,
                        fecha = asunto.date_fecha_asunto.HasValue ? asunto.date_fecha_asunto.Value.ToString("dd-MM-yyyy") : "",
                        shape = "box",
                        level = 1,
                        color = new
                        {
                            background = "#7f1d1d",
                            border = "#450a0a",
                            highlight = new
                            {
                                background = "#991b1b",
                                border = "#450a0a"
                            }
                        },
                        font = new
                        {
                            color = "#ffffff",
                            size = 16,
                            face = "Arial",
                            bold = true
                        },
                        margin = 15
                    });
                }

                AgregarEdgeMapa(
                    edges,
                    edgesAgregados,
                    idObjetivoPrincipalNode,
                    idAsuntoNode,
                    string.IsNullOrWhiteSpace(rolPrincipal) ? "Participa" : rolPrincipal,
                    "#dc2626"
                );

                var participantesAsunto = db.tb_FichaAsunto
                    .AsNoTracking()
                    .Where(x => x.int_id_asunto_relacionado == asunto.int_id_asunto_relacionado
                             && x.bit_estatus == true)
                    .ToList();

                var participantesInfo = new List<dynamic>();

                foreach (var participanteRelacion in participantesAsunto)
                {
                    var fichaParticipante = db.tb_FichaObjetivo
                        .AsNoTracking()
                        .FirstOrDefault(x => x.int_id_ficha_objetivo == participanteRelacion.int_id_ficha_objetivo);

                    if (fichaParticipante == null)
                        continue;

                    int idObjetivoParticipante = fichaParticipante.int_id_objetivo;

                    if (idObjetivoParticipante != int_id_objetivo)
                        objetivosRelacionadosUnicos.Add(idObjetivoParticipante);

                    var gruposParticipante = db.tb_ObjetivoGrupo
                        .AsNoTracking()
                        .Where(x => x.int_id_objetivo == idObjetivoParticipante
                                 && x.bit_estatus == true)
                        .ToList();

                    participantesInfo.Add(new
                    {
                        idObjetivo = idObjetivoParticipante,
                        nodeId = "obj_" + idObjetivoParticipante,
                        nombre = ObtenerNombrePrincipal(idObjetivoParticipante),
                        alias = ObtenerAliasObjetivo(idObjetivoParticipante),
                        rol = ObtenerRolParticipacion(participanteRelacion.int_id_rol_participacion),
                        participacion = participanteRelacion.nvarchar_descripcion_participacion,
                        observaciones = participanteRelacion.nvarchar_observaciones,
                        grupos = gruposParticipante
                    });
                }

                var idsGrupos = participantesInfo
                    .SelectMany(p => ((IEnumerable<tb_ObjetivoGrupo>)p.grupos).Select(g => g.int_id_grupo))
                    .Distinct()
                    .ToList();

                foreach (var idGrupo in idsGrupos)
                {
                    var grupo = db.tb_Grupo_Delictivo
                        .AsNoTracking()
                        .FirstOrDefault(x => x.int_id_grupo == idGrupo);

                    if (grupo == null)
                        continue;

                    string idGrupoCasoNode = "grupoCaso_" + asunto.int_id_asunto_relacionado + "_" + grupo.int_id_grupo;

                    string nombreGrupo = !string.IsNullOrWhiteSpace(grupo.nvarchar_alias)
                        ? grupo.nvarchar_alias
                        : grupo.nvarchar_grupo;

                    if (string.IsNullOrWhiteSpace(nombreGrupo))
                        nombreGrupo = "GRUPO " + grupo.int_id_grupo;

                    var participantesDelGrupo = participantesInfo
                        .Where(p => ((IEnumerable<tb_ObjetivoGrupo>)p.grupos).Any(g => g.int_id_grupo == grupo.int_id_grupo))
                        .ToList();

                    bool perteneceObjetivoPrincipal = participantesDelGrupo
    .Any(p => p.idObjetivo == int_id_objetivo);

                    if (!nodosAgregados.Contains(idGrupoCasoNode))
                    {
                        nodosAgregados.Add(idGrupoCasoNode);
                        totalGrupos++;

                        nodes.Add(new
                        {
                            id = idGrupoCasoNode,
                            label = nombreGrupo + "\n" +
            participantesDelGrupo.Count + " participante(s)" +
            (perteneceObjetivoPrincipal ? "\n★ OBJETIVO" : ""),
                            title = grupo.nvarchar_grupo,
                            tipo = "grupoCaso",
                            int_id_grupo = grupo.int_id_grupo,
                            int_id_asunto_relacionado = asunto.int_id_asunto_relacionado,
                            nombre = grupo.nvarchar_grupo,
                            alias = grupo.nvarchar_alias,
                            asunto = tituloAsunto,
                            perteneceObjetivoPrincipal = perteneceObjetivoPrincipal,
                            level = 2,
                            participantesCaso = participantesDelGrupo.Select(p => new
                            {
                                int_id_objetivo = p.idObjetivo,
                                nombre = p.nombre,
                                alias = p.alias,
                                rol = p.rol,
                                participacion = p.participacion,
                                observaciones = p.observaciones
                            }).ToList(),
                            shape = "box",
                            borderWidth = perteneceObjetivoPrincipal ? 4 : 2,
                            color = new
                            {
                                background = "#0b2364",
                                border = perteneceObjetivoPrincipal ? "#f59e0b" : "#061742",
                                highlight = new
                                {
                                    background = "#123a9c",
                                    border = perteneceObjetivoPrincipal ? "#d97706" : "#061742"
                                }
                            },
                            font = new
                            {
                                color = "#ffffff",
                                size = 15,
                                face = "Arial",
                                bold = true
                            },
                            margin = 15
                        });
                    }

                    AgregarEdgeMapa(
    edges,
    edgesAgregados,
    idAsuntoNode,
    idGrupoCasoNode,
    perteneceObjetivoPrincipal ? "Grupo del objetivo" : "Grupo",
    perteneceObjetivoPrincipal ? "#f59e0b" : "#2563eb"
);
                }

                var participantesSinGrupo = participantesInfo
                    .Where(p => ((IEnumerable<tb_ObjetivoGrupo>)p.grupos).Count() == 0)
                    .ToList();

                if (participantesSinGrupo.Count > 0)
                {
                    string idSinGrupoNode = "grupoCaso_" + asunto.int_id_asunto_relacionado + "_sinGrupo";

                    if (!nodosAgregados.Contains(idSinGrupoNode))
                    {
                        nodosAgregados.Add(idSinGrupoNode);

                        nodes.Add(new
                        {
                            id = idSinGrupoNode,
                            label = "SIN GRUPO\n" + participantesSinGrupo.Count + " participante(s)",
                            title = "Participantes sin grupo registrado",
                            tipo = "grupoCaso",
                            int_id_grupo = 0,
                            int_id_asunto_relacionado = asunto.int_id_asunto_relacionado,
                            nombre = "Sin grupo registrado",
                            alias = "SIN GRUPO",
                            asunto = tituloAsunto,
                            level = 2,
                            participantesCaso = participantesSinGrupo.Select(p => new
                            {
                                int_id_objetivo = p.idObjetivo,
                                nombre = p.nombre,
                                alias = p.alias,
                                rol = p.rol,
                                participacion = p.participacion,
                                observaciones = p.observaciones
                            }).ToList(),
                            shape = "box",
                            color = new
                            {
                                background = "#475569",
                                border = "#334155",
                                highlight = new
                                {
                                    background = "#64748b",
                                    border = "#334155"
                                }
                            },
                            font = new
                            {
                                color = "#ffffff",
                                size = 15,
                                face = "Arial",
                                bold = true
                            },
                            margin = 15
                        });
                    }

                    AgregarEdgeMapa(
                        edges,
                        edgesAgregados,
                        idAsuntoNode,
                        idSinGrupoNode,
                        "Sin grupo",
                        "#64748b"
                    );
                }

                var victimasAsunto = db.tb_AsuntoVictimas
                    .AsNoTracking()
                    .Where(x => x.int_id_asunto_relacionado == asunto.int_id_asunto_relacionado
                             && x.bit_estatus == true)
                    .ToList();

                if (victimasAsunto.Count > 0)
                {
                    string idVictimasCasoNode = "victimasCaso_" + asunto.int_id_asunto_relacionado;

                    var victimasInfo = new List<object>();

                    foreach (var victimaRelacion in victimasAsunto)
                    {
                        var victima = db.tb_Victimas
                            .AsNoTracking()
                            .FirstOrDefault(x => x.int_id_victima == victimaRelacion.int_id_victima);

                        if (victima == null)
                            continue;

                        string nombreVictima = (
                            (victima.nvarchar_nombre ?? "") + " " +
                            (victima.nvarchar_paterno ?? "") + " " +
                            (victima.nvarchar_materno ?? "")
                        ).Trim();

                        if (string.IsNullOrWhiteSpace(nombreVictima))
                            nombreVictima = "VÍCTIMA SIN NOMBRE";

                        victimasInfo.Add(new
                        {
                            int_id_victima = victima.int_id_victima,
                            nombre = nombreVictima.ToUpper()
                        });
                    }

                    if (!nodosAgregados.Contains(idVictimasCasoNode))
                    {
                        nodosAgregados.Add(idVictimasCasoNode);
                        totalVictimas += victimasInfo.Count;

                        nodes.Add(new
                        {
                            id = idVictimasCasoNode,
                            label = "VÍCTIMAS\n" + victimasInfo.Count,
                            title = "Víctimas relacionadas al asunto",
                            tipo = "victimasCaso",
                            asunto = tituloAsunto,
                            total = victimasInfo.Count,
                            victimasCaso = victimasInfo,
                            shape = "box",
                            level = 2,
                            color = new
                            {
                                background = "#991b1b",
                                border = "#7f1d1d",
                                highlight = new
                                {
                                    background = "#b91c1c",
                                    border = "#7f1d1d"
                                }
                            },
                            font = new
                            {
                                color = "#ffffff",
                                size = 15,
                                face = "Arial",
                                bold = true
                            },
                            margin = 15
                        });
                    }

                    AgregarEdgeMapa(
                        edges,
                        edgesAgregados,
                        idAsuntoNode,
                        idVictimasCasoNode,
                        "Víctimas",
                        "#dc2626"
                    );
                }
            }

            totalObjetivosRelacionados = objetivosRelacionadosUnicos.Count;

            return new
            {
                success = true,
                message = "Red generada correctamente.",
                objetivo = nombrePrincipal,
                totalGrupos = totalGrupos,
                totalAsuntos = totalAsuntos,
                totalObjetivosRelacionados = totalObjetivosRelacionados,
                totalVictimas = totalVictimas,
                nodes = nodes,
                edges = edges
            };
        }
        private void AgregarEdge(
            List<object> edges,
            HashSet<string> edgesAgregados,
            string from,
            string to,
            string label,
            string colorHex)
        {
            string key = from + "_" + to + "_" + label;

            if (edgesAgregados.Contains(key))
                return;

            edgesAgregados.Add(key);

            edges.Add(new
            {
                from = from,
                to = to,
                label = string.IsNullOrWhiteSpace(label) ? "" : label,
                arrows = "to",
                color = new
                {
                    color = colorHex,
                    highlight = "#2f5bff"
                },
                font = new
                {
                    size = 11,
                    color = "#334155",
                    background = "#ffffff"
                }
            });
        }
        public List<object> GetAlbumesActivos()
        {
            var albumes = db.tb_AlbumFichaObjetivo
                .AsNoTracking()
                .Where(x => x.bit_estatus == true)
                .OrderByDescending(x => x.date_fecha_creacion)
                .ToList();

            var lista = albumes.Select(x => new
            {
                int_id_album_ficha_objetivo = x.int_id_album_ficha_objetivo,
                nombre = x.nvarchar_nombre_album,
                descripcion = x.nvarchar_descripcion_album,
                total = db.tb_AlbumFichaObjetivoDetalle
                    .Count(d => d.int_id_album_ficha_objetivo == x.int_id_album_ficha_objetivo
                             && d.bit_estatus == true)
            })
            .Cast<object>()
            .ToList();

            return lista;
        }

        public object GetRedPorAlbum(int int_id_album_ficha_objetivo, string urlFotoObjetivo)
        {
            var album = db.tb_AlbumFichaObjetivo
                .AsNoTracking()
                .FirstOrDefault(x => x.int_id_album_ficha_objetivo == int_id_album_ficha_objetivo);

            if (album == null)
            {
                return new
                {
                    success = false,
                    message = "No se encontró el álbum seleccionado.",
                    nodes = new List<object>(),
                    edges = new List<object>()
                };
            }

            var detalles = db.tb_AlbumFichaObjetivoDetalle
                .AsNoTracking()
                .Where(x => x.int_id_album_ficha_objetivo == int_id_album_ficha_objetivo
                         && x.bit_estatus == true)
                .ToList();

            var nodes = new List<object>();
            var edges = new List<object>();

            string idAlbumNode = "album_" + album.int_id_album_ficha_objetivo;

            nodes.Add(new
            {
                id = idAlbumNode,
                label = album.nvarchar_nombre_album,
                title = album.nvarchar_descripcion_album,
                tipo = "album",
                nombre = album.nvarchar_nombre_album,
                descripcion = album.nvarchar_descripcion_album,
                total = detalles.Count,
                shape = "box",
                color = new
                {
                    background = "#14532d",
                    border = "#052e16",
                    highlight = new
                    {
                        background = "#166534",
                        border = "#052e16"
                    }
                },
                font = new
                {
                    color = "#ffffff",
                    size = 18,
                    face = "Arial",
                    bold = true
                },
                margin = 18
            });

            foreach (var detalle in detalles)
            {
                var ficha = db.tb_FichaObjetivo
                    .AsNoTracking()
                    .FirstOrDefault(x => x.int_id_ficha_objetivo == detalle.int_id_ficha_objetivo);

                if (ficha == null)
                    continue;

                int idObjetivo = ficha.int_id_objetivo;

                var objetivo = db.tb_Objetivo
                    .AsNoTracking()
                    .FirstOrDefault(x => x.int_id_objetivo == idObjetivo);

                if (objetivo == null)
                    continue;

                string nombre = ObtenerNombrePrincipal(idObjetivo);
                string alias = ObtenerAliasObjetivo(idObjetivo);
                string grupos = ObtenerGruposObjetivoTexto(idObjetivo);
                string puestos = ObtenerPuestosObjetivoTexto(idObjetivo);
                string foto = urlFotoObjetivo + "?int_id_objetivo=" + idObjetivo;
                string estatus = ObtenerEstatusFicha(ficha.int_id_estatus_proceso);

                string idObjetivoNode = "obj_" + idObjetivo;

                nodes.Add(new
                {
                    id = idObjetivoNode,
                    label = nombre + "\n" + alias,
                    title = grupos,
                    tipo = "objetivo",
                    int_id_objetivo = idObjetivo,
                    nombre = nombre,
                    alias = alias,
                    grupos = grupos,
                    puestos = puestos,
                    estatus = estatus,
                    descripcionEstatus = ficha.nvarchar_descripcion_estatus,
                    observaciones = ficha.nvarchar_observaciones,
                    image = foto,
                    shape = "circularImage",
                    size = 34,
                    font = new
                    {
                        color = "#0f172a",
                        size = 13,
                        face = "Arial"
                    },
                    borderWidth = 3,
                    color = new
                    {
                        border = "#16a34a",
                        background = "#dcfce7",
                        highlight = new
                        {
                            border = "#15803d",
                            background = "#bbf7d0"
                        }
                    }
                });

                edges.Add(new
                {
                    from = idAlbumNode,
                    to = idObjetivoNode,
                    label = string.IsNullOrWhiteSpace(puestos) ? "Integrante" : puestos,
                    arrows = "to",
                    color = new
                    {
                        color = "#16a34a",
                        highlight = "#15803d"
                    },
                    font = new
                    {
                        size = 11,
                        color = "#14532d",
                        background = "#ffffff"
                    }
                });
            }

            return new
            {
                success = true,
                message = "Red generada correctamente.",
                album = album.nvarchar_nombre_album,
                descripcion = album.nvarchar_descripcion_album,
                totalObjetivos = detalles.Count,
                nodes = nodes,
                edges = edges
            };
        }


        private string ObtenerGruposObjetivoTexto(int int_id_objetivo)
        {
            var grupos = db.tb_ObjetivoGrupo
                .AsNoTracking()
                .Where(x => x.int_id_objetivo == int_id_objetivo
                         && x.bit_estatus == true)
                .Select(x => x.tb_Grupo_Delictivo.nvarchar_alias)
                .ToList();

            if (grupos == null || grupos.Count == 0)
                return "Sin grupo";

            return string.Join(", ", grupos);
        }

        private string ObtenerPuestosObjetivoTexto(int int_id_objetivo)
        {
            var puestos = db.tb_ObjetivoGrupo
                .AsNoTracking()
                .Where(x => x.int_id_objetivo == int_id_objetivo
                         && x.bit_estatus == true)
                .Select(x => x.cat_Nivel_Organizacion != null
                    ? x.cat_Nivel_Organizacion.Puesto
                    : "Por definir")
                .Distinct()
                .ToList();

            if (puestos == null || puestos.Count == 0)
                return "Por definir";

            return string.Join(", ", puestos);
        }

        private string ObtenerEstatusFicha(int? int_id_estatus_proceso)
        {
            if (!int_id_estatus_proceso.HasValue)
                return "Sin estatus";

            var estatus = db.cat_EstatusProceso
                .AsNoTracking()
                .Where(x => x.int_id_estatus_proceso == int_id_estatus_proceso.Value)
                .Select(x => x.nvarchar_estatus)
                .FirstOrDefault();

            return string.IsNullOrWhiteSpace(estatus) ? "Sin estatus" : estatus;
        }

    }
}