using Common.DataModels.Enums;
using System;

namespace Common.DataModels.Responses
{
    public class UserPrompt
    {
        /// <summary>
        /// Guid to uniquely identify a prompt/formSubmit pair.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Enum representing which user prompt is being used. Mostly used in testing
        /// </summary>
        public UserPromptId UserPromptId { get; set; } = UserPromptId.Unknown;

        /// <summary>
        /// The amount of Time before calling this endpoint again.
        /// </summary>
        public int RefreshTimeInMs { get; set; } = 5000;

        /// <summary>
        /// The current server time.
        /// </summary>
        public DateTime CurrentServerTime { get { return DateTime.UtcNow; } }

        /// <summary>
        /// The current server time.
        /// </summary>
        public DateTime? AutoSubmitAtTime { get; set; } = null;

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

        /// <summary>
        /// A list of sub-prompts.
        /// </summary>
        public SubPrompt[] SubPrompts { get; set; }
    }
}
