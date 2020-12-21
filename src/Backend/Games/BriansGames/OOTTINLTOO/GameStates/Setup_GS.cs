using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.OOTTINLTOO.DataModels;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using static System.FormattableString;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;
using Common.Code.WordLists;

namespace Backend.Games.BriansGames.OOTTINLTOO.GameStates
{
    public class Setup_GS : GameState
    {
        private UserState GetWordsUserState()
        {
            return new SimplePromptUserState(
                promptGenerator: (User user) => new UserPrompt()
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
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {
                    this.SubChallenges.Add(new ChallengeTracker
                    {
                        Owner = user,
                        RealPrompt = input.SubForms[0].ShortAnswer,
                        DeceptionPrompt = input.SubForms[1].ShortAnswer,
                    });
                    return (true, string.Empty);
                },
                exit: new WaitForUsers_StateExit(this.Lobby));
        }

        private List<ChallengeTracker> SubChallenges { get; set; }

        private Random Rand { get; set; } = new Random();

        /// <summary>
        /// Returns a chain of user states which will prompt for the proper drawings, assumes this.SubChallenges is fully set up.
        /// </summary>
        /// <param name="user">The user to build a chain for.</param>
        /// <returns>A list of user states designed for a given user.</returns>
        private List<State> GetDrawingsUserStateChain(User user)
        {
            List<State> stateChain = new List<State>();
            List<ChallengeTracker> challenges = this.SubChallenges.OrderBy(_ => Rand.Next()).ToList();
            int index = 0;
            foreach (ChallengeTracker challenge in challenges)
            {
                if (challenge.Owner == user || !challenge.UserSubmittedDrawings.ContainsKey(user))
                {
                    continue;
                }

                var lambdaSafeIndex = index;
                stateChain.Add(new SimplePromptUserState(
                    promptGenerator: (User user) => new UserPrompt()
                    {
                        UserPromptId = UserPromptId.ImposterSyndrome_Draw,
                        Title = Invariant($"Drawing {lambdaSafeIndex + 1} of {stateChain.Count()}"),
                        Description = "Draw the prompt below. Careful, if you aren't the odd one out and people think you are, you will lose points for being a terrible artist.",
                        SubPrompts = new SubPrompt[]
                        {
                            new SubPrompt
                            {
                                Prompt = Invariant($"Your prompt:\"{(challenge.OddOneOut == user ? challenge.DeceptionPrompt : challenge.RealPrompt)}\""),
                                Drawing = new DrawingPromptMetadata(){
                                    GalleryOptions = null
                                },
                            },
                        },
                        SubmitButton = true
                    },
                    formSubmitHandler: (User user, UserFormSubmission input) =>
                    {
                        challenge.UserSubmittedDrawings[user] = input.SubForms[0].Drawing;
                        return (true, string.Empty);
                    }));
                index++;
            }

            return stateChain;
        }

        public Setup_GS(Lobby lobby, List<ChallengeTracker> challengeTrackers, int numDrawingsPerPrompt) : base(lobby)
        {
            numDrawingsPerPrompt = Math.Min(numDrawingsPerPrompt, lobby.GetAllUsers().Count - 1);
            this.SubChallenges = challengeTrackers;

            State getWordsState = GetWordsUserState();
            MultiStateChain contestantsMultiStateChain = new MultiStateChain(GetDrawingsUserStateChain, exit: new WaitForUsers_StateExit(lobby));

            this.Entrance.Transition(getWordsState);
            getWordsState.AddExitListener(() => this.AssignPrompts(numDrawingsPerPrompt));
            getWordsState.Transition(contestantsMultiStateChain);
            contestantsMultiStateChain.Transition(this.Exit);

            this.UnityView = new UnityView(this.Lobby)
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
            List<ChallengeTracker> randomizedOrderChallenges = this.SubChallenges.OrderBy(_ => Rand.Next()).ToList();
            IReadOnlyList<User> users = this.Lobby.GetAllUsers();
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
                    int rand1a = Rand.Next(0, randomizedOrderChallenges.Count);
                    int rand1b = Rand.Next(0, randomizedOrderChallenges[0].UserSubmittedDrawings.Count);

                    int rand2a = Rand.Next(0, randomizedOrderChallenges.Count);
                    int rand2b = Rand.Next(0, randomizedOrderChallenges[0].UserSubmittedDrawings.Count);

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
                challenge.OddOneOut = usersGivenPrompt[Rand.Next(0, usersGivenPrompt.Count)];
            }

            Debug.WriteLine(Invariant($"Assigned user prompts in ({stopwatch.ElapsedMilliseconds} ms)"), "Timing");
        }
    }
}
