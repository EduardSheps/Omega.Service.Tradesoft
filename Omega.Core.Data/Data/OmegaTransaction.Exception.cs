using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Data
{
    public class AlreadyCompletedTransactionException : Exception
    {
        private const string ERROR_MESSAGE = "Транзакция уже завершена";

        internal AlreadyCompletedTransactionException()
            : base(ERROR_MESSAGE)
        {

        }

        internal AlreadyCompletedTransactionException(Exception ex)
            : base(ERROR_MESSAGE, ex)
        {

        }

    }
}
