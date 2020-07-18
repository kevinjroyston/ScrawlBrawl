using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.States.StateGroups;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.ImposterText.DataModels;
using RoystonGame.TV.GameModes.BriansGames.ImposterText.GameStates;
using RoystonGame.TV.GameModes.Common;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.BriansGames.ImposterText
{
    public class ImposterTextGameMode : IGameMode
    {
        private Setup_GS Setup { get; set; }
        private Random Rand { get; } = new Random();
        public ImposterTextGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            ValidateOptions(lobby, gameModeOptions);
            int gameSpeed = (int)gameModeOptions[(int)GameModeOptionsEnum.gameSpeed].ValueParsed;
            TimeSpan? setupTimer = null;
            TimeSpan? answeringTimer = null;
            TimeSpan? votingTimer = null;
            if (gameSpeed > 0)
            {
                setupTimer = TimeSpan.FromSeconds(CommonHelpers.LinearMapping(
                    minPosition: 1,
                    maxPosition: 10,
                    position: (double)gameSpeed,
                    minValue: ImposterTextConstants.SetupTimerMin,
                    maxValue: ImposterTextConstants.SetupTimerMax));
                answeringTimer = TimeSpan.FromSeconds(CommonHelpers.LinearMapping(
                    minPosition: 1,
                    maxPosition: 10,
                    position: (double)gameSpeed,
                    minValue: ImposterTextConstants.AnsweringTimerMin,
                    maxValue: ImposterTextConstants.AnsweringTimerMax));
                votingTimer = TimeSpan.FromSeconds(CommonHelpers.LinearMapping(
                    minPosition: 1,
                    maxPosition: 10,
                    position: (double)gameSpeed,
                    minValue: ImposterTextConstants.VotingTimerMin,
                    maxValue: ImposterTextConstants.VotingTimerMax));
            }    
            int numWritingsPerPrompt = lobby.GetAllUsers().Count - 1;
            List<Prompt> prompts = new List<Prompt>();
            Setup = new Setup_GS(
                lobby: lobby,
                promptsToPopulate: prompts,
                setupTimeDurration: setupTimer);

            Dictionary<Prompt, List<User>> promptsToPromptedUsers = new Dictionary<Prompt, List<User>>();
            Setup.AddExitListener(() =>
            {
                promptsToPromptedUsers = CommonHelpers.EvenlyDistribute(
                    groups: prompts,
                    toDistribute: lobby.GetAllUsers().ToList(),
                    maxGroupSize: numWritingsPerPrompt,
                    validDistributeCheck: (Prompt prompt, User user) => user != prompt.Owner);
                foreach (Prompt prompt in promptsToPromptedUsers.Keys)
                {
                    prompt.ImposterCreator = promptsToPromptedUsers[prompt][Rand.Next(0, promptsToPromptedUsers[prompt].Count)];
                }
            });
            StateChain CreateGamePlayLoop()
            {
                List<State> stateList = new List<State>();
                foreach (Prompt prompt in prompts)
                {
                    stateList.Add(GetImposterLoop(prompt));
                }
                return new StateChain(states: stateList, exit: this.Exit);
            }
            this.Entrance.Transition(Setup);
            Setup.Transition(CreateGamePlayLoop);

            StateChain GetImposterLoop(Prompt prompt)
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
                                writingTimeDurration: answeringTimer);
                        }
                        if (counter == 1)
                        {
                            return new Voting_GS(
                                lobby: lobby,
                                prompt: prompt,
                                randomizedUsersToShow: randomizedUsers.Where((User user) => prompt.UsersToAnswers.ContainsKey(user)).ToList(),
                                possibleNone: (prompt.UsersToAnswers.Count < randomizedUsers.Count),
                                votingTimeDurration: votingTimer);
                        }
                        if (counter == 2)
                        {
                            return new VoteRevealed_GS(
                                lobby: lobby,
                                prompt: prompt,
                                randomizedUsersToShow: randomizedUsers.Where((User user) => prompt.UsersToAnswers.ContainsKey(user)).ToList(),
                                possibleNone: (prompt.UsersToAnswers.Count < randomizedUsers.Count));
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
    }
}
