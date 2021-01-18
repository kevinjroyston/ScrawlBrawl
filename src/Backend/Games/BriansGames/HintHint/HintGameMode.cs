using Backend.GameInfrastructure;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.HintHint.DataModels;
using Backend.Games.BriansGames.HintHint.GameStates;
using Backend.Games.Common;
using Backend.Games.Common.GameStates;
using Common.Code.Helpers;
using Common.DataModels.Enums;
using Common.DataModels.Interfaces;
using Common.DataModels.Requests.LobbyManagement;
using Common.DataModels.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Games.BriansGames.HintHint
{
    public class HintGameMode : IGameMode
    {
        private Lobby Lobby { get; set; }
        private Random Rand { get; } = new Random();

        public static GameModeMetadata GameModeMetadata { get; } = new GameModeMetadata
        {
            Title = "Hint Hint",
            GameId = GameModeId.HintHint,
            Description = "Try to guess what's being hinted at, but be warned, one of hint givers may be trying to manipulate you!",     
            MinPlayers = 6,
            MaxPlayers = null,
            Attributes = new GameModeAttributes
            {
                ProductionReady = false,
            },
            Options = new List<GameModeOptionResponse>
            {
                new GameModeOptionResponse
                {
                    Description = "The number of real hint givers",
                    ResponseType = ResponseType.Integer,
                    DefaultValue = 2,
                    MinValue = 1,
                    MaxValue = 10,
                },
                new GameModeOptionResponse
                {
                    Description = "The number of fake hint givers",
                    ResponseType = ResponseType.Integer,
                    DefaultValue = 1,
                    MinValue = 1,
                    MaxValue = 5,
                },
                new GameModeOptionResponse
                {
                    Description = "Max number of hints (per person)",
                    ResponseType = ResponseType.Integer,
                    DefaultValue = 10,
                    MinValue = 1,
                    MaxValue = 50,
                },
                new GameModeOptionResponse
                {
                    Description = "Max number of guesses (per person)",
                    ResponseType = ResponseType.Integer,
                    DefaultValue = 10,
                    MinValue = 1,
                    MaxValue = 20,
                },
                new GameModeOptionResponse
                {
                    Description = "Length of the game (10 for longest 1 for shortest 0 for no timer)",
                    ResponseType = ResponseType.Integer,
                    DefaultValue = 5,
                    MinValue = 0,
                    MaxValue = 10,
                },
            }
        };

        public HintGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            ValidateOptions(gameModeOptions);

            int numRealHintGivers = (int)gameModeOptions[(int)GameModeOptionsEnum.NumRealHints].ValueParsed;
            int numFakeHintGivers = (int)gameModeOptions[(int)GameModeOptionsEnum.NumFakeHints].ValueParsed;
            int maxHints = (int)gameModeOptions[(int)GameModeOptionsEnum.MaxHints].ValueParsed;
            int maxGuesses = (int)gameModeOptions[(int)GameModeOptionsEnum.MaxGuesses].ValueParsed;
            int numBannedWords = 4;
            int gameLength = (int)gameModeOptions[(int)GameModeOptionsEnum.GameLength].ValueParsed;


            TimeSpan? setupRound1Timer = null;
            TimeSpan? setupRound2Timer = null;
            TimeSpan? setupRound3Timer = null;
            TimeSpan? guessingTimer = null;
            if (gameLength > 0)
            {
                setupRound1Timer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: HintConstants.SetupRound1TimerMin,
                    aveTimerLength: HintConstants.SetupRound1TimerAve,
                    maxTimerLength: HintConstants.SetupRound1TimerMax);
                setupRound2Timer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: HintConstants.SetupRound2TimerMin,
                    aveTimerLength: HintConstants.SetupRound2TimerAve,
                    maxTimerLength: HintConstants.SetupRound2TimerMax);
                setupRound3Timer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: HintConstants.SetupRound3TimerMin,
                    aveTimerLength: HintConstants.SetupRound3TimerAve,
                    maxTimerLength: HintConstants.SetupRound3TimerMax);
                guessingTimer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: HintConstants.GuessingTimerMin,
                    aveTimerLength: HintConstants.GuessingTimerAve,
                    maxTimerLength: HintConstants.GuessingTimerMax);
            }

            this.Lobby = lobby;

            ConcurrentBag<RealFakePair> realFakePairs = new ConcurrentBag<RealFakePair>();

            StateChain setupStateChain = new StateChain(
                stateGenerator: (int counter) =>
                {
                    switch (counter)
                    {
                        case 0:
                            return new SetupRound1_GS(lobby, realFakePairs, setupRound1Timer);

                        case 1:
                            setupRound2Timer = setupRound2Timer?.Multiply(Math.Ceiling(1.0 * numBannedWords / numFakeHintGivers));
                            AssignRealFakeUsers();
                            return new SetupRound2_GS(lobby, realFakePairs.ToList(), (int) Math.Ceiling( 1.0 * numBannedWords / numFakeHintGivers), numBannedWords, setupRound2Timer);

                        case 2:
                            return new SetupRound3_GS(lobby, realFakePairs.ToList(), setupRound3Timer);

                        default:
                            return null;
                    }

                });

            List<State> chain = new List<State>();
            List<RealFakePair> randomizedRealFakePairs = realFakePairs.OrderBy(_ => Rand.Next()).ToList();
            StateChain hintScoreChain = new StateChain(states: randomizedRealFakePairs.Select(realFakePair =>
            {
                return (State) new StateChain(stateGenerator: (int counter) =>
                {
                    switch (counter)
                    {
                        case 0:
                            return new HintRound_GS(lobby, realFakePair, maxHints, maxGuesses, guessingTimer);
                        case 1:
                            return new HintGuessReveal(lobby, realFakePair);
                        case 2:
                            if (realFakePair.Equals(randomizedRealFakePairs.Last()))
                            {
                                return new ScoreBoardGameState(lobby, "Final Scores:");
                            }
                            else
                            {
                                return new ScoreBoardGameState(lobby);
                            }
                        default:
                            return null;
                    }
                });
            }).ToList());


            StateChain gameplayStateChain = new StateChain(
                stateGenerator: (int counter) =>
                {
                    switch (counter)
                    {
                        case 0:
                            return setupStateChain;

                        case 1:
                            return hintScoreChain;

                        default:
                            return null;
                    }

                });

            this.Entrance.Transition(gameplayStateChain);
            gameplayStateChain.Transition(this.Exit);

            void AssignRealFakeUsers()
            {
                foreach (RealFakePair rfp in realFakePairs)
                {
                    rfp.MaxMemberCount = numRealHintGivers + numFakeHintGivers;
                }
                List<RealFakePair> rfpList = realFakePairs.ToList();
                List<IGroup<User>> groups = MemberHelpers<User>.Assign(
                    constraints: rfpList.Cast<IConstraints<User>>().ToList(),
                    members: lobby.GetAllUsers());
                if (groups.Count != rfpList.Count)
                {
                    throw new Exception("Something went wrong assigning hint givers");
                }
                for (int i = 0; i < rfpList.Count; i++)
                {
                    rfpList[i].PopulateHintGivers(groups[i].Members.ToList(), numFakeHintGivers);
                }
            }
        }
        
        public void ValidateOptions(List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            
        }
    }
}
