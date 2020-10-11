using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.Common.ThreePartPeople.DataModels;
using System.Collections.Generic;

namespace Backend.Games.BriansGames.BodyBuilder.DataModels
{
    public class Setup_Person: Person
    {
        public Dictionary<User, PeopleUserDrawing> UserSubmittedDrawingsByUser { get; set; } = new Dictionary<User, PeopleUserDrawing>();
    }
}
