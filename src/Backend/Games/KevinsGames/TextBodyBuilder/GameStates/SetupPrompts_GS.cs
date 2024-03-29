﻿using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.KevinsGames.TextBodyBuilder.DataModels;
using Backend.Games.Common.GameStates.Setup;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;
using System.Collections.Concurrent;
using System.Linq;
using static System.FormattableString;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;
using Common.DataModels.Responses.Gameplay;
using Common.Code.Extensions;

namespace Backend.Games.KevinsGames.TextBodyBuilder.GameStates
{
    public class SetupPrompts_GS : FixedCountSetupGameState
    {
        private Random Rand { get; } = new Random();
        private ConcurrentBag<Prompt> Prompts { get; set; }
        private TimeSpan? SetupDuration;
        public SetupPrompts_GS(
            Lobby lobby,
            ConcurrentBag<Prompt> prompts,
            int numExpectedPerUser,
            TimeSpan? setupDuration = null)
            : base(
                lobby: lobby,
                numExpectedPerUser: numExpectedPerUser,
                unityTitle: "Complete the prompts on your device",
                unityInstructions: "",
                setupDuration: setupDuration)
        {
            this.SetupDuration = setupDuration;
            this.Prompts = prompts;
        }

        public override UserPrompt CountingPromptGenerator(User user, int counter)
        {
            return new UserPrompt()
            {
                UserPromptId = UserPromptId.TextBodyBuilder_CreatePrompts,
                Title = Invariant($"Now lets make some prompts/scenarios!"),
                PromptHeader = new PromptHeaderMetadata
                {
                    MaxProgress = NumExpectedPerUser,
                    CurrentProgress = counter + 1,
                    ExpectedTimePerPrompt = this.SetupDuration.MultipliedBy(1.0f / NumExpectedPerUser)
                },
                Description = "Examples: 'Who would win in a fight?', 'A true problem solver', 'Jack of all trades', Etc.",
                Suggestion = new SuggestionMetadata { SuggestionKey = "TextBodyBuilder-Prompt" },
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt="Prompt",
                        ShortAnswer=true
                    },
                },
                SubmitButton = true
            };
        }
        public override (bool, string) CountingFormSubmitHandler(User user, UserFormSubmission input, int counter)
        {
            if (Prompts.Select((prompt) => prompt.Text).Contains(input.SubForms[0].ShortAnswer))
            {
                return (false, "Someone has already entered that prompt");
            }
            Prompts.Add( 
                new Prompt()
                {
                    Owner = user,
                    Text = input.SubForms[0].ShortAnswer
                });
            return (true, String.Empty);
        }
        public override UserTimeoutAction CountingUserTimeoutHandler(User user, UserFormSubmission input, int counter)
        {
            if (input?.SubForms?[0]?.ShortAnswer != null
            && !Prompts.Select((prompt) => prompt.Text).Contains(input.SubForms[0].ShortAnswer))
            {
                Prompts.Add(
                    new Prompt()
                    {
                        Owner = user,
                        Text = input.SubForms[0].ShortAnswer
                    });
            }
            return UserTimeoutAction.None;
        }
    }
}
