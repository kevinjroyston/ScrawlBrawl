using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.States.StateGroups;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.ImposterDrawing.DataModels;
using RoystonGame.TV.GameModes.BriansGames.ImposterDrawing.GameStates;
using RoystonGame.TV.GameModes.Common;
using RoystonGame.TV.GameModes.Common.GameStates;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.BriansGames.ImposterDrawing
{
    public class ImposterDrawingGameMode : IGameMode
    {
        private Setup_GS Setup { get; set; }
        private Random Rand { get; } = new Random();
        public ImposterDrawingGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            ValidateOptions(lobby, gameModeOptions);
            int gameSpeed = (int)gameModeOptions[(int)GameModeOptionsEnum.gameSpeed].ValueParsed;
            TimeSpan? setupTimer = null;
            TimeSpan? answeringTimer = null;
            TimeSpan? votingTimer = null;
            if (gameSpeed > 0)
            {
                setupTimer = TimeSpan.FromSeconds(CommonHelpers.ThreePointLerp(
                    minX: 1,
                    aveX: 5,
                    maxX: 10,
                    x: (double)gameSpeed,
                    minValue: ImposterDrawingConstants.SetupTimerMin,
                    aveValue: ImposterDrawingConstants.SetupTimerAve,
                    maxValue: ImposterDrawingConstants.SetupTimerMax));
                answeringTimer = TimeSpan.FromSeconds(CommonHelpers.ThreePointLerp(
                    minX: 1,
                    aveX: 5,
                    maxX: 10,
                    x: (double)gameSpeed,
                    minValue: ImposterDrawingConstants.AnsweringTimerMin,
                    aveValue: ImposterDrawingConstants.AnsweringTimerAve,
                    maxValue: ImposterDrawingConstants.AnsweringTimerMax));
                votingTimer = TimeSpan.FromSeconds(CommonHelpers.ThreePointLerp(
                    minX: 1,
                    aveX: 5,
                    maxX: 10,
                    x: (double)gameSpeed,
                    minValue: ImposterDrawingConstants.VotingTimerMin,
                    aveValue: ImposterDrawingConstants.VotingTimerAve,
                    maxValue: ImposterDrawingConstants.VotingTimerMax));
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
                                writingTimeDurration: answeringTimer);
                        }
                        if (counter == 1)
                        {
                            return new Voting_GS(
                                lobby: lobby,
                                prompt: prompt,
                                randomizedUsersToShow: randomizedUsers.Where((User user) => prompt.UsersToDrawings.ContainsKey(user)).ToList(),
                                possibleNone: (prompt.UsersToDrawings.Count < randomizedUsers.Count),
                                votingTimeDurration: votingTimer);
                        }
                        if (counter == 2)
                        {
                            return new VoteRevealed_GS(
                                lobby: lobby,
                                prompt: prompt,
                                randomizedUsersToShow: randomizedUsers.Where((User user) => prompt.UsersToDrawings.ContainsKey(user)).ToList(),
                                possibleNone: (prompt.UsersToDrawings.Count < randomizedUsers.Count));
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
