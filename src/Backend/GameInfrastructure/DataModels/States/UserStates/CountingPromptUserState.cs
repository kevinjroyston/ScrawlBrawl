using Backend.GameInfrastructure.ControlFlows.Enter;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.Users;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;

namespace Backend.GameInfrastructure.DataModels.States.UserStates
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
