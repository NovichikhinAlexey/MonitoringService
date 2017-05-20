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
                SkipUntil = x.SkipCheckUntil
            });

            return Ok(new ListData<MonitoringObjectModel>() { Data = model });
        }

        [HttpPost]
        [Route("ping")]
        public async Task Post([FromBody]MonitoringObjectPingModel model)
        {
            var mappedModel = new MonitoringObject()
            {
                ServiceName = model.ServiceName,
                SkipCheckUntil = model.SkipUntil,
                Version = model.Version,
                LastTime = DateTime.UtcNow
            };

            await _monitoringService.Ping(mappedModel);
        }
    }
}
