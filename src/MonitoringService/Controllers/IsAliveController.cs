using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;
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
                    Name = PlatformServices.Default.Application.ApplicationName,
                    Version = PlatformServices.Default.Application.ApplicationVersion,
                    Env = Environment.GetEnvironmentVariable("ENV_INFO"),
                });
        }
    }
}
