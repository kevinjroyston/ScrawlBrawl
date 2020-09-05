
using RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels;


namespace RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels
{
    public class Gameplay_Person: Person
    {
        public bool DoneWithRound { get; set; }
        public bool BeenScored { get; set; } = false;
        public string FinishedPosition { get; set; }
    }
}
