﻿using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.ImposterDrawing.DataModels;
using Backend.Games.BriansGames.ImposterDrawing.GameStates;
using Backend.Games.Common;
using Backend.Games.Common.DataModels;
using Backend.Games.Common.GameStates;
using Backend.Games.Common.GameStates.VoteAndReveal;
using Common.DataModels.Enums;
using Common.DataModels.Requests.LobbyManagement;
using Common.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;
using Constants = Common.DataModels.Constants;

namespace Backend.Games.BriansGames.ImposterDrawing
{
    public class ImposterDrawingGameMode : IGameMode
    {
        public static GameModeMetadata GameModeMetadata { get; } =
            new GameModeMetadata
            {
                Title = "Imposter Syndrome",
                GameId = GameModeId.Imposter,
                Description = "Come up with a difference only you'll be able to spot!",
                MinPlayers = 4,
                MaxPlayers = null,
                Attributes = new GameModeAttributes
                {
                       ProductionReady = true,
                },
                Options = new List<GameModeOptionResponse>
                {
                    new GameModeOptionResponse
                    {
                        Description = "Length of the game (10 for longest 1 for shortest 0 for no timer)",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 5,
                        MinValue = 0,
                        MaxValue = 10,
                    }
                },
            };
        private Setup_GS Setup { get; set; }
        private Random Rand { get; } = new Random();
        private Lobby Lobby { get; set; }
        public ImposterDrawingGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            this.Lobby = lobby;
            ValidateOptions(lobby, gameModeOptions);
            int gameLength = (int)gameModeOptions[(int)GameModeOptionsEnum.GameLength].ValueParsed;
            TimeSpan? setupTimer = null;
            TimeSpan? answeringTimer = null;
            TimeSpan? votingTimer = null;
            if (gameLength > 0)
            {
                setupTimer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: ImposterDrawingConstants.SetupTimerMin,
                    aveTimerLength: ImposterDrawingConstants.SetupTimerAve,
                    maxTimerLength: ImposterDrawingConstants.SetupTimerMax);
                answeringTimer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: ImposterDrawingConstants.AnsweringTimerMin,
                    aveTimerLength: ImposterDrawingConstants.AnsweringTimerAve,
                    maxTimerLength: ImposterDrawingConstants.AnsweringTimerMax);
                votingTimer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: ImposterDrawingConstants.VotingTimerMin,
                    aveTimerLength: ImposterDrawingConstants.VotingTimerAve,
                    maxTimerLength: ImposterDrawingConstants.VotingTimerMax);
            }
            int numWritingsPerPrompt = lobby.GetAllUsers().Count - 1;
            List<Prompt> prompts = new List<Prompt>();
            Setup = new Setup_GS(
                lobby: lobby,
                promptsToPopulate: prompts,
                setupTimeDuration: setupTimer);

            Dictionary<Prompt, List<User>> promptsToPromptedUsers = new Dictionary<Prompt, List<User>>();
            Setup.AddExitListener(() =>
            {
                /*promptsToPromptedUsers = CommonHelpers.EvenlyDistribute(
                    groups: prompts,
                    toDistribute: lobby.GetAllUsers().ToList(),
                    maxGroupSize: numWritingsPerPrompt,
                    validDistributeCheck: (Prompt prompt, User user) => user != prompt.Owner);
                foreach (Prompt prompt in promptsToPromptedUsers.Keys)
                {
                    prompt.Imposter = promptsToPromptedUsers[prompt][Rand.Next(0, promptsToPromptedUsers[prompt].Count)];
                }*/
                foreach (Prompt prompt in prompts) //todo fix EvenlyDistribute and return to that solution
                {
                    promptsToPromptedUsers.Add(prompt, lobby.GetAllUsers().Where(user => user != prompt.Owner).ToList());
                    prompt.Imposter = promptsToPromptedUsers[prompt][Rand.Next(0, promptsToPromptedUsers[prompt].Count)];
                }
            });
            StateChain CreateGamePlayLoop()
            {
                List<State> stateList = new List<State>();
                foreach (Prompt prompt in prompts)
                {
                    if (prompt == prompts.Last())
                    {
                        stateList.Add(GetImposterLoop(prompt, true));
                    }
                    else
                    {
                        stateList.Add(GetImposterLoop(prompt));
                    }          
                }
                StateChain gamePlayChain = new StateChain(states: stateList);
                gamePlayChain.Transition(this.Exit);
                return gamePlayChain;
            }
            this.Entrance.Transition(Setup);
            Setup.Transition(CreateGamePlayLoop);

            StateChain GetImposterLoop(Prompt prompt, bool lastRound = false)
            {
                List<User> randomizedUsers = promptsToPromptedUsers[prompt].OrderBy(_ => Rand.Next()).ToList();
                return new StateChain(
                    stateGenerator: (int counter) =>
                    {
                        if (counter == 0)
                        {
                            return new MakeDrawings_GS(
                                lobby: lobby,
                                promptToDraw: prompt,
                                usersToPrompt: randomizedUsers,
                                writingTimeDuration: answeringTimer);
                        }
                        if (counter == 1)
                        {
                            return GetVotingAndRevealState(prompt, (prompt.UsersToDrawings.Count < randomizedUsers.Count), votingTimer);   
                        }
                        if (counter == 2)
                        {
                            if (lastRound)
                            {
                                return new ScoreBoardGameState(lobby, "Final Scores");
                            }
                            else
                            {
                                return new ScoreBoardGameState(lobby);
                            }        
                        }
                        else
                        {
                            return null;
                        }
                    });
            }
        }

        
       /*public Dictionary<Prompt, List<User>> AssignPrompts(List<Prompt> prompts, List<User> users, int maxTextsPerPrompt)
        {
            return CommonHelpers.EvenlyDistribute(
                groups: prompts,
                toDistribute: users, 
                maxGroupSize: maxTextsPerPrompt,
                validDistributeCheck: (Prompt prompt, User user) => user != prompt.Owner);
        }*/
        public void ValidateOptions(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            // Empty
        }

        private State GetVotingAndRevealState(Prompt prompt, bool possibleNone, TimeSpan? votingTime)
        {
            int indexOfImposter = 0;
            List<User> randomizedUsersToShow = prompt.UsersToDrawings.Keys.OrderBy(_=>Rand.Next()).ToList();
            List<UserDrawing> drawings = randomizedUsersToShow.Select(user => prompt.UsersToDrawings[user]).ToList();
            /*foreach (UserDrawing drawing in drawings)
            {
                drawing.UnityImageRevealOverrides = new UnityObjectOverrides()
                {
                    Title = drawing.Owner.Equals(prompt.Imposter) ?  prompt.FakePrompt : prompt.RealPrompt 
                };
            }*/
            List<string> userNames = randomizedUsersToShow.Select(user => user.DisplayName).ToList();
            bool noneIsCorrect = possibleNone && !randomizedUsersToShow.Contains(prompt.Imposter);
            if (!noneIsCorrect)
            {
                indexOfImposter = drawings.IndexOf(prompt.UsersToDrawings[prompt.Imposter]);
            }
            if (possibleNone)
            {
                if (noneIsCorrect)
                {
                    indexOfImposter = drawings.Count;
                    drawings.Add(new UserDrawing()
                    {
                        Owner = prompt.Imposter,
                        Drawing = Constants.Drawings.NoneUnityImage,
                    });
                    randomizedUsersToShow.Add(prompt.Imposter);
                }
                else
                {
                    drawings.Add(new UserDrawing()
                    {
                        Owner = prompt.Owner,
                        Drawing = Constants.Drawings.NoneUnityImage,
                    });
                    randomizedUsersToShow.Add(prompt.Owner);
                }
            }

            return new DrawingVoteAndRevealState(
                lobby: this.Lobby,
                drawings: drawings,
                votingTime: votingTime)
            {
                VotingPromptTitle = (user)=>"Find the Imposter!",
                VotingPromptDescription = (User user)=>$"{((prompt.Owner == user)?($"You created this prompt. Real:'{prompt.RealPrompt}', Imposter:'{prompt.FakePrompt}'"):(!prompt.UsersToDrawings.ContainsKey(user) ? "You didn't draw anything for this prompt" : $"Your prompt was: '{(prompt.Imposter==user?prompt.FakePrompt:prompt.RealPrompt)}'"))}",
                VotingViewOverrides = new UnityViewOverrides
                {
                    Title = "Find the Imposter!",
                    Instructions = possibleNone ? "Someone didn't finish so there may not be an imposter in this group" : "",
                },
                RevealViewOverrides = new UnityViewOverrides
                {
                    Title = Invariant($"<color=green>{prompt.Imposter.DisplayName}</color> was the imposter!"),
                    Instructions = Invariant($"Real: '{prompt.RealPrompt}', Imposter: <color=green>'{prompt.FakePrompt}'</color>"),
                },
                VoteCountManager = CountVotes(prompt)
            };
        }

        private Action<List<UserDrawing>, IDictionary<User, VoteInfo>> CountVotes(Prompt prompt)
        {
            return (List<UserDrawing> choices, IDictionary<User, VoteInfo> votes) =>
            {
                foreach ((User user, VoteInfo vote) in votes)
                {
                    if (((UserDrawing)vote.ObjectsVotedFor[0]).Owner == prompt.Imposter)
                    {
                        user.ScoreHolder.AddScore(ImposterDrawingConstants.PointsForCorrectAnswer, Score.Reason.CorrectAnswer);
                    }
                }

                foreach (UserDrawing drawing in choices)
                {
                    if(drawing.Owner != prompt.Imposter)
                    {
                        // Calculates how good a job a non-imposter did.
                        int playersMisled = drawing.VotesCastForThisObject.Where(vote => vote.UserWhoVoted != drawing.Owner).Count();
                        int pointDeduction = playersMisled * ImposterDrawingConstants.LostPointsForBadNormal;
                        int netPoints = Math.Max(0, ImposterDrawingConstants.FreebiePointsForNormal - pointDeduction);
                        drawing.Owner.ScoreHolder.AddScore(netPoints, Score.Reason.Imposter_GoodNormal);
                    }
                    else
                    {
                        // Calculates how good a job the imposter did.
                        float portionVotersCorrect = (drawing.VotesCastForThisObject.Count() * 1.0f / votes.Count());
                        drawing.Owner.ScoreHolder.AddScore((int)Math.Round(portionVotersCorrect * ImposterDrawingConstants.BonusPointsForGoodImposter), Score.Reason.Imposter_GoodImposter);
                    }
                }
            };
        }
    }
}
