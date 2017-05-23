using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Services;
using MonitoringService.Models;
using Core.Models;

namespace MonitoringService.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MonitoringController : Controller
    {
        private readonly IMonitoringService _monitoringService;

        public MonitoringController(IMonitoringService monitoringService)
        {
            _monitoringService = monitoringService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ListData<MonitoringObjectModel>), 200)]
        public async Task<IActionResult> Get()
        {
            var snapshot = await _monitoringService.GetCurrentSnapshot();
            var model = snapshot.Select(x => new MonitoringObjectModel()
            {
                ServiceName = x.ServiceName,
                Version = x.Version,
                LastPing = x.LastTime,
                SkipUntil = x.SkipCheckUntil,
                Url = x.Url
            });

            return Ok(new ListData<MonitoringObjectModel>() { Data = model });
        }

        [HttpGet]
        [Route("{serviceName}")]
        [ProducesResponseType(typeof(MonitoringObjectModel), 200)]
        public async Task<IActionResult> Get([FromRoute]string serviceName)
        {
            IMonitoringObject mObject = await _monitoringService.GetByName(serviceName);

            return Ok(new MonitoringObjectModel()
            {
                LastPing = mObject.LastTime,
                ServiceName = mObject.ServiceName,
                SkipUntil = mObject.SkipCheckUntil,
                Version = mObject.Version,
                Url = mObject.Url
            });
        }

        [HttpPost]
        [Route("ping")]
        public async Task Post([FromBody]MonitoringObjectPingModel model)
        {
            var mappedModel = new MonitoringObject()
            {
                ServiceName = model.ServiceName,
                Version = model.Version,
                LastTime = DateTime.UtcNow
            };

            await _monitoringService.Ping(mappedModel);
        }

        [HttpPost]
        [Route("mute")]
        public async Task Mute([FromBody]MonitoringObjectMuteModel model)
        {
            await _monitoringService.Mute(model.ServiceName, model.Minutes);
        }

        [HttpPost]
        [Route("unmute")]
        public async Task Unmute([FromBody]MonitoringObjectUnmuteModel model)
        {
            await _monitoringService.Unmute(model.ServiceName);
        }

        [HttpDelete]
        [Route("remove/{serviceName}")]
        public async Task Remove([FromRoute]string serviceName)
        {
            await _monitoringService.Remove(serviceName);
        }
    }
}
