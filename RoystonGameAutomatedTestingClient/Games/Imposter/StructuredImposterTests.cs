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
    [EndToEndGameTest("ImposterSyndrome_Struct1")]
    public class StructuredImposterTest1 : ImposterTest, IStructuredTest
    {
        private const int NumPlayers = 5;
        public TestOptions TestOptions =>
            new TestOptions
            {
                NumPlayers = NumPlayers,
                GameModeOptions = new List<GameModeOptionRequest>()
                {
                    new GameModeOptionRequest(){ Value = ""+5 } // game speed
                },
            };
        public IReadOnlyList<GameStep> UserPromptIdValidations
        {
            get
            {
                var toReturn = new List<GameStep>()
                {
                    TestCaseHelpers.AllPlayers(UserPromptId.ImposterSyndrome_CreatePrompt, NumPlayers),
                };

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.OneVsAll(
                            numPlayers:NumPlayers,
                            onePrompt:UserPromptId.SitTight,
                            allPrompt: UserPromptId.ImposterSyndrome_Draw),
                        TestCaseHelpers.AllPlayers(UserPromptId.Voting, NumPlayers),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipReveal, NumPlayers),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipScoreboard, NumPlayers)
                    },
                    repeatCounter: NumPlayers);

                return toReturn;
            }
        }
    }
}
