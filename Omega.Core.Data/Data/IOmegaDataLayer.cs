using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Data
{
    public interface IOmegaDataLayer
    {
        ITransaction BeginTransaction(ITransaction transaction, IsolationLevel isolationLevel = IsolationLevel.Unspecified);
        ITransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
    }
}
