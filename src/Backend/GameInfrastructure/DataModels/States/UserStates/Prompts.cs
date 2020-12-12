using Backend.GameInfrastructure.DataModels.Users;
using Common.DataModels.Enums;
using Common.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.GameInfrastructure.DataModels.States.UserStates
{

    public static class Prompts
    {
        public static Func<User, UserPrompt> DisplayText(string description = null, UserPromptId? promptId=null) {
            description ??= Text.Waiting;
            promptId ??= UserPromptId.Waiting;
            return (User user) => new UserPrompt()
            {
                UserPromptId = promptId.Value,
                Description = description,
            };
        }
        public static class Text
        {
            public const string Waiting = "Waiting for other players . . .";
        }
    }
}
