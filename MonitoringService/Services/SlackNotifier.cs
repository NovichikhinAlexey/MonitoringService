using AzureStorage.Queue;
using Core.Repositories;
using Core.Services;
using Core.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Models;

namespace Services
{
    public class SlackNotifier : ISlackNotifier
    {
        private readonly IQueueExt _queue;
        private const string _sender = "monitoring-service";
        private readonly IBaseSettings _settings;

        public SlackNotifier(IQueueFactory queueFactory, IBaseSettings settings)
        {
            _settings = settings;
            _queue = queueFactory.GetQueue();
        }

        private async Task MessageAsync(string type, string message)
        {
            var obj = new
            {
                Type = type,
                Sender = _sender,
                Message = message
            };

            await _queue.PutRawMessageAsync(JsonConvert.SerializeObject(obj));
        }

        public async Task WarningAsync(string message)
        {
            await MessageAsync(SlackChannel.Warnings.ToString(), message);
        }

        public async Task ErrorAsync(string message)
        {
            await MessageAsync(SlackChannel.Errors.ToString(), message);
        }

        public async Task ResilienceAsync(string message)
        {
            await MessageAsync(SlackChannel.Resilience.ToString(), message);
        }
    }
}
