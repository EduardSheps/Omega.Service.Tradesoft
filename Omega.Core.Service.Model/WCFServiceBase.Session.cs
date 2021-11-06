using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Service.Model
{
    partial class WCFServiceBase
    {

        /// <summary>
        /// Возвращает признак авторизован или нет пользователь
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns>Возвращает true, если пользователь авторизован</returns>
        internal bool GetIsAuthorization(WCFServiceBase service)
        {
#if DEBUG || STAND
            return true;
#else
            return string.IsNullOrWhiteSpace(this.GetUserLogin());
#endif
        }


    }
}
