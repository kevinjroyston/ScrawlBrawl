using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.KylesGames.QuestionQuest.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using RoystonGame.WordLists;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using static System.FormattableString;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.GameModes.KylesGames.QuestionQuest.GameStates
{
    public class Setup_GS : GameState
    {
        private const int NumWords = 4;
        private UserState ChallengeCreatorState(Connector outlet = null) =>
            new SimplePromptUserState(
                prompt: (User user) => {
                    ChallengeTracker challenge;
                    if (this.SubChallenges.ContainsKey(user))
                    {
                        challenge = this.SubChallenges[user];
                    }
                    else
                    {
                        challenge = new ChallengeTracker
                        {
                            DrawingOwner = user,
                            AchillesAnswer = rand.Next(0, NumWords - 1),
                        };
                        challenge.Answers = RandomLineFromFile.GetRandomLines(FileNames.Nouns, NumWords);
                        if (!this.SubChallenges.TryAdd(user, challenge))
                        {
                            throw new Exception("Something went wrong");
                        }
                    }

                    List<string> copyOfAnswers = challenge.Answers.ToList();
                    copyOfAnswers[challenge.AchillesAnswer] = Invariant($"{copyOfAnswers[challenge.AchillesAnswer]} - Your achilles");

                    UserPrompt prompt = new UserPrompt()
                    {
                        Title = "Game setup",
                        Description = "Your word:",
                        RefreshTimeInMs = 1000,
                        SubPrompts = new SubPrompt[]
                        {
                            new SubPrompt
                            {
                                StringList = copyOfAnswers.ToArray(),
                                Prompt = Invariant($"Draw something to distract your foes"),
                                Drawing = new DrawingPromptMetadata(),
                            },
                        },
                        SubmitButton = true
                    };
                    return prompt;
                },
                outlet: outlet,
                formSubmitListener: (User user, UserFormSubmission input) =>
                {
                    this.SubChallenges[user].Drawing = input.SubForms[0].Drawing;
                    return (true, string.Empty);
                });

        private ConcurrentDictionary<User, ChallengeTracker> SubChallenges { get; set; }

        private Random rand { get; set; } = new Random();

        public Setup_GS(Lobby lobby, ConcurrentDictionary<User, ChallengeTracker> challengeTrackers, Connector outlet = null) : base(lobby, outlet)
        {
            this.SubChallenges = challengeTrackers;

            UserStateTransition waitForAllDrawings = new WaitForAllPlayers(this.Lobby, null, this.Outlet, null);
            this.Entrance = ChallengeCreatorState(waitForAllDrawings.Inlet);

            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Complete all the prompts on your devices." },
            };
        }
    }
}
