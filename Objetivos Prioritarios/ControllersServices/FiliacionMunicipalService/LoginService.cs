using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using Web_SIPAEIC.ControllerServices;
using Web_SIPAEIC.Utils;

namespace FiliacionMunicipal.ControllerServices
{
    public class LoginService : BaseService
    {
        public BasicOperationResponse validateCredentialsToaccesss(string user, string pass)
        {
            try
            {
                var Valida_User = db.tb_Usuario.Where(x => x.Usuario == user).FirstOrDefault();
                if (Valida_User != null)
                {
                    try
                    {
                        if (Valida_User.Activo)
                        {
                            var Sede_User = db.tb_UsuarioSede.Where(x => x.idUsuario == Valida_User.idUsuario).FirstOrDefault();
                            if (Sede_User != null)
                            {
                                var Sede = db.cat_SedesPoliciales.Where(x => x.idSede == Sede_User.idSede).FirstOrDefault();
                                var Muni = bdCatalogos.Municipio.Where(x => x.Cve_mun == Sede.idMunicipio).FirstOrDefault();
                                if (Encriptar(pass) == Valida_User.Contrasena)
                                //if (pass == Valida_User.Contrasena)
                                {
                                    return new BasicOperationResponse() { IsSuccess = true, Message = "Acceso correcto al sistema", user = Valida_User, IdSede = Sede.idMunicipio, Mun = Muni.Municipio1 };
                                }
                                else
                                {
                                    return new BasicOperationResponse() { IsSuccess = false, Message = "Contraseña incorrecta favor de verificar." };
                                }
                            }
                            else
                            {
                                return new BasicOperationResponse() { IsSuccess = false, Message = "Usuario sin Asignar" };
                            }
                        }
                        else
                        {
                            return new BasicOperationResponse() { IsSuccess = false, Message = "Usuario dado de Baja" };
                        }
                    }
                    catch (Exception ex)
                    {
                        return new BasicOperationResponse() { IsSuccess = false, Message = "A ocurrido un error al acceder al sistema." + Environment.NewLine + "  Error Code 2:" + Environment.NewLine +  ex.Message};
                    }
                }
                else
                {
                    return new BasicOperationResponse() { IsSuccess = false, Message = "Usuario no existe en el sistema favor de verificar" };
                }
            }
            catch (Exception e)
            {
                return new BasicOperationResponse() { IsSuccess = false, Message = "A ocurrido un error al acceder al sistema." + Environment.NewLine + " Error Code 1:" + Environment.NewLine  + e.Message};
            }
        }
        public string Encriptar(string texto)
        {
            byte[] A = System.Text.Encoding.UTF8.GetBytes(texto);
            var B= System.Convert.ToBase64String(A);
            byte[] A2 = System.Text.Encoding.UTF8.GetBytes(B);
            return BitConverter.ToString(A2).Replace("-", "");
        }
    }
}
