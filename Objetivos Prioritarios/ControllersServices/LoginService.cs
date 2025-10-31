using Objetivos_Prioritarios.Utils;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Objetivos_Prioritarios.ControllersServices
{
    public class LoginService : BaseService
    {
        public BasicOperationResponse validateCredentialsToaccesss(string user, string pass)
        {
            try
            {

                int unidadId = 0;
                var res = db.tb_Usuarios.Where(x => x.nvarchar_no_interno == user).FirstOrDefault();


                if (res != null)
                {
                    bool isValid = true;
                    bool entro = false;
                    try
                    {


                        using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, "pgj.gob")) //No need to add LDAP:// with the domain
                        {
                            // validate the credentials
                            isValid = pc.ValidateCredentials(user, pass);
                            if (isValid)
                            {

                                return new BasicOperationResponse() { IsSuccess = true, Message = "Acceso correcto al sistema", user = res, Id = unidadId };

                            }
                            else
                            {
                                return new BasicOperationResponse() { IsSuccess = false, Message = "Contraseña incorrecta favor de verificar." };
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        return new BasicOperationResponse() { IsSuccess = false, Message = "A ocurrido un error al acceder al sistema Error Code 2 (" + ex.Message + ")" };
                    }
                }
                else
                {
                    return new BasicOperationResponse() { IsSuccess = false, Message = "Usuario no existe en el sistema favor de verificar (Error Code 1)" };
                }
            }
            catch (Exception e)
            {
                return new BasicOperationResponse() { IsSuccess = false, Message = "A ocurrido un error al acceder al sistema Error Code 1 (" + e.Message + ")" };
            }
        }
    }
}