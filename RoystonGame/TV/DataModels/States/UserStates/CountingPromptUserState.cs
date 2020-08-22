using RoystonGame.TV.ControlFlows.Enter;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.DataModels.States.UserStates
{
    public class CountingPromptUserState : SimplePromptUserState
    {
        private static int counter = -1;
        public CountingPromptUserState( // not working not used
            Func<User, int, UserPrompt> countingPromptGenerator = null,
            TimeSpan? maxPromptDuration = null,
            Func<User, UserFormSubmission, int, (bool, string)> countingFormSubmitHandler = null,
            StateEntrance entrance = null,
            StateExit exit = null,
            Func<User, UserFormSubmission, int, UserTimeoutAction> countingUserTimeoutHandler = null)
            : base(
                  promptGenerator: (User user) =>
                  {
                      counter++;
                      return countingPromptGenerator(user, counter);
                  },
                  formSubmitHandler: (User user, UserFormSubmission input) =>
                  {
                      return countingFormSubmitHandler(user, input, counter);
                  },
                  userTimeoutHandler: (User user, UserFormSubmission input) =>
                  {
                      return countingUserTimeoutHandler(user, input, counter);
                  },
                  entrance: entrance,
                  exit: exit,
                  maxPromptDuration: maxPromptDuration)
        {
            //empty
        }
    }
}
