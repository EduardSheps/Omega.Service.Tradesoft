using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Service.Model
{
    /// <summary>
    /// Описывает интерфейс базового сервисного контракта данных
    /// </summary>
    [ServiceContract]
    public interface IWCFServiceBase
    {
        /// <summary>
        /// Возвращает текущее время и дату на сервиск
        /// </summary>
        /// <returns>Дата и время</returns>
        /// <remarks>
        /// В некоторых случаях используется для реализации проверки работоспособности сервиса
        /// </remarks>
        [OperationContract]
        [WebInvoke(UriTemplate = "/GetServiceDateTime")]
        DateTime GetServiceDateTime();
    }
}
