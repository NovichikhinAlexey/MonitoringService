using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using Core.Services;
using Core.Models;
using MonitoringService.Models;

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
        [SwaggerOperation("Get")]
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
        [SwaggerOperation("GetByName")]
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
        [SwaggerOperation("Ping")]
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
        [SwaggerOperation("Mute")]
        public async Task Mute([FromBody]MonitoringObjectMuteModel model)
        {
            await _monitoringService.Mute(model.ServiceName, model.Minutes);
        }

        [HttpPost]
        [Route("unmute")]
        [SwaggerOperation("Unmute")]
        public async Task Unmute([FromBody]MonitoringObjectUnmuteModel model)
        {
            await _monitoringService.Unmute(model.ServiceName);
        }

        [HttpDelete]
        [SwaggerOperation("Remove")]
        [Route("remove/{serviceName}")]
        public async Task Remove([FromRoute]string serviceName)
        {
            await _monitoringService.Remove(serviceName);
        }
    }
}
