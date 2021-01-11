using System;
using Microsoft.AspNetCore.Mvc;
using simple_netcore_source.Services;
namespace simple_netcore_source.Controllers {
    [Produces ("application/json")]
    [Route ("actuator/")]
    [ApiController]
    public class ActuatorsController : ControllerBase {

        private readonly IHealthCheckService healthChek;

        public ActuatorsController (IHealthCheckService hc) {
            this.healthChek = hc;
        }

        //retorna health status
        [HttpGet ("health")]
        public IActionResult GetHealth()
        {
            var result = healthChek.check();
           
            return Ok(result);
        }

        //retorna lista de objetos do tipo Actuator
        [HttpGet ("info")]
        public IActionResult GetInfo()
        {
            var result = healthChek.info();
            return Ok(result);
        }

    }
}