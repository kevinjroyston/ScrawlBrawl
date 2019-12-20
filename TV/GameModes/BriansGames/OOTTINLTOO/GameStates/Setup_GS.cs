using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameEngine;
using RoystonGame.TV.GameEngine.Rendering;
using RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO.DataModels;
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

namespace RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO.GameStates
{
    public class Setup_GS : GameState
    {
        private UserState GetPartyLeaderChooseNumberOfDrawingsState()
        {
            return new SimplePromptUserState((User user)=> new UserPrompt()
            {
                Title = "Game Options",
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = "Max drawing prompts per player",
                        ShortAnswer = true
                    }
                },
                SubmitButton = true
            });
        }

        private UserState GetWordsUserState(Connector outlet = null)
        {
            return new SimplePromptUserState((User user) => new UserPrompt()
            {
                Title = "Game setup",
                Description = "In the boxes below, enter two drawing prompts such that only you will be able to tell the drawings apart.",
                RefreshTimeInMs = 1000,
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
            outlet,
            formSubmitCallback: (User user, UserFormSubmission input) =>
            {
                this.SubChallenges.Add(new ChallengeTracker
                {
                    Owner = user,
                    RealPrompt = input.SubForms[0].ShortAnswer,
                    DeceptionPrompt = input.SubForms[1].ShortAnswer,
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
            foreach(ChallengeTracker challenge in challenges)
            {
                if (challenge.Owner == user || !challenge.UserSubmittedDrawings.ContainsKey(user))
                {
                    continue;
                }

                stateChain.Add(new SimplePromptUserState((User user)=> new UserPrompt()
                {
                    Title = "Time to draw!",
                    Description = "Draw the prompt below. Careful, if you aren't the odd one out and people think you are, you will lose points for being a terrible artist.",
                    RefreshTimeInMs = 1000,
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            Prompt = Invariant($"Your prompt:\"{(challenge.OddOneOut == user ? challenge.DeceptionPrompt : challenge.RealPrompt)}\""),
                            Drawing = true,
                        },
                    },
                    SubmitButton = true
                },
                formSubmitCallback: (User user, UserFormSubmission input) =>
                {
                    challenge.UserSubmittedDrawings[user] = input.SubForms[0].Drawing;
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
            int numDrawingsPerPrompt = 6;

            WaitForPartyLeader setNumPrompts = new WaitForPartyLeader(
                null,
                partyLeaderPrompt: GetPartyLeaderChooseNumberOfDrawingsState(),
                partyLeaderSubmission: (User user, UserStateResult result, UserFormSubmission input) =>
                {
                    numDrawingsPerPrompt = Int32.Parse(input.SubForms[0].ShortAnswer);
                });

            UserStateTransition waitForAllDrawings = new WaitForAllPlayers(null, this.Outlet, null);
            UserStateTransition waitForAllPrompts = new WaitForAllPlayers(null, outlet: (User user, UserStateResult result, UserFormSubmission input) =>
            {
                GetDrawingsUserStateChain(user, waitForAllDrawings.Inlet)[0].Inlet(user, result, input);
            });
            waitForAllPrompts.SetStateEndingCallback(() => this.AssignPrompts(numDrawingsPerPrompt));
            setNumPrompts.SetOutlet(GetWordsUserState(waitForAllPrompts.Inlet).Inlet);

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
        /// <param name="maxDrawingsPerPrompt">The max drawings to allow per prompt (will only be less than this value if there arent enough users).</param>
        private void AssignPrompts(int maxDrawingsPerPrompt = 6)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<ChallengeTracker> randomizedOrderChallenges = this.SubChallenges.OrderBy(_ => rand.Next()).ToList();
            IReadOnlyList<User> users = GameManager.GetActiveUsers();
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
                        &&(randomizedOrderChallenges[rand2a].Owner != user1 && !randomizedOrderChallenges[rand2a].UserSubmittedDrawings.ContainsKey(user1)))
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
