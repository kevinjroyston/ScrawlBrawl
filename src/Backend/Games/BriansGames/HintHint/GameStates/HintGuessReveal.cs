using Backend.APIs.DataModels.Enums;
using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.HintHint.DataModels;
using Common.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.FormattableString;

namespace Backend.Games.BriansGames.HintHint.GameStates
{
    public class HintGuessReveal : GameState
    {
        public HintGuessReveal(Lobby lobby, RealFakePair realFakePair) : base(
                lobby: lobby,
                exit: new WaitForPartyLeader_StateExit(
                    lobby: lobby,
                    partyLeaderPromptGenerator: Prompts.ShowScoreBreakdowns(
                        lobby: lobby,
                        promptTitle: "",
                        userPromptId: UserPromptId.PartyLeader_SkipReveal,
                        userScoreBreakdownScope: Score.Scope.Reveal,
                        showPartyLeaderSkipButton: true),

                    waitingPromptGenerator: Prompts.ShowScoreBreakdowns(
                        lobby: lobby,
                        promptTitle: "",
                        userScoreBreakdownScope: Score.Scope.Reveal)))
        {
            this.Entrance.Transition(this.Exit);


            List<StackItemHolder> revealStack = new List<StackItemHolder>();

            switch (realFakePair.RoundEndCondition)
            {
                case EndCondition.GuessedReal:
                    revealStack.Add(new StackItemHolder()
                    {
                        Text = $"Congrats! <color=green>{realFakePair.RelaventUsersWhoGuessed[0]}</color> guessed the word!"
                    });
                    break;
                case EndCondition.GuessedFake:
                    revealStack.Add(new StackItemHolder()
                    {
                        Text = $"Oops! <color=red>{realFakePair.RelaventUsersWhoGuessed[0]}</color> was tricked!"
                    });
                    break;
                default:
                    revealStack.Add(new StackItemHolder()
                    {
                        Text = $"Nobody guessed the word."
                    });
                    break;
            }
            revealStack.Add(new StackItemHolder()
            {
                Text = $"Real: <color=green>{realFakePair.RealGoal}</color>"
            });
            revealStack.Add(new StackItemHolder()
            {
                Text = $"Trap: <color=red>{realFakePair.FakeGoal}</color>"
            });
            this.UnityView = new UnityView(lobby)
            {
                ScreenId = TVScreenId.HintGuessView,
                UnityObjects = new UnityField<IReadOnlyList<UnityObject>>()
                {
                    Value = new List<UnityTextStack>()
                    {
                        new UnityTextStack()
                        {
                            Title = new UnityField<string> { Value = "Hints" },
                            FixedNumItems = HintConstants.NumHintGuessesToShow,
                            TextStackList = new UnityField<IReadOnlyList<StackItemHolder>>()
                            {
                                Value = realFakePair.LastHints,
                                Options = new Dictionary<UnityFieldOptions, object>()
                                {
                                    { UnityFieldOptions.ScrollingTextStack, false}
                                }
                            }
                        },
                        new UnityTextStack()
                        {
                            TextStackList = new UnityField<IReadOnlyList<StackItemHolder>>()
                            {
                                Value = revealStack,
                                Options = new Dictionary<UnityFieldOptions, object>()
                                {
                                    { UnityFieldOptions.ScrollingTextStack, false}
                                }
                            }
                        },
                    }
                }
            };
        }
    }
}
