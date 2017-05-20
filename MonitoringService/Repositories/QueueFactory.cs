using Core.Repositories;
using System;
using AzureStorage.Queue;
using Core.Settings;

namespace Repositories
{
    public class QueueFactory : IQueueFactory
    {
        private readonly IBaseSettings _settings;

        public QueueFactory(IBaseSettings settings)
        {
            _settings = settings;
        }

        public IQueueExt GetQueue(string queueName)
        {
            return new AzureQueueExt(_settings.Db.SlackConnectionString, queueName);
        }
    }
}
