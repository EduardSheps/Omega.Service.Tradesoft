using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Service.Tradesoft
{
    [ServiceContract]
    public interface ITradesoftService
    {
        [OperationContract]
        void Test();
    }


}
