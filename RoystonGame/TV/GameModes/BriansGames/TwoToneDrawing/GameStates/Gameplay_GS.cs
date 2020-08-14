﻿using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;
using RoystonGame.TV.ControlFlows.Exit;
using static RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.DataModels.ChallengeTracker;
using System.Collections.Immutable;

namespace RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.GameStates
{
    public class Gameplay_GS : GameState
    {
        private Random Rand { get; } = new Random();
        private static Func<User, UserPrompt> PickADrawing(ChallengeTracker challenge, List<string> choices) => (User user) =>
        {
            List<string> detailedChoices = choices.Select(val => (challenge.UserSubmittedDrawings.ContainsKey(user) && challenge.UserSubmittedDrawings[user].TeamId == val) ? Invariant($"{val} - You helped draw this") : val).ToList();
            detailedChoices.Add("They all suck. Mix them up!");
            string description;
            if (challenge.Owner == user)
            {
                description = Invariant($"This was your prompt!\nPrompt: '{challenge.Prompt}'.");
            }
            else
            {
                description = Invariant($"'{challenge.Owner.DisplayName}' came up with this prompt.\nPrompt: '{challenge.Prompt}'");
            }

            // TODO: Rank several drawings rather than pick one.
            return new UserPrompt
            {
                Title = "Vote for the best drawing!",
                Description = description,
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = $"Which drawing is best",
                        Answers = detailedChoices.ToArray()
                    }
                },
                SubmitButton = true,
            };
        };

        private ChallengeTracker SubChallenge { get; set; }
        private int NumVotesToSwap { get; set; } = 0;
        public Gameplay_GS(Lobby lobby, ChallengeTracker challengeTracker) : base(lobby)
        {
            SubChallenge = challengeTracker;
            this.Entrance.Transition(RandomizeAndMakeVote());      
        }

        private UserState RandomizeAndMakeVote()
        {
            NumVotesToSwap = 0;
            SubChallenge.TeamIdToUsersWhoVotedMapping.Clear();
            foreach (var kvp in SubChallenge.UserSubmittedDrawings)
            {
                SubChallenge.TeamIdToDrawingMapping.AddOrUpdate(kvp.Value.TeamId, _ => new ConcurrentDictionary<string, TeamUserDrawing>(new Dictionary<string, TeamUserDrawing> { { kvp.Value.Color, kvp.Value} }),
                    (key, currentDictionary) =>
                    {
                        currentDictionary[kvp.Value.Color] = kvp.Value;
                        return currentDictionary;
                    });
                SubChallenge.TeamIdToUsersWhoVotedMapping.GetOrAdd(kvp.Value.TeamId, _ => new ConcurrentBag<User>());
            }
            int count = 0;
            
            for(int i =0; i<500 && count<133; i++)
            {
                var teamIds = SubChallenge.TeamIdToDrawingMapping.Keys.ToList();
                string teamId1 = teamIds[Rand.Next(teamIds.Count)];
                string teamId2 = teamIds[Rand.Next(teamIds.Count)];
                if (teamId1 == teamId2)
                {
                    continue;
                }
                var colors = SubChallenge.TeamIdToDrawingMapping[teamId1].Keys.ToList();
                string color = colors[Rand.Next(colors.Count)];
                TeamUserDrawing userDrawing1 = SubChallenge.TeamIdToDrawingMapping[teamId1][color];
                TeamUserDrawing userDrawing2 = SubChallenge.TeamIdToDrawingMapping[teamId2][color];
                SubChallenge.TeamIdToDrawingMapping[teamId1][color] = userDrawing2;
                SubChallenge.TeamIdToDrawingMapping[teamId2][color] = userDrawing1;
                userDrawing1.TeamId = teamId2;
                userDrawing2.TeamId = teamId1;
                count++;
            }
            List<string> choices = SubChallenge.TeamIdToDrawingMapping.Keys.OrderBy(val => val).ToList();
            SimplePromptUserState pickDrawing = new SimplePromptUserState(
                promptGenerator: PickADrawing(SubChallenge, choices),
                formSubmitHandler: (User user, UserFormSubmission submission) =>
                {
                    if (submission.SubForms[0].RadioAnswer == SubChallenge.TeamIdToDrawingMapping.Keys.Count)
                    {
                        NumVotesToSwap++;
                        return (true, string.Empty);
                    }
                    else
                    {
                        SubChallenge.TeamIdToUsersWhoVotedMapping[choices[submission.SubForms[0].RadioAnswer??-1]].Add(user);
                    }
                    return (true, string.Empty);
                },
                exit: new WaitForUsers_StateExit(lobby: this.Lobby));  
            pickDrawing.AddExitListener(HandleStateEnding);

            void HandleStateEnding()
            {
                int mostVotes = this.SubChallenge.TeamIdToUsersWhoVotedMapping.Max((kvp) => kvp.Value.Count);
                if (NumVotesToSwap > mostVotes)
                {
                    pickDrawing.Transition(this.RandomizeAndMakeVote());
                    return;
                }
                foreach ((string id, ConcurrentBag<User> users) in this.SubChallenge.TeamIdToUsersWhoVotedMapping)
                {
                    foreach (User user in users)
                    {
                        if (users.Count == mostVotes)
                        {
                            // If the user voted for the drawing with the most votes, give them 100 points
                            user.AddScore( 100);
                        }
                        else if (this.SubChallenge.UserSubmittedDrawings.ContainsKey(user) && this.SubChallenge.UserSubmittedDrawings[user].TeamId == id)
                        {
                            // If the drawing didn't get the most votes and the user voted for themselves subtract points
                            user.AddScore(-1000);
                        }
                    }

                    if (users.Count == mostVotes)
                    {
                        foreach (User user in this.SubChallenge.UserSubmittedDrawings.Where(kvp => kvp.Value.TeamId == id).Select(kvp => kvp.Key))
                        {
                            // 500 points if they helped draw the best drawing.
                            user.AddScore( 500);
                        }
                    }
                }
                pickDrawing.Transition(this.Exit);
                return;
            }

            var unityImages = new List<UnityImage>();
            foreach ((string id, ConcurrentDictionary<string, TeamUserDrawing> colorMap) in this.SubChallenge.TeamIdToDrawingMapping.OrderBy(val=>val.Key))
            {
                unityImages.Add(new UnityImage
                {
                    Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = this.SubChallenge.Colors.Select(color => colorMap[color].Drawing).ToList() },
                    ImageIdentifier = new StaticAccessor<string> { Value = id.ToString() },
                    BackgroundColor = new StaticAccessor<IReadOnlyList<int>> { Value = new List<int> { 255, 255, 255 } }
                });
            }
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = unityImages },
                Title = new StaticAccessor<string> { Value = "Behold!" },
            };
            return pickDrawing;
        }
    }
}