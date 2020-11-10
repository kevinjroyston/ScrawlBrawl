using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.ImposterText.DataModels;
using Backend.Games.BriansGames.ImposterText.GameStates;
using Backend.Games.Common;
using Backend.Games.Common.GameStates;
using Common.DataModels.Requests.LobbyManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using Backend.GameInfrastructure;
using Common.Code.Helpers;
using System.Collections.Immutable;
using Common.Code.Extensions;
using Common.DataModels.Interfaces;

namespace Backend.Games.BriansGames.ImposterText
{
    public class ImposterTextGameMode : IGameMode
    {
        private Setup_GS Setup { get; set; }
        private Random Rand { get; } = new Random();
        public ImposterTextGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            ValidateOptions(lobby, gameModeOptions);
            int gameLength = (int)gameModeOptions[(int)GameModeOptionsEnum.GameLength].ValueParsed;
            TimeSpan? setupTimer = null;
            TimeSpan? answeringTimer = null;
            TimeSpan? votingTimer = null;
            if (gameLength > 0)
            {
                setupTimer = TimeSpan.FromSeconds(MathHelpers.ThreePointLerp(
                    minX: 1,
                    aveX: 5,
                    maxX: 10,
                    x: (double)gameLength,
                    minValue: ImposterTextConstants.SetupTimerMin,
                    aveValue: ImposterTextConstants.SetupTimerAve,
                    maxValue: ImposterTextConstants.SetupTimerMax));
                answeringTimer = TimeSpan.FromSeconds(MathHelpers.ThreePointLerp(
                    minX: 1,
                    aveX: 5,
                    maxX: 10,
                    x: (double)gameLength,
                    minValue: ImposterTextConstants.AnsweringTimerMin,
                    aveValue: ImposterTextConstants.AnsweringTimerAve,
                    maxValue: ImposterTextConstants.AnsweringTimerMax));
                votingTimer = TimeSpan.FromSeconds(MathHelpers.ThreePointLerp(
                    minX: 1,
                    aveX: 5,
                    maxX: 10,
                    x: (double)gameLength,
                    minValue: ImposterTextConstants.VotingTimerMin,
                    aveValue: ImposterTextConstants.VotingTimerAve,
                    maxValue: ImposterTextConstants.VotingTimerMax));
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
                StateChain gamePlayChain = new StateChain(states: stateList, exit: this.Exit);
                gamePlayChain.Transition(this.Exit);
                return gamePlayChain;
            }
            this.Entrance.Transition(Setup);
            Setup.Transition(CreateGamePlayLoop);

            StateChain GetImposterLoop(Prompt prompt, bool lastRound = false)
            {
                List<User> randomizedUsers = promptsToPromptedUsers[prompt].OrderBy(_=>Rand.Next()).ToList();
                return new StateChain(
                    stateGenerator: (int counter) =>
                    {
                        if (counter == 0)
                        {
                            return new MakeTexts_GS(
                                lobby: lobby,
                                promptToDraw: prompt,
                                usersToPrompt: randomizedUsers,
                                writingTimeDuration: answeringTimer);
                        }
                        if (counter == 1)
                        {
                            return new Voting_GS(
                                lobby: lobby,
                                prompt: prompt,
                                randomizedUsersToShow: randomizedUsers.Where((User user) => prompt.UsersToAnswers.ContainsKey(user)).ToList(),
                                possibleNone: (prompt.UsersToAnswers.Count < randomizedUsers.Count),
                                votingTimeDuration: votingTimer);
                        }
                        if (counter == 2)
                        {
                            return new VoteRevealed_GS(
                                lobby: lobby,
                                prompt: prompt,
                                randomizedUsersToShow: randomizedUsers.Where((User user) => prompt.UsersToAnswers.ContainsKey(user)).ToList(),
                                possibleNone: (prompt.UsersToAnswers.Count < randomizedUsers.Count));
                        }
                        if (counter == 3)
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

       /* public Dictionary<Prompt, List<User>> AssignPrompts(List<Prompt> prompts, List<User> users, int maxTextsPerPrompt)
        {
            prompts.ForEach((Prompt prompt) =>
            {
                prompt.MaxMemberCount = maxTextsPerPrompt;
                prompt.BannedMemberIds = ImmutableHashSet.Create(prompt.Owner.Id);
                prompt.AllowDuplicateIds = false;
            });

            return MemberHelpers<User>.Assign(
                constraints: prompts.Cast<IConstraints<User>>().ToList(),
                members: users,
                maxDuplicates: prompts.Count*maxTextsPerPrompt/users.Count);
        }*/
        public void ValidateOptions(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            // Empty
        }
    }
}
