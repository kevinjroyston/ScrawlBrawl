using Common.DataModels.Enums;
using BackendAutomatedTestingClient.DataModels;
using BackendAutomatedTestingClient.TestFramework;
using System.Collections.Generic;
using static Common.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;
using GameStep = System.Collections.Generic.IReadOnlyDictionary<Common.DataModels.Enums.UserPromptId, int>;
using Common.DataModels.Requests.LobbyManagement;
using System;

namespace BackendAutomatedTestingClient.Games
{
    public abstract class StructuredMimicTest : MimicTest, IStructuredTest
    {
        protected abstract int NumPlayers { get; }
        protected virtual int NumDrawingsBeforeVote { get; } = 1;
        public TestOptions TestOptions =>
            new TestOptions
            {
                NumPlayers = NumPlayers,
                GameModeOptions = new List<GameModeOptionRequest>()
                {
                    new GameModeOptionRequest(){ Value = NumDrawingsBeforeVote + "" }, // num drawings before vote
                },
                StandardGameModeOptions = new StandardGameModeOptions
                {
                    GameDuration = GameDuration.Normal,
                    ShowTutorial = false,
                    TimerEnabled = false
                }
            };

        public IReadOnlyList<GameStep> UserPromptIdValidations
        {
            get {
                var toReturn = new List<GameStep>()
                {
                    TestCaseHelpers.AllPlayers(UserPromptId.Mimic_DrawAnything, NumPlayers),
                };

                int numRounds = Math.Min(12, this.NumPlayers);

                toReturn.AppendRepetitiveGameSteps(
                copyFrom:new List<GameStep>
                {
                    TestCaseHelpers.OneVsAll(
                        numPlayers:NumPlayers,
                        onePrompt:UserPromptId.Waiting,
                        allPrompt: UserPromptId.Mimic_RecreateDrawing),
                    TestCaseHelpers.AllPlayers(UserPromptId.Voting, NumPlayers),
                    TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipReveal, NumPlayers, UserPromptId.RevealScoreBreakdowns)
                },
                repeatCounter: numRounds);

                toReturn.Add(TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipScoreboard, NumPlayers, UserPromptId.RevealScoreBreakdowns));

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
