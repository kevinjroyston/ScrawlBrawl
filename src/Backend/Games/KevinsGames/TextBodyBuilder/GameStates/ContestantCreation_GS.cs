using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.KevinsGames.TextBodyBuilder.DataModels;
using Backend.Games.Common.ThreePartPeople;
using Backend.Games.Common.ThreePartPeople.DataModels;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using static Backend.Games.Common.ThreePartPeople.DataModels.Person;
using static System.FormattableString;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;
using Common.Code.Extensions;
using static Backend.Games.KevinsGames.TextBodyBuilder.DataModels.TextPerson;
using Common.DataModels.Responses.Gameplay;

namespace Backend.Games.KevinsGames.TextBodyBuilder.GameStates
{
    public class ContestantCreation_GS : GameState
    {
        private RoundTracker RoundTracker { get; set; }

        private TimeSpan? TotalChainDuration { get; }

        public ContestantCreation_GS(Lobby lobby, RoundTracker roundTracker, TimeSpan? creationDuration)
            : base(
                  lobby:lobby,
                  exit: new WaitForUsers_StateExit(lobby))
        {
            this.RoundTracker = roundTracker;
            TimeSpan? multipliedCreationDuration = creationDuration.MultipliedBy(roundTracker.UsersToAssignedPrompts.Values.Max(list=>list?.Count??1));
            this.TotalChainDuration = multipliedCreationDuration;
            
            MultiStateChain contestantsMultiStateChain = new MultiStateChain(MakePeopleUserStateChain, stateDuration: multipliedCreationDuration);

            this.Entrance.Transition(contestantsMultiStateChain);
            contestantsMultiStateChain.Transition(this.Exit);

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = TVScreenId.WaitForUserInputs,
                Title = new UnityField<string> { Value = "Make your contestants on your devices." },
            };
        }

        private List<State> MakePeopleUserStateChain(User user)
        {
            List<State> stateChain = new List<State>();
            if (!RoundTracker.UsersToAssignedPrompts.ContainsKey(user))
            {
                return stateChain;
            }
            int index = 0;
            foreach(Prompt promptIter in RoundTracker.UsersToAssignedPrompts[user])
            {
                Prompt prompt = promptIter;
                var lambdaSafeIndex = index;
                stateChain.Add(new SimplePromptUserState(
                    promptGenerator: (User user) =>
                    {       
                        return new UserPrompt
                        {
                            UserPromptId = UserPromptId.TextBodyBuilder_ContestantCreation,
                            Title = Invariant($"Make the best character for the prompt<div class='createPrompt'>{prompt.Text}</div>"),
                            PromptHeader = new PromptHeaderMetadata
                            {
                                MaxProgress = RoundTracker.UsersToAssignedPrompts[user].Count,
                                CurrentProgress = lambdaSafeIndex + 1,
                                ExpectedTimePerPrompt = this.TotalChainDuration.MultipliedBy(1.0f / RoundTracker.UsersToAssignedPrompts[user].Count)
                            },
                            Description = "Format is <span class='characterClass'>Character</span> <span class='actionClass'>Action</span> <span class='modifierClass'>Modifier</span>",
                            Suggestion = new SuggestionMetadata { SuggestionKey = $"TextBodyBuilder-Modifier" },
                            SubPrompts = new SubPrompt[]
                            {
                                new SubPrompt
                                {
                                    Prompt = "<span class='characterClass'>Character:</span>",
                                    Answers = prompt.UsersToUserHands[user].CharacterChoices.Select(userText => userText.Text).ToArray(),
                                },
                                new SubPrompt
                                {
                                    Prompt = "<span class='actionClass'>Action:</span>",
                                    Answers = prompt.UsersToUserHands[user].ActionChoices.Select(userText => userText.Text).ToArray(),
                                },
                                new SubPrompt
                                {
                                    Prompt = "<span class='modifierClass'>Modifier:</span>",
                                    ShortAnswer = true,
                                    //Answers = prompt.UsersToUserHands[user].ModifierChoices.Select(userText => userText.Text).ToArray(),
                                },
                            },
                            SubmitButton = true,
                        };
                    },
                    formSubmitHandler: (User user, UserFormSubmission input) =>
                    {
                        string modifier = input.SubForms[2].ShortAnswer;
                        // fix casing of modifier.
                        if (Char.IsUpper(modifier[0]) && !Char.IsUpper(modifier[1]))
                        { modifier = Char.ToLower(modifier[0]).ToString() + modifier.Substring(1); }

                        prompt.UsersToUserHands[user].Descriptors = new Dictionary<CAMType, CAMUserText>{
                                {CAMType.Character, prompt.UsersToUserHands[user].CharacterChoices[(int)input.SubForms[0].RadioAnswer] },
                                {CAMType.Action, prompt.UsersToUserHands[user].ActionChoices[(int)input.SubForms[1].RadioAnswer] },
                                {CAMType.Modifier, new CAMUserText(){ Text = modifier, Type = CAMType.Modifier, Owner = user} }
                            };
                        prompt.UsersToUserHands[user].Owner = user;
                        return (true, String.Empty);
                    }
                    ));
                index++;
            }
            return stateChain;
        }
    }
}
