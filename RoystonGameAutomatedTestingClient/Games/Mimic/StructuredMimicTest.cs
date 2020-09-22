using RoystonGame.Web.DataModels.Enums;
using RoystonGameAutomatedTestingClient.DataModels;
using RoystonGameAutomatedTestingClient.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;
using static RoystonGame.Web.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;
using GameStep = System.Collections.Generic.IReadOnlyDictionary<RoystonGame.Web.DataModels.Enums.UserPromptId, int>;

namespace RoystonGameAutomatedTestingClient.Games
{
    [EndToEndGameTest("Mimic_Struct1")]
    public class StructuredMimicTest1 : MimicTest, IStructuredTest
    {
        private const int NumPlayers = 3;// Test should work for almost any value (if options succed, test succeds).
        public override TimeSpan MaxTotalPollingTime => TimeSpan.FromSeconds(15); // memorization timer will be 10 seconds
        public TestOptions TestOptions { get; } =
            new TestOptions
            {
                NumPlayers = NumPlayers,
                GameModeOptions = new List<GameModeOptionRequest>()
                {
                    new GameModeOptionRequest(){ Value = "1" }, // num starting drawings
                    new GameModeOptionRequest(){ Value = "1" }, // num drawings before vote
                    new GameModeOptionRequest(){ Value = NumPlayers + "" }, // num sets
                    new GameModeOptionRequest(){ Value = NumPlayers + "" }, // max for vote
                    new GameModeOptionRequest(){ Value = "5" } // game speed
                },
            };

        public IReadOnlyList<GameStep> UserPromptIdValidations
        {
            get {
                var toReturn = new List<GameStep>()
                {
                    TestCaseHelpers.AllPlayers(UserPromptId.Mimic_DrawAnything, NumPlayers),
                };

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom:new List<GameStep>
                    {
                        TestCaseHelpers.OneVsAll(
                            numPlayers:NumPlayers,
                            onePrompt:UserPromptId.Waiting,
                            allPrompt: UserPromptId.Mimic_RecreateDrawing),
                        TestCaseHelpers.AllPlayers(UserPromptId.Voting, NumPlayers),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipReveal, NumPlayers)
                    },
                    repeatCounter: NumPlayers);

                toReturn.Add(TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipScoreboard, NumPlayers));

                return toReturn;
            }
        }
    }
}
