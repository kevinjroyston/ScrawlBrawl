using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels.Setup_Person;
using static System.FormattableString;

namespace RoystonGame.TV.GameModes.BriansGames.BodyBuilder.GameStates
{
    public class Gameplay_GS : GameState
    {
        private Random Rand { get; } = new Random();
        private RoundTracker roundTracker;
        private Func<User, UserPrompt> PickADrawing()
        {
            return (User user) => {
                Gameplay_Person PlayerHand = roundTracker.AssignedPeople[user];
                Gameplay_Person PlayerTrade = roundTracker.UnassignedPeople[roundTracker.UsersToSeatNumber[user]];
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
                                    HtmlImageWrapper(PlayerHand.BodyPartDrawings[DrawingType.Head].Drawing,BodyBuilderConstants.widths[DrawingType.Head],BodyBuilderConstants.heights[DrawingType.Head]),
                                    HtmlImageWrapper(PlayerHand.BodyPartDrawings[DrawingType.Body].Drawing,BodyBuilderConstants.widths[DrawingType.Body],BodyBuilderConstants.heights[DrawingType.Body]),
                                    HtmlImageWrapper(PlayerHand.BodyPartDrawings[DrawingType.Legs].Drawing,BodyBuilderConstants.widths[DrawingType.Legs],BodyBuilderConstants.heights[DrawingType.Legs])
                                },
                            },
                            new SubPrompt
                            {
                                Prompt = "Please Just select None and hit submit, this will be fixed soon",
                                Answers = new string[]
                                {
                                    "None"
                                },
                            }
                        },
                        SubmitButton = true,
                    };
                }
                else
                {
                    return new UserPrompt
                    {
                        Title = "This is your current person",
                        SubPrompts = new SubPrompt[]
                        {
                            new SubPrompt
                            {
                                StringList = new string[]
                                {
                                    HtmlImageWrapper(PlayerHand.BodyPartDrawings[DrawingType.Head].Drawing,BodyBuilderConstants.widths[DrawingType.Head],BodyBuilderConstants.heights[DrawingType.Head]),
                                    HtmlImageWrapper(PlayerHand.BodyPartDrawings[DrawingType.Body].Drawing,BodyBuilderConstants.widths[DrawingType.Body],BodyBuilderConstants.heights[DrawingType.Body]),
                                    HtmlImageWrapper(PlayerHand.BodyPartDrawings[DrawingType.Legs].Drawing,BodyBuilderConstants.widths[DrawingType.Legs],BodyBuilderConstants.heights[DrawingType.Legs])
                                },
                            },
                            new SubPrompt
                            {
                                Prompt = "Which body part do you want to trade?",
                                Answers = new string[]
                                {
                                    HtmlImageWrapper(PlayerTrade.BodyPartDrawings[DrawingType.Head].Drawing,BodyBuilderConstants.widths[DrawingType.Head],BodyBuilderConstants.heights[DrawingType.Head]),
                                    HtmlImageWrapper(PlayerTrade.BodyPartDrawings[DrawingType.Body].Drawing,BodyBuilderConstants.widths[DrawingType.Body],BodyBuilderConstants.heights[DrawingType.Body]),
                                    HtmlImageWrapper(PlayerTrade.BodyPartDrawings[DrawingType.Legs].Drawing,BodyBuilderConstants.widths[DrawingType.Legs],BodyBuilderConstants.heights[DrawingType.Legs]),
                                    "None"
                                },
                            }
                        },
                        SubmitButton = true,
                    };
                }           
            };
        }

        private string HtmlImageWrapper(string image, int width = 240, int height = 240)
        {
            return Invariant($"<img width=\"{width}\" height=\"{height}\" src=\"{image}\"/>");
        }
       
        private int CurrentFinishingPosition { get; set; } = 0;
        private int NumPlayersDoneWithRound { get; set; } = 0;
        Dictionary<int, int> WinningScoresByPlace { get; set; } = new Dictionary<int, int>()
        {
            {0, 1000},   // First Place
            {1, 500},    // Second Place
            {2, 250}     // Third Place
        };
        public Gameplay_GS(Lobby lobby, List<Setup_Person> setup_PeopleList, RoundTracker roundTracker, bool displayPool, bool displayNames, Action<User, UserStateResult, UserFormSubmission> outlet = null) : base(lobby, outlet)
        {
            this.AssignPeople(setup_PeopleList);
            this.AssignSeats();
            this.roundTracker = roundTracker;
            this.Entrance = AddGameplayCycle();

            var unityImages = new List<UnityImage>();
            string instructions = null;
            string title = null;

            if (displayNames)
            {
                instructions = String.Join("         ", setup_PeopleList.Select((Setup_Person person) => person.Prompt));
            }

            if (displayPool)
            {
                title = "Here's What's in the pool";
                for (int i = 0; i < roundTracker.UnassignedPeople.Count; i++)
                {
                    unityImages.Add(new UnityImage
                    {
                        Base64Pngs = new DynamicAccessor<IReadOnlyList<string>> { DynamicBacker = () => roundTracker.UnassignedPeople[i].GetOrderedDrawings() },
                        BackgroundColor = new StaticAccessor<IReadOnlyList<int>> { Value = new List<int> { 255, 255, 255 } },
                        SpriteGridWidth = new StaticAccessor<int?> { Value = 1 },
                        SpriteGridHeight = new StaticAccessor<int?> { Value = 3 },
                        Header = new StaticAccessor<string> { Value = roundTracker.OrderedUsers[i].DisplayName }
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

        private UserState AddGameplayCycle()
        {
            /* ask users what changes they want to make
             * perform said changes
             * see if anyone won
             */
            RotateSeats();
            UserStateTransition waitForUsers = new WaitForAllPlayers(lobby: this.Lobby);
            SimplePromptUserState pickDrawing = new SimplePromptUserState(
                prompt: PickADrawing(),
                outlet: waitForUsers.Inlet,
                formSubmitListener: (User user, UserFormSubmission submission) =>
                {
                    Gameplay_Person PlayerHand = roundTracker.AssignedPeople[user];
                    Gameplay_Person PlayerTrade = roundTracker.UnassignedPeople[roundTracker.UsersToSeatNumber[user]];
                    DrawingType? answer = (DrawingType?)submission.SubForms[1].RadioAnswer;
                    if (roundTracker.AssignedPeople[user].DoneWithRound)
                    {
                        return (true, string.Empty);
                    }
                    if (answer == null)
                    {
                        return (false, "Please enter a valid option");
                    }
                    if (answer != DrawingType.None)
                    {
                        UserDrawing temp = PlayerHand.BodyPartDrawings[answer.Value];
                        PlayerHand.BodyPartDrawings[answer.Value] = PlayerTrade.BodyPartDrawings[answer.Value];
                        PlayerTrade.BodyPartDrawings[answer.Value] = temp;
                    }
                    return (true, string.Empty);
                });
            waitForUsers.AddStateEndingListener(()=>
            {
                if (this.PlayerWon())
                {
                    waitForUsers.SetOutlet(this.Outlet);
                }
                else
                {
                    waitForUsers.Transition(AddGameplayCycle());
                }

            });
            return pickDrawing;
        }
        private void AssignPeople(List<Setup_Person> setup_People)
        {
            roundTracker.ResetRoundVariables();
            List<UserDrawing> heads;
            List<UserDrawing> bodies;
            List<UserDrawing> legs;
            heads = setup_People.Select(val => val.UserSubmittedDrawingsByDrawingType[DrawingType.Head]).OrderBy(_ => Rand.Next()).ToList();
            bodies = setup_People.Select(val => val.UserSubmittedDrawingsByDrawingType[DrawingType.Body]).OrderBy(_ => Rand.Next()).ToList();
            legs = setup_People.Select(val => val.UserSubmittedDrawingsByDrawingType[DrawingType.Legs]).OrderBy(_ => Rand.Next()).ToList();
            foreach (User user in this.Lobby.GetActiveUsers())
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
                roundTracker.AssignedPeople.Add(user, temp);
            }
            if (heads.Count != this.Lobby.GetActiveUsers().Count)
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
                roundTracker.UnassignedPeople.Add(temp);
            }

        }

        private bool PlayerWon()
        {
            bool someoneFinished = false;
            foreach(User user in this.Lobby.GetActiveUsers())
            {
                Guid headId = roundTracker.AssignedPeople[user].BodyPartDrawings[DrawingType.Head].Id;
                Guid bodyId = roundTracker.AssignedPeople[user].BodyPartDrawings[DrawingType.Body].Id;
                Guid legsId = roundTracker.AssignedPeople[user].BodyPartDrawings[DrawingType.Legs].Id;
                if (headId == bodyId && bodyId == legsId && !roundTracker.AssignedPeople[user].DoneWithRound)
                {
                    roundTracker.AssignedPeople[user].DoneWithRound = true;
                    user.Score += WinningScoresByPlace[CurrentFinishingPosition];
                    someoneFinished = true;
                    NumPlayersDoneWithRound++;
                }
            }
            if (someoneFinished)
            {
                CurrentFinishingPosition++;
            }
            if (CurrentFinishingPosition >= WinningScoresByPlace.Count() || NumPlayersDoneWithRound >= this.Lobby.GetActiveUsers().Count)
            {
                return true;
            }
            return false;
        }

        private void RotateSeats()
        {
            
            Gameplay_Person first = roundTracker.UnassignedPeople[0];
            roundTracker.UnassignedPeople.RemoveAt(0);
            roundTracker.UnassignedPeople.Add(first);
        }
        private void AssignSeats()
        {
            int count = 0;
            foreach (User user in this.Lobby.GetActiveUsers().OrderBy(_ => Rand.Next()).ToList())
            {
                roundTracker.OrderedUsers.Add(user);
                roundTracker.UsersToSeatNumber.Add(user, count);
                count++;
            }
        }
    }
}
