using Microsoft.Ajax.Utilities;
using Objetivos_Prioritarios.Models;
using Objetivos_Prioritarios.Models.Extends;
using Objetivos_Prioritarios.Utils;
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


        public UsuarioInstitucionalDto BuscarUsuarioInstitucional(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                return null;
            }

            login = login.Trim().ToUpper();

            using (AccesosEntities db = new AccesosEntities())
            {
                var usuario =
                    (
                        from u in db.Usuarios

                        join m in db.Ministerios
                            on u.Cve_Usuario equals m.Cve_Usuario

                        join a in db.Agencias
                            on m.Cve_Age equals a.Cve_age

                        where
                            u.Login != null &&
                            u.Login.Trim().ToUpper() == login

                        select new
                        {
                            u.Cve_Usuario,
                            u.Login,
                            u.Nombre,
                            u.Paterno,
                            u.Materno,
                            u.Puesto,
                            u.Area,

                            CveAgencia =
                                m.Cve_Age,

                            Agencia =
                                a.Agencia,

                            UnidadID =
                                a.UnidadID
                        }
                    )
                    .FirstOrDefault();

                if (usuario == null)
                {
                    return null;
                }


                /*
                 * ============================================================
                 * NOMBRE COMPLETO
                 * ============================================================
                 */

                string nombreCompleto =
                    string.Join(
                        " ",
                        new[]
                        {
                    usuario.Nombre,
                    usuario.Paterno,
                    usuario.Materno
                        }
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(x)
                        )
                        .Select(x =>
                            x.Trim()
                        )
                    );


                /*
                 * ============================================================
                 * UNIDAD DE INVESTIGACIÓN
                 * ============================================================
                 *
                 * Agencia.UnidadID
                 *          ↓
                 * Catalogos.dbo.CT_UnidadesInvestigacion.ID
                 * ============================================================
                 */

                int idUnidad =
                    Convert.ToInt32(
                        usuario.UnidadID
                    );

                string nombreUnidad =
                    "";

                if (idUnidad > 0)
                {
                    using (CatalogosEntities dbCatalogos = new CatalogosEntities())
                    {
                        nombreUnidad =
                            dbCatalogos
                                .CT_UnidadesInvestigacion
                                .AsNoTracking()
                                .Where(x =>
                                    x.ID == idUnidad
                                )
                                .Select(x =>
                                    x.Unidad
                                )
                                .FirstOrDefault()
                                ?? "";
                    }
                }


                /*
                 * ============================================================
                 * CONSTRUIR RESULTADO
                 * ============================================================
                 */

                UsuarioInstitucionalDto resultado =
                    new UsuarioInstitucionalDto
                    {
                        CveUsuario =
                            (int)usuario.Cve_Usuario,

                        Login =
                            string.IsNullOrWhiteSpace(usuario.Login)
                                ? ""
                                : usuario.Login.Trim(),

                        Nombre =
                            string.IsNullOrWhiteSpace(usuario.Nombre)
                                ? ""
                                : usuario.Nombre.Trim(),

                        Paterno =
                            string.IsNullOrWhiteSpace(usuario.Paterno)
                                ? ""
                                : usuario.Paterno.Trim(),

                        Materno =
                            string.IsNullOrWhiteSpace(usuario.Materno)
                                ? ""
                                : usuario.Materno.Trim(),

                        NombreCompleto =
                            nombreCompleto,

                        Puesto =
                            string.IsNullOrWhiteSpace(usuario.Puesto)
                                ? ""
                                : usuario.Puesto.Trim(),

                        Area =
                            string.IsNullOrWhiteSpace(usuario.Area.ToString())
                                ? ""
                                : usuario.Area.ToString().Trim(),

                        CveAgencia =
                            usuario.CveAgencia == null
                                ? ""
                                : usuario.CveAgencia.ToString(),

                        Agencia =
                            string.IsNullOrWhiteSpace(usuario.Agencia)
                                ? ""
                                : usuario.Agencia.Trim(),

                        IdUnidad =
                            idUnidad,

                        Unidad =
                            string.IsNullOrWhiteSpace(nombreUnidad)
                                ? ""
                                : nombreUnidad.Trim()
                    };


                /*
                 * ============================================================
                 * VALIDAR SI YA EXISTE EN OBJETIVOS PRIORITARIOS
                 * ============================================================
                 */

                using (Objetivos_Prioritarios_Perfiles_Entities dbPermisos = new Objetivos_Prioritarios_Perfiles_Entities())
                {
                    var usuarioObjetivos =
                        dbPermisos
                            .tb_Usuarios
                            .AsNoTracking()
                            .FirstOrDefault(x =>
                                x.nvarchar_no_interno != null &&
                                x.nvarchar_no_interno.Trim().ToUpper() == login
                            );

                    resultado.ExisteEnObjetivos =
                        usuarioObjetivos != null;

                    resultado.ActivoEnObjetivos =
                        usuarioObjetivos != null &&
                        usuarioObjetivos.bit_status == true;
                }


                return resultado;
            }
        }


        public AdministracionUsuarioViewModel ObtenerUsuarioAdministracion(string login)
        {
            UsuarioInstitucionalDto usuario = BuscarUsuarioInstitucional(login);

            if (usuario == null)
            {
                return null;
            }

            AdministracionUsuarioViewModel resultado = new AdministracionUsuarioViewModel
            {
                Usuario = usuario
            };

            using (Objetivos_Prioritarios_Perfiles_Entities db = new Objetivos_Prioritarios_Perfiles_Entities())
            {
                int idUsuarioObjetivos = 0;

                var usuarioObjetivos =
                    db.tb_Usuarios
                        .AsNoTracking()
                        .FirstOrDefault(x =>
                            x.nvarchar_no_interno != null &&
                            x.nvarchar_no_interno.Trim().ToUpper() == login.Trim().ToUpper()
                        );

                if (usuarioObjetivos != null)
                {
                    idUsuarioObjetivos = usuarioObjetivos.int_id_usuario;
                }

                List<int> perfilesUsuario = new List<int>();

                if (idUsuarioObjetivos > 0)
                {
                    perfilesUsuario =
                        db.tb_UsuarioPerfil
                            .AsNoTracking()
                            .Where(x =>
                                x.int_id_usuario == idUsuarioObjetivos &&
                                x.bit_status == true
                            )
                            .Select(x =>
                                x.int_id_perfil
                            )
                            .ToList();
                }

                resultado.Perfiles =
                    db.cat_Perfil
                        .AsNoTracking()
                        .Where(x =>
                            x.bit_status == true
                        )
                        .OrderBy(x =>
                            x.nvarchar_nombre
                        )
                        .Select(x =>
                            new PerfilUsuarioViewModel
                            {
                                IdPerfil = x.int_id_perfil,
                                Clave = x.nvarchar_clave,
                                Nombre = x.nvarchar_nombre,
                                Descripcion = x.nvarchar_descripcion,
                                Seleccionado = perfilesUsuario.Contains(x.int_id_perfil)
                            }
                        )
                        .ToList();
            }

            return resultado;
        }


        public BasicOperationResponse GuardarUsuarioSistema(string login, List<int> idsPerfiles, string usuarioModificacion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(login))
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "Debe indicar el usuario."
                    };
                }

                login = login.Trim().ToUpper();

                UsuarioInstitucionalDto usuarioInstitucional = BuscarUsuarioInstitucional(login);

                if (usuarioInstitucional == null)
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "El usuario no existe en la base institucional de Accesos."
                    };
                }

                if (idsPerfiles == null)
                {
                    idsPerfiles = new List<int>();
                }

                idsPerfiles =
                    idsPerfiles
                        .Distinct()
                        .ToList();

                using (Objetivos_Prioritarios_Perfiles_Entities db = new Objetivos_Prioritarios_Perfiles_Entities())
                {
                    using (var transaccion = db.Database.BeginTransaction())
                    {
                        try
                        {
                            tb_Usuarios usuario =
                                db.tb_Usuarios
                                    .FirstOrDefault(x =>
                                        x.nvarchar_no_interno != null &&
                                        x.nvarchar_no_interno.Trim().ToUpper() == login
                                    );

                            if (usuario == null)
                            {
                                usuario = new tb_Usuarios
                                {
                                    nvarchar_nombre_usuario = usuarioInstitucional.NombreCompleto,
                                    nvarchar_no_interno = usuarioInstitucional.Login,
                                    nvarchar_puesto = usuarioInstitucional.Puesto,
                                    date_fecha_alta = DateTime.Now,
                                    bit_status = true,
                                    date_fecha_modificacion = null,
                                    nvarchar_usuario_modificacion = usuarioModificacion
                                };

                                db.tb_Usuarios.Add(usuario);

                                db.SaveChanges();
                            }
                            else
                            {
                                usuario.nvarchar_nombre_usuario =
                                    usuarioInstitucional.NombreCompleto;

                                usuario.nvarchar_puesto =
                                    usuarioInstitucional.Puesto;

                                usuario.bit_status =
                                    true;

                                usuario.date_fecha_modificacion =
                                    DateTime.Now;

                                usuario.nvarchar_usuario_modificacion =
                                    usuarioModificacion;

                                db.SaveChanges();
                            }

                            int idUsuario =
                                usuario.int_id_usuario;

                            /*
                             * ========================================================
                             * DESACTIVAR PERFILES QUE YA NO FUERON SELECCIONADOS
                             * ========================================================
                             */

                            List<tb_UsuarioPerfil> perfilesActuales =
                                db.tb_UsuarioPerfil
                                    .Where(x =>
                                        x.int_id_usuario == idUsuario
                                    )
                                    .ToList();

                            foreach (tb_UsuarioPerfil perfilActual in perfilesActuales)
                            {
                                bool seleccionado =
                                    idsPerfiles.Contains(
                                        perfilActual.int_id_perfil
                                    );

                                if (perfilActual.bit_status != seleccionado)
                                {
                                    perfilActual.bit_status =
                                        seleccionado;

                                    perfilActual.date_fecha_modificacion =
                                        DateTime.Now;

                                    perfilActual.nvarchar_usuario_modificacion =
                                        usuarioModificacion;
                                }
                            }

                            /*
                             * ========================================================
                             * AGREGAR PERFILES QUE TODAVÍA NO EXISTEN
                             * ========================================================
                             */

                            foreach (int idPerfil in idsPerfiles)
                            {
                                bool existe =
                                    perfilesActuales.Any(x =>
                                        x.int_id_perfil == idPerfil
                                    );

                                if (existe)
                                {
                                    continue;
                                }

                                bool perfilValido =
                                    db.cat_Perfil.Any(x =>
                                        x.int_id_perfil == idPerfil &&
                                        x.bit_status == true
                                    );

                                if (!perfilValido)
                                {
                                    continue;
                                }

                                db.tb_UsuarioPerfil.Add(
                                    new tb_UsuarioPerfil
                                    {
                                        int_id_usuario = idUsuario,
                                        int_id_perfil = idPerfil,
                                        bit_status = true,
                                        date_fecha_alta = DateTime.Now,
                                        nvarchar_usuario_modificacion = usuarioModificacion
                                    }
                                );
                            }

                            db.SaveChanges();

                            transaccion.Commit();

                            return new BasicOperationResponse
                            {
                                IsSuccess = true,
                                Message = "Usuario y perfiles guardados correctamente."
                            };
                        }
                        catch
                        {
                            transaccion.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al guardar el usuario. " + ex.Message
                };
            }
        }


        public BasicOperationResponse CambiarEstatusUsuario(string login, bool activo, string usuarioModificacion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(login))
                {
                    return new BasicOperationResponse
                    {
                        IsSuccess = false,
                        Message = "Debe indicar el usuario."
                    };
                }

                login = login.Trim().ToUpper();

                using (Objetivos_Prioritarios_Perfiles_Entities db = new Objetivos_Prioritarios_Perfiles_Entities())
                {
                    tb_Usuarios usuario =
                        db.tb_Usuarios
                            .FirstOrDefault(x =>
                                x.nvarchar_no_interno != null &&
                                x.nvarchar_no_interno.Trim().ToUpper() == login
                            );

                    if (usuario == null)
                    {
                        return new BasicOperationResponse
                        {
                            IsSuccess = false,
                            Message = "El usuario no está registrado en Objetivos Prioritarios."
                        };
                    }

                    usuario.bit_status =
                        activo;

                    usuario.date_fecha_modificacion =
                        DateTime.Now;

                    usuario.nvarchar_usuario_modificacion =
                        usuarioModificacion;

                    db.SaveChanges();

                    return new BasicOperationResponse
                    {
                        IsSuccess = true,
                        Message =
                            activo
                                ? "Usuario activado correctamente."
                                : "Usuario dado de baja correctamente."
                    };
                }
            }
            catch (Exception ex)
            {
                return new BasicOperationResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al actualizar el usuario. " + ex.Message
                };
            }
        }


    }
}