using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace MonitoringService.Controllers
{
    [Route("api/system")]
    [Produces("application/json")]
    public class SystemController : Controller
    {

        public SystemController()
        {
        }

        [Route("isalive")]
        [HttpGet]
        public async Task<IActionResult> IsAlive()
        {
            return Ok(new { Version = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion });
        }
    }
}
