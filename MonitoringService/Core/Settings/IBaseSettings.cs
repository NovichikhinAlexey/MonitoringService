using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Settings
{
    public class BaseSettings : IBaseSettings
    {
        public DB Db { get; set; }
        public int MaxTimeDifferenceInSeconds { get; set; }
        public string SlackQueueName { get; set; }
    }

    public interface IBaseSettings
    {
        DB Db { get; set; }
        int MaxTimeDifferenceInSeconds { get; set; }
        string SlackQueueName { get; set; }
    }

    public class DB
    {
        public string SlackConnectionString{ get; set; }
    }
}
