using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels;
using RoystonGame.TV.GameModes.Common.ThreePartPeople;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using RoystonGame.WordLists;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels.Setup_Person;
using static RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels.Person;
using static System.FormattableString;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.GameModes.BriansGames.BodyBuilder.GameStates
{
    public class Setup_GS : GameState
    {

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
            formSubmitListener: (User user, UserFormSubmission input) =>
            {
                string person1 = input.SubForms[0].ShortAnswer;
                string person2 = input.SubForms[1].ShortAnswer;
                if (person1.FuzzyEquals(person2))
                {
                    return (false, "Please enter 2 distinct people");
                }
                if(PeopleList.Any(val => val.Name.FuzzyEquals(person1)))
                {
                    return (false, "Somebody beat you the punch on your first prompt");
                }
                if (PeopleList.Any(val => val.Name.FuzzyEquals(person2)))
                {
                    return (false, "Somebody beat you the punch on your second prompt");
                }
                this.PeopleList.Add(new Setup_Person
                {
                    Owner = user,
                    Name = input.SubForms[0].ShortAnswer
                });
                this.PeopleList.Add(new Setup_Person
                {
                    Owner = user,
                    Name = input.SubForms[1].ShortAnswer
                });
                return (true, String.Empty);
            });
        }


        private List<Setup_Person> PeopleList { get; set; }

        private Random Rand { get; set; } = new Random();

        /// <summary>
        /// Returns a chain of user states which will prompt for the proper drawings, assumes this.SubChallenges is fully set up.
        /// </summary>
        /// <param name="user">The user to build a chain for.</param>
        /// <param name="outlet">The state to link the end of the chain to.</param>
        /// <returns>A list of user states designed for a given user.</returns>
        private List<UserState> GetDrawingsUserStateChain(User user, Connector outlet)
        {
            List<UserState> stateChain = new List<UserState>();
            List<Setup_Person> people = this.PeopleList.OrderBy(_ => Rand.Next()).ToList();
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
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            Prompt = Invariant($"You are drawing the \"{person.UserSubmittedDrawingsByUser[user].Type}\" of \"{person.Name}\""),
                            Drawing = new DrawingPromptMetadata()
                            {
                                WidthInPx = ThreePartPeopleConstants.Widths[person.UserSubmittedDrawingsByUser[user].Type],
                                HeightInPx = ThreePartPeopleConstants.Heights[person.UserSubmittedDrawingsByUser[user].Type],
                                CanvasBackground = ThreePartPeopleConstants.Backgrounds[person.UserSubmittedDrawingsByUser[user].Type],
                            },
                        },
                    },
                    SubmitButton = true
                },
                formSubmitListener: (User user, UserFormSubmission input) =>
                {
                    person.UserSubmittedDrawingsByUser[user].Drawing = input.SubForms[0].Drawing;
                    return (true, String.Empty);
                }));
            }

            for (int i = 1; i < stateChain.Count; i++)
            {
                stateChain[i - 1].Transition(stateChain[i]);
            }
            stateChain.Last().SetOutlet(outlet);

            return stateChain;
        }

        public Setup_GS(Lobby lobby, List<Setup_Person> peopleList, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions, Connector outlet = null) : base(lobby, outlet)
        {
            this.PeopleList = peopleList;

            State waitForAllDrawings = new WaitForAllPlayers(lobby: lobby, outlet: this.Outlet);
            State waitForAllPrompts = new WaitForAllPlayers(lobby: lobby, outlet:(User user, UserStateResult result, UserFormSubmission input) =>
            {
                // This call doesn't actually happen until after all prompts are submitted
                GetDrawingsUserStateChain(user, waitForAllDrawings.Inlet)[0].Inlet(user, result, input);
            });
            // Just before users call the line above, call AssignPrompts
            waitForAllPrompts.AddStateEndingListener(() => this.AssignPrompts());

            this.Entrance = GetPeoplePrompts_State(waitForAllPrompts.Inlet);

            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Complete all the prompts on your devices." },
            };
        }

        /// <summary>
        /// This method of assigning prompts is not the most efficient but the next best algorithm I could find for getting a 
        /// good solution was even more inefficient. All the efficient algorithms gave suboptimal solutions.
        /// </summary>
        private void AssignPrompts()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Setup_Person> randomlyOrderedPeople = this.PeopleList.OrderBy(_ => Rand.Next()).ToList();
            IReadOnlyList<User> users = this.Lobby.GetActiveUsers();
            for (int i = 0; i < randomlyOrderedPeople.Count; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    PeopleUserDrawing temp = new PeopleUserDrawing
                    {
                        Type = (Setup_Person.DrawingType)j,
                        Owner = users[(i + j + 1) % users.Count],
                        Id = randomlyOrderedPeople[i].Id
                    };

                    randomlyOrderedPeople[i].UserSubmittedDrawingsByUser.Add(
                        temp.Owner, temp);
                    randomlyOrderedPeople[i].BodyPartDrawings.Add(
                        temp.Type, temp);
                }
            }

            Debug.WriteLine(Invariant($"Assigned user prompts in ({stopwatch.ElapsedMilliseconds} ms)"), "Timing");
        }
    }
}
