﻿using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.BodyBuilder.DataModels;
using Backend.Games.Common.ThreePartPeople;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Backend.Games.Common.ThreePartPeople.DataModels.Person;
using static System.FormattableString;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;
using Common.Code.Extensions;
using Backend.Games.Common.ThreePartPeople.Extensions;

namespace Backend.Games.BriansGames.BodyBuilder.GameStates
{
    public class Setup_GS : GameState
    {

        private UserState GetPeoplePrompts_State(TimeSpan? setupTimer)
        {
            return new SimplePromptUserState((User user) => new UserPrompt()
            {
                UserPromptId = UserPromptId.BodyBuilder_CreatePrompts,
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
            formSubmitHandler: (User user, UserFormSubmission input) =>
            {
                string person1 = input.SubForms[0].ShortAnswer;
                string person2 = input.SubForms[1].ShortAnswer;
                if (person1.FuzzyEquals(person2))
                {
                    return (false, "Please enter 2 distinct people");
                }
                if (PeopleList.Any(val => val.Name.FuzzyEquals(person1)))
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
            },
            userTimeoutHandler: (User user, UserFormSubmission input) =>
            {
                // TODO: set up default people list to pull from.
                throw new Exception("this game doesn't support timers for setup");
            },
            exit: new WaitForUsers_StateExit(this.Lobby)//,            maxPromptDuration: setupTimer
            );
        }


        private List<Setup_Person> PeopleList { get; set; }

        private Random Rand { get; set; } = new Random();

        /// <summary>
        /// Returns a chain of user states which will prompt for the proper drawings, assumes this.SubChallenges is fully set up.
        /// </summary>
        /// <param name="user">The user to build a chain for.</param>
        /// <param name="outlet">The state to link the end of the chain to.</param>
        /// <returns>A list of user states designed for a given user.</returns>
        private List<State> GetDrawingsUserStateChain(User user)
        {
            List<State> stateChain = new List<State>();
            List<Setup_Person> people = this.PeopleList.OrderBy(_ => Rand.Next()).ToList();
            foreach (Setup_Person person in people)
            {
                if (!person.UserSubmittedDrawingsByUser.ContainsKey(user))
                {
                    continue;
                }

                stateChain.Add(new SimplePromptUserState((User user) => new UserPrompt()
                {
                    UserPromptId = UserPromptId.BodyBuilder_DrawBodyPart,
                    Title = "Time to draw!",
                    Description = "Draw the prompt below. Keep in mind you are only drawing part of the person!",
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            Prompt = Invariant($"You are drawing the \"{person.UserSubmittedDrawingsByUser[user].Type}\" of \"{person.Name}\""),
                            Drawing = new DrawingPromptMetadata()
                            {
                                DrawingType = person.UserSubmittedDrawingsByUser[user].Type.GetDrawingType(),
                            },
                        },
                    },
                    SubmitButton = true
                },
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {
                    person.UserSubmittedDrawingsByUser[user].Drawing = input.SubForms[0].Drawing;
                    return (true, String.Empty);
                }));
            }

            return stateChain;
        }

        public Setup_GS(Lobby lobby, List<Setup_Person> peopleList, TimeSpan? setupTimeDuration = null, TimeSpan? drawingTimeDuration = null) : base(lobby)
        {
            this.PeopleList = peopleList;

            AddExitListener(() =>
            {
                if (this.PeopleList.Count != 2 * this.Lobby.GetAllUsers().Count)
                {
                    throw new Exception("Not enough prompts submitted in order to play game.");
                }
            });

            State getPeoplePrompts = GetPeoplePrompts_State(setupTimeDuration);
            this.Entrance.Transition(getPeoplePrompts);
            getPeoplePrompts.Transition(() => 
            {
                this.AssignPrompts();
                MultiStateChain drawingsStateChains = new MultiStateChain(
                    GetDrawingsUserStateChain,
                    exit: new WaitForUsers_StateExit(this.Lobby),
                    stateDuration: drawingTimeDuration);
                drawingsStateChains.Transition(this.Exit);
                return drawingsStateChains;
            });

            this.Legacy_UnityView = new Legacy_UnityView(this.Lobby)
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
            IReadOnlyList<User> users = this.Lobby.GetAllUsers();
            for (int i = 0; i < randomlyOrderedPeople.Count; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    PeopleUserDrawing temp = new PeopleUserDrawing
                    {
                        Type = (Setup_Person.BodyPartType)j,
                        Owner = users[(i + j + 1) % users.Count],
                        Id = randomlyOrderedPeople[i].Id
                    };

                    randomlyOrderedPeople[i].UserSubmittedDrawingsByUser.Add(
                        temp.Owner, temp);
                    randomlyOrderedPeople[i].BodyPartDrawings[temp.Type]= temp;
                }
            }

            Debug.WriteLine(Invariant($"Assigned user prompts in ({stopwatch.ElapsedMilliseconds} ms)"), "Timing");
        }
    }
}
