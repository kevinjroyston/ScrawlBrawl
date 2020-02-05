using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Web.DataModels.Responses
{
    public class GameModeOptionResponse
    {
        public string Description { get; set; }
        public bool ShortAnswer { get; set; }
        public List<string> RadioAnswers { get; set; }
    }
}
