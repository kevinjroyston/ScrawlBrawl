
using Backend.Games.Common.ThreePartPeople.DataModels;


namespace Backend.Games.BriansGames.BodyBuilder.DataModels
{
    public class Gameplay_Person: Person
    {
        public bool DoneWithRound { get; set; }
        public bool BeenScored { get; set; } = false;
        public string FinishedPosition { get; set; }
    }
}
