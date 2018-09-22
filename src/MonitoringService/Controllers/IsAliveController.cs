using System;
using Lykke.Common;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MonitoringService.Controllers
{
    [Route("api")]
    [Produces("application/json")]
    public class IsAliveController : Controller
    {
        [Route("isalive")]
        [HttpGet]
        [SwaggerOperation("IsAlive")]
        public IActionResult Get()
        {
            return Ok(
                new
                {
                    Name = AppEnvironment.Name,
                    Version = AppEnvironment.Version,
                    Env = Environment.GetEnvironmentVariable("ENV_INFO"),
                });
        }
    }
}
