using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Enums;
using Common.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.GameInfrastructure.DataModels.States.UserStates
{

    public static class Prompts
    {
        public static Func<User, UserPrompt> PartyLeaderSkipRevealButton()
        {
            return (User user) => new UserPrompt()
            {
                UserPromptId = UserPromptId.PartyLeader_SkipReveal,
                Title = Text.SkipReveal,
                SubmitButtonText = "Continue",
                SubmitButton = true
            };
        }

        public static Func<User, UserPrompt> DisplayText(string description = null, UserPromptId? promptId=null) {
            description ??= Text.Waiting;
            promptId ??= UserPromptId.Waiting;
            return (User user) => new UserPrompt()
            {
                UserPromptId = promptId.Value,
                Description = description,
            };
        }
        public static class Text
        {
            public const string Waiting = "Waiting for other players . . .";
            public const string SkipReveal = "Skip Reveal";
            public const string ShowScores = "Check Out The Scores";
            public const string YourPoints = "Your Points This Round";
            public const string YourScore = "Your Total Score";
            public const string TopScores = "Top Total Scores";
            public const string WaitingForGameToStart = "Waiting for the game to start";
        }

        public static Func<User, UserPrompt> ShowScoreBreakdowns(
            Lobby lobby, 
            string promptTitle, 
            Score.Scope? userScoreBreakdownScope = null,
            Score.Scope? userScoreScope = null,
            Score.Scope? leaderboardScope = null,
            UserPromptId userPromptId = UserPromptId.RevealScoreBreakdowns,
            bool showPartyLeaderSkipButton =false)
        {
            return (User user) =>
            {

                List<SubPrompt> subPrompts = new List<SubPrompt>();
                if (userScoreBreakdownScope.HasValue)
                {
                    var scoreBreakdowns = user.ScoreHolder.ScoreBreakdowns[userScoreBreakdownScope.Value];
                    subPrompts.Add(new SubPrompt { Prompt = Text.YourPoints, StringList = scoreBreakdowns.Keys.Select((Score.Reason reason) => $"{Score.ReasonDescriptions[reason]}: {scoreBreakdowns[reason]}").ToArray() });
                }

                if (userScoreScope.HasValue)
                {
                    subPrompts.Add(new SubPrompt { StringList = new string[] { Text.YourScore + ": " + user.ScoreHolder.ScoreAggregates[userScoreScope.Value].ToString() } });
                }

                if (leaderboardScope.HasValue)
                {
                    var userScoreList = lobby.GetUsersSortedByTopScores(3, leaderboardScope.Value);
                    subPrompts.Add(new SubPrompt { Prompt = Text.TopScores, StringList = userScoreList.Select((User user) => $"{user.DisplayName}: {user.ScoreHolder.ScoreAggregates[Score.Scope.Total]}").ToArray() });
                }

                if (showPartyLeaderSkipButton) 
                    subPrompts.Add(new SubPrompt { Prompt = Text.SkipReveal });

                return new UserPrompt()
                {
                    UserPromptId = userPromptId,
                    Title = promptTitle,  // could access this.Lobby for game specific info
                    SubPrompts = subPrompts.ToArray(),
                    SubmitButtonText = "Continue",
                    SubmitButton = (showPartyLeaderSkipButton)
                };
            };
        }
    }

}
