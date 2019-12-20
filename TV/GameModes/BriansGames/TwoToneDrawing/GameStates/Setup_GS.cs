﻿using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameEngine;
using RoystonGame.TV.GameEngine.Rendering;
using RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.DataModels;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.WordLists;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using static System.FormattableString;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.GameStates
{
    public class Setup_GS : GameState
    {
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
        }

        private UserState GetChallengesUserState(Connector outlet = null)
        {
            return new SimplePromptUserState((User user) =>
            {
                List<SubPrompt> subPrompts = new List<SubPrompt>()
                {
                    new SubPrompt
                    {
                        Prompt = Invariant($"The drawing prompt. Suggestions: '{string.Join("', '",RandomLineFromFile.GetRandomLines(FileNames.Nouns, 5))}'"),
                        ShortAnswer = true,
                    },
                };
                for(int i = 0; i < ColorsPerDrawing; i++)
                {
                    subPrompts.Add(new SubPrompt()
                    {
                        Prompt = Invariant($"Color # {i + 1}"),
                        ColorPicker = true
                    });
                }

                return new UserPrompt()
                {
                    Title = "Game setup",
                    Description = "In the boxes below, enter a drawing prompt and the colors which will be given to different players.",
                    RefreshTimeInMs = 1000,
                    SubPrompts = subPrompts.ToArray(),
                    SubmitButton = true
                };
            },
            outlet,
            formSubmitCallback: (User user, UserFormSubmission input) =>
            {
                this.SubChallenges.Add(new ChallengeTracker
                {
                    Owner = user,
                    Prompt = input.SubForms[0].ShortAnswer,
                    Colors = input.SubForms.Where((subForm, index) => index > 0).Select((subForm) => subForm.Color).Reverse().ToList()
                });
            });
        }

        private List<ChallengeTracker> SubChallenges { get; set; }

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
            List<ChallengeTracker> challenges = this.SubChallenges.OrderBy(_ => rand.Next()).ToList();
            foreach (ChallengeTracker challenge in challenges)
            {
                if (!challenge.UserSubmittedDrawings.ContainsKey(user))
                {
                    continue;
                }

                stateChain.Add(new SimplePromptUserState((User user) => new UserPrompt()
                {
                    Title = "Time to draw!",
                    Description = "Draw the prompt below. Keep in mind you are only drawing part of the picture!",
                    RefreshTimeInMs = 1000,
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            Prompt = Invariant($"Your prompt:\"{challenge.Prompt}\""),
                            Color = challenge.UserSubmittedDrawings[user].Color,
                            Drawing = true,
                        },
                    },
                    SubmitButton = true
                },
                formSubmitCallback: (User user, UserFormSubmission input) =>
                {
                    challenge.UserSubmittedDrawings[user].Drawing = input.SubForms[0].Drawing;
                }));
            }

            for (int i = 1; i < stateChain.Count; i++)
            {
                stateChain[i - 1].Transition(stateChain[i]);
            }
            stateChain.Last().SetOutlet(outlet);

            return stateChain;
        }

        public Setup_GS(List<ChallengeTracker> challengeTrackers, Connector outlet = null) : base(outlet)
        {
            this.SubChallenges = challengeTrackers;

            WaitForPartyLeader setNumPrompts = new WaitForPartyLeader(
                null,
                partyLeaderPrompt: GetPartyLeaderChooseNumberOfDrawingsState());

            UserStateTransition waitForAllDrawings = new WaitForAllPlayers(null, this.Outlet, null);
            UserStateTransition waitForAllPrompts = new WaitForAllPlayers(null, outlet: (User user, UserStateResult result, UserFormSubmission input) =>
            {
                // This call doesn't actually happen until after all prompts are submitted
                GetDrawingsUserStateChain(user, waitForAllDrawings.Inlet)[0].Inlet(user, result, input);
            });
            // Just before users call the line above, call AssignPrompts
            waitForAllPrompts.SetStateEndingCallback(() => this.AssignPrompts());
            setNumPrompts.SetOutlet(GetChallengesUserState(waitForAllPrompts.Inlet).Inlet);

            this.Entrance = setNumPrompts;

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
            List<ChallengeTracker> randomizedOrderChallenges = this.SubChallenges.OrderBy(_ => rand.Next()).ToList();
            IReadOnlyList<User> users = GameManager.GetActiveUsers();
            for (int i = 0; i < randomizedOrderChallenges.Count; i++)
            {
                for (int j = 0; j < ((int)Math.Min(DrawingsPerPlayer, users.Count) / ColorsPerDrawing) * ColorsPerDrawing; j++)
                {
                    randomizedOrderChallenges[i].UserSubmittedDrawings.Add(
                        randomizedOrderChallenges[(i + j) % randomizedOrderChallenges.Count].Owner,
                        new ChallengeTracker.UserDrawing
                        {
                            TeamId = Invariant($"{j/ColorsPerDrawing}"),
                            Color = randomizedOrderChallenges[i].Colors[j % ColorsPerDrawing],
                        });
                }
            }

            // Make 100 attempts at valid swaps
            for (int i = 0; i < 100; i++)
            {
                int rand1 = rand.Next(0, randomizedOrderChallenges.Count);
                int rand1b = rand.Next(0, randomizedOrderChallenges[0].UserSubmittedDrawings.Count);

                //int rand2a = rand.Next(0, randomizedOrderChallenges.Count);
                int rand2b = rand.Next(0, randomizedOrderChallenges[0].UserSubmittedDrawings.Count);

                User user1 = randomizedOrderChallenges[rand1].UserSubmittedDrawings.Keys.ToList()[rand1b];
                User user2 = randomizedOrderChallenges[rand1].UserSubmittedDrawings.Keys.ToList()[rand2b];

                // Unforunately does not swap remainder players. Oh well.
                if (rand1b != rand2b)
                {
                    ChallengeTracker.UserDrawing user1Drawing = randomizedOrderChallenges[rand1].UserSubmittedDrawings[user1];
                    ChallengeTracker.UserDrawing user2Drawing = randomizedOrderChallenges[rand1].UserSubmittedDrawings[user2];
                    randomizedOrderChallenges[rand1].UserSubmittedDrawings.Remove(user1);
                    randomizedOrderChallenges[rand1].UserSubmittedDrawings.Remove(user2);
                    randomizedOrderChallenges[rand1].UserSubmittedDrawings.Add(user2, user1Drawing);
                    randomizedOrderChallenges[rand1].UserSubmittedDrawings.Add(user1, user2Drawing);
                }
            }

            Debug.WriteLine(Invariant($"Assigned user prompts in ({stopwatch.ElapsedMilliseconds} ms)"), "Timing");
        }
    }
}
