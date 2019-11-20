﻿using System;
using System.Collections.Generic;
using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.TV.GameEngine;
using RoystonGame.TV.GameEngine.Rendering;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;

using static System.FormattableString;

namespace RoystonGame.TV.DataModels.GameStates
{
    public class UserSignupGameState : GameState
    {
        public static UserPrompt UserNamePrompt() => new UserPrompt()
        {
            Title = "Welcome to the game!",
            Description = "Follow the instructions below!",
            RefreshTimeInMs = 5000,
            SubPrompts = new SubPrompt[]
            {
                new SubPrompt()
                {
                    Prompt = "Nickname:",
                    ShortAnswer = true
                },
                new SubPrompt()
                {
                    Prompt = "Self Portrait:",
                    Drawing = true
                }
            },
            SubmitButton = true,
        };

        public UserSignupGameState(Action<User, UserStateResult, UserFormSubmission> userStateCompletedCallback = null) : base(userStateCompletedCallback)
        {
            UserState entrance = new SimplePromptUserState(UserNamePrompt());
            WaitForPartyLeader transition = new WaitForPartyLeader(this.UserOutlet);
            entrance.SetOutlet((User user, UserStateResult result, UserFormSubmission userInput) =>
            {
                GameManager.RegisterUser(user, userInput.SubForms[0].ShortAnswer, userInput.SubForms[1].Drawing);
                // TODO: this.GameObjects.Add(new TextObject + ImageObject)
                // TODO: vertical/horizontal alignment group. Depends on anchor/resizing code

                // TODO: remove hacky below implementation.
                ((TextObject)this.GameObjects[0]).Content = Invariant($"{((TextObject)this.GameObjects[0]).Content} {userInput.SubForms[0].ShortAnswer}");

                transition.Inlet(user, result, userInput);
            });

            this.Entrance = entrance;

            this.GameObjects = new List<GameObject>()
            {
                new TextObject { Content = "Waiting for players :). Joined so far: " }
            };
        }
    }
}