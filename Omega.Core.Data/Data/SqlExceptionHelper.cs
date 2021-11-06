using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Data
{
    public static class SqlExceptionHelper
    {

        /// <summary>
        /// Возвращает True, если ошибка является ошибкой Timeout
        /// </summary>
        /// <param name="ex">ошибка</param>
        /// <returns></returns>
        public static bool IsTimeoutException(Exception ex)
        {
            return ex is SqlException ? IsTimeoutException(ex as SqlException) : false;
        }

        /// <summary>
        /// Возвращает True, если ошибка является ошибкой Timeout
        /// </summary>
        /// <param name="ex">ошибка</param>
        /// <returns></returns>
        public static bool IsTimeoutException(SqlException ex)
        {
            return ex.Message.IndexOf("Timeout expired", StringComparison.CurrentCultureIgnoreCase) > 0
                || ex.Message.IndexOf("(Timeout)", StringComparison.CurrentCultureIgnoreCase) > 0;
        }
    }
}
