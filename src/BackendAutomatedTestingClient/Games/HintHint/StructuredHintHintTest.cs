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

namespace BackendAutomatedTestingClient.Games.HintHint
{
    public abstract class StructuredHintHintTest : HintHintTest, IStructuredTest
    {
        protected abstract int NumPlayers { get; }
        protected abstract int NumRealHintGivers { get; }
        protected abstract int NumFakeHintGivers { get; }
        protected abstract int MaxHints { get; }
        protected abstract int MaxGuesses { get; }
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
                     new GameModeOptionRequest(){ Value = MaxHints + "" },
                    new GameModeOptionRequest(){ Value = MaxGuesses + "" },
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
                            prompt: UserPromptId.FriendQuiz_CreateQuestion),
                    },
                    repeatCounter: NumQuestionsSetup);

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.AllPlayers(
                            numPlayers:NumPlayers,
                            prompt: UserPromptId.FriendQuiz_AnswerQuestion),
                    },
                    repeatCounter: Math.Min(NumQuestionsToAnswer, NumPlayers * NumQuestionsSetup));

                toReturn.AppendRepetitiveGameSteps(
                    copyFrom: new List<GameStep>
                    {
                        TestCaseHelpers.OneVsAll(UserPromptId.Waiting, NumPlayers, UserPromptId.FriendQuiz_Query),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipReveal, NumPlayers, UserPromptId.RevealScoreBreakdowns),
                        TestCaseHelpers.OneVsAll(UserPromptId.PartyLeader_SkipScoreboard, NumPlayers, UserPromptId.RevealScoreBreakdowns)
                    },
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
}
