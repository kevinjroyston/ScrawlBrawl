using System.Collections.Generic;

namespace Backend.Games.KevinsGames.StoryTime.DataModels
{
    public static class StoryTimeConstants
    {
        public static List<string> WritingPrompts = new List<string>()
        {
            "Joke",
            "Poem",
            "Insult",
            "Horror Story",
            "Love Story",
            "Tragedy",

        };

        public const int PointsForVote = 100;
    }
}
