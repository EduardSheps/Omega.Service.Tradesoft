using Omega.Core.Job;
using Omega.Core.Service.Model;
using Omega.Job.Tradesoft;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Unity.Resolution;

namespace Omega.Service.Tradesoft
{
    public class TradesoftServiceApplication : WCFServiceApplicationBase
    {
        private TradesoftService service = null;
        protected override void OnStarting()
        {
            this.RegisterHost(typeof(TradesoftService));
            base.OnStarting();
            StartJob().GetAwaiter().GetResult();
        }

        static async Task StartJob()
        {
            var container = new UnityContainer();
            container.RegisterType<IConfigManager, InMemoryConfigManager>();
            container.RegisterType<IDisposableResource, DisposableResource>(new HierarchicalLifetimeManager());
            container.RegisterType<IJob, TradesoftClientOrderJob>();
            await container.Resolve<TradesoftClientOrderJob>().InitializeQuartz();
        }

        public override long ApplicationId => -1;
        public TradesoftServiceApplication()
        {
            try
            {
                this.service = new TradesoftService();
            }
            catch
            {

            }
        }
    }
}
