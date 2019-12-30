using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameEngine;
using RoystonGame.TV.GameEngine.Rendering;
using RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.WordLists;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels.Setup_Person;
using static System.FormattableString;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.GameModes.BriansGames.BodyBuilder.GameStates
{
    public class Setup_GS : GameState
    {/*
        private int ColorsPerDrawing { get; set; }
        private int DrawingsPerPlayer { get; set; }
        private UserState GetPartyLeaderChooseNumberOfDrawingsState()
        {
            return new SimplePromptUserState((User user) => new UserPrompt()
            {
                Title = "Game Options",
                Description = Invariant($"Configure options, there are {GameManager.GetActiveUsers().Count} players total."),
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = "Number of colors per drawing",
                        ShortAnswer = true
                    },
                    new SubPrompt
                    {
                        Prompt = "Number of drawings per player",
                        ShortAnswer = true
                    }
                },
                SubmitButton = true
            },
            formSubmitCallback: (User user, UserFormSubmission userInput) =>
            {
                ColorsPerDrawing = Convert.ToInt32(userInput.SubForms[0].ShortAnswer);
                DrawingsPerPlayer = Convert.ToInt32(userInput.SubForms[1].ShortAnswer);
            });
        }*/

        private UserState GetPeoplePrompts_State(Connector outlet = null)
        {
            return new SimplePromptUserState((User user) => new UserPrompt()
            {
                Title = "Game setup",
                Description = "In the boxes below, enter drawing prompts which will be given to different players.",
                RefreshTimeInMs = 1000,
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt="Person#1",
                        ShortAnswer=true
                    },
                    new SubPrompt
                    {
                        Prompt="Person#2",
                        ShortAnswer=true
                    },
                },
                SubmitButton = true
            },
            outlet,
            formSubmitCallback: (User user, UserFormSubmission input) =>
            {
                this.PeopleList.Add(new Setup_Person
                {
                    Owner = user,
                    Prompt = input.SubForms[0].ShortAnswer
                });
                this.PeopleList.Add(new Setup_Person
                {
                    Owner = user,
                    Prompt = input.SubForms[1].ShortAnswer
                });
            }) ;
        }

        private List<Setup_Person> PeopleList { get; set; }

        private Random rand { get; set; } = new Random();

        /// <summary>
        /// Returns a chain of user states which will prompt for the proper drawings, assumes this.SubChallenges is fully set up.
        /// </summary>
        /// <param name="user">The user to build a chain for.</param>
        /// <param name="outlet">The state to link the end of the chain to.</param>
        /// <returns>A list of user states designed for a given user.</returns>
        private List<UserState> GetDrawingsUserStateChain(User user, Connector outlet)
        {
            List<UserState> stateChain = new List<UserState>();
            List<Setup_Person> people = this.PeopleList.OrderBy(_ => rand.Next()).ToList();
            foreach (Setup_Person person in people)
            {
                if (!person.UserSubmittedDrawingsByUser.ContainsKey(user))
                {
                    continue;
                }

                stateChain.Add(new SimplePromptUserState((User user) => new UserPrompt()
                {
                    Title = "Time to draw!",
                    Description = "Draw the prompt below. Keep in mind you are only drawing part of the person!",
                    RefreshTimeInMs = 1000,
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            Prompt = Invariant($"You are drawing the \"{person.UserSubmittedDrawingsByUser[user].Type}\" of \"{person.Prompt}\""),
                            Drawing = true,
                        },
                    },
                    SubmitButton = true
                },
                formSubmitCallback: (User user, UserFormSubmission input) =>
                {
                    person.UserSubmittedDrawingsByUser[user].Drawing = input.SubForms[0].Drawing;
                }));
            }

            for (int i = 1; i < stateChain.Count; i++)
            {
                stateChain[i - 1].Transition(stateChain[i]);
            }
            stateChain.Last().SetOutlet(outlet);

            return stateChain;
        }

        public Setup_GS(List<Setup_Person> peopleList, Connector outlet = null) : base(outlet)
        {
            this.PeopleList = peopleList;

            UserStateTransition waitForAllDrawings = new WaitForAllPlayers(null, this.Outlet, null);
            UserStateTransition waitForAllPrompts = new WaitForAllPlayers(null, outlet: (User user, UserStateResult result, UserFormSubmission input) =>
            {
                // This call doesn't actually happen until after all prompts are submitted
                GetDrawingsUserStateChain(user, waitForAllDrawings.Inlet)[0].Inlet(user, result, input);
            });
            // Just before users call the line above, call AssignPrompts
            waitForAllPrompts.SetStateEndingCallback(() => this.AssignPrompts());

            this.Entrance = GetPeoplePrompts_State(waitForAllPrompts.Inlet);

            this.GameObjects = new List<GameObject>()
            {
                new TextObject { Content = "Complete all the prompts on your devices." }
            };
        }

        /// <summary>
        /// This method of assigning prompts is not the most efficient but the next best algorithm I could find for getting a 
        /// good solution was even more inefficient. All the efficient algorithms gave suboptimal solutions.
        /// </summary>
        private void AssignPrompts()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Setup_Person> randomlyOrderedPeople = this.PeopleList.OrderBy(_ => rand.Next()).ToList();
            IReadOnlyList<User> users = GameManager.GetActiveUsers();
            for (int i = 0; i < randomlyOrderedPeople.Count; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    UserDrawing temp = new UserDrawing
                    {
                        Type = (Setup_Person.DrawingType)j,
                        Owner = randomlyOrderedPeople[(i + j + 1) % randomlyOrderedPeople.Count].Owner,
                        Id = randomlyOrderedPeople[i].Id
                    };

                    randomlyOrderedPeople[i].UserSubmittedDrawingsByUser.Add(
                        temp.Owner,temp) ;
                    randomlyOrderedPeople[i].UserSubmittedDrawingsByDrawingType.Add(
                        temp.Type, temp);
                }
            }

            Debug.WriteLine(Invariant($"Assigned user prompts in ({stopwatch.ElapsedMilliseconds} ms)"), "Timing");
        }
    }
}
