using Common.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.DataModels.Responses.Gameplay
{
    public class CurrentContent
    {
        public Guid PromptId { get; set; }

        /// <summary>
        /// The amount of Time before calling this endpoint again.
        /// </summary>
        public int RefreshTimeInMs { get; set; }

        public UserPrompt UserPrompt { get; set; }

        public UserListMetadata UserListMetadata { get; set; }

    }
}
