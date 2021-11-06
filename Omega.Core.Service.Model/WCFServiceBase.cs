using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Service.Model
{
    /// <summary>
    /// Описывает реализацию базового интерфейса сервиса
    /// </summary>
    [ServiceBehavior]
    public partial class WCFServiceBase
        : IWCFServiceBase, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WCFServiceBase"/> class. 
        ///   Создаёт новый экземпляр класса
        /// </summary>
        public WCFServiceBase()
        {

        }

        public virtual string ServiceName 
        {
            get; set;
        }

        /// <summary>
        /// Gets Возвращает флаг, указывающий, что текущая сессия авторизованна пользователем
        /// </summary>
        public bool IsAuthorization
        {
            get { return this.GetIsAuthorization(this); }
        }

        #region IWCFServiceBase Members

        /// <summary>
        /// The get service date time.
        /// </summary>
        /// <returns>
        /// </returns>
        DateTime IWCFServiceBase.GetServiceDateTime()
        {
            return DateTime.Now;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="WCFServiceBase"/> class. 
        /// </summary>
        ~WCFServiceBase()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}