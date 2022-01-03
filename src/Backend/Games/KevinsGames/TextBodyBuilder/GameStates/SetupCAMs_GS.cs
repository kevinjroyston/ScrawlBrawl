using Backend.GameInfrastructure.DataModels.Users;
using Common.DataModels.Responses;
using Common.DataModels.Requests;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;
using static Backend.Games.Common.ThreePartPeople.DataModels.Person;
using Backend.Games.Common.ThreePartPeople;
using Common.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.Games.Common.GameStates;
using Backend.GameInfrastructure;
using Backend.Games.Common.ThreePartPeople.Extensions;
using Common.DataModels.Responses.Gameplay;
using Backend.Games.KevinsGames.TextBodyBuilder.DataModels;
using Backend.Games.Common.DataModels;
using static Backend.Games.KevinsGames.TextBodyBuilder.DataModels.TextPerson;

namespace Backend.Games.KevinsGames.TextBodyBuilder.GameStates
{
    public class SetupCAMs_GS : SetupGameState
    {
        private Random Rand { get; } = new Random();
        private Dictionary<User, List<CAMType>> UsersToRandomizedCAMTypes { get; set; } = new Dictionary<User, List<CAMType>>();
        private IReadOnlyList<CAMType> CamTypes { get; } = new List<CAMType>() { CAMType.Character, CAMType.Action, CAMType.Modifier };
        private ConcurrentBag<CAMUserText> CAMs { get; set; }


        public SetupCAMs_GS(
            Lobby lobby,
            ConcurrentBag<CAMUserText> cams,
            int numExpectedPerUser,
            TimeSpan? setupDurration = null)
            : base(
                lobby: lobby,
                numExpectedPerUser: numExpectedPerUser,
                unityTitle: "",
                unityInstructions: "Complete as many descriptors as possible before the time runs out",
                setupDuration: setupDurration)
        {
            this.CAMs = cams;
            
            foreach(User user in lobby.GetAllUsers())
            {
                UsersToRandomizedCAMTypes.Add(user, CamTypes.OrderBy(_ => Rand.Next()).ToList());
            }
        }

        public override UserPrompt CountingPromptGenerator(User user, int counter)
        {
            if (counter % 3 == 0)
            {
                UsersToRandomizedCAMTypes[user] = CamTypes.OrderBy(_ => Rand.Next()).ToList();
            }
            CAMType camType = UsersToRandomizedCAMTypes[user][counter % 3];
            string Description="";
            switch (camType)
            {
                case CAMType.Character:
                    {
                        Description = "Enter a <span class='characterClass'>Character</span>";
                        break;
                    }
                case CAMType.Action:
                    {
                        Description = "Describe an <span class='actionClass'>Action</span>";
                        break;
                    }
                case CAMType.Modifier:
                    {
                        Description = "Enter a <span class='modifierClass'>Modifier</span>";
                        break;
                    }
            }
            return new UserPrompt()
            {
                UserPromptId = UserPromptId.TextBodyBuilder_CreateCAM,
                Title = Invariant($"Time to write!"),
                PromptHeader = new PromptHeaderMetadata()
                {
                    MaxProgress = NumExpectedPerUser,
                    CurrentProgress = counter + 1,
                },
                Suggestion = new SuggestionMetadata { SuggestionKey = $"TextBodyBuilder-{camType}" },
                SubPrompts = new SubPrompt[]
                {
                    new SubPrompt
                    {
                        Prompt = Description,
                        ShortAnswer = true,
                    },
                },
                SubmitButton = true
            };
        }
        private string correctCase(string prompt, CAMType type)
        {  // remove lead cap on actions and modifiers, unless 2nd char is also cap, then leave it alone
            if (((type != CAMType.Character) && (prompt.Length > 1)) &&
                (Char.IsUpper(prompt[0]) && !Char.IsUpper(prompt[1])))
                { prompt = Char.ToLower(prompt[0]).ToString() + prompt.Substring(1); }
            return prompt;
        }
        public override (bool, string) CountingFormSubmitHandler(User user, UserFormSubmission input, int counter)
        {
            if (this.CAMs.Select((prompt) => prompt.Text).Contains(input.SubForms[0].ShortAnswer))
            {
                return (false, "Someone has already entered that prompt");
            }
            this.CAMs.Add(new CAMUserText
            {
                Text = correctCase(input.SubForms[0].ShortAnswer, UsersToRandomizedCAMTypes[user][counter % 3]),
                Owner = user,
                Type = UsersToRandomizedCAMTypes[user][counter % 3]
            });
            return (true, string.Empty);
        }
        public override UserTimeoutAction CountingUserTimeoutHandler(User user, UserFormSubmission input, int counter)
        {
            if (!string.IsNullOrWhiteSpace(input?.SubForms?[0]?.ShortAnswer))
            {
                this.CAMs.Add(new CAMUserText
                {
                    Text = correctCase(input.SubForms[0].ShortAnswer, UsersToRandomizedCAMTypes[user][counter % 3]),
                    Owner = user,
                    Type = UsersToRandomizedCAMTypes[user][counter % 3]
                });
            }
            return UserTimeoutAction.None;
        }
    }
}
