using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Service
{
    /// <summary>
    /// Предоставляет абстрактный класс для реализации методов и свойства базового приложения
    /// </summary>
    public abstract class ApplicationBase
         : IDisposable
    {
        /// <summary>
        /// Конструктор приложения
        /// </summary>
        protected ApplicationBase()
        {
            ApplicationHostHelpers.HookApplication(this);
        }

        /// <summary>
        /// Возвращает наименование приложения
        /// </summary>
        public abstract string ApplicationName { get; }

        /// <summary>
        /// Возвращает зарегистрированный уникальный код приложения
        /// </summary>
        public abstract long ApplicationId { get; }

        /// <summary>
        /// Выполняет обработку закрытия приложения
        /// </summary>
        protected virtual void OnProcessExit() { }

        /// <summary>
        /// Обработка необработанных исключений
        /// </summary>
        protected virtual void OnUnhandledException() { }

        /// <summary>
        /// Обработка выхода из приложения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void ProcessExit(object sender, EventArgs e)
        {
            OnProcessExit();
            ApplicationHostHelpers.UnHookApplication(this);
        }

        ///// <summary>
        ///// Учетные данные пользователя
        ///// </summary>
        //public abstract EmexCredentials Credentials { get; }

        /// <summary>
        /// Обрабывает не перехваченное исключение
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            //Log.LogStorage.RegisterException(ex ?? new Exception(e.ExceptionObject.ToString()), "ApplicationHostHelpers::Unhandled");
            OnUnhandledException();
        }

        #region IDisposable Members
        /// <summary>
        /// Метод освобождения ресурсов
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
        }
        /// <summary>
        /// Деструктор
        /// </summary>
        ~ApplicationBase()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
