using Microsoft.Identity.Client;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.StateGroups;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.DataModels;
using RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.GameStates;
using RoystonGame.TV.GameModes.Common.DataModels;
using RoystonGame.TV.GameModes.Common.GameStates;
using RoystonGame.TV.GameModes.Common.GameStates.VoteAndReveal;
using RoystonGame.Web.DataModels.Exceptions;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.DataModels.ChallengeTracker;
using static System.FormattableString;

namespace RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing
{
    public class TwoToneDrawingGameMode : IGameMode
    {
        private List<ChallengeTracker> SubChallenges { get; set; } = new List<ChallengeTracker>();
        private Lobby Lobby {get; set;}
        private GameState Setup { get; set; }
        private List<GameState> Gameplay { get; set; } = new List<GameState>();
        private List<GameState> Scoreboards { get; set; } = new List<GameState>();
        private List<GameState> VoteReveals { get; set; } = new List<GameState>();
        private Random Rand { get; } = new Random();
        public TwoToneDrawingGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            ValidateOptions(lobby, gameModeOptions);
            this.Lobby = lobby;
            //ToDo Refactor to move to vote reveal after the votes are done and scrambled 
            Setup = new Setup_GS(lobby, this.SubChallenges, gameModeOptions);

            StateChain GamePlayLoopGenerator()
            {
                List<ChallengeTracker> challenges = SubChallenges.OrderBy(_ => Rand.Next()).ToList();
                StateChain chain = new StateChain(
                stateGenerator: (int counter) =>
                {
                    if (counter < challenges.Count)
                    {
                        return new StateChain(states: new List<State>
                        {
                            GetVotingAndRevealState(challenges[counter], null),
                            (counter == challenges.Count - 1) ? new ScoreBoardGameState(lobby, "Final Scores") : new ScoreBoardGameState(lobby),
                        });
                    }
                    else
                    {
                        return null;
                    }
                });
                chain.Transition(this.Exit);
                return chain;
            }

            Setup.Transition(GamePlayLoopGenerator);
            this.Entrance.Transition(Setup);
        }

        private State GetVotingAndRevealState(ChallengeTracker challenge, TimeSpan? votingTime)
        {
            AssignUsersToChallenge(challenge);
            List<string> randomizedTeamIds = challenge.TeamIdToDrawingMapping.Keys.OrderBy(_=> Rand.Next()).ToList();
            List<List<UserDrawing>> stackedDrawings = randomizedTeamIds.Select(teamId => challenge.TeamIdToDrawingMapping[teamId].Values.Select(drawing => (UserDrawing)drawing).ToList()).ToList();


            return new StackedDrawingVoteAndRevealState(
                lobby: this.Lobby,
                stackedDrawings: stackedDrawings,
                promptAnswerAddOnGenerator: (User user, int answer) =>
                {
                    if (challenge.TeamIdToDrawingMapping[randomizedTeamIds[answer]].Values.Any(drawing => drawing.Owner == user))
                    {
                        return " - You helped make this";
                    }
                    else
                    {
                        return "";
                    }    
                },
                voteCountManager: (Dictionary<User, int> usersToVotes) =>
                {
                    CountVotes(usersToVotes, challenge, randomizedTeamIds);
                },
                votingTime: votingTime)
                {
                    VotingTitle = Invariant($"Which one is the best \"{challenge.Prompt}\"?")
                };
        }
        private void CountVotes(Dictionary<User, int> usersToVotes, ChallengeTracker challenge, List<string> randomizedTeamIds)
        {
            List<List<User>> usersVotedForEachAnswer = new List<List<User>>();
            int mostVotes = 0;
            for (int i = 0; i < randomizedTeamIds.Count; i++)
            {
                usersVotedForEachAnswer.Add(new List<User>());
            }
            foreach (User user in usersToVotes.Keys)
            {
                int indexVotedFor = usersToVotes[user];
                usersVotedForEachAnswer[indexVotedFor].Add(user);
                if (usersVotedForEachAnswer[indexVotedFor].Count > mostVotes)
                {
                    mostVotes = usersVotedForEachAnswer[indexVotedFor].Count;
                }
            }
            for (int i = 0; i < usersVotedForEachAnswer.Count; i++)
            {
                List<User> users = usersVotedForEachAnswer[i];
                foreach (User user in users)
                {
                    if (users.Count == mostVotes)
                    {
                        // If the user voted for the drawing with the most votes, give them 100 points
                        user.AddScore(TwoToneDrawingConstants.PointsForVotingForWinningDrawing);
                    }
                    else if (challenge.TeamIdToDrawingMapping[randomizedTeamIds[i]].Values.Any(drawing => drawing.Owner == user))
                    {
                        // If the drawing didn't get the most votes and the user voted for themselves subtract points
                        user.AddScore(TwoToneDrawingConstants.PointsToLoseForBadSelfVote);
                    }
                }

                foreach (User user in usersToVotes.Keys)
                {
                    if (users.Count == mostVotes)
                    {
                        foreach (User drawingUser in challenge.TeamIdToDrawingMapping[randomizedTeamIds[i]].Values.Select(drawing => drawing.Owner))
                        {
                            // 500 points if they helped draw the best drawing.
                            user.AddScore(TwoToneDrawingConstants.PointsForMakingWinningDrawing);
                        }
                    }
                }
            }
            
        }
        private void AssignUsersToChallenge(ChallengeTracker challenge)
        {
            foreach (var kvp in challenge.UserSubmittedDrawings)
            {
                challenge.TeamIdToDrawingMapping.AddOrUpdate(kvp.Value.TeamId, _ => new ConcurrentDictionary<string, TeamUserDrawing>(new Dictionary<string, TeamUserDrawing> { { kvp.Value.Color, kvp.Value } }),
                    (key, currentDictionary) =>
                    {
                        currentDictionary[kvp.Value.Color] = kvp.Value;
                        return currentDictionary;
                    });
                challenge.TeamIdToUsersWhoVotedMapping.GetOrAdd(kvp.Value.TeamId, _ => new ConcurrentBag<User>());
            }
            int count = 0;

            for (int i = 0; i < 500 && count < 133; i++)
            {
                var teamIds = challenge.TeamIdToDrawingMapping.Keys.ToList();
                string teamId1 = teamIds[Rand.Next(teamIds.Count)];
                string teamId2 = teamIds[Rand.Next(teamIds.Count)];
                if (teamId1 == teamId2)
                {
                    continue;
                }
                var colors = challenge.TeamIdToDrawingMapping[teamId1].Keys.ToList();
                string color = colors[Rand.Next(colors.Count)];
                TeamUserDrawing userDrawing1 = challenge.TeamIdToDrawingMapping[teamId1][color];
                TeamUserDrawing userDrawing2 = challenge.TeamIdToDrawingMapping[teamId2][color];
                challenge.TeamIdToDrawingMapping[teamId1][color] = userDrawing2;
                challenge.TeamIdToDrawingMapping[teamId2][color] = userDrawing1;
                userDrawing1.TeamId = teamId2;
                userDrawing2.TeamId = teamId1;
                count++;
            }
        }
        public void ValidateOptions(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            // None
        }
    }
}
