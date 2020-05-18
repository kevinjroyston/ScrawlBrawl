using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels;
using System.Collections.Generic;

namespace RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels
{
    public class Setup_Person: Person
    {
        public Dictionary<User, PeopleUserDrawing> UserSubmittedDrawingsByUser { get; set; } = new Dictionary<User, PeopleUserDrawing>();
    }
}
