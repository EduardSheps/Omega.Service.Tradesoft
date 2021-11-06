using Omega.Core.Service.Model;
using System;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using SysConfiguration = System.Configuration;

namespace Omega.Service.Tradesoft
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class TradesoftService: WCFServiceBase, ITradesoftService
    {
        private const string ERROR_NOTFOUND_SECTION_IN_CONFIGURATION = "Нет секции {0} в файле конфигурации: {1}";


        public TradesoftService()
        {
            SysConfiguration.Configuration config = SysConfiguration.ConfigurationManager.OpenExeConfiguration(SysConfiguration.ConfigurationUserLevel.None);
            var section = config.Sections[Omega.Core.Service.Model.WindowsServices.ServiceInstallerBase.WINDOW_SERVICE_SECTION_NAME] as Omega.Core.Service.Model.Configuration.WindowServicesSection;
            if (section == null)
                throw new SysConfiguration.ConfigurationErrorsException(String.Format(ERROR_NOTFOUND_SECTION_IN_CONFIGURATION, Omega.Core.Service.Model.WindowsServices.ServiceInstallerBase.WINDOW_SERVICE_SECTION_NAME, config.FilePath));

            ServiceName = section.ServiceName;
        }

        public void Test()
        {
            
        }

        ~TradesoftService()
        {
            Dispose(false);
        }

    }
}
