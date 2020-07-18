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
            int numWritingsPerPrompt = lobby.GetAllUsers().Count - 1;
            List<Prompt> prompts = new List<Prompt>();
            Setup = new Setup_GS(
                lobby: lobby,
                promptsToPopulate: prompts,
                setupTimeDurration: null);

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
                                writingTimeDurration: null);
                        }
                        if (counter == 1)
                        {
                            return new Voting_GS(
                                lobby: lobby,
                                prompt: prompt,
                                randomizedUsersToShow: randomizedUsers,
                                possibleNone: (prompt.UsersToAnswers.Count < randomizedUsers.Count),
                                drawingTimeDurration: null);
                        }
                        if (counter == 2)
                        {
                            return new VoteRevealed_GS(
                                lobby: lobby,
                                prompt: prompt,
                                randomizedUsersToShow: randomizedUsers,
                                possibleNone: (prompt.UsersToAnswers.Count < randomizedUsers.Count),
                                drawingTimeDurration: null);
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
