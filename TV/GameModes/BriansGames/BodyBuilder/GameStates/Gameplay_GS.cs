﻿using RoystonGame.TV.ControlFlows;
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
        private Random rand { get; } = new Random();
        private Func<User, UserPrompt> PickADrawing()
        {
            return (User user) => {
                Gameplay_Person PlayerHand = AssignedPeople[user];
                Gameplay_Person PlayerTrade = UnassignedPeople[UsersToSeatNumber[user]];
                if(PlayerHand.doneWithRound)
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
                                    HtmlImageWrapper(PlayerTrade.BodyPartDrawings[DrawingType.Head].Drawing,BodyBuilderConstants.heights[DrawingType.Head]),
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

        private List<Gameplay_Person> UnassignedPeople { get; set; } = new List<Gameplay_Person>();
        Dictionary<User, Gameplay_Person> AssignedPeople { get; set; } = new Dictionary<User, Gameplay_Person>();
        private Dictionary<User, int> UsersToSeatNumber { get; set; } = new Dictionary<User, int>();
        private List<User> OrderedUsers { get; set; } = new List<User>();
        private int winCount { get; set; } = 0;
        private int numWon { get; set; } = 0;
        Dictionary<int, int> WinningScoresByPlace { get; set; } = new Dictionary<int, int>()
        {
            {0, 1000},
            {1, 500},
            {2, 250}
        };
        public Gameplay_GS(Lobby lobby, List<Setup_Person> setup_PeopleList, bool displayPool, bool displayNames, Action<User, UserStateResult, UserFormSubmission> outlet = null) : base(lobby, outlet)
        {
            this.AssignPeople(setup_PeopleList);
            this.AssignSeats();
            
            this.Entrance = AddGameplayCycle();

            var unityImages = new List<UnityImage>();
            string instructions = null;
            string title = null;
            if (displayNames)
            {
                instructions = String.Join("         ", setup_PeopleList.Select((Setup_Person person) => person.Prompt));
            }
            if(displayPool)
            {
                title = "Here's What's in the pool";
                for (int i = 0; i < this.UnassignedPeople.Count; i++)
                {
                    unityImages.Add(new UnityImage
                    {
                        Base64Pngs = new DynamicAccessor<IReadOnlyList<string>> { DynamicBacker = () => this.UnassignedPeople[i].GetOrderedDrawings() },
                        BackgroundColor = new StaticAccessor<IReadOnlyList<int>> { Value = new List<int> { 255, 255, 255 } },
                        SpriteGridWidth = new StaticAccessor<int?> { Value = 1 },
                        SpriteGridHeight = new StaticAccessor<int?> { Value = 3 },
                        Header = new StaticAccessor<string> { Value = OrderedUsers[i].DisplayName }
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
                    Gameplay_Person PlayerHand = AssignedPeople[user];
                    Gameplay_Person PlayerTrade = UnassignedPeople[UsersToSeatNumber[user]];
                    DrawingType? answer = (DrawingType?)submission.SubForms[1].RadioAnswer;
                    if(AssignedPeople[user].doneWithRound)
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
            List<UserDrawing> heads;
            List<UserDrawing> bodies;
            List<UserDrawing> legs;
            heads = setup_People.Select(val => val.UserSubmittedDrawingsByDrawingType[DrawingType.Head]).OrderBy(_ => rand.Next()).ToList();
            bodies = setup_People.Select(val => val.UserSubmittedDrawingsByDrawingType[DrawingType.Body]).OrderBy(_ => rand.Next()).ToList();
            legs = setup_People.Select(val => val.UserSubmittedDrawingsByDrawingType[DrawingType.Legs]).OrderBy(_ => rand.Next()).ToList();
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

                temp.doneWithRound = false;
                AssignedPeople.Add(user, temp);
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

                temp.doneWithRound = false;
                UnassignedPeople.Add(temp);
            }

        }

        private bool PlayerWon()
        {
            bool someoneFinished = false;
            foreach(User user in this.Lobby.GetActiveUsers())
            {
                Guid headId = AssignedPeople[user].BodyPartDrawings[DrawingType.Head].Id;
                Guid bodyId = AssignedPeople[user].BodyPartDrawings[DrawingType.Body].Id;
                Guid legsId = AssignedPeople[user].BodyPartDrawings[DrawingType.Legs].Id;
                if (headId == bodyId && bodyId == legsId && !AssignedPeople[user].doneWithRound)
                {
                    AssignedPeople[user].doneWithRound = true;
                    user.Score += WinningScoresByPlace[winCount];
                    someoneFinished = true;
                    numWon++;
                }
            }
            if(someoneFinished)
            {
                winCount++;
            }
            if(winCount>=WinningScoresByPlace.Count() || numWon>=this.Lobby.GetActiveUsers().Count)
            {
                //winCount = 0;
                //numWon = 0;
                return true;
            }
            return false;
        }

        private void RotateSeats()
        {
            
            Gameplay_Person first = UnassignedPeople[0];
            UnassignedPeople.RemoveAt(0);
            UnassignedPeople.Add(first);
        }
        private void AssignSeats()
        {
            int count = 0;
            foreach (User user in this.Lobby.GetActiveUsers().OrderBy(_ => rand.Next()).ToList())
            {
                OrderedUsers.Add(user);
                UsersToSeatNumber.Add(user, count);
                count++;
            }
        }
    }
}
