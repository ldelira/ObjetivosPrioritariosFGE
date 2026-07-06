using Antlr.Runtime.Misc;
using FiliacionMunicipal.Models;
using FiliacionMunicipal.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Net.NetworkInformation;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Web_SIPAEIC.ControllerServices;

namespace FiliacionMunicipal.ControllerServices
{
    public class AdminService : BaseService
    {
        public List<UsuarioVM> ListarUsuarios(bool ban)
        {
            if (ban)
            {
                return db.tb_Usuario.Select(u => new UsuarioVM
                {
                    IdUsuario = u.idUsuario,
                    Nombre = u.Nombre,
                    Paterno = u.Paterno,
                    Materno = u.Materno,
                    Usuario = u.Usuario,
                    Contrasena = u.Contrasena,
                    Activo = u.Activo
                }).ToList();
            }
            else {
                // 1. Obtenemos los IDs de los usuarios que ya están en tb_UsuarioSede y están activos ahí
                var usuariosConSede = db.tb_UsuarioSede
                                        .Where(us => us.Activo == true)
                                        .Select(us => us.idUsuario);

                // 2. Traemos los usuarios que NO están en esa lista
                return db.tb_Usuario
                    .Where(u => u.Activo == true && !usuariosConSede.Contains(u.idUsuario))
                    .Select(u => new UsuarioVM
                    {
                        IdUsuario = u.idUsuario,
                        Nombre = u.Nombre + " " + u.Paterno + " " + u.Materno,
                        Usuario = u.Usuario,
                        Activo = u.Activo
                    }).ToList();
            }

        }

        public void Guardar(UsuarioVM model)
        {
            try
            {
             //Usuario
                tb_Usuario usuarios;
                if (model.IdUsuario > 0)
                {
                    // EDITAR
                    usuarios = db.tb_Usuario.Find(model.IdUsuario);

                    if (usuarios == null)
                        throw new Exception("Usuario no encontrado");

                    usuarios.Nombre = model.Nombre;
                    usuarios.Paterno = model.Paterno;
                    usuarios.Materno = model.Materno;
                    usuarios.Usuario = model.Usuario;
                    
                    if(model.Contrasena!="null")
                    usuarios.Contrasena = Encriptar(model.Contrasena);

                    usuarios.FechaRegistro = DateTime.Now;
                    usuarios.UsuarioRegistro = Convert.ToInt32(HttpContext.Current.Session["idUsuario"]?.ToString()); 
                }
                else
                {
                    //NUEVO
                    usuarios = new tb_Usuario
                    {
                        Nombre = model.Nombre,
                        Paterno = model.Paterno,
                        Materno = model.Materno,
                        Usuario = model.Usuario,
                        Contrasena = Encriptar(model.Contrasena),
                        FechaRegistro = DateTime.Now,
                        UsuarioRegistro = Convert.ToInt32(HttpContext.Current.Session["idUsuario"]?.ToString()),
                        Activo = true
                    };

                    db.tb_Usuario.Add(usuarios);
                }
                db.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }

        }
        public string Encriptar(string texto)
        {
            byte[] A = System.Text.Encoding.UTF8.GetBytes(texto);
            var B = System.Convert.ToBase64String(A);
            byte[] A2 = System.Text.Encoding.UTF8.GetBytes(B);
            return BitConverter.ToString(A2).Replace("-", "");
        }

        public static string Des(string txt)
        {
            if (string.IsNullOrEmpty(txt)) return string.Empty;

            try
            {
                byte[] A2 = new byte[txt.Length / 2];
                for (int i = 0; i < txt.Length; i += 2)
                {
                    A2[i / 2] = Convert.ToByte(txt.Substring(i, 2), 16);
                }

                byte[] A = System.Convert.FromBase64String(Encoding.UTF8.GetString(A2));
                return System.Text.Encoding.UTF8.GetString(A);
            }
            catch
            {
                return "Error: El formato no es válido.";
            }
        }
        public void Estatus(int id,bool ban, string TB)
        {
            try
            {
                var tipoEntidad = Type.GetType("FiliacionMunicipal.Models." + TB);
                if (tipoEntidad == null) throw new Exception("La tabla no existe");

                //tb_Usuario usuarios;
                if (id > 0)
                {
                    // EDITAR
                    var registro = db.Set(tipoEntidad).Find(id);
                    if (registro == null) throw new Exception("Registro no encontrado");

                    // 3. Usamos Reflection para asignar valores a las columnas comunes
                    // Esto busca la propiedad por nombre y le asigna el valor si existe
                    tipoEntidad.GetProperty("Activo")?.SetValue(registro, ban);
                    tipoEntidad.GetProperty("FechaRegistro")?.SetValue(registro, DateTime.Now);

                    var idSesion = Convert.ToInt32(HttpContext.Current.Session["idUsuario"]);
                    tipoEntidad.GetProperty("UsuarioRegistro")?.SetValue(registro, idSesion);

                    //usuarios = db.tb_Usuario.Find(id);

                    //if (usuarios == null)
                    //    throw new Exception("Usuario no encontrado");

                    //usuarios.Activo = ban;

                    //usuarios.FechaRegistro = DateTime.Now;
                    //usuarios.UsuarioRegistro = Convert.ToInt32(HttpContext.Current.Session["idUsuario"]?.ToString());

                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<AlertasVM> ListarAlertas()
        {
            var listaAlertas = db.Database.SqlQuery<AlertasVM>("EXEC sp_Alertas").ToList();

            foreach (var item in listaAlertas)
            {
                if (!string.IsNullOrEmpty(item.ArchivoB64))
                {
                    // Limpiamos la extensión (quitamos el punto y pasamos a minúsculas)
                    string ext = item.RutaArchivo?.ToLower().Replace(".", "") ?? "jpg";

                    // Determinamos el MIME type
                    string mime = (ext == "png") ? "image/png" : "image/jpeg";

                    // Creamos la URL lista para el <img> de HTML
                    item.Ruta = $"data:{mime};base64,{item.ArchivoB64}";
                }
            }
            return listaAlertas;
        }


        public List<SedesVM> ListarSedes()
        {
            return db.Database.SqlQuery<SedesVM>(
                @"SELECT [idSede]
      ,[Sede]
      ,[idMunicipio]
	  ,M.Municipio
      ,[Activo]
  FROM [Filiacion_Municipios].[dbo].[cat_SedesPoliciales] SP
  INNER JOIN [Catalogos].[dbo].[Municipio] M ON SP.idMunicipio=M.Cve_mun").ToList();

        }
        public void GuardarSedes(SedesVM model)
        {
            try
            {
                cat_SedesPoliciales sedes;
                if (model.idSede > 0)
                {
                    // EDITAR
                    sedes = db.cat_SedesPoliciales.Find(model.idSede);

                    if (sedes == null)
                        throw new Exception("Sede no encontrada");

                    sedes.Sede = model.Sede;
                    sedes.idMunicipio = model.idMunicipio;

                }
                else
                {
                    //NUEVO
                    sedes = new cat_SedesPoliciales
                    {
                        Sede = model.Sede,
                        idMunicipio = model.idMunicipio,
                        Activo = true
                    };

                    db.cat_SedesPoliciales.Add(sedes);
                }
                db.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }

        }

        public List<UsuarioSedeVM> UsuariosSede(int IDsede)
        {
            return db.Database.SqlQuery<UsuarioSedeVM>(
                @"SELECT US.idUsuario 
                  ,[idSede]
	              ,ISNULL(U.Nombre, '') + ' ' + ISNULL(U.Paterno, '') + ' ' + ISNULL(U.Materno, '') AS Nombre
                  ,US.Activo
              FROM [Filiacion_Municipios].[dbo].[tb_UsuarioSede] US
              INNER JOIN [Filiacion_Municipios].[dbo].[tb_Usuario] U ON US.idUsuario=U.idUsuario
              WHERE idSede=" + IDsede)
                .ToList();

        }
        public void AgregarUsuarioSede(UsuarioSedeVM model)
        {
            try
            {
                var sedesU = new tb_UsuarioSede
                {
                    idUsuario = model.idUsuario,
                    idSede = model.idSede,

                    Activo = true,
                    FechaRegistro = DateTime.Now,
                    UsuarioRegistro = Convert.ToInt32(HttpContext.Current.Session["idUsuario"]?.ToString())
                };
                db.tb_UsuarioSede.Add(sedesU);

                db.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}

