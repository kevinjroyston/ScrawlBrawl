using Common.DataModels.Enums;
using Common.DataModels.Responses.Gameplay;
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

        public PromptHeaderMetadata PromptHeader { get; set; } = new PromptHeaderMetadata();

        public string GameIdString { get { return this.GameId?.ToString()??String.Empty; } }

        public string LobbyId { get; set; } = String.Empty;

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public GameModeId? GameId { get; set; }


        /// <summary>
        /// The amount of Time before calling this endpoint again.
        /// </summary>
        public int RefreshTimeInMs { get; set; } = 5000;

        /// <summary>
        /// The current server time.
        /// </summary>
        public DateTime CurrentServerTime { get { return DateTime.UtcNow; } }

        /// <summary>
        /// The time to submit at.
        /// </summary>
        public DateTime? AutoSubmitAtTime { get; set; } = null;

        /// <summary>
        /// Bool indicating whether to render the Submit button.
        /// </summary>
        public bool SubmitButton { get; set; }

        /// <summary>
        /// The title/text to display
        /// </summary>
        public string SubmitButtonText { get; set; } = "Submit";

        public SuggestionMetadata Suggestion { get; set; }

        /// <summary>
        /// The title/text to display
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The top-level question/text to display
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Presence of this object indicates a tutorial component should be rendered.
        /// GameIdString will be used to determine the targetted game.
        /// </summary>
        public TutorialMetadata Tutorial { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public UserListMetadataMode? DisplayUsers { get; set; }

        public string DisplayUsersString { get { return this.DisplayUsers?.ToString() ?? String.Empty; } }

        /// <summary>
        /// A list of sub-prompts.
        /// </summary>
        public SubPrompt[] SubPrompts { get; set; }
    }
}
