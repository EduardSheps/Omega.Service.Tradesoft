using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Data
{
    /// <summary>
    /// Ошибка при выполнение запроса к базе данных
    /// </summary>
    [Serializable]
    public class DataAccessExecuteException : Exception
    {
        /// <summary>
        /// Описание SQL команды
        /// </summary>
        public string SqlCommand { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public DataAccessExecuteException() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sqlCommand"></param>
        public DataAccessExecuteException(string message, string sqlCommand)
            : base(message)
        {
            SqlCommand = sqlCommand;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sqlCommand"></param>
        /// <param name="inner"></param>
        public DataAccessExecuteException(string message, string sqlCommand, Exception inner)
            : base(message, inner)
        {
            SqlCommand = sqlCommand;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        [SecuritySafeCritical]
        protected DataAccessExecuteException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            SqlCommand = (string)info.GetValue("SqlCommand", typeof(string));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("SqlCommand", SqlCommand, typeof(string));
        }
    }
}
