using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Job
{
    public abstract class DisposableResource : IDisposableResource
    {
        public void DisposeResource()
        {

        }

        public void Dispose()
        {
        }
    }
}
