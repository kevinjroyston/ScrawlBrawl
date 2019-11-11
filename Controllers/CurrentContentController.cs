﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RoystonGame.DataModels.Responses;

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
        public IActionResult Get()
        {
            // TODO handle IDs for real
            // TODO scramble radio button answer order
            return new JsonResult(new UserPrompt
            {
                Title = "Trivia Time!",
                Description = "Select the answer(s) below which best answer the question. Careful, you only have 10 seconds!",
                RefreshTime = TimeSpan.FromSeconds(10),
                SubmitButton = true,
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = "What is Kevin's favorite color?",
                        Answers = new string [] {"Red", "Blue", "Green", "Orange", "Purple", "Turquoise", "Pink"},
                        ShortAnswer = true,
                        Drawing = true,
                    },
                    new SubPrompt
                    {
                        Prompt = "What is Kevin's 2nd favorite color?",
                        Answers = new string [] {"Red", "Blue", "Green", "Orange", "Purple", "Turquoise", "Pink"},
                        ShortAnswer = true,
                    }
                },
            });
        }
    }
}
