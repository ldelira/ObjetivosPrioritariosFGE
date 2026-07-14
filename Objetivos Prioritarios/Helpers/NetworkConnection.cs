using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Objetivos_Prioritarios.Helpers
{
    public class NetworkConnection : IDisposable
    {
        private readonly string _networkName;
        private bool _disposed;

        public NetworkConnection(string networkName, string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(networkName))
                throw new ArgumentException("La ruta de red no puede estar vacía.", nameof(networkName));

            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("El usuario de red no puede estar vacío.", nameof(userName));

            _networkName = networkName;

            var netResource = new NetResource
            {
                Scope = ResourceScope.GlobalNetwork,
                ResourceType = ResourceType.Disk,
                DisplayType = ResourceDisplaytype.Share,
                RemoteName = networkName
            };

            int result = WNetAddConnection2(
                netResource,
                password,
                userName,
                0
            );

            // 1219 = Ya existe conexión al recurso con otras credenciales
            if (result == 1219)
            {
                WNetCancelConnection2(_networkName, 0, true);

                result = WNetAddConnection2(
                    netResource,
                    password,
                    userName,
                    0
                );
            }

            if (result != 0)
            {
                throw new Win32Exception(result, "Error al conectar a la ruta de red: " + networkName);
            }
        }

        ~NetworkConnection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            WNetCancelConnection2(_networkName, 0, true);
            _disposed = true;
        }

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(
            NetResource netResource,
            string password,
            string username,
            int flags
        );

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(
            string name,
            int flags,
            bool force
        );

        [StructLayout(LayoutKind.Sequential)]
        private class NetResource
        {
            public ResourceScope Scope;
            public ResourceType ResourceType;
            public ResourceDisplaytype DisplayType;
            public int Usage;
            public string LocalName;
            public string RemoteName;
            public string Comment;
            public string Provider;
        }

        private enum ResourceScope : int
        {
            Connected = 1,
            GlobalNetwork,
            Remembered,
            Recent,
            Context
        }

        private enum ResourceType : int
        {
            Any = 0,
            Disk = 1,
            Print = 2,
            Reserved = 8
        }

        private enum ResourceDisplaytype : int
        {
            Generic = 0,
            Domain = 1,
            Server = 2,
            Share = 3,
            File = 4,
            Group = 5,
            Network = 6,
            Root = 7,
            Shareadmin = 8,
            Directory = 9,
            Tree = 10,
            Ndscontainer = 11
        }
    }
}