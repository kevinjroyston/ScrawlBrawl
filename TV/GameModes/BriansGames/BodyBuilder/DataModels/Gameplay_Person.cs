
using RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels;


namespace RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels
{
    public class Gameplay_Person:Person
    {
        public bool DoneWithRound { get; set; }
        protected override string UnityImageIdentifier { get { return FinishedPosition; } }
        public string FinishedPosition { get; set; }
    }
}
