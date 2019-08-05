using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Core.Services;
using Lykke.MonitoringServiceApiCaller;
using Lykke.MonitoringServiceApiCaller.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MonitoringService.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MonitoringController : Controller, IMonitoring
    {
        private readonly IMonitoringService _monitoringService;

        public MonitoringController(IMonitoringService monitoringService)
        {
            _monitoringService = monitoringService;
        }

        [HttpGet]
        [SwaggerOperation("Get")]
        [ProducesResponseType(typeof(ListDataMonitoringObjectModel), 200)]
        public async Task<ListDataMonitoringObjectModel> Get()
        {
            var snapshot = await _monitoringService.GetCurrentSnapshotAsync();
            var model = snapshot
                .Select(x => new MonitoringObjectModel
                {
                    ServiceName = x.ServiceName,
                    Version = x.Version,
                    LastPing = x.LastTime,
                    SkipUntil = x.SkipCheckUntil,
                    Url = x.Url,
                    EnvInfo = x.EnvInfo,
                })
                .ToList();

            return new ListDataMonitoringObjectModel { Data = model };
        }

        [HttpGet]
        [Route("{serviceName}")]
        [SwaggerOperation("GetByName")]
        [ProducesResponseType(typeof(MonitoringObjectModel), 200)]
        public async Task<MonitoringObjectModel> GetByServiceName([FromRoute]string serviceName)
        {
            IMonitoringObject mObject = await _monitoringService.GetByNameAsync(serviceName);

            return new MonitoringObjectModel
            {
                LastPing = mObject.LastTime,
                ServiceName = mObject.ServiceName,
                SkipUntil = mObject.SkipCheckUntil,
                Version = mObject.Version,
                Url = mObject.Url,
                EnvInfo = mObject.EnvInfo,
            };
        }

        [HttpPost]
        [Route("ping")]
        [SwaggerOperation("Ping")]
        public async Task Ping([FromBody]MonitoringObjectPingModel model)
        {
            var mappedModel = new MonitoringObject()
            {
                ServiceName = model.ServiceName,
                Version = model.Version,
                LastTime = DateTime.UtcNow
            };

            await _monitoringService.PingAsync(mappedModel);
        }

        [HttpPost]
        [Route("mute")]
        [SwaggerOperation("Mute")]
        public async Task Mute([FromBody]MonitoringObjectMuteModel model)
        {
            await _monitoringService.MuteAsync(model.ServiceName, model.Minutes ?? 60);
        }

        [HttpPost]
        [Route("unmute")]
        [SwaggerOperation("Unmute")]
        public async Task Unmute([FromBody]MonitoringObjectUnmuteModel model)
        {
            await _monitoringService.UnmuteAsync(model.ServiceName);
        }

        [HttpDelete]
        [SwaggerOperation("RemoveByName")]
        [Route("remove/{serviceName}")]
        public async Task RemoveByServiceName([FromRoute]string serviceName)
        {
            await _monitoringService.RemoveByNameAsync(serviceName);
        }

        [HttpDelete]
        [SwaggerOperation("RemoveByUrl")]
        [Route("removebyurl")]
        public async Task RemoveByUrl([FromQuery] string url)
        {
            await _monitoringService.RemoveByUrlAsync(url);
        }
    }
}
