using Newtonsoft.Json;

namespace Common.DataModels.Responses
{

    /// <summary>
    /// Class containing metadata about a suggestion button.
    /// </summary>
    public class SuggestionMetadata
    {
        /// <summary>
        /// SuggestionKey identified which asset will be loaded with the list of suggestions
        /// </summary>
        public string SuggestionKey { get; set; }

    }
}
