using AzureStorage.Queue;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Repositories
{
    public interface IQueueFactory
    {
        IQueueExt GetQueue(string queueName);
    }
}
