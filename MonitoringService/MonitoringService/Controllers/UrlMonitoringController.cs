using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Services;
using MonitoringService.Models;
using Core.Models;
using Core.Repositories;

namespace MonitoringService.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UrlMonitoringController : Controller
    {
        private readonly IUrlMonitoringService _monitoringService;

        public UrlMonitoringController(IUrlMonitoringService monitoringService)
        {
            _monitoringService = monitoringService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ListData<UrlMonitoringObjectModel>), 200)]
        public async Task<IActionResult> Get()
        {
            var snapshot = await _monitoringService.GetAll();
            var model = snapshot.Select(x => new UrlMonitoringObjectModel()
            {
                ServiceName = x.ServiceName,
                Url = x.Url
            });

            return Ok(new ListData<UrlMonitoringObjectModel>() { Data = model });
        }

        [HttpPost]
        [Route("monitor")]
        public async Task Post([FromBody]UrlMonitoringObjectModel model)
        {
            var mappedModel = new ApiMonitoringObject()
            {
                ServiceName = model.ServiceName,
                Url = model.Url
            };

            await _monitoringService.Monitor(mappedModel);
        }
    }
}
