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
    public class UrlMonitoringController : Controller, IUrlMonitoring
    {
        private readonly IUrlMonitoringService _monitoringService;

        public UrlMonitoringController(IUrlMonitoringService monitoringService)
        {
            _monitoringService = monitoringService;
        }

        [HttpGet]
        [SwaggerOperation("Get")]
        [ProducesResponseType(typeof(ListDataUrlMonitoringObjectModel), 200)]
        public async Task<ListDataUrlMonitoringObjectModel> Get()
        {
            var snapshot = await _monitoringService.GetAllAsync();
            var model = snapshot
                .Select(x => new UrlMonitoringObjectModel
                {
                    ServiceName = x.ServiceName,
                    Url = x.Url
                })
                .ToList();

            return new ListDataUrlMonitoringObjectModel { Data = model };
        }

        [HttpPost]
        [Route("monitor")]
        [SwaggerOperation("Monitor")]
        public async Task Monitor([FromBody]UrlMonitoringObjectModel model)
        {
            var mappedModel = new MonitoringObject()
            {
                ServiceName = model.ServiceName,
                Url = model.Url
            };

            await _monitoringService.MonitorAsync(mappedModel);
        }
    }
}
