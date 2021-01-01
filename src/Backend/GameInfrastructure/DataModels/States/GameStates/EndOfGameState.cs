using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Common.DataModels.Enums;

namespace Backend.GameInfrastructure.DataModels.States.GameStates
{
    public class EndOfGameState : GameState
    {
        public static UserPrompt ContinuePrompt(User user) => new UserPrompt()
        {
            Title = "What shall we do next?",
            UserPromptId = UserPromptId.PartyLeader_GameEnd,
            RefreshTimeInMs = 5000,
            SubPrompts = new SubPrompt[]
            {
                new SubPrompt()
                {
                    Prompt = "Lets play some more!",
                    Answers = RestartTypes.Values.ToArray()
                },
            },
            SubmitButton = true,
        };

        private static readonly Dictionary<EndOfGameRestartType, string> RestartTypes = new Dictionary<EndOfGameRestartType, string>()
        {
            { EndOfGameRestartType.ResetScore, "Back to lobby, reset scores" },
            { EndOfGameRestartType.KeepScore, "Back to lobby, keep scores" },
            { EndOfGameRestartType.Disband, "Disband" }
        };

        public EndOfGameState(Lobby lobby, Action<EndOfGameRestartType> endOfGameRestartCallback)
            : base(
                  lobby,
                  exit: new WaitForPartyLeader_StateExit(
                      lobby: lobby,
                      partyLeaderPromptGenerator: ContinuePrompt,
                      partyLeaderFormSubmitListener: (User user, UserFormSubmission submission) =>
                      {
                          int? selectedIndex = submission.SubForms[0].RadioAnswer;
                          if (selectedIndex == null)
                          {
                              throw new Exception("Should have been caught in user input validation");
                          }
                          endOfGameRestartCallback(RestartTypes.Keys.ToList()[selectedIndex.Value]);
                          return (true, string.Empty);
                      }))
        {
            this.Entrance.Transition(this.Exit);

            this.UnityView = new Legacy_UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForPartyLeader },
                Instructions = new StaticAccessor<string> { Value = "Waiting for party leader . . ." },
            };
        }
    }
}
