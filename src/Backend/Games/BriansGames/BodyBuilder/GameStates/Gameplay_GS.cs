using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.BodyBuilder.DataModels;
using Backend.Games.Common;
using Backend.Games.Common.ThreePartPeople;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using static Backend.Games.Common.ThreePartPeople.DataModels.Person;
using static System.FormattableString;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.Enums;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;

namespace Backend.Games.BriansGames.BodyBuilder.GameStates
{
    public class Gameplay_GS : GameState
    {
        private Random Rand { get; } = new Random();
        private RoundTracker RoundTracker { get; }
        private List<User> UsersStillPlaying { get; set; } 
        private UserPrompt PickADrawing(User user)
        {
            Gameplay_Person PlayerHand = RoundTracker.AssignedPeople[user];
            Gameplay_Person PlayerTrade = RoundTracker.UnassignedPeople[RoundTracker.UsersToSeatNumber[user]];
                
            return new UserPrompt
            {
                UserPromptId = UserPromptId.BodyBuilder_TradeBodyPart,
                Title = "This is your current person",
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        StringList = new string[]
                        {
                            CommonHelpers.HtmlImageWrapper(PlayerHand.BodyPartDrawings[BodyPartType.Head].Drawing,ThreePartPeopleConstants.Widths[BodyPartType.Head],ThreePartPeopleConstants.Heights[BodyPartType.Head]),
                            CommonHelpers.HtmlImageWrapper(PlayerHand.BodyPartDrawings[BodyPartType.Body].Drawing,ThreePartPeopleConstants.Widths[BodyPartType.Body],ThreePartPeopleConstants.Heights[BodyPartType.Body]),
                            CommonHelpers.HtmlImageWrapper(PlayerHand.BodyPartDrawings[BodyPartType.Legs].Drawing,ThreePartPeopleConstants.Widths[BodyPartType.Legs],ThreePartPeopleConstants.Heights[BodyPartType.Legs])
                        },
                    },
                    new SubPrompt
                    {
                        Prompt = "Which body part do you want to trade?",
                        Answers = new string[]
                        {
                            CommonHelpers.HtmlImageWrapper(PlayerTrade.BodyPartDrawings[BodyPartType.Head].Drawing,ThreePartPeopleConstants.Widths[BodyPartType.Head],ThreePartPeopleConstants.Heights[BodyPartType.Head]),
                            CommonHelpers.HtmlImageWrapper(PlayerTrade.BodyPartDrawings[BodyPartType.Body].Drawing,ThreePartPeopleConstants.Widths[BodyPartType.Body],ThreePartPeopleConstants.Heights[BodyPartType.Body]),
                            CommonHelpers.HtmlImageWrapper(PlayerTrade.BodyPartDrawings[BodyPartType.Legs].Drawing,ThreePartPeopleConstants.Widths[BodyPartType.Legs],ThreePartPeopleConstants.Heights[BodyPartType.Legs]),
                            "None"
                        },
                    }
                },
                SubmitButton = true,
            };
        }
        private int CurrentFinishingPosition { get; set; } = 1;
        private int NumPlayersDoneWithRound { get; set; } = 0;
        private int RoundMaxTurnLimit { get; }
        private int RoundCount = 0;
        private State CurrentRoundUserState { get; set; }
        private TimeSpan? TurnTimeLimit { get; set; } = null;
        Dictionary<int, int> WinningScoresByPlace { get; set; } = new Dictionary<int, int>()
        {
            {1, 1000},   // First Place
            {2, 800},    // Second Place
            {3, 600}     // Third Place
        };
        public Gameplay_GS(Lobby lobby, List<Setup_Person> setup_PeopleList, RoundTracker roundTracker, bool displayPool, bool displayNames, int roundTimeoutLimit, TimeSpan? perRoundTimeoutDuration = null) : base(lobby)
        {
            this.RoundTracker = roundTracker;
            this.AssignPeople(setup_PeopleList);
            this.AssignSeats();
            this.TurnTimeLimit = perRoundTimeoutDuration;
            this.RoundMaxTurnLimit = roundTimeoutLimit;
            this.Entrance.Transition(AddGameplayCycle());
            var unityImages = new List<Legacy_UnityImage>();
            string instructions = null;
            string title = null;

            if (displayNames)
            {
                instructions = String.Join("         ", setup_PeopleList.Select((Setup_Person person) => person.Name));
            }

            if (displayPool)
            {
                title = "Here's What's in the pool";
                for (int i = 0; i < roundTracker.UnassignedPeople.Count; i++)
                {
                    var lambdaSafeIndex = i;
                    unityImages.Add(new Legacy_UnityImage
                    {
                        Base64Pngs = new DynamicAccessor<IReadOnlyList<string>> { DynamicBacker = () => roundTracker.UnassignedPeople[lambdaSafeIndex].GetOrderedDrawings() },
                        BackgroundColor = new StaticAccessor<IReadOnlyList<int>> { Value = new List<int> { 255, 255, 255 } },
                        SpriteGridWidth = new StaticAccessor<int?> { Value = 1 },
                        SpriteGridHeight = new StaticAccessor<int?> { Value = 3 },
                        Header = new StaticAccessor<string> { Value = roundTracker.OrderedUsers[lambdaSafeIndex].DisplayName }
                    });
                }
                
            }

            this.Legacy_UnityView = new Legacy_UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<Legacy_UnityImage>> { Value = unityImages },
                Title = new StaticAccessor<string> { Value = title },
                Instructions = new StaticAccessor<string> { Value = instructions },
            };

        }

        private void ProcessUserInput(User user, BodyPartType? answer)
        {
            Gameplay_Person PlayerHand = RoundTracker.AssignedPeople[user];
            Gameplay_Person PlayerTrade = RoundTracker.UnassignedPeople[RoundTracker.UsersToSeatNumber[user]];

            PeopleUserDrawing temp = PlayerHand.BodyPartDrawings[answer.Value];
            PlayerHand.BodyPartDrawings[answer.Value] = PlayerTrade.BodyPartDrawings[answer.Value];
            PlayerTrade.BodyPartDrawings[answer.Value] = temp;
        }

        private State AddGameplayCycle()
        {
            /* ask users what changes they want to make
             * perform said changes
             * see if anyone won
             */
            RotateSeats();
            UserTimeoutAction UserTimeoutHandler(User user, UserFormSubmission input)
            {
                BodyPartType? answer = (BodyPartType?)input?.SubForms?[1]?.RadioAnswer;

                if (answer == null)
                {
                    // Make a random decision.
                    BodyPartType randAnswer = (BodyPartType)Rand.Next(3);
                }

                if (answer != BodyPartType.None)
                {
                    // Normal flow.
                    ProcessUserInput(user, answer);
                }
                CheckPlayerWon(user);
                return UserTimeoutAction.None;
            }

            (bool, string) PromptedUserFormSubmission( User user, UserFormSubmission submission)
            {
                BodyPartType? answer = (BodyPartType?)submission.SubForms[1].RadioAnswer;
                if (answer == null)
                {
                    return (false, "Please enter a valid option");
                }
                if (answer != BodyPartType.None)
                {
                    ProcessUserInput(user, answer);
                }
                CheckPlayerWon(user);
                return (true, string.Empty);
            }
            Func<User,UserPrompt> WaitingStatePromptGenerator = new Func<User, UserPrompt>((user) =>
            {
                Gameplay_Person PlayerHand = RoundTracker.AssignedPeople[user];
                Gameplay_Person PlayerTrade = RoundTracker.UnassignedPeople[RoundTracker.UsersToSeatNumber[user]];
                if (PlayerHand.DoneWithRound)
                {
                    return new UserPrompt
                    {
                        UserPromptId = UserPromptId.BodyBuilder_FinishedPerson,
                        Title = "You are done! This is your final person",
                        SubPrompts = new SubPrompt[]
                        {
                            new SubPrompt
                            {
                                StringList = new string[]
                                {
                                    CommonHelpers.HtmlImageWrapper(PlayerHand.BodyPartDrawings[BodyPartType.Head].Drawing, ThreePartPeopleConstants.Widths[BodyPartType.Head], ThreePartPeopleConstants.Heights[BodyPartType.Head]),
                                    CommonHelpers.HtmlImageWrapper(PlayerHand.BodyPartDrawings[BodyPartType.Body].Drawing, ThreePartPeopleConstants.Widths[BodyPartType.Body], ThreePartPeopleConstants.Heights[BodyPartType.Body]),
                                    CommonHelpers.HtmlImageWrapper(PlayerHand.BodyPartDrawings[BodyPartType.Legs].Drawing, ThreePartPeopleConstants.Widths[BodyPartType.Legs], ThreePartPeopleConstants.Heights[BodyPartType.Legs])
                                },
                            },
                        },
                        SubmitButton = false
                    };
                }
                else
                {
                    return Prompts.DisplayText()(user);
                }
            });
  
            UserState promptAndWaitForUsers = new SelectivePromptUserState(
                usersToPrompt: UsersStillPlaying,
                promptGenerator: PickADrawing,
                formSubmitHandler: PromptedUserFormSubmission,
                maxPromptDuration: this.TurnTimeLimit,
                userTimeoutHandler: UserTimeoutHandler,
                exit: new WaitForUsers_StateExit(
                    lobby: this.Lobby,
                    usersToWaitFor: WaitForUsersType.All,
                    waitingPromptGenerator: WaitingStatePromptGenerator));
            this.CurrentRoundUserState = promptAndWaitForUsers;

            promptAndWaitForUsers.Transition(()=>
            {
                if (this.GameFinished() || RoundCount >= RoundMaxTurnLimit)
                {
                    return this.Exit;
                }
                else
                {
                    RoundCount++;
                    return AddGameplayCycle();
                }

            });
            return promptAndWaitForUsers;
        }
        private void AssignPeople(List<Setup_Person> setup_People)
        {
            RoundTracker.ResetRoundVariables();
            List<PeopleUserDrawing> heads;
            List<PeopleUserDrawing> bodies;
            List<PeopleUserDrawing> legs;
            
            heads = setup_People.Select(val => val.BodyPartDrawings[BodyPartType.Head]).Where(val => !string.IsNullOrWhiteSpace(val.Drawing)).OrderBy(_ => Rand.Next()).ToList();
            bodies = setup_People.Select(val => val.BodyPartDrawings[BodyPartType.Body]).Where(val => !string.IsNullOrWhiteSpace(val.Drawing)).OrderBy(_ => Rand.Next()).ToList();
            legs = setup_People.Select(val => val.BodyPartDrawings[BodyPartType.Legs]).Where(val => !string.IsNullOrWhiteSpace(val.Drawing)).OrderBy(_ => Rand.Next()).ToList();

            if (heads.Count != bodies.Count || bodies.Count != legs.Count || heads.Count != 2*this.Lobby.GetAllUsers().Count)
            {
                throw new Exception("Something Went Wrong While Setting Up Game");
            }
            
            foreach (User user in this.Lobby.GetAllUsers())
            {
                Gameplay_Person temp = new Gameplay_Person
                {
                    Owner = user,
                };
                temp.BodyPartDrawings[BodyPartType.Head] = heads.First();
                temp.BodyPartDrawings[BodyPartType.Body] = bodies.First();
                temp.BodyPartDrawings[BodyPartType.Legs] = legs.First();
                heads.RemoveAt(0);
                bodies.RemoveAt(0);
                legs.RemoveAt(0);

                temp.DoneWithRound = false;
                temp.Name = user.DisplayName;
                RoundTracker.AssignedPeople.Add(user, temp);
            }
            while (heads.Any())
            {
                Gameplay_Person temp = new Gameplay_Person
                {
                    Owner = null,
                };
                temp.BodyPartDrawings[BodyPartType.Head] = heads.First();
                temp.BodyPartDrawings[BodyPartType.Body] = bodies.First();
                temp.BodyPartDrawings[BodyPartType.Legs] = legs.First();
                heads.RemoveAt(0);
                bodies.RemoveAt(0);
                legs.RemoveAt(0);

                temp.DoneWithRound = false;
                RoundTracker.UnassignedPeople.Add(temp);
            }

        }
        private bool GameFinished()
        {
            bool someoneFinished = false;
            foreach(User user in this.Lobby.GetAllUsers())
            {
                if(RoundTracker.AssignedPeople[user].DoneWithRound && !RoundTracker.AssignedPeople[user].BeenScored)
                {
                    RoundTracker.AssignedPeople[user].BeenScored = true;
                    RoundTracker.AssignedPeople[user].FinishedPosition = Invariant($"{CurrentFinishingPosition}");
                    user.ScoreHolder.AddScore(WinningScoresByPlace[CurrentFinishingPosition], Score.Reason.Finished);
                    someoneFinished = true;
                }
            }
            if (someoneFinished)
            {
                CurrentFinishingPosition++;
            }
            if (NumPlayersDoneWithRound >= 3)
            {
                return true;
            }
            return false;
        }
        private void CheckPlayerWon(User user)
        {
            Guid headId = RoundTracker.AssignedPeople[user].BodyPartDrawings[BodyPartType.Head].Id;
            Guid bodyId = RoundTracker.AssignedPeople[user].BodyPartDrawings[BodyPartType.Body].Id;
            Guid legsId = RoundTracker.AssignedPeople[user].BodyPartDrawings[BodyPartType.Legs].Id;
            if (headId == bodyId && bodyId == legsId && !RoundTracker.AssignedPeople[user].DoneWithRound)
            {
                NumPlayersDoneWithRound++;
                RoundTracker.AssignedPeople[user].DoneWithRound = true;
                UsersStillPlaying.Remove(user);
            }
        }
        private void RotateSeats()
        {
            
            Gameplay_Person first = RoundTracker.UnassignedPeople[0];
            RoundTracker.UnassignedPeople.RemoveAt(0);
            RoundTracker.UnassignedPeople.Add(first);
        }
        private void AssignSeats()
        {
            int count = 0;
            foreach (User user in this.Lobby.GetAllUsers().OrderBy(_ => Rand.Next()).ToList())
            {
                RoundTracker.OrderedUsers.Add(user);
                RoundTracker.UsersToSeatNumber.Add(user, count);
                count++;
            }
            UsersStillPlaying = new List<User>(RoundTracker.OrderedUsers);
        }
    }
}
