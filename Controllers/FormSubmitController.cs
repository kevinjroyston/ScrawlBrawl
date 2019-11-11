using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RoystonGame.DataModels.Requests;

namespace RoystonGame.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FormSubmitController : ControllerBase
    {
        private readonly ILogger<FormSubmitController> _logger;

        public FormSubmitController(ILogger<FormSubmitController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post([FromBody] UserFormSubmitRequestDetails formData)
        {
            int x = 5 + 2;
            return new OkResult();
        }
    }
}
