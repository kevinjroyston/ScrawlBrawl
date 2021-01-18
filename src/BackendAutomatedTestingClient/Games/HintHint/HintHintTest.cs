using BackendAutomatedTestingClient.DataModels;
using BackendAutomatedTestingClient.WebClient;
using Common.DataModels.Enums;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BackendAutomatedTestingClient.Games.HintHint
{
    [EndToEndGameTest(TestName)]
    public class HintHintTest : GameTest
    {
        private const string TestName = "HintHint";
        public override string GameModeTitle { get; } = "Hint Hint";

        public override UserFormSubmission HandleUserPrompt(UserPrompt userPrompt, LobbyPlayer player, int gameStep)
        {
            switch (userPrompt.UserPromptId)
            {
                case UserPromptId.HintHint_SetupRound1:
                    Console.WriteLine($"{TestName}:Making Word");
                    return MakeWord(player);

                case UserPromptId.HintHint_SetupRound2:
                    Console.WriteLine($"{TestName}:Creating Banned Words");
                    return MakeBanned(player, userPrompt.SubPrompts.Count());

                case UserPromptId.HintHint_SetupRound3:
                    Console.WriteLine($"{TestName}:Selecting Fake Word");
                    return PickFake(player);

                case UserPromptId.HintHint_Hint:
                    Console.WriteLine($"{TestName}:Submitting Hint");
                    return SubmitHint(player);

                case UserPromptId.HintHint_Guess:
                    Console.WriteLine($"{TestName}:Submitting Guess");
                    return SubmitGuess(player);

                case UserPromptId.PartyLeader_SkipReveal:

                case UserPromptId.PartyLeader_SkipScoreboard:
                    Console.WriteLine($"{TestName}:Submitting Skip");
                    return CommonSubmissions.SubmitSkipReveal(player.UserId, userPrompt);

                case UserPromptId.RevealScoreBreakdowns:

                case UserPromptId.Waiting:
                    return null;
                default:
                    throw new Exception($"Unexpected UserPromptId '{userPrompt.UserPromptId}', userId='{player.UserId}'");
            }
        }

        protected virtual UserFormSubmission MakeWord(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSingleText(player.UserId, "Real");
        }
        protected virtual UserFormSubmission MakeBanned(LobbyPlayer player, int numBanned)
        {
            Debug.Assert(player.UserId.Length == 50);
            List<UserSubForm> subForms = new List<UserSubForm>();
            for (int i = 0; i < numBanned; i++)
            {
                subForms.Add(new UserSubForm()
                {
                    ShortAnswer = "Banned"
                });
            }
            return new UserFormSubmission
            {
                SubForms = subForms
            };
        }
        protected virtual UserFormSubmission PickFake(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSingleRadio(player.UserId);
        }
        protected virtual UserFormSubmission SubmitHint(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSingleText(player.UserId, "Hint");
        }
        protected enum GuessType
        {
            Real,
            Fake,
            Standard,
        }
        protected virtual UserFormSubmission SubmitGuess(LobbyPlayer player, GuessType guessType = GuessType.Standard)
        {
            switch (guessType)
            {
                case GuessType.Real:
                    return CommonSubmissions.SubmitSingleText(player.UserId, "Real");
                case GuessType.Fake:
                    return CommonSubmissions.SubmitSingleText(player.UserId, "Banned");
                default:
                    return CommonSubmissions.SubmitSingleText(player.UserId, "Guess");
            }
        }
    }
}
