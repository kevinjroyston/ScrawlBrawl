using System;
using System.Collections.Generic;
using System.Text;

namespace Common.DataModels.Responses.Gameplay
{
    /// <summary>
    /// Class containing metadata about a tutorial component.
    /// </summary>
    public class TutorialMetadata
    {
        // GameId is just defaulted to the GameId of the parent UserPrompt object.

        /// <summary>
        /// List of classes that should be hidden from the GameId's default tutorial page.
        /// </summary>
        public List<string> HideClasses { get; set; }
    }
}
