using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Objetivos_Prioritarios.Helpers
{
    public class UserCredentials
    {
        private readonly string _domain;
        private readonly string _username;
        private readonly string _password;

        public UserCredentials(string domain, string username, string password)
        {
            _domain = domain;
            _username = username;
            _password = password;
        }

        public SafeAccessTokenHandle LogonUser(LogonType logonType)
        {
            SafeAccessTokenHandle safeAccessTokenHandle;

            bool returnValue = LogonUser(
                _username,
                _domain,
                _password,
                (int)logonType,
                0,
                out safeAccessTokenHandle
            );

            if (!returnValue)
            {
                int error = Marshal.GetLastWin32Error();
                throw new Win32Exception(error, "No se pudo iniciar sesión con el usuario de red.");
            }

            return safeAccessTokenHandle;
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out SafeAccessTokenHandle phToken
        );
    }

    public enum LogonType
    {
        Interactive = 2,
        Network = 3,
        Batch = 4,
        Service = 5,
        NetworkCleartext = 8,
        NewCredentials = 9
    }
}