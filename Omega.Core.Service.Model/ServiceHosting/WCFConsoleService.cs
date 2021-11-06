using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Omega.Core.Service.Model.ServiceHosting
{
    /// <summary>
    /// Базовый интерфейс консольного приложения 
    /// </summary>
    public class WCFConsoleService
        : Component
    {
        private WCFServiceApplicationBase _serviceApp;
        public WCFConsoleService(WCFServiceApplicationBase serviceApp)
        {
            if (serviceApp == null)
                throw new ArgumentNullException("serviceApp");
            _serviceApp = serviceApp;
        }
        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnStart()
        {
            _serviceApp.StartWCFServices();

            foreach (ServiceHost host in _serviceApp.GetHosts())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Service: {0}", host.Description.ServiceType.Name);
                foreach (Uri uri in host.BaseAddresses)
                    Console.WriteLine("\tBase address: {0}", uri.AbsoluteUri);
                foreach (System.ServiceModel.Description.ServiceEndpoint endPoint in host.Description.Endpoints)
                {
                    Console.WriteLine("\tEndpoint: {0}", endPoint.Name);
                    Console.WriteLine("\t\tEndpoint address: {0}", endPoint.Address);
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Database connection:");
                Console.WriteLine("\tDataSource: {0}", _serviceApp.Connection.DataSource);
                Console.WriteLine("\tInitialCatalog: {0}", _serviceApp.Connection.InitialCatalog);

#if(DEBUG || STAND)
                if (_serviceApp.Connection.DataSource.ToUpper().Contains("M-DTS01") == false)
                    if (MessageBox.Show(string.Format("Сервис подключён в отладочном режиме к базе:{0}. Продолжить работу?", _serviceApp.Connection.DataSource),
                                        "Внимание!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, MessageBoxOptions.ServiceNotification) == DialogResult.No)
                        Environment.Exit(-1);
#endif

                Console.ResetColor();
                Console.WriteLine();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnStop()
        {
            if (_serviceApp != null)
            {
                _serviceApp.Dispose();
                _serviceApp = null;
            }
        }
        /// <summary>
        /// Выполняет запуск консольного приложения сервиса
        /// </summary>
        public void Run()
        {
            Win32PInvoke.SetConsoleCtrlHandler(new Win32PInvoke.HandlerRoutine((ctrlType) =>
            {
                switch (ctrlType)
                {
                    case Win32PInvoke.CtrlTypes.CTRL_CLOSE_EVENT:
                    case Win32PInvoke.CtrlTypes.CTRL_C_EVENT:
                    case Win32PInvoke.CtrlTypes.CTRL_BREAK_EVENT:
                        return true;
                }
                return false;
            }), true);


            OnStart();

            Console.WriteLine("Сервис {0} стартовал", _serviceApp.ApplicationName);
            Console.WriteLine("Нажмите CTRL+B для завершения...");

            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey(true);
            }
            while (!(keyInfo.Key == ConsoleKey.B && keyInfo.Modifiers == ConsoleModifiers.Control));

            OnStop();

            Environment.Exit(0);
        }

        protected override void Dispose(bool disposing)
        {
            OnStop();
            base.Dispose(disposing);
        }

        protected class Win32PInvoke
        {
            public enum CtrlTypes
            {
                CTRL_C_EVENT = 0,
                CTRL_BREAK_EVENT,
                CTRL_CLOSE_EVENT,
                CTRL_LOGOFF_EVENT = 5,
                CTRL_SHUTDOWN_EVENT
            }


            public delegate Boolean HandlerRoutine(CtrlTypes ctrlType);

            [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "SetConsoleCtrlHandler")]
            public static extern Boolean SetConsoleCtrlHandler(HandlerRoutine Handler, Boolean Add);

        }
    }
}
