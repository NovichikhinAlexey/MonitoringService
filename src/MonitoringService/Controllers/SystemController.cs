using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MonitoringService.Controllers
{
    [Route("api/system")]
    [Produces("application/json")]
    public class SystemController : Controller
    {
        [Route("isalive")]
        [HttpGet]
        [SwaggerOperation("IsAlive")]
        public async Task<IActionResult> IsAlive()
        {
            return Ok(
                new
                {
                    Name = PlatformServices.Default.Application.ApplicationName,
                    Version = PlatformServices.Default.Application.ApplicationVersion,
                });
        }
    }
}
