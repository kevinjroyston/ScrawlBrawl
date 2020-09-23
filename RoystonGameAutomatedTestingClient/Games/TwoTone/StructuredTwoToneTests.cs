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
    [EndToEndGameTest("TwoTone_Struct1")]
    public class StructuredTwoToneTest1 : TwoToneTest, IStructuredTest
    {
        private const int NumPlayers = 7;
        private const int TeamsPerPrompt = 2;
        private const int ColorsPerTeam = 3;
        public TestOptions TestOptions =>
            new TestOptions
            {
                NumPlayers = NumPlayers,
                GameModeOptions = new List<GameModeOptionRequest>()
                {
                    new GameModeOptionRequest(){ Value = ColorsPerTeam + "" }, // max num colors
                    new GameModeOptionRequest(){ Value = TeamsPerPrompt + "" }, // max num teams per prompt
                    new GameModeOptionRequest(){ Value = "true"}, // show other colors
                    new GameModeOptionRequest(){ Value = "5" } // game speed
                },
            };

        public IReadOnlyList<GameStep> UserPromptIdValidations
        {
            get
            {
                var toReturn = new List<GameStep>()
                {
                    TestCaseHelpers.AllPlayers(UserPromptId.ChaoticCooperation_Setup, NumPlayers),
                };

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(
                            numPlayers:NumPlayers,
                            prompt: UserPromptId.ChaoticCooperation_Draw),
                    },
                    repeatCounter: TeamsPerPrompt * ColorsPerTeam);

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(UserPromptId.Voting, NumPlayers),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipReveal, NumPlayers),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipScoreboard, NumPlayers),
                    },
                    repeatCounter: NumPlayers);

                return toReturn;
            }
        }
    }
}
