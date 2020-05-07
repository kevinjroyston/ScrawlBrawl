using System.Collections.Generic;

namespace RoystonGame.Web.DataModels.Responses
{
    public class GameModeOptionResponse
    {
        public string Description { get; set; }
        public bool ShortAnswer { get; set; }
        public List<string> RadioAnswers { get; set; }
    }
}
