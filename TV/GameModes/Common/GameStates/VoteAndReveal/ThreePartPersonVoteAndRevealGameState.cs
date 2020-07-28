using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.Common.GameStates.VoteAndReveal
{
    public class ThreePartPersonVoteAndRevealGameState : VoteAndRevealGameState
    {
        private Lobby Lobby { get; set; }
        private List<Person> PeopleToVoteOn { get; set; }

        public ThreePartPersonVoteAndRevealGameState(
            Lobby lobby,
            List<Person> peopleToVoteOn,
            List<User> votingUsers = null,
            TimeSpan? votingTime = null) : base(lobby, votingUsers, votingTime)
        {
            this.lobby = lobby;
            this.peopleToVoteOn = peopleToVoteOn;
            this.votingUsers = votingUsers;

            if (imageTitles != null && imageTitles.Count != peopleToVoteOn.Count)
            {
                //todo log error
            }

        }

        public override SetP
    }
}
