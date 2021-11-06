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
    /// Timeout expired
    /// </summary>
    [Serializable]
    public class DataAccessTimeOutException : Exception
    {
        /// <summary>
        /// Описание SQL команды
        /// </summary>
        public string SqlCommand { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public DataAccessTimeOutException() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sqlCommand"></param>
        public DataAccessTimeOutException(string message, string sqlCommand)
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
        public DataAccessTimeOutException(string message, string sqlCommand, Exception inner)
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
        protected DataAccessTimeOutException(SerializationInfo info, StreamingContext context)
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
