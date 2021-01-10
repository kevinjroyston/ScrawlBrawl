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
using Common.Code.Helpers;
using Common.DataModels.Interfaces;

namespace Backend.Games.BriansGames.TwoToneDrawing.GameStates
{
    public class Setup_GS : GameState
    {
        private bool UseSingleColor { get; set; }
        private int LayersPerTeam { get; set; }
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
                    if (UseSingleColor)
                    {
                        for (int i = 0; i < LayersPerTeam; i++)
                        {
                            subPrompts.Add(new SubPrompt()
                            {
                                Prompt = Invariant($"Color # {i + 1}{(i == 0 ? " - This will be drawn above all other colors" : i == LayersPerTeam - 1 ? " - This will be drawn below all other colors" : string.Empty)}"),
                                ColorPicker = true
                            });
                        }
                    }
                    else
                    {
                        subPrompts.Add(new SubPrompt()
                        {
                            Prompt = "Foreground Layer Description - (This will be drawn ABOVE all other layers)",
                            ShortAnswer = true
                        });
                        for (int i = 1; i < LayersPerTeam-1; i++)
                        {
                            subPrompts.Add(new SubPrompt()
                            {
                                Prompt = LayersPerTeam == 3?"Middle Layer Description":Invariant($"Middle Layer # {i} Description"),
                                ShortAnswer = true
                            });
                        }
                        subPrompts.Add(new SubPrompt()
                        {
                            Prompt = "Background Layer Description - (This will be drawn BELOW all other layers)",
                            ShortAnswer = true
                        });

                    }

                    return new UserPrompt()
                    {
                        UserPromptId = UserPromptId.ChaoticCooperation_Setup,
                        Title = "Game setup",
                        Description = "In the boxes below, enter a drawing prompt and "+(UseSingleColor?"the colors": "a description of each layer") + " that will be given to different players.",
                        SubPrompts = subPrompts.ToArray(),
                        SubmitButton = true
                    };
                },
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {
                    List<string> colors = null;
                    if (this.UseSingleColor)
                    {
                        colors = input.SubForms.Where((subForm, index) => index > 0).Select((subForm) => subForm.Color).Reverse().ToList();
                        if (colors.Count != new HashSet<string>(colors).Count)
                        {
                            return (false, "Server doesn't handle identical colors well, change one slightly.");
                        }
                    } else
                    {
                        colors = input.SubForms.Where((subForm, index) => index > 0).Select((subForm) => subForm.ShortAnswer).Reverse().ToList();
                    }

                    bool success = this.SubChallenges.TryAdd(new ChallengeTracker
                    {
                        Owner = user,
                        Prompt = input.SubForms[0].ShortAnswer,
                        Colors = colors,
                        MaxMemberCount = colors.Count * TeamsPerPrompt,
                    }, null);
                    return (success, success ? string.Empty : "Server error, try again");
                },
                userTimeoutHandler: (User user, UserFormSubmission input) =>
                {
                    return GameInfrastructure.DataModels.Enums.UserTimeoutAction.None;
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
                                StringList = (this.UseSingleColor && this.ShowColors) ? challenge.Colors.Select(val=> val == challenge.UserSubmittedDrawings[user].Color ? Invariant($"<div class=\"color-box\" style=\"background-color: {val};\"></div>This is your color.") : Invariant($"<div class=\"color-box\" style=\"background-color: {val};\"></div>")).Reverse().ToArray() 
                                                : (!this.UseSingleColor) ? challenge.Colors.Select(val=> val == challenge.UserSubmittedDrawings[user].Color ? val + "<-- YOU ARE DRAWING THIS" : val).Reverse().ToArray()
                                                    : null,
                                Drawing = new DrawingPromptMetadata
                                {
                                    ColorList = (this.UseSingleColor)? new List<string> { challenge.UserSubmittedDrawings[user].Color }:null,
                                    GalleryOptions = null
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

        public Setup_GS(Lobby lobby, ConcurrentDictionary<ChallengeTracker, object> challengeTrackers, bool useSingleColor, int numLayersPerTeam, int numTeamsPerPrompt, bool showColors, TimeSpan? setupTimer = null, TimeSpan? drawingTimer = null) : base(lobby)
        {
            this.SubChallenges = challengeTrackers;
            this.UseSingleColor = useSingleColor;
            this.LayersPerTeam = numLayersPerTeam;
            this.TeamsPerPrompt = numTeamsPerPrompt;
            this.PromptTimer = setupTimer;
            this.DrawingTimer = drawingTimer;

            // Cap the values at 2 teams using maximal colors (attempts to use all players).
            this.LayersPerTeam = Math.Min(this.LayersPerTeam, this.Lobby.GetAllUsers().Count() / 2);
            this.TeamsPerPrompt = Math.Min(this.TeamsPerPrompt, this.Lobby.GetAllUsers().Count() / this.LayersPerTeam);

            this.ShowColors = showColors;

            State getChallenges = GetChallengesUserState();

            this.Entrance.Transition(getChallenges);
            getChallenges.AddExitListener(() => this.AssignPrompts());
            getChallenges.Transition(() =>
            {
                var getDrawings = new MultiStateChain(GetDrawingsUserStateChain, exit: new WaitForUsers_StateExit(this.Lobby), stateDuration: drawingTimer.MultipliedBy(challengeTrackers.Count));
                getDrawings.Transition(this.Exit);
                return getDrawings;
            });

            this.Legacy_UnityView = new Legacy_UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Complete all the prompts on your devices." },
            };
        }

        private void AssignPrompts()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            IReadOnlyList<User> users = this.Lobby.GetAllUsers();
            List<ChallengeTracker> randomizedOrderChallenges = this.SubChallenges.Keys.OrderBy(_ => Rand.Next()).ToList();

            if (randomizedOrderChallenges.Count == 0)
            {
                throw new Exception("Can't play the game if there are no prompts");
            }

            List<IGroup<User>> groups = MemberHelpers<User>.Assign(
                this.SubChallenges.Keys.Cast<IConstraints<User>>().ToList(),
                users,
                this.LayersPerTeam * this.TeamsPerPrompt);

            var assignments = groups.Zip(this.SubChallenges.Keys);

            foreach ((IGroup<User> groupedUsers, ChallengeTracker tracker) in assignments)
            {
                int badCodingPractice = 0;
                tracker.UserSubmittedDrawings = groupedUsers.Members.ToDictionary((user)=> user, (user)=>
                    new ChallengeTracker.TeamUserDrawing
                    {
                        TeamId = Invariant($"{(badCodingPractice / LayersPerTeam)+1}"),
                        Color = tracker.Colors[badCodingPractice++ % LayersPerTeam],
                        Owner = user
                    });
            }
            Debug.WriteLine(Invariant($"Assigned user prompts in ({stopwatch.ElapsedMilliseconds} ms)"), "Timing");
        }
    }
}
