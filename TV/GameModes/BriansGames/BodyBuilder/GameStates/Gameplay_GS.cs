using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels;
using RoystonGame.TV.GameModes.Common;
using RoystonGame.TV.GameModes.Common.ThreePartPeople;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels.Person;
using static System.FormattableString;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.ControlFlows.Exit;

namespace RoystonGame.TV.GameModes.BriansGames.BodyBuilder.GameStates
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
                Title = "This is your current person",
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        StringList = new string[]
                        {
                            CommonHelpers.HtmlImageWrapper(PlayerHand.BodyPartDrawings[DrawingType.Head].Drawing,ThreePartPeopleConstants.Widths[DrawingType.Head],ThreePartPeopleConstants.Heights[DrawingType.Head]),
                            CommonHelpers.HtmlImageWrapper(PlayerHand.BodyPartDrawings[DrawingType.Body].Drawing,ThreePartPeopleConstants.Widths[DrawingType.Body],ThreePartPeopleConstants.Heights[DrawingType.Body]),
                            CommonHelpers.HtmlImageWrapper(PlayerHand.BodyPartDrawings[DrawingType.Legs].Drawing,ThreePartPeopleConstants.Widths[DrawingType.Legs],ThreePartPeopleConstants.Heights[DrawingType.Legs])
                        },
                    },
                    new SubPrompt
                    {
                        Prompt = "Which body part do you want to trade?",
                        Answers = new string[]
                        {
                            CommonHelpers.HtmlImageWrapper(PlayerTrade.BodyPartDrawings[DrawingType.Head].Drawing,ThreePartPeopleConstants.Widths[DrawingType.Head],ThreePartPeopleConstants.Heights[DrawingType.Head]),
                            CommonHelpers.HtmlImageWrapper(PlayerTrade.BodyPartDrawings[DrawingType.Body].Drawing,ThreePartPeopleConstants.Widths[DrawingType.Body],ThreePartPeopleConstants.Heights[DrawingType.Body]),
                            CommonHelpers.HtmlImageWrapper(PlayerTrade.BodyPartDrawings[DrawingType.Legs].Drawing,ThreePartPeopleConstants.Widths[DrawingType.Legs],ThreePartPeopleConstants.Heights[DrawingType.Legs]),
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
        Dictionary<int, int> WinningScoresByPlace { get; set; } = new Dictionary<int, int>()
        {
            {1, 1000},   // First Place
            {2, 500},    // Second Place
            {3, 250}     // Third Place
        };
        public Gameplay_GS(Lobby lobby, List<Setup_Person> setup_PeopleList, RoundTracker roundTracker, bool displayPool, bool displayNames, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions) : base(lobby)
        {
            this.RoundTracker = roundTracker;
            this.AssignPeople(setup_PeopleList);
            this.AssignSeats();       
            this.Entrance.Transition(AddGameplayCycle());
            RoundMaxTurnLimit = int.Parse(gameModeOptions[3].ShortAnswer);
            var unityImages = new List<UnityImage>();
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
                    unityImages.Add(new UnityImage
                    {
                        Base64Pngs = new DynamicAccessor<IReadOnlyList<string>> { DynamicBacker = () => roundTracker.UnassignedPeople[lambdaSafeIndex].GetOrderedDrawings() },
                        BackgroundColor = new StaticAccessor<IReadOnlyList<int>> { Value = new List<int> { 255, 255, 255 } },
                        SpriteGridWidth = new StaticAccessor<int?> { Value = 1 },
                        SpriteGridHeight = new StaticAccessor<int?> { Value = 3 },
                        Header = new StaticAccessor<string> { Value = roundTracker.OrderedUsers[lambdaSafeIndex].DisplayName }
                    });
                }
                
            }

            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages },
                Title = new StaticAccessor<string> { Value = title },
                Instructions = new StaticAccessor<string> { Value = instructions },
            };

        }
        private State AddGameplayCycle()
        {
            /* ask users what changes they want to make
             * perform said changes
             * see if anyone won
             */
            RotateSeats();
            (bool, string) PromptedUserFormSubmission( User user, UserFormSubmission submission)
            {
                Gameplay_Person PlayerHand = RoundTracker.AssignedPeople[user];
                Gameplay_Person PlayerTrade = RoundTracker.UnassignedPeople[RoundTracker.UsersToSeatNumber[user]];
                DrawingType? answer = (DrawingType?)submission.SubForms[1].RadioAnswer;
                if (answer == null)
                {
                    return (false, "Please enter a valid option");
                }
                if (answer != DrawingType.None)
                {
                    PeopleUserDrawing temp = PlayerHand.BodyPartDrawings[answer.Value];
                    PlayerHand.BodyPartDrawings[answer.Value] = PlayerTrade.BodyPartDrawings[answer.Value];
                    PlayerTrade.BodyPartDrawings[answer.Value] = temp;
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
                        Title = "You are done! This is your final person",

                        SubPrompts = new SubPrompt[]
                                            {
                        new SubPrompt
                        {
                            StringList = new string[]
                                {
                                    CommonHelpers.HtmlImageWrapper(PlayerHand.BodyPartDrawings[DrawingType.Head].Drawing, ThreePartPeopleConstants.Widths[DrawingType.Head], ThreePartPeopleConstants.Heights[DrawingType.Head]),
                                    CommonHelpers.HtmlImageWrapper(PlayerHand.BodyPartDrawings[DrawingType.Body].Drawing, ThreePartPeopleConstants.Widths[DrawingType.Body], ThreePartPeopleConstants.Heights[DrawingType.Body]),
                                    CommonHelpers.HtmlImageWrapper(PlayerHand.BodyPartDrawings[DrawingType.Legs].Drawing, ThreePartPeopleConstants.Widths[DrawingType.Legs], ThreePartPeopleConstants.Heights[DrawingType.Legs])
                                },
                        },
                        },
                        SubmitButton = false
                    };
                }
                else
                {
                    return SimplePromptUserState.DefaultWaitingPrompt(user);
                }
            });
  
            UserState promptAndWaitForUsers = new SelectivePromptUserState(
                usersToPrompt: UsersStillPlaying,
                promptGenerator: PickADrawing,
                formSubmitHandler: PromptedUserFormSubmission,
                exit: new WaitForUsers_StateExit(
                    lobby: this.Lobby,
                    usersToWaitFor: WaitForUsersType.All,
                    waitingPromptGenerator: WaitingStatePromptGenerator));

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
            heads = setup_People.Select(val => val.BodyPartDrawings[DrawingType.Head]).OrderBy(_ => Rand.Next()).ToList();
            bodies = setup_People.Select(val => val.BodyPartDrawings[DrawingType.Body]).OrderBy(_ => Rand.Next()).ToList();
            legs = setup_People.Select(val => val.BodyPartDrawings[DrawingType.Legs]).OrderBy(_ => Rand.Next()).ToList();
            foreach (User user in this.Lobby.GetAllUsers())
            {
                Gameplay_Person temp = new Gameplay_Person
                {
                    Owner = user,
                };
                temp.BodyPartDrawings.Add(DrawingType.Head, heads.First());
                temp.BodyPartDrawings.Add(DrawingType.Body, bodies.First());
                temp.BodyPartDrawings.Add(DrawingType.Legs, legs.First());
                heads.RemoveAt(0);
                bodies.RemoveAt(0);
                legs.RemoveAt(0);

                temp.DoneWithRound = false;
                temp.Name = user.DisplayName;
                RoundTracker.AssignedPeople.Add(user, temp);
            }
            if (heads.Count != this.Lobby.GetAllUsers().Count)
            {
                throw new Exception("Something Went Wrong While Setting Up Game");
            }
            while (heads.Any())
            {
                Gameplay_Person temp = new Gameplay_Person
                {
                    Owner = null,
                };
                temp.BodyPartDrawings.Add(DrawingType.Head, heads.First());
                temp.BodyPartDrawings.Add(DrawingType.Body, bodies.First());
                temp.BodyPartDrawings.Add(DrawingType.Legs, legs.First());
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
                    user.Score += WinningScoresByPlace[CurrentFinishingPosition];
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
            Guid headId = RoundTracker.AssignedPeople[user].BodyPartDrawings[DrawingType.Head].Id;
            Guid bodyId = RoundTracker.AssignedPeople[user].BodyPartDrawings[DrawingType.Body].Id;
            Guid legsId = RoundTracker.AssignedPeople[user].BodyPartDrawings[DrawingType.Legs].Id;
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
