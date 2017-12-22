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
    public class UrlMonitoringController : Controller
    {
        private readonly IUrlMonitoringService _monitoringService;

        public UrlMonitoringController(IUrlMonitoringService monitoringService)
        {
            _monitoringService = monitoringService;
        }

        [HttpGet]
        [SwaggerOperation("Get")]
        [ProducesResponseType(typeof(ListData<UrlMonitoringObjectModel>), 200)]
        public async Task<IActionResult> Get()
        {
            var snapshot = await _monitoringService.GetAllAsync();
            var model = snapshot.Select(x => new UrlMonitoringObjectModel()
            {
                ServiceName = x.ServiceName,
                Url = x.Url
            });

            return Ok(new ListData<UrlMonitoringObjectModel>() { Data = model });
        }

        [HttpPost]
        [Route("monitor")]
        [SwaggerOperation("Monitor")]
        public async Task Post([FromBody]UrlMonitoringObjectModel model)
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
