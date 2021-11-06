using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omega.Core.Job
{
    public abstract class ImplementingQuartz<T> where T : IJob, INotifyPropertyChanged
    {
        private string _cronSetting;
        private object _lock = new object();
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public virtual async Task InitializeQuartz()
        {

            if (_cronSetting == null) return;

            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();
            IJobDetail job = JobBuilder.Create<T>().Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(typeof(T).Name + "Job", typeof(T).Name + "Group")
                .StartNow()
                .WithCronSchedule(_cronSetting)
                .Build();

            await scheduler.ScheduleJob(job, trigger);

            await scheduler.Start();
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        public virtual string CronSetting
        {
            get => _cronSetting;
            set
            {
                lock (_lock)
                {
                    if (value != _cronSetting)
                    {
                        _cronSetting = value;
                        OnPropertyChanged("CronSetting");
                    }
                }
            }
        }

       
    }
}
