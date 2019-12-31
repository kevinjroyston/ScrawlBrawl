﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace RoystonGame.Web.DataModels.Responses
{
    public class UserPrompt
    {
        /// <summary>
        /// Guid to uniquely identify a prompt/formSubmit pair.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The amount of Time before calling this endpoint again.
        /// </summary>
        public int RefreshTimeInMs { get; set; } = 1000;

        /// <summary>
        /// Bool indicating whether to render the Submit button.
        /// </summary>
        public bool SubmitButton { get; set; }

        /// <summary>
        /// The title/text to display
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The top-level question/text to display
        /// </summary>
        public string Description { get; set; }

        // TODO: Pipe useful error descriptions to user here (No html execution)

        /// <summary>
        /// A list of sub-prompts.
        /// </summary>
        public SubPrompt[] SubPrompts { get; set; }
    }
}
