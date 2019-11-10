using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RoystonGame.DataModels;

namespace RoystonGame.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CurrentContentController : ControllerBase
    {
        private readonly ILogger<CurrentContentController> _logger;

        public CurrentContentController(ILogger<CurrentContentController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public UserPrompt Get()
        {
            return new UserPrompt
            {
                Question = "This is only a test",
                Answers = new string[] { "A", "B", "C", "D" }
            };
        }
    }
}
