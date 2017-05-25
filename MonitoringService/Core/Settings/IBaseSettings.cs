using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Settings
{
    public class BaseSettings : IBaseSettings
    {
        public DB Db { get; set; }
        public int MaxTimeDifferenceInSeconds { get; set; }
        public int MonitoringJobFrequencyInSeconds { get; set; }
        public int MonitoringApiFrequencyInSeconds { get; set; }
    }

    public class SlackNotificationSettings : ISlackNotificationSettings
    {
        public AzureQueue AzureQueue { get; set; }
    }

    public class AzureQueue
    {
      public string ConnectionString { get; set; }
      public string QueueName { get; set; }
    }

    public interface ISlackNotificationSettings
    {
        AzureQueue AzureQueue { get; set; }
    }

    public interface IBaseSettings
    {
        DB Db { get; set; }
        int MaxTimeDifferenceInSeconds { get; set; }
        int MonitoringJobFrequencyInSeconds { get; set; }
        int MonitoringApiFrequencyInSeconds { get; set; }
    }

    public class DB
    {
        public string DataConnectionString { get; set; }
        public string LogsConnectionString { get; set; }
    }
}
