using Backend.GameInfrastructure.DataModels.Users;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Backend.Games.TimsGames.FriendQuiz.DataModels
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
            {AnswerTypes.YesNo, new List<string>(){ "Abstain", "Yes", "No"} },
            {AnswerTypes.AlwaysOftenSometimesNever, new List<string>(){ "Abstain", "Always", "Often", "Sometimes", "Never" } },
            {AnswerTypes.AgreeDisagree, new List<string>(){ "Abstain", "Strongly Agree", "Agree", "Disagree", "Strongly Disagree"} }
        };

        public AnswerTypes AnswerType { get; set; }
        public User Owner { get; set; }
        public string Text { get; set; }
        public ConcurrentDictionary<User, int> UsersToAnswers { get; set; } = new ConcurrentDictionary<User, int>();

        public ConcurrentDictionary<User, int> ExtraRoundUserToVotesRecieved { get; set; } = new ConcurrentDictionary<User, int>();
    }
}
