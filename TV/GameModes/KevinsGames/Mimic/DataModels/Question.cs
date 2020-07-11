using RoystonGame.TV.DataModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.KevinsGames.Mimic.DataModels
{
    public class Question
    {
        public enum AnswerTypes
        {
            YesNo,
            AlwaysOftenSometimesNever,
            AgreeDisagree,
        };

        public static Dictionary<AnswerTypes, List<string>> AnswerTypeToStrings { get; } = new Dictionary<AnswerTypes, List<string>>()
        {
            {AnswerTypes.YesNo, new List<string>(){"Strong Yes", "Yes", "No", "Strong No"} },
            {AnswerTypes.AlwaysOftenSometimesNever, new List<string>(){"Always", "Often", "Sometimes", "Never" } },
            {AnswerTypes.AgreeDisagree, new List<string>(){"Strongly Agree", "Agree", "Disagree", "StronglyDisagree"} }
        };

        public AnswerTypes AnswerType { get; set; }
        public User Owner { get; set; }
        public string Text { get; set; }
        public Dictionary<User, string> UsersToAnswers { get; set; } = new Dictionary<User, string>();
    }
}
