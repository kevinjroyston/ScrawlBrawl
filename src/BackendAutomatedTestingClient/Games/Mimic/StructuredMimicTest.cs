using Common.DataModels.Enums;
using BackendAutomatedTestingClient.DataModels;
using BackendAutomatedTestingClient.TestFramework;
using System.Collections.Generic;
using static Common.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;
using GameStep = System.Collections.Generic.IReadOnlyDictionary<Common.DataModels.Enums.UserPromptId, int>;

namespace BackendAutomatedTestingClient.Games
{
    public abstract class StructuredMimicTest : MimicTest, IStructuredTest
    {
        // TODO: make test work for more settings combinations.
        protected abstract int NumPlayers { get; }
        protected virtual int NumStartingDrawings { get; } = 1;
        protected virtual int NumDrawingsBeforeVote { get; } = 1;
        protected virtual int NumSets => NumPlayers;
        protected virtual int MaxPerVote => NumPlayers;
        public TestOptions TestOptions =>
            new TestOptions
            {
                NumPlayers = NumPlayers,
                GameModeOptions = new List<GameModeOptionRequest>()
                {
                    new GameModeOptionRequest(){ Value = NumStartingDrawings + "" }, // num starting drawings
                    new GameModeOptionRequest(){ Value = NumDrawingsBeforeVote + "" }, // num drawings before vote
                    new GameModeOptionRequest(){ Value = NumSets + "" }, // num sets
                    new GameModeOptionRequest(){ Value = MaxPerVote + "" }, // max for vote
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
                    repeatCounter: NumPlayers * NumStartingDrawings);

                toReturn.Add(TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipScoreboard, NumPlayers));

                return toReturn;
            }
        }
    }
    [EndToEndGameTest("Mimic_Struct1")]
    public class Struct_Mimic_Test1 : StructuredMimicTest
    {
        protected override int NumPlayers => 3;
    }
}
