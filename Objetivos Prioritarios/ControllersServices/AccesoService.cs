using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Models.Extends;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Objetivos_Prioritarios.ControllersServices
{
    public class AccesoService
    {
        public PermisosUsuarioDto ObtenerPermisosUsuario(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                return null;
            }

            login = login.Trim().ToUpper();

            using (Objetivos_Prioritarios_Perfiles_Entities db = new Objetivos_Prioritarios_Perfiles_Entities())
            {
                tb_Usuarios usuario =
                    db.tb_Usuarios
                        .AsNoTracking()
                        .FirstOrDefault(x =>
                            x.nvarchar_no_interno != null &&
                            x.nvarchar_no_interno.Trim().ToUpper() == login &&
                            x.bit_status == true
                        );

                if (usuario == null)
                {
                    return null;
                }


                /*
                 * ============================================================
                 * PERFILES
                 * ============================================================
                 */

                var perfilesDatos =
                    (
                        from up in db.tb_UsuarioPerfil

                        join p in db.cat_Perfil
                            on up.int_id_perfil
                            equals p.int_id_perfil

                        where
                            up.int_id_usuario == usuario.int_id_usuario &&
                            up.bit_status == true &&
                            p.bit_status == true

                        select new
                        {
                            p.int_id_perfil,
                            p.nvarchar_clave
                        }
                    )
                    .Distinct()
                    .ToList();


                List<int> idsPerfiles =
                    perfilesDatos
                        .Select(x =>
                            x.int_id_perfil
                        )
                        .Distinct()
                        .ToList();


                List<string> perfiles =
                    perfilesDatos
                        .Select(x =>
                            x.nvarchar_clave
                        )
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(x)
                        )
                        .Distinct()
                        .ToList();


                /*
                 * ============================================================
                 * MÓDULOS
                 * ============================================================
                 */

                List<string> modulos =
                    (
                        from pm in db.tb_PerfilModulo

                        join m in db.cat_ModuloSistema
                            on pm.int_id_modulo
                            equals m.int_id_modulo

                        where
                            idsPerfiles.Contains(pm.int_id_perfil) &&
                            pm.bit_status == true &&
                            m.bit_status == true

                        orderby
                            m.int_orden

                        select
                            m.nvarchar_clave
                    )
                    .Distinct()
                    .ToList();


                /*
                 * ============================================================
                 * FUENTES DE BÚSQUEDA
                 * ============================================================
                 */

                List<int> fuentes =
                    (
                        from pf in db.tb_PerfilFuenteBusqueda

                        join f in db.cat_FuenteBusqueda
                            on pf.int_id_fuente
                            equals f.int_id_fuente

                        where
                            idsPerfiles.Contains(pf.int_id_perfil) &&
                            pf.bit_status == true &&
                            f.bit_status == true

                        orderby
                            f.int_orden

                        select
                            f.int_id_fuente
                    )
                    .Distinct()
                    .ToList();


                /*
                 * ============================================================
                 * PERMISOS ESPECIALES
                 * ============================================================
                 */

                List<string> permisos =
                    (
                        from pp in db.tb_PerfilPermiso

                        join p in db.cat_PermisoSistema
                            on pp.int_id_permiso
                            equals p.int_id_permiso

                        where
                            idsPerfiles.Contains(pp.int_id_perfil) &&
                            pp.bit_status == true &&
                            p.bit_status == true

                        select
                            p.nvarchar_clave
                    )
                    .Distinct()
                    .ToList();


                /*
                 * ============================================================
                 * RESULTADO
                 * ============================================================
                 */

                return new PermisosUsuarioDto
                {
                    IdUsuario =
                        usuario.int_id_usuario,

                    Login =
                        string.IsNullOrWhiteSpace(
                            usuario.nvarchar_no_interno
                        )
                            ? ""
                            : usuario.nvarchar_no_interno.Trim(),

                    Nombre =
                        string.IsNullOrWhiteSpace(
                            usuario.nvarchar_nombre_usuario
                        )
                            ? ""
                            : usuario.nvarchar_nombre_usuario.Trim(),

                    Puesto =
                        string.IsNullOrWhiteSpace(
                            usuario.nvarchar_puesto
                        )
                            ? ""
                            : usuario.nvarchar_puesto.Trim(),

                    Activo =
                        usuario.bit_status == true,

                    EsAdministrador =
                        perfiles.Any(x =>
                            string.Equals(
                                x,
                                "ADMINISTRADOR",
                                StringComparison.OrdinalIgnoreCase
                            )
                        ),

                    Perfiles =
                        perfiles,

                    Modulos =
                        modulos,

                    FuentesBusqueda =
                        fuentes,

                    Permisos =
                        permisos
                };
            }
        }


        public bool TieneAccesoSistema(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                return false;
            }

            login =
                login
                    .Trim()
                    .ToUpper();

            using (Objetivos_Prioritarios_Perfiles_Entities db = new Objetivos_Prioritarios_Perfiles_Entities())
            {
                return db.tb_Usuarios
                    .AsNoTracking()
                    .Any(x =>
                        x.nvarchar_no_interno != null &&
                        x.nvarchar_no_interno.Trim().ToUpper() == login &&
                        x.bit_status == true
                    );
            }
        }


        public bool TienePerfil(string login, string perfil)
        {
            if (
                string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(perfil)
            )
            {
                return false;
            }

            PermisosUsuarioDto usuario =
                ObtenerPermisosUsuario(
                    login
                );

            if (usuario == null)
            {
                return false;
            }

            return usuario.Perfiles.Any(x =>
                string.Equals(
                    x,
                    perfil,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }


        public bool TieneModulo(string login, string modulo)
        {
            if (
                string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(modulo)
            )
            {
                return false;
            }

            PermisosUsuarioDto usuario =
                ObtenerPermisosUsuario(
                    login
                );

            if (usuario == null)
            {
                return false;
            }

            return usuario.Modulos.Any(x =>
                string.Equals(
                    x,
                    modulo,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }


        public bool TieneFuenteBusqueda(string login, int idFuente)
        {
            if (
                string.IsNullOrWhiteSpace(login) ||
                idFuente <= 0
            )
            {
                return false;
            }

            PermisosUsuarioDto usuario =
                ObtenerPermisosUsuario(
                    login
                );

            if (usuario == null)
            {
                return false;
            }

            return usuario.FuentesBusqueda.Contains(
                idFuente
            );
        }


        public List<int> ObtenerFuentesBusqueda(string login)
        {
            PermisosUsuarioDto usuario =
                ObtenerPermisosUsuario(
                    login
                );

            if (usuario == null)
            {
                return new List<int>();
            }

            return usuario
                .FuentesBusqueda
                .Distinct()
                .ToList();
        }


        public bool TienePermiso(string login, string permiso)
        {
            if (
                string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(permiso)
            )
            {
                return false;
            }

            PermisosUsuarioDto usuario =
                ObtenerPermisosUsuario(
                    login
                );

            if (usuario == null)
            {
                return false;
            }

            return usuario.Permisos.Any(x =>
                string.Equals(
                    x,
                    permiso,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }
    }
}