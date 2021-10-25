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

        public ContestantCreation_GS(Lobby lobby, RoundTracker roundTracker, TimeSpan? creationDuration)
            : base(
                  lobby:lobby,
                  exit: new WaitForUsers_StateExit(lobby))
        {
            this.RoundTracker = roundTracker;
            TimeSpan? multipliedCreationDuration = creationDuration.MultipliedBy(roundTracker.UsersToAssignedPrompts.Values.ToList()[0].Count);
            
            MultiStateChain contestantsMultiStateChain = new MultiStateChain(MakePeopleUserStateChain, stateDuration: multipliedCreationDuration);

            this.Entrance.Transition(contestantsMultiStateChain);
            contestantsMultiStateChain.Transition(this.Exit);

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = TVScreenId.WaitForUserInputs,
                Instructions = new UnityField<string> { Value = "Make your contestants on your devices." },
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
                            Title = Invariant($"Make the best character for this prompt: \"{prompt.Text}\""),
                            PromptHeader = new PromptHeaderMetadata
                            {
                                MaxProgress = RoundTracker.UsersToAssignedPrompts[user].Count,
                                CurrentProgress = lambdaSafeIndex + 1,
                            },
                            Description = "Format is '<Character> <Action> <Modifier>'",
                            SubPrompts = new SubPrompt[]
                            {
                                new SubPrompt
                                {
                                    Prompt = "Character:",
                                    Answers = prompt.UsersToUserHands[user].CharacterChoices.Select(userText => userText.Text).ToArray(),
                                },
                                new SubPrompt
                                {
                                    Prompt = "Action:",
                                    Answers = prompt.UsersToUserHands[user].ActionChoices.Select(userText => userText.Text).ToArray(),
                                },
                                new SubPrompt
                                {
                                    Prompt = "Modifier:",
                                    Answers = prompt.UsersToUserHands[user].ModifierChoices.Select(userText => userText.Text).ToArray(),
                                },
                            },
                            SubmitButton = true,
                        };
                    },
                    formSubmitHandler: (User user, UserFormSubmission input) =>
                    {
                        prompt.UsersToUserHands[user].Descriptors = new Dictionary<CAMType, CAMUserText>{
                                {CAMType.Character, prompt.UsersToUserHands[user].CharacterChoices[(int)input.SubForms[0].RadioAnswer] },
                                {CAMType.Action, prompt.UsersToUserHands[user].ActionChoices[(int)input.SubForms[1].RadioAnswer] },
                                {CAMType.Modifier, prompt.UsersToUserHands[user].ModifierChoices[(int)input.SubForms[2].RadioAnswer] }
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
