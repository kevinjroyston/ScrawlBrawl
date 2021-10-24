﻿using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.Games.Common.ThreePartPeople.DataModels;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.DataModels.States.UserStates;

namespace Backend.Games.KevinsGames.TextBodyBuilder.GameStates
{
    public class DisplayContestants_GS : GameState
    {
        public DisplayContestants_GS(
            Lobby lobby,
            string title,
            IReadOnlyList<Person> peopleList,     
            Func<Person, Color?> backgroundColor = null,
            Func<Person, string> imageIdentifier = null,
            Func<Person, string> imageTitle = null,
            Func<Person, string> imageHeader = null)
            : base(
                  lobby: lobby,
                  exit: new WaitForPartyLeader_StateExit(
                      lobby: lobby,
                      partyLeaderPromptGenerator: Prompts.PartyLeaderSkipRevealButton(),
                      waitingPromptGenerator: Prompts.DisplayText()
                      )
                  )
        {
            if(peopleList == null || peopleList.Count == 0)
            {
                throw new ArgumentException("PeopleList cannot be empty");
            }

            this.Entrance.Transition(this.Exit);

            backgroundColor ??= (person) => null;
            imageIdentifier ??= (person) => null;
            imageTitle ??= (person) => null;
            imageHeader ??= (person) => null;
            var unityObjects = new List<UnityObject>();
            foreach(Person person in peopleList)
            {
                unityObjects.Add(person.GetUnityObject(
                    backgroundColor: backgroundColor(person),
                    imageIdentifier: imageIdentifier(person),
                    title: imageTitle(person),
                    header: imageHeader(person)
                    ));
            }
        
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = TVScreenId.ShowDrawings,
                UnityObjects = new UnityField<IReadOnlyList<UnityObject>> { Value = unityObjects },
                Title = new UnityField<string> { Value = title },
            };
        }
    }
}
