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
using System.Linq;
using System.Threading.Tasks;

using static System.FormattableString;

namespace RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO.GameStates
{
    public class Setup_GS: GameState
    {
        private UserState GetWordsUserState(Action<User, UserStateResult, UserFormSubmission> outlet = null)
        {
            return new SimplePromptUserState(new UserPrompt()
            {
                Title = "Game setup",
                Description = "In the boxes below, enter two drawing prompts such that only you will be able to tell the drawings apart.",
                RefreshTimeInMs = 1000,
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = Invariant($"The drawing prompt to show all users. Suggestions: '{string.Join("', '",RandomLineFromFile.GetRandomLines(FileNames.Nouns, 3))}'"),
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
        private List<UserState> GetDrawingsUserStateChain(User user, Action<User, UserStateResult, UserFormSubmission> outlet)
        {
            List<UserState> stateChain = new List<UserState>();
            List<ChallengeTracker> challenges = this.SubChallenges.OrderBy(_ => rand.Next()).ToList();
            foreach(ChallengeTracker challenge in challenges)
            {
                if (challenge.Owner == user)
                {
                    continue;
                }

                stateChain.Add(new SimplePromptUserState(new UserPrompt()
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
                    challenge.UserSubmittedDrawings.Add(user, input.SubForms[0].Drawing);
                }));
            }

            for (int i = 1; i < stateChain.Count; i++)
            {
                stateChain[i - 1].Transition(stateChain[i]);
            }
            stateChain.Last().SetOutlet(outlet);

            return stateChain;
        }

        public Setup_GS(List<ChallengeTracker> challengeTrackers, Action<User, UserStateResult, UserFormSubmission> outlet = null) : base(outlet)
        {
            this.SubChallenges = challengeTrackers;
            bool promptsAssigned = false;
            WaitForTrigger waitForTrigger = new WaitForTrigger();
            waitForTrigger.Transition(GetWordsUserState());
            UserStateTransition waitForAllDrawings = new WaitForAllPlayers(null, this.Outlet, null);
            UserStateTransition waitForAllPrompts = new WaitForAllPlayers(null, outlet: (User user, UserStateResult result, UserFormSubmission input)=>
            {
                if (!promptsAssigned)
                {
                    promptsAssigned = true;
                    this.AssignPrompts();
                }
                GetDrawingsUserStateChain(user, waitForAllDrawings.Inlet)[0].Inlet(user, result, input);
            });

            foreach (User user in GameManager.GetActiveUsers())
            {
                waitForTrigger.SetOutlet(GetWordsUserState(waitForAllPrompts.Inlet).Inlet, new List<User> { user });
            }

            this.Entrance = waitForTrigger;
            waitForTrigger.Trigger();

            this.GameObjects = new List<GameObject>()
            {
                new TextObject { Content = "Complete all the prompts on your devices." }
            };
        }

        private void AssignPrompts()
        {
            List<ChallengeTracker> randomizedOrderChallenges = this.SubChallenges.OrderBy(_ => rand.Next()).ToList();
            for(int i = 1; i<randomizedOrderChallenges.Count; i++)
            {
                randomizedOrderChallenges[i - 1].OddOneOut = randomizedOrderChallenges[i].Owner;
            }
            randomizedOrderChallenges.Last().OddOneOut = randomizedOrderChallenges[0].Owner;
        }
    }
}
