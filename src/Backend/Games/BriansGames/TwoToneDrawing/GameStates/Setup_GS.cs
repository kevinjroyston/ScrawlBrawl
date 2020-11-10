using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.TwoToneDrawing.DataModels;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using Common.Code.WordLists;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using static System.FormattableString;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using System.Collections.Concurrent;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;
using Common.Code.Extensions;

namespace Backend.Games.BriansGames.TwoToneDrawing.GameStates
{
    public class Setup_GS : GameState
    {
        private int ColorsPerTeam { get; set; }
        private int TeamsPerPrompt { get; set; }
        private bool ShowColors { get; set; }
        private TimeSpan? PromptTimer { get; set; }
        private TimeSpan? DrawingTimer { get; set; }

        private UserState GetChallengesUserState()
        {
            UserState toReturn = new SimplePromptUserState(
                promptGenerator: (User user) =>
                {
                    List<SubPrompt> subPrompts = new List<SubPrompt>()
                    {
                        new SubPrompt
                        {
                            Prompt = Invariant($"The drawing prompt. Suggestions: '{string.Join("', '",RandomLineFromFile.GetRandomLines(FileNames.Nouns, 5))}'"),
                            ShortAnswer = true,
                        },
                    };
                    for (int i = 0; i < ColorsPerTeam; i++)
                    {
                        subPrompts.Add(new SubPrompt()
                        {
                            Prompt = Invariant($"Color # {i + 1}{(i == 0 ? " - This will be drawn above all other colors" : i == ColorsPerTeam - 1 ? " - This will be drawn below all other colors" : string.Empty)}"),
                            ColorPicker = true
                        });
                    }

                    return new UserPrompt()
                    {
                        UserPromptId = UserPromptId.ChaoticCooperation_Setup,
                        Title = "Game setup",
                        Description = "In the boxes below, enter a drawing prompt and the colors which will be given to different players.",
                        SubPrompts = subPrompts.ToArray(),
                        SubmitButton = true
                    };
                },
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {
                    List<string> colors = input.SubForms.Where((subForm, index) => index > 0).Select((subForm) => subForm.Color).Reverse().ToList();
                    if (colors.Count != new HashSet<string>(colors).Count)
                    {
                        return (true, "Server doesn't handle identical colors well, change one slightly.");
                    }

                    bool success = this.SubChallenges.TryAdd(new ChallengeTracker
                    {
                        Owner = user,
                        Prompt = input.SubForms[0].ShortAnswer,
                        Colors = colors
                    }, null);
                    return (success, success ? string.Empty : "Server error, try again");
                },
                exit: new WaitForUsers_StateExit(this.Lobby),
                maxPromptDuration: this.PromptTimer);
            return toReturn;
        }

        private ConcurrentDictionary<ChallengeTracker, object> SubChallenges { get; set; }

        private Random Rand { get; set; } = new Random();

        /// <summary>
        /// Returns a chain of user states which will prompt for the proper drawings, assumes this.SubChallenges is fully set up.
        /// </summary>
        /// <param name="user">The user to build a chain for.</param>
        /// <returns>A list of user states designed for a given user.</returns>
        private List<State> GetDrawingsUserStateChain(User user)
        {
            List<State> stateChain = new List<State>();
            List<ChallengeTracker> challenges = this.SubChallenges.Keys.OrderBy(_ => Rand.Next()).ToList();
            int index = 0;
            foreach (ChallengeTracker challenge in challenges)
            {
                if (!challenge.UserSubmittedDrawings.ContainsKey(user))
                {
                    continue;
                }

                var lambdaSafeIndex = index;
                stateChain.Add(new SimplePromptUserState(
                    promptGenerator: (User user) => new UserPrompt()
                    {
                        UserPromptId = UserPromptId.ChaoticCooperation_Draw,
                        Title = Invariant($"Drawing { lambdaSafeIndex + 1} of {stateChain.Count()}"),
                        Description = "Draw the prompt below. Keep in mind you are only drawing part of the picture!",
                        SubPrompts = new SubPrompt[]
                        {
                            new SubPrompt
                            {
                                Prompt = Invariant($"Your prompt:\"{challenge.Prompt}\""),
                                StringList = this.ShowColors ? challenge.Colors.Select(val=> val == challenge.UserSubmittedDrawings[user].Color ? Invariant($"<div class=\"color-box\" style=\"background-color: {val};\"></div>This is your color.") : Invariant($"<div class=\"color-box\" style=\"background-color: {val};\"></div>")).Reverse().ToArray() : null,
                                Drawing = new DrawingPromptMetadata
                                {
                                    ColorList = new List<string> { challenge.UserSubmittedDrawings[user].Color }
                                }
                            },
                        },
                        SubmitButton = true
                    },
                    formSubmitHandler: (User user, UserFormSubmission input) =>
                    {
                        challenge.UserSubmittedDrawings[user].Drawing = input.SubForms[0].Drawing;
                        challenge.UserSubmittedDrawings[user].Owner = user;
                        return (true, string.Empty);
                    }));
                index++;
            }

            return stateChain;
        }

        public Setup_GS(Lobby lobby, ConcurrentDictionary<ChallengeTracker, object> challengeTrackers, int numColorsPerTeam, int numTeamsPerPrompt, bool showColors, TimeSpan? setupTimer = null, TimeSpan? drawingTimer = null) : base(lobby)
        {
            this.SubChallenges = challengeTrackers;

            this.ColorsPerTeam = numColorsPerTeam;
            this.TeamsPerPrompt = numTeamsPerPrompt;
            this.PromptTimer = setupTimer;
            this.DrawingTimer = drawingTimer;

            // Cap the values at 2 teams using maximal colors (attempts to use all players).
            this.ColorsPerTeam = Math.Min(this.ColorsPerTeam, this.Lobby.GetAllUsers().Count() / 2);
            this.TeamsPerPrompt = Math.Min(this.TeamsPerPrompt, this.Lobby.GetAllUsers().Count() / this.ColorsPerTeam);

            this.ShowColors = showColors;

            TimeSpan? multipliedDrawingTimer = drawingTimer.MultipliedBy(challengeTrackers.Count);


            State getChallenges = GetChallengesUserState();
            MultiStateChain getDrawings = new MultiStateChain(GetDrawingsUserStateChain, exit: new WaitForUsers_StateExit(this.Lobby), stateDuration: drawingTimer);

            this.Entrance.Transition(getChallenges);
            getChallenges.AddExitListener(() => this.AssignPrompts());
            getChallenges.Transition(getDrawings);
            getDrawings.Transition(this.Exit);

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
        private void AssignPrompts()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            IReadOnlyList<User> users = this.Lobby.GetAllUsers();
            List<ChallengeTracker> randomizedOrderChallenges = this.SubChallenges.Keys.OrderBy(_ => Rand.Next()).ToList();

            for (int i = 0; i < randomizedOrderChallenges.Count; i++)
            {
                for (int j = 0; j < ColorsPerTeam * TeamsPerPrompt; j++)
                {
                    randomizedOrderChallenges[i].UserSubmittedDrawings.Add(
                        users[(i + j) % users.Count],
                        new ChallengeTracker.TeamUserDrawing
                        {
                            TeamId = Invariant($"{(j / ColorsPerTeam)+1}"),
                            Color = randomizedOrderChallenges[i].Colors[j % ColorsPerTeam],
                            Owner = users[(i+j)% users.Count]
                        });
                }
            }

            // Make 100 attempts at valid swaps
            for (int i = 0; i < 100; i++)
            {
                int rand1 = Rand.Next(0, randomizedOrderChallenges.Count);
                int rand1b = Rand.Next(0, randomizedOrderChallenges[0].UserSubmittedDrawings.Count);

                //int rand2a = rand.Next(0, randomizedOrderChallenges.Count);
                int rand2b = Rand.Next(0, randomizedOrderChallenges[0].UserSubmittedDrawings.Count);

                User user1 = randomizedOrderChallenges[rand1].UserSubmittedDrawings.Keys.ToList()[rand1b];
                User user2 = randomizedOrderChallenges[rand1].UserSubmittedDrawings.Keys.ToList()[rand2b];

                // Unforunately does not swap remainder players. Oh well.
                if (rand1b != rand2b)
                {
                    ChallengeTracker.TeamUserDrawing user1Drawing = randomizedOrderChallenges[rand1].UserSubmittedDrawings[user1];
                    ChallengeTracker.TeamUserDrawing user2Drawing = randomizedOrderChallenges[rand1].UserSubmittedDrawings[user2];
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
