using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using RoystonGame.WordLists;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using static System.FormattableString;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO.GameStates
{
    public class Setup_GS : GameState
    {
        private UserState GetWordsUserState(Connector outlet = null)
        {
            return new SimplePromptUserState(
                prompt: (User user) => new UserPrompt()
                {
                    Title = "Game setup",
                    Description = "In the boxes below, enter two drawing prompts such that only you will be able to tell the drawings apart.",
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            Prompt = Invariant($"The drawing prompt to show all users. Suggestions: '{string.Join("', '",RandomLineFromFile.GetRandomLines(FileNames.Nouns, 5))}'"),
                            ShortAnswer = true,
                        },
                        new SubPrompt
                        {
                            Prompt = "The drawing prompt to show to imposter",
                            ShortAnswer = true,
                        }
                    },
                    SubmitButton = true
                },
                outlet: outlet,
                formSubmitListener: (User user, UserFormSubmission input) =>
                {
                    this.SubChallenges.Add(new ChallengeTracker
                    {
                        Owner = user,
                        RealPrompt = input.SubForms[0].ShortAnswer,
                        DeceptionPrompt = input.SubForms[1].ShortAnswer,
                    });
                    return (true, string.Empty);
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
            int index = 0;
            foreach (ChallengeTracker challenge in challenges)
            {
                if (challenge.Owner == user || !challenge.UserSubmittedDrawings.ContainsKey(user))
                {
                    continue;
                }

                var lambdaSafeIndex = index;
                stateChain.Add(new SimplePromptUserState(
                    prompt: (User user) => new UserPrompt()
                    {
                        Title = Invariant($"Drawing {lambdaSafeIndex + 1} of {stateChain.Count()}"),
                        Description = "Draw the prompt below. Careful, if you aren't the odd one out and people think you are, you will lose points for being a terrible artist.",
                        SubPrompts = new SubPrompt[]
                        {
                            new SubPrompt
                            {
                                Prompt = Invariant($"Your prompt:\"{(challenge.OddOneOut == user ? challenge.DeceptionPrompt : challenge.RealPrompt)}\""),
                                Drawing = new DrawingPromptMetadata(),
                            },
                        },
                        SubmitButton = true
                    },
                    formSubmitListener: (User user, UserFormSubmission input) =>
                    {
                        challenge.UserSubmittedDrawings[user] = input.SubForms[0].Drawing;
                        return (true, string.Empty);
                    }));
                index++;
            }

            for (int i = 1; i < stateChain.Count; i++)
            {
                stateChain[i - 1].Transition(stateChain[i]);
            }
            stateChain.Last().SetOutlet(outlet);

            return stateChain;
        }

        public Setup_GS(Lobby lobby, List<ChallengeTracker> challengeTrackers, int numDrawingsPerPrompt, Connector outlet = null) : base(lobby, outlet)
        {
            this.SubChallenges = challengeTrackers;
            GetWordsUserState();

            State waitForAllDrawings = new WaitForAllPlayers(this.Lobby, null, this.Outlet, null);
            State waitForAllPrompts = new WaitForAllPlayers(this.Lobby, null, outlet: (User user, UserStateResult result, UserFormSubmission input) =>
            {
                GetDrawingsUserStateChain(user, waitForAllDrawings.Inlet)[0].Inlet(user, result, input);
            });
            waitForAllPrompts.AddStateEndingListener(() => this.AssignPrompts(numDrawingsPerPrompt));

            this.Entrance = GetWordsUserState(waitForAllPrompts.Inlet);

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
        /// <param name="maxDrawingsPerPrompt">The max drawings to allow per prompt (will only be less than this value if there arent enough users).</param>
        private void AssignPrompts(int maxDrawingsPerPrompt = 6)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<ChallengeTracker> randomizedOrderChallenges = this.SubChallenges.OrderBy(_ => rand.Next()).ToList();
            IReadOnlyList<User> users = this.Lobby.GetActiveUsers();
            if (users.Count - 1 > maxDrawingsPerPrompt)
            {
                for (int i = 0; i < randomizedOrderChallenges.Count; i++)
                {
                    for (int j = 1; j <= maxDrawingsPerPrompt; j++)
                    {
                        randomizedOrderChallenges[i].UserSubmittedDrawings.Add(
                            randomizedOrderChallenges[(i + j) % randomizedOrderChallenges.Count].Owner,
                            string.Empty);
                    }
                }

                // Make 100 attempts at valid swaps
                for (int i = 0; i < 100; i++)
                {
                    int rand1a = rand.Next(0, randomizedOrderChallenges.Count);
                    int rand1b = rand.Next(0, randomizedOrderChallenges[0].UserSubmittedDrawings.Count);

                    int rand2a = rand.Next(0, randomizedOrderChallenges.Count);
                    int rand2b = rand.Next(0, randomizedOrderChallenges[0].UserSubmittedDrawings.Count);

                    User user1 = randomizedOrderChallenges[rand1a].UserSubmittedDrawings.Keys.ToList()[rand1b];
                    User user2 = randomizedOrderChallenges[rand2a].UserSubmittedDrawings.Keys.ToList()[rand2b];

                    // Determines if a swap is valid
                    if ((randomizedOrderChallenges[rand1a].Owner != user2 && !randomizedOrderChallenges[rand1a].UserSubmittedDrawings.ContainsKey(user2))
                        && (randomizedOrderChallenges[rand2a].Owner != user1 && !randomizedOrderChallenges[rand2a].UserSubmittedDrawings.ContainsKey(user1)))
                    {
                        randomizedOrderChallenges[rand1a].UserSubmittedDrawings.Remove(user1);
                        randomizedOrderChallenges[rand2a].UserSubmittedDrawings.Remove(user2);
                        randomizedOrderChallenges[rand1a].UserSubmittedDrawings.Add(user2, string.Empty);
                        randomizedOrderChallenges[rand2a].UserSubmittedDrawings.Add(user1, string.Empty);
                    }
                }
            }
            else
            {
                foreach (ChallengeTracker challenge in randomizedOrderChallenges)
                {
                    foreach (User user in users)
                    {
                        // Owner of a prompt shouldnt be prompted to draw it
                        if (user != challenge.Owner)
                        {
                            // Add it to the dictionary indicating this user should be prompted for a drawing.
                            challenge.UserSubmittedDrawings.Add(user, string.Empty);
                        }
                    }
                }
            }

            // Assign odd one outs randomly
            foreach (ChallengeTracker challenge in randomizedOrderChallenges)
            {
                List<User> usersGivenPrompt = challenge.UserSubmittedDrawings.Keys.ToList();
                challenge.OddOneOut = usersGivenPrompt[rand.Next(0, usersGivenPrompt.Count)];
            }

            Debug.WriteLine(Invariant($"Assigned user prompts in ({stopwatch.ElapsedMilliseconds} ms)"), "Timing");
        }
    }
}
