using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Omega.Core.Service
{
    public static class ApplicationHostHelpers
    {
        private static ApplicationBase _applicationBase { get; set; }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        public static void HookApplication(ApplicationBase applicationBase)
        {
            if (_applicationBase != null)
                UnHookApplication(_applicationBase);

            _applicationBase = applicationBase;

            AppDomain.CurrentDomain.UnhandledException += _applicationBase.UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += _applicationBase.ProcessExit;
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        public static void UnHookApplication(ApplicationBase applicationBase)
        {
            AppDomain.CurrentDomain.UnhandledException -= _applicationBase.UnhandledException;
            AppDomain.CurrentDomain.ProcessExit -= _applicationBase.ProcessExit;
        }

        public static bool IsRunning(string applicationName)
        {
            var isFirstInstance = false;
            var windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();

            if (windowsIdentity != null)
            {
                var userName = windowsIdentity.Name.Replace("\\", string.Empty);
                var m = new Mutex(true, "Local\\" + applicationName + "." + userName, out isFirstInstance);

                GC.KeepAlive(m);
            }

            return !isFirstInstance;
        }
    }
}
