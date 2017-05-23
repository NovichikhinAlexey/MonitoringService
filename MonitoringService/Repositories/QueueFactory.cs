using Core.Repositories;
using System;
using AzureStorage.Queue;
using Core.Settings;

namespace Repositories
{
    public class QueueFactory : IQueueFactory
    {
        private readonly ISlackNotificationSettings _settings;

        public QueueFactory(ISlackNotificationSettings settings)
        {
            _settings = settings;
        }

        public IQueueExt GetQueue()
        {
            return new AzureQueueExt(_settings.AzureQueue.ConnectionString, _settings.AzureQueue.QueueName);
        }
    }
}
