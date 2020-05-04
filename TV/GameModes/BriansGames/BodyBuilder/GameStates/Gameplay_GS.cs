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
                        },
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
        Dictionary<User, int> UsersToSeatNumber { get; set; } = new Dictionary<User, int>();
        public Gameplay_GS(Lobby lobby, List<Setup_Person> setup_PeopleList, Action<User, UserStateResult, UserFormSubmission> outlet = null) : base(lobby, outlet)
        {
            this.AssignPeople(setup_PeopleList);
            this.AssignSeats();
            
            this.Entrance = AddGameplayCycle();
            // TODO: Add unity view
            /*
            var unityImages = new List<UnityImage>();
            foreach ((string id, ConcurrentDictionary<string, string> colorMap) in this.SubChallenge.TeamIdToDrawingMapping)
            {
                unityImages.Add(new UnityImage
                {
                    Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = this.SubChallenge.Colors.Select(color => colorMap[color]).ToList() },
                    ImageIdentifier = new StaticAccessor<string> { Value = id.ToString() },
                    BackgroundColor = new StaticAccessor<IReadOnlyList<int>> { Value = new List<int> { 255, 255, 255 } }
                });
            }
            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages },
                Title = new StaticAccessor<string> { Value = "Behold!" },
            };*/
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

        private int winCount = 0;
        private int numWon = 0;
        private bool PlayerWon()
        {          
            foreach(User user in this.Lobby.GetActiveUsers())
            {
                Guid headId = AssignedPeople[user].BodyPartDrawings[DrawingType.Head].Id;
                Guid bodyId = AssignedPeople[user].BodyPartDrawings[DrawingType.Body].Id;
                Guid legsId = AssignedPeople[user].BodyPartDrawings[DrawingType.Legs].Id;
                if (headId == bodyId && bodyId == legsId)
                {
                    if(winCount==0)
                    {
                        user.Score += 1000;
                        numWon++;
                    }
                    else if(winCount==1)
                    {
                        user.Score += 500;
                        numWon++;
                    }
                    else if(winCount==2)
                    {
                        user.Score += 250;
                        numWon++;
                    }
                    winCount++;
                }
            }
            if(winCount>=3 || numWon>=this.Lobby.GetActiveUsers().Count)
            {
                winCount = 0;
                numWon = 0;
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
                UsersToSeatNumber.Add(user, count);
                count++;
            }
        }
    }
}
