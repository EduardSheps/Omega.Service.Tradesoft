using Omega.Core.Service.Model.ServiceHosting;
using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Omega.Service.Tradesoft;
using Mxg.Jobs;
using Omega.Job.Tradesoft;

namespace Omega.Service.Tradesoft.Host
{
    public class Program
    {
        static void Main(string[] args)
        {

            var application = new TradesoftServiceApplication();

            if (args.Select(x => x.ToLower()).Contains("/install"))
            {
                var fileName = Assembly.GetExecutingAssembly().Location;
                ManagedInstallerClass.InstallHelper(new[] { fileName, "/LogToConsole=true", "/ShowCallStack" });
            }
            else if (args.Select(x => x.ToLower()).Contains("/uninstall"))
            {
                var fileName = Assembly.GetExecutingAssembly().Location;
                ManagedInstallerClass.InstallHelper(new[] { "/uninstall", fileName, "/LogToConsole=true", "/ShowCallStack" });
            }
            else if (Environment.UserInteractive || Debugger.IsAttached || args.Select(x => x.ToLower()).Contains("/console"))
            {
                new WCFConsoleService(application).Run();
            }
            else
            {
                var servicesToRun = new ServiceBase[]
                    {
                        new WCFService(application)
                    };
                ServiceBase.Run(servicesToRun);
            }
        }

       

        
    }
}
