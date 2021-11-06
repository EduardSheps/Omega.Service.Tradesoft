using Omega.Core.Data.Application;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Omega.Core.Service.Model
{
    /// <summary>
    /// Предоставляет абстрактный класс для реализации методов и свойства базового приложения поддерживаюшего сервисы WCF
    /// </summary>
    public abstract class WCFServiceApplicationBase
        : DataLayerApplicationBase
    {
        /// <summary>
        /// Текущий экземпляр приложения-сервиса
        /// </summary>
        public static new WCFServiceApplicationBase Current { get; private set; }

        private enum WCFServicesStatus { NotLoaded, Running, Suspended, Stopped };

        private volatile WCFServicesStatus _currentServiceStatus;

        private readonly List<ServiceHost> _hostList;

        protected WCFServiceApplicationBase()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

            Current = this;

            _currentServiceStatus = WCFServicesStatus.NotLoaded;
            _hostList = new List<ServiceHost>();

            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = GetType().Name;
            }
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            //LogStorage.RegisterException(exception ?? new Exception(e.ExceptionObject.ToString()), "Domain Unhandled Exception");
            //LogStorage.WaitForAllMessageIsSave();
        }

        /// <summary>
        /// Регистрирует новый WCF сервис в системе по его типу
        /// </summary>
        /// <param name="serviceType"></param>
        public void RegisterHost(Type serviceType)
        {
            RegisterHost(new ServiceHost(serviceType));
        }

        /// <summary>
        /// Регистрирует новый WCF сервис в системе по его экземпляру
        /// </summary>
        /// <param name="serviceInstance"></param>
        public void RegisterHost(WCFServiceBase serviceInstance)
        {
            RegisterHost(new ServiceHost(serviceInstance));
        }

        /// <summary>
        /// Регистрирует новый WCF сервис в системе по <see cref="ServiceHost"/>
        /// </summary>
        public void RegisterHost(ServiceHost host)
        {
            if (!_hostList.Select(x => x.Description.ServiceType).Contains(host.Description.ServiceType))
            {
                _hostList.Add(host);
            }
        }

        /// <summary>
        /// Возвращает WCF сервис загеристрированный в системе
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public ServiceHost GetHost(Type serviceType)
        {
            return this._hostList.FirstOrDefault(x => x.Description.ServiceType == serviceType);
        }

        /// <summary>
        /// Возвращает коллекцию WСF сервисов
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<ServiceHost> GetHosts()
        {
            return new ReadOnlyCollection<ServiceHost>(_hostList);
        }


        /// <summary>
        /// Запускает зарегистрованные WCF сервисы
        /// </summary>
        public void StartWCFServices()
        {
            OnStarting();
            ParallelLoopResult result = Parallel.ForEach(GetHosts(), (host, state) =>
            {
                try
                {
                    OnStartingWCFServices(host);
                    host.Open();
                    OnStartedWCFServices(host, false);
                }
                catch (Exception ex)
                {
                    //LogStorage.RegisterException(ex, "WCFServiceApplicationBase::StartWCFServices");
                    state.Break();
                }
            });
            if (!result.IsCompleted)
            {
                //LogStorage.WaitForAllMessageIsSave();
                Environment.Exit(1);
            }
            else
            {
                _currentServiceStatus = WCFServicesStatus.Running;

                OnStarted();
            }
        }

        /// <summary>
        /// Завершает работу WCF сервисов
        /// </summary>
        public void StopWCFServices()
        {
            if (_currentServiceStatus == WCFServicesStatus.NotLoaded || _currentServiceStatus == WCFServicesStatus.Stopped)
                return;

            OnStoping();

            Parallel.ForEach(GetHosts(), host =>
            {
                try
                {
                    OnStopingWCFServices(host, false);
                    if (_currentServiceStatus == WCFServicesStatus.Running)
                        host.Close();
                    ((IDisposable)host).Dispose();
                }
                catch (Exception ex)
                {
                    //LogStorage.RegisterException(ex, "WCFServiceApplicationBase::StopWCFServices");
                }
            });
            _hostList.Clear();

            _currentServiceStatus = WCFServicesStatus.Stopped;

            OnStoped();

            /* Ожидание, когда будут записаны все логи */
            //LogStorage.WaitForAllMessageIsSave();
        }

        /// <summary>
        /// Приостанавливает работу WCF сервисов
        /// </summary>
        public void SuspendWCFServices()
        {
            OnSuspend();
            Parallel.ForEach(GetHosts(), host =>
            {
                try
                {
                    OnStopingWCFServices(host, true);
                    if (_currentServiceStatus == WCFServicesStatus.Running)
                        host.Close();
                    ((IDisposable)host).Dispose();
                }
                catch (Exception ex)
                {
                    //LogStorage.RegisterException(ex, "WCFServiceApplicationBase::StopWCFServices");
                }
            });
            _currentServiceStatus = WCFServicesStatus.Suspended;
        }

        /// <summary>
        /// Восстанавливает работу WCF сервисов
        /// </summary>
        public void ResumeWCFServices()
        {
            OnResume();
            Parallel.ForEach(GetHosts(), host =>
            {
                try
                {
                    if (_currentServiceStatus == WCFServicesStatus.Suspended)
                        host.Open();
                    OnStartedWCFServices(host, true);
                }
                catch (Exception ex)
                {
                    //LogStorage.RegisterException(ex, "WCFServiceApplicationBase::StopWCFServices");
                }
            });
            _currentServiceStatus = WCFServicesStatus.Suspended;
        }

        /// <summary>
        /// При переопределении в производном классе представляет метод, который будет вызван перез запуском WCF сервисов зарегистрованных в системе
        /// </summary>
        /// <remarks>
        /// Метод можно использовать для регистрации WCF сервисов в системе.
        /// При этом можно быть уверенным, что поднята вся инфрасткрутура системы
        /// </remarks>
        protected virtual void OnStarting() { }

        /// <summary>
        /// При переопределении в производном классе представляет метод, который будет вызван после запуска WCF сервисов в системе
        /// </summary>
        protected virtual void OnStarted() { }

        /// <summary>
        /// При переопределении в производном классе представляет метод, который будет если необходимо временно приостановит работу WCF сервисов
        /// </summary>
        /// <remarks>
        /// Метод вызывается перед остановкой работающих сервисов
        /// </remarks>
        protected virtual void OnSuspend() { }

        /// <summary>
        /// При переопределении в производном классе представляет метод, который будет если необходимо восстановить работу WCF сервисов после временной остановки
        /// </summary>
        /// <remarks>
        /// Метод вызывается перед перезапуском сервисов
        /// </remarks>
        protected virtual void OnResume() { }

        /// <summary>
        /// При переопределении в производном классе представляет метод, который будет вызван перез запуском WCF сервиса
        /// </summary>
        /// <param name="serviceHost"></param>
        protected virtual void OnStartingWCFServices(ServiceHost serviceHost)
        {
            //if (serviceHost.Credentials.UserNameAuthentication.UserNamePasswordValidationMode == UserNamePasswordValidationMode.Custom
            //    && serviceHost.Description.Endpoints.Any(endpoint => String.Compare(endpoint.Address.Uri.Scheme, "net.tcp", StringComparison.OrdinalIgnoreCase) == 0))
            //serviceHost.Credentials.ServiceCertificate.Certificate =
            //    new X509Certificate2(
            //        Properties.Resources.Emex_Application,
            //        Properties.Resources.CertificatePassword,
            //        X509KeyStorageFlags.Exportable);
        }

        /// <summary>
        /// При переопределении в производном классе представляет метод, который будет вызван после успешного запуска WCF сервиса
        /// </summary>
        /// <param name="serviceHost"></param>
        /// <param name="isSuspendState">true, если система восстанавливается после временной остановки</param>
        /// <remarks>
        /// Вызывается в двух случаях, если система поднимается после временной остановки WCF сервисов и первый раз при запуске сервиса
        /// </remarks>
        protected virtual void OnStartedWCFServices(ServiceHost serviceHost, bool isSuspendState) { }

        /// <summary>
        ///  При переопределении в производном классе представляет метод, который будет вызван после успешного запуска WCF сервиса
        /// </summary>
        /// <param name="serviceHost"></param>
        /// <param name="isSuspendState">true, если система восстанавливается после временной остановки</param>
        /// <remarks>
        /// Вызывается двух случаях, если система входит в состояние временной остановки WCF сервисов
        /// </remarks>
        protected virtual void OnStopingWCFServices(ServiceHost serviceHost, bool isSuspendState) { }

        /// <summary>
        /// При переопределении в производном классе представляет метод, который будет вызван перед остановкой работы системы
        /// </summary>
        protected virtual void OnStoping() { }

        /// <summary>
        /// При переопределении в производном классе представляет метод, который будет вызван после как все сервисы системы будут остановленны
        /// </summary>
        protected virtual void OnStoped() { }
    }

}