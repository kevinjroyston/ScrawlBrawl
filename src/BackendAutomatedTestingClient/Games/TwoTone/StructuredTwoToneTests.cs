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
    public abstract class StructuredTwoToneTest : TwoToneTest, IStructuredTest
    {
        protected abstract int NumPlayers { get; }
        protected abstract int ColorsPerTeam { get; }
        public TestOptions TestOptions =>
            new TestOptions
            {
                NumPlayers = NumPlayers,
                GameModeOptions = new List<GameModeOptionRequest>()
                {
                    // TODO: test other modes.
                    new GameModeOptionRequest(){ Value = "true"}, // one color per person
                    new GameModeOptionRequest(){ Value = ColorsPerTeam + "" }, // max num colors
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
                int numRounds = Math.Min(6, this.NumPlayers);
                int numDrawingsPerPlayer = Math.Min(4, numRounds);
                int maxPossibleTeamCount = 8; // Can go higher than this in extreme circumstances.
                int numTeamsLowerBound = Math.Max(2, 1 * this.NumPlayers / (numRounds * this.NumPlayers)); // Lower bound.
                int numTeamsUpperBound = Math.Min(maxPossibleTeamCount, numDrawingsPerPlayer * this.NumPlayers / (numRounds * this.ColorsPerTeam)); // Upper bound.
                int numTeams = Math.Max(numTeamsLowerBound, numTeamsUpperBound); // Possible for lower bound to be higher than upper bound. that is okay.

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
                    repeatCounter: Math.Max(numDrawingsPerPlayer,numTeams*this.ColorsPerTeam*numRounds/this.NumPlayers));

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(UserPromptId.Voting, NumPlayers),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipReveal, NumPlayers, UserPromptId.RevealScoreBreakdowns),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipScoreboard, NumPlayers, UserPromptId.RevealScoreBreakdowns),
                    },
                    repeatCounter: NumPlayers);

                return toReturn;
            }
        }
    }

    [EndToEndGameTest("TwoTone_Struct1")]
    public class Struct_TwoTone_Test1 : StructuredTwoToneTest
    {
        protected override int NumPlayers => 6;

        protected override int ColorsPerTeam => 2;
    }
    [EndToEndGameTest("TwoTone_Struct2")]
    public class Struct_TwoTone_Test2 : StructuredTwoToneTest
    {
        protected override int NumPlayers => 6;

        protected override int ColorsPerTeam => 3;
    }
}
