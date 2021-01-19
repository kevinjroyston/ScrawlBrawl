using BackendAutomatedTestingClient.DataModels;
using BackendAutomatedTestingClient.TestFramework;
using Common.DataModels.Requests;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using static Common.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;
using GameStep = System.Collections.Generic.IReadOnlyDictionary<Common.DataModels.Enums.UserPromptId, int>;
using Common.Code.Extensions;
using Common.DataModels.Enums;

namespace BackendAutomatedTestingClient.Games.HintHint
{
    public abstract class StructuredHintHintTest : HintHintTest, IStructuredTest
    {
        protected abstract int NumPlayers { get; }
        protected abstract int NumRealHintGivers { get; }
        protected abstract int NumFakeHintGivers { get; }
        protected abstract List<GuessType> Guesses { get; }
        private int GuessIndex { get; set; } = 0;

        public TestOptions TestOptions =>
            new TestOptions
            {
                NumPlayers = NumPlayers,
                GameModeOptions = new List<GameModeOptionRequest>()
                {
                    new GameModeOptionRequest(){ Value = NumRealHintGivers + "" }, 
                    new GameModeOptionRequest(){ Value = NumFakeHintGivers + "" },
                     new GameModeOptionRequest(){ Value = "50" },
                    new GameModeOptionRequest(){ Value = "20" },
                    new GameModeOptionRequest(){ Value = "5" } // game speed
                },
            };

        public IReadOnlyList<GameStep> UserPromptIdValidations
        {
            get
            {
                var toReturn = new List<GameStep>();

                toReturn.Add(TestCaseHelpers.AllPlayers(
                            numPlayers: NumPlayers,
                            prompt: UserPromptId.HintHint_SetupRound1));
                toReturn.Add(TestCaseHelpers.AllPlayers(
                            numPlayers: NumPlayers,
                            prompt: UserPromptId.HintHint_SetupRound2));
                toReturn.Add(TestCaseHelpers.AllPlayers(
                            numPlayers: NumPlayers,
                            prompt: UserPromptId.HintHint_SetupRound3));

                var roundHintGuesses = new List<GameStep>();
                roundHintGuesses.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        new Dictionary<UserPromptId, int>
                        {
                            { UserPromptId.HintHint_Hint, NumRealHintGivers + NumFakeHintGivers },
                            { UserPromptId.HintHint_Guess, NumPlayers - NumRealHintGivers - NumFakeHintGivers },
                        },
                    },
                    repeatCounter: Guesses.Count);
                roundHintGuesses.Add(TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipReveal, NumPlayers, UserPromptId.RevealScoreBreakdowns));
                roundHintGuesses.Add(TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipScoreboard, NumPlayers, UserPromptId.RevealScoreBreakdowns));
                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: roundHintGuesses,
                    repeatCounter: NumPlayers);


                return toReturn;
            }
        }
        protected override UserFormSubmission SubmitGuess(LobbyPlayer player, GuessType guessType = GuessType.Standard)
        {
            if (player.Equals(this.Lobby.Players[0]))
            {
                int lastIndex = GuessIndex;
                GuessIndex++;
                if (GuessIndex >= Guesses.Count)
                {
                    GuessIndex = 0;
                }
                return base.SubmitGuess(player, Guesses[lastIndex]);
            }
            else
            {
                return base.SubmitGuess(player);
            }
        }
    }

    //[EndToEndGameTest("HintHint_Struct1")]
    public class Struct_HH_Test1 : StructuredHintHintTest
    {
        protected override int NumPlayers => 5;
        protected override int NumRealHintGivers => 2;
        protected override int NumFakeHintGivers => 1;
        protected override List<GuessType> Guesses => new List<GuessType>()
        {
            GuessType.Standard,
            GuessType.Standard,
            GuessType.Standard,
            GuessType.Standard,
            GuessType.Standard,
            GuessType.Standard,
            GuessType.Real,
        };
    }

  //  [EndToEndGameTest("HintHint_Struct2")]
    public class Struct_HH_Test2 : StructuredHintHintTest
    {
        protected override int NumPlayers => 10;
        protected override int NumRealHintGivers => 3;
        protected override int NumFakeHintGivers => 2;
        protected override List<GuessType> Guesses => new List<GuessType>()
        {
            GuessType.Standard,
            GuessType.Standard,
            GuessType.Standard,
            GuessType.Standard,
            GuessType.Standard,
            GuessType.Standard,
            GuessType.Standard,
            GuessType.Standard,
            GuessType.Standard,
            GuessType.Standard,
            GuessType.Fake,
        };
    }

}
