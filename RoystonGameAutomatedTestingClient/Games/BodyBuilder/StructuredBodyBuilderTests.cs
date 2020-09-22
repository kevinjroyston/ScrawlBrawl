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
    [EndToEndGameTest("BodyBuilder_Struct1")]
    public class StructuredBodyBuildTest1 : BodyBuilderTest, IStructuredTest
    {
        // Test might not work with some param combos.
        private int NumPlayers = 5;
        private int NumDrawings = 4;
        private int NumPlayersPerPrompt = 2;
        private int NumPrompts = 2;
        private int NumRounds = 3;
        public TestOptions TestOptions =>
            new TestOptions
            {
                NumPlayers = NumPlayers,
                GameModeOptions = new List<GameModeOptionRequest>()
                {
                    new GameModeOptionRequest(){ Value = NumRounds + "" }, // num rounds
                    new GameModeOptionRequest(){ Value = NumPrompts + "" }, //num prompts
                    new GameModeOptionRequest(){ Value = NumDrawings + ""}, // num drawings expected
                    new GameModeOptionRequest(){ Value = NumPlayersPerPrompt + ""}, // num players per prompt
                    new GameModeOptionRequest(){ Value = "5" } // game speed
                },
            };

        public IReadOnlyList<GameStep> UserPromptIdValidations
        {
            get
            {
                var toReturn = new List<GameStep>();

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(
                            numPlayers:NumPlayers,
                            prompt: UserPromptId.BattleReady_BodyPartDrawing),
                    },
                    repeatCounter: NumDrawings);

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(
                            numPlayers:NumPlayers,
                            prompt: UserPromptId.BattleReady_BattlePrompts),
                    },
                    repeatCounter: NumPrompts);

                var round = new List<GameStep>();
                round.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(
                            numPlayers:NumPlayers,
                            prompt: UserPromptId.BattleReady_ContestantCreation),
                    },
                    repeatCounter: NumPrompts * NumPlayers / NumRounds / NumPlayersPerPrompt);

                round.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(UserPromptId.Voting, NumPlayers),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipReveal, NumPlayers),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipScoreboard, NumPlayers),
                    },
                    repeatCounter: NumPlayers);

                toReturn.AppendRepetitiveGameSteps(round, NumRounds);

                return toReturn;
            }
        }
    }
}
