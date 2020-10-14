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

namespace Backend.Games.BriansGames.ImposterDrawing
{
    public class ImposterDrawingGameMode : IGameMode
    {
        public static GameModeMetadata GameModeMetadata { get; } =
            new GameModeMetadata
            {
                Title = "Imposter Syndrome",
                GameId = GameModeId.Imposter.ToString(),
                Description = "Come up with a difference only you'll be able to spot!",
                MinPlayers = 4,
                MaxPlayers = null,
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
                }
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

        public Dictionary<Prompt, List<User>> AssignPrompts(List<Prompt> prompts, List<User> users, int maxTextsPerPrompt)
        {
            return CommonHelpers.EvenlyDistribute(
                groups: prompts,
                toDistribute: users, 
                maxGroupSize: maxTextsPerPrompt,
                validDistributeCheck: (Prompt prompt, User user) => user != prompt.Owner);
        }
        public void ValidateOptions(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            // Empty
        }

        private State GetVotingAndRevealState(Prompt prompt, bool possibleNone, TimeSpan? votingTime)
        {
            int indexOfImposter = 0;
            List<User> randomizedUsersToShow = prompt.UsersToDrawings.Keys.OrderBy(_=>Rand.Next()).ToList();
            List<UserDrawing> drawings = randomizedUsersToShow.Select(user => prompt.UsersToDrawings[user]).ToList();
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
                        Drawing = Constants.NoneUnityImage,
                    });
                    randomizedUsersToShow.Add(prompt.Imposter);
                }
                else
                {
                    drawings.Add(new UserDrawing()
                    {
                        Owner = prompt.Owner,
                        Drawing = Constants.NoneUnityImage,
                    });
                    randomizedUsersToShow.Add(prompt.Owner);
                }
            }

            return new DrawingVoteAndRevealState(
                lobby: this.Lobby,
                drawings: drawings,
                voteCountManager: (Dictionary<User, int> usersToVotes) =>
                {
                    CountVotes(usersToVotes, prompt, randomizedUsersToShow);
                },
                votingTime: votingTime)
                {
                    VotingTitle = "Find the Imposter!",
                    VotingInstructions = possibleNone ? "Someone didn't finish so there may not be an imposter in this group" : "",
                    RevealTitle = Invariant($"<color=green>{prompt.Imposter.DisplayName}</color> was the imposter!"),
                    RevealInstructions = Invariant($"Real: '{prompt.RealPrompt}', Imposter: <color=green>'{prompt.FakePrompt}'</color>"),
                    IndexesOfObjectsToReveal = new List<int>() { indexOfImposter },
                    ObjectTitles = userNames,
                    ShowObjectTitlesForVoting = false,
            };
        }

        private void CountVotes(Dictionary<User, int> usersToVotes, Prompt prompt, List<User> randomizedUsers)
        {
            List<User> correctUsers = new List<User>();
            foreach (User user in usersToVotes.Keys)
            {
                if (randomizedUsers[usersToVotes[user]] == prompt.Imposter)
                {
                    correctUsers.Add(user);
                }
                else
                {
                    if (randomizedUsers[usersToVotes[user]] != user)
                    {
                        randomizedUsers[usersToVotes[user]].AddScore(ImposterDrawingConstants.PointsToLooseForWrongVote); // user was voted for when they weren't the imposter so they lose points
                    }
                }
            }

            int totalPointsToAward = usersToVotes.Count * ImposterDrawingConstants.TotalPointsToAwardPerVote; // determine the total number of points to distribute
            foreach (User user in correctUsers)
            {
                user.AddScore(totalPointsToAward / correctUsers.Count); //distribute those evenly to the correct users
            }

            // If EVERYBODY figures out the diff, the owner loses some points but not as many.
            if (correctUsers.Where(user => user != prompt.Owner).Count() == (this.Lobby.GetAllUsers().Count - 1))
            {
                prompt.Owner.AddScore(totalPointsToAward / -4);
            }

            // If the owner couldnt find the diff, they lose a bunch of points.
            if (!correctUsers.Contains(prompt.Owner))
            {
                prompt.Owner.AddScore(totalPointsToAward / -2);
            }
        }
    }
}
