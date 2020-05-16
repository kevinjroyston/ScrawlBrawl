using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Connector = System.Action<
    RoystonGame.TV.DataModels.Users.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.DataModels.States.GameStates
{
    public class EndOfGameState : GameState
    {
        public static UserPrompt ContinuePrompt(User user) => new UserPrompt()
        {
            Title = "What shall we do next?",
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

        private static Dictionary<EndOfGameRestartType, string> RestartTypes = new Dictionary<EndOfGameRestartType, string>()
        {
            { EndOfGameRestartType.SameGameAndPlayers, "Replay" },
            { EndOfGameRestartType.SamePlayers, "New game, Same players" },
            { EndOfGameRestartType.NewPlayers, "Change Players" },
        };

        public EndOfGameState(Lobby lobby, Action<EndOfGameRestartType> endOfGameRestartCallback)
            : base(
                  lobby,
                  exit: new WaitForPartyLeader_StateExit(
                      lobby: lobby,
                      partyLeaderPrompt: ContinuePrompt,
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

            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForPartyLeader },
                Instructions = new StaticAccessor<string> { Value = "Waiting for party leader . . ." },
            };
        }
    }
}
