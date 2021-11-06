using System;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using AppConfiguration = Omega.Core.Service.Model.Configuration;
using SysConfiguration = System.Configuration;
using SysConfigurationInstaller = System.Configuration.Install;

namespace Omega.Core.Service.Model.WindowsServices
{
    public class ServiceInstallerBase
        : SysConfigurationInstaller.Installer
    {
        public const string WINDOW_SERVICE_SECTION_NAME = "windowService";

        public const string ERROR_NOTFOUND_SECTION_IN_CONFIGURATION = "Нет секции {0} в файле конфигурации: {1}";

        private readonly ServiceProcessInstaller _processInstaller;
        private readonly ServiceInstaller _serviceInstaller;

        public ServiceInstallerBase()
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                return;

            /*Получение имени конфигурационого файла (Текущий код выполняется под управлением InstallUtil и метод SysConfiguration.ConfigurationManager.OpenExeConfiguration возвращает его конфигурацию)*/
            string fileName = GetType().Assembly.Location;
            string directoryname = Path.GetDirectoryName(fileName);

            ResolveEventHandler resolveDelegate = (sender, args) =>
            {
                if (args.Name.StartsWith("EmEx"))
                {
                    if (directoryname != null)
                    {
                        string fullAssemblyName = Path.Combine(directoryname, string.Concat(args.Name, ".dll"));
                        return Assembly.LoadFile(fullAssemblyName);
                    }
                }
                return null;
            };
            AppConfiguration.WindowServicesSection section;
            System.Configuration.Configuration config = SysConfiguration.ConfigurationManager.OpenExeConfiguration(fileName);
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += resolveDelegate;
                section = config.Sections[WINDOW_SERVICE_SECTION_NAME] as AppConfiguration.WindowServicesSection;
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= resolveDelegate;
            }

            if (section == null)
                throw new SysConfiguration.ConfigurationErrorsException(String.Format(ERROR_NOTFOUND_SECTION_IN_CONFIGURATION, WINDOW_SERVICE_SECTION_NAME, config.FilePath));

            _processInstaller = new ServiceProcessInstaller
            {
                Account = section.ServiceAccount,
            };
            if (_processInstaller.Account == ServiceAccount.User)
            {
                if (string.IsNullOrWhiteSpace(section.Username))
                    throw new SysConfiguration.ConfigurationErrorsException(String.Format(ERROR_NOTFOUND_SECTION_IN_CONFIGURATION, string.Concat(WINDOW_SERVICE_SECTION_NAME, ".", AppConfiguration.WindowServicesSection.USERNAME_ELEMENT_NAME), config.FilePath));

                _processInstaller.Username = section.Username;
                _processInstaller.Password = section.Password;
            }

            _serviceInstaller = new ServiceInstaller
            {
                DelayedAutoStart = section.DelayedAutoStart,
                Description = section.Description,
                DisplayName = section.DisplayName,
                ServiceName = section.ServiceName,
                StartType = section.StartMode,
            };

            Installers.Add(_processInstaller);
            Installers.Add(_serviceInstaller);
        }
    }
}
