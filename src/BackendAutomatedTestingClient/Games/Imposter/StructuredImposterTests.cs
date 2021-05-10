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
    public abstract class StructuredImposterTest : ImposterTest, IStructuredTest
    {
        protected abstract int NumPlayers { get; }
        public TestOptions TestOptions =>
            new TestOptions
            {
                NumPlayers = NumPlayers,
                GameModeOptions = new List<GameModeOptionRequest>()
                {
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
            get
            {
                int numRounds = Math.Min(8, this.NumPlayers);
                int numDrawingsPerUser = Math.Min(4, numRounds - 1);

                var toReturn = new List<GameStep>()
                {
                    TestCaseHelpers.AllPlayers(UserPromptId.ImposterSyndrome_CreatePrompt, NumPlayers),
                };
                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(
                            numPlayers: NumPlayers,
                            prompt: UserPromptId.ImposterSyndrome_Draw)
                    },
                    repeatCounter:numDrawingsPerUser);

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(UserPromptId.Voting, NumPlayers),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipReveal, NumPlayers, UserPromptId.RevealScoreBreakdowns),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipScoreboard, NumPlayers, UserPromptId.RevealScoreBreakdowns)
                    },
                    repeatCounter: numRounds);

                return toReturn;
            }
        }
    }


    [EndToEndGameTest("ImposterSyndrome_Struct1")]
    public class Struct_Imp_Test1 : StructuredImposterTest
    {
        protected override int NumPlayers => 5;
    }

    [EndToEndGameTest("ImposterSyndrome_Struct2")]
    public class Struct_Imp_Test2 : StructuredImposterTest
    {
        protected override int NumPlayers => 4;
    }

    [EndToEndGameTest("ImposterSyndrome_Struct3")]
    public class Struct_Imp_Test3 : StructuredImposterTest
    {
        protected override int NumPlayers => 10;
    }
}
