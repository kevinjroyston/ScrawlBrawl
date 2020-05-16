using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.GameModes.Common.DataModels;
using RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels
{
    public class Setup_Person: Person
    {
        public Dictionary<User, PeopleUserDrawing> UserSubmittedDrawingsByUser { get; set; } = new Dictionary<User, PeopleUserDrawing>();
    }
}
