﻿using Common.DataModels.Enums;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using BackendAutomatedTestingClient.DataModels;
using BackendAutomatedTestingClient.WebClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BackendAutomatedTestingClient.Games
{
    [EndToEndGameTest(TestName)]
    public class BodySwapTest : GameTest
    {
        private const string TestName = "BodySwap";
        public override string GameModeTitle => "Body Swap";
        private Random Rand = new Random();
        public override UserFormSubmission HandleUserPrompt(UserPrompt userPrompt, LobbyPlayer player, int gameStep)
        {
            switch (userPrompt.UserPromptId)
            {
                case UserPromptId.BodyBuilder_CreatePrompts:
                    Console.WriteLine($"{TestName}:Submitting Prompt");
                    return MakePrompts(player);
                case UserPromptId.BodyBuilder_DrawBodyPart:
                    Console.WriteLine($"{TestName}:Submitting Drawing");
                    return MakeDrawing(player);
                case UserPromptId.BodyBuilder_TradeBodyPart:
                    Console.WriteLine($"{TestName}:Submitting Swap");
                    return Swap(userPrompt, player);
                case UserPromptId.PartyLeader_SkipReveal:
                case UserPromptId.PartyLeader_SkipScoreboard:
                    Console.WriteLine($"{TestName}:Submitting Skip");
                    return CommonSubmissions.SubmitSkipReveal(player.UserId, userPrompt);
                case UserPromptId.BodyBuilder_FinishedPerson:
                case UserPromptId.RevealScoreBreakdowns:
                case UserPromptId.Waiting:
                    return null;
                default:
                    throw new Exception($"Unexpected UserPromptId '{userPrompt.UserPromptId}', userId='{player.UserId}'");
            }
        }
        private UserFormSubmission MakePrompts(LobbyPlayer player)
        {
            Debug.Assert(player.UserId.Length == 50);

            return new UserFormSubmission
            {
                SubForms = new List<UserSubForm>()
                {
                    new UserSubForm()
                    {
                        ShortAnswer = Helpers.GetRandomString()
                    },
                    new UserSubForm()
                    {
                        ShortAnswer = Helpers.GetRandomString()
                    },
                }
            };
           
        }

        private UserFormSubmission MakeDrawing(LobbyPlayer player)
        {
            return CommonSubmissions.SubmitSingleDrawing(player.UserId);
        }
        private UserFormSubmission Swap(UserPrompt prompt, LobbyPlayer player)
        {
            int answer = Rand.Next(0, prompt.SubPrompts[1].Answers.Length);

            return new UserFormSubmission
            {
                SubForms = new List<UserSubForm>()
                {
                    new UserSubForm()
                    {
                    },
                    new UserSubForm()
                    {
                        RadioAnswer = answer
                    }
                }
            };
        }

    }
}
