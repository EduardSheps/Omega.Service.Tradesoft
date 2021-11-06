using System;
using System.ServiceProcess;
using SysConfiguration = System.Configuration;
using AppConfiguration = Omega.Core.Service.Model.Configuration;
using Omega.Core.Service.Model.WindowsServices;

namespace Omega.Core.Service.Model.ServiceHosting
{
    // <summary>
    /// Базовый интерфейс приложения в виде системного сервиса
    /// </summary>
    public sealed class WCFService : ServiceBase
    {
        private WCFServiceApplicationBase _serviceApp;
        public const string ERROR_NOTFOUND_SECTION_IN_CONFIGURATION = "Нет секции {0} в файле конфигурации: {1}";

        public WCFService()
        {
            SysConfiguration.Configuration config = SysConfiguration.ConfigurationManager.OpenExeConfiguration(SysConfiguration.ConfigurationUserLevel.None);
            AppConfiguration.WindowServicesSection section = config.Sections[ServiceInstallerBase.WINDOW_SERVICE_SECTION_NAME] as AppConfiguration.WindowServicesSection;
            if (section == null)
                throw new SysConfiguration.ConfigurationErrorsException(String.Format(ERROR_NOTFOUND_SECTION_IN_CONFIGURATION, ServiceInstallerBase.WINDOW_SERVICE_SECTION_NAME, config.FilePath));

            ServiceName = section.ServiceName;
        }
        public WCFService(WCFServiceApplicationBase serviceApp)
            : this()
        {
            if (serviceApp == null)
                throw new ArgumentNullException("serviceApp");
            _serviceApp = serviceApp;
        }
        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            _serviceApp.StartWCFServices();
        }

        protected override void OnPause()
        {
            base.OnPause();
            _serviceApp.SuspendWCFServices();
        }

        protected override void OnContinue()
        {
            base.OnContinue();
            _serviceApp.ResumeWCFServices();
        }

        protected override void OnStop()
        {
            base.OnStop();
            _serviceApp.StopWCFServices();
        }
    }
}
