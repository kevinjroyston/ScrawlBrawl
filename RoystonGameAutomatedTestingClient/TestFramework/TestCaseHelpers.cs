using RoystonGame.Web.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using GameStep = System.Collections.Generic.IReadOnlyDictionary<RoystonGame.Web.DataModels.Enums.UserPromptId, int>;

namespace RoystonGameAutomatedTestingClient.TestFramework
{
    public static class TestCaseHelpers
    {
        public static GameStep AllPlayers(UserPromptId prompt, int numPlayers)
        {
            return new Dictionary<UserPromptId, int>
            {
                { prompt, numPlayers }
            };
        }
        public static GameStep OneVsAll(UserPromptId onePrompt, int numPlayers, UserPromptId allPrompt = UserPromptId.Waiting)
        {
            return new Dictionary<UserPromptId, int>
            {
                { onePrompt, 1 },
                { allPrompt, numPlayers -1 }
            };
        }
        public static void AppendRepetitiveGameSteps(this List<GameStep> destination, IReadOnlyList<GameStep> copyFrom, int repeatCounter)
        {
            for (int i = 0; i < repeatCounter; i++)
            {
                destination.AddRange(copyFrom);
            }
        }
    }
}
