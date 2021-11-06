using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Job
{
    public interface IDisposableResource : IDisposable
    {
        void DisposeResource();
    }
}
