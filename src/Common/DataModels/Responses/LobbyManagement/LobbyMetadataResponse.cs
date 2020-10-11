namespace Common.DataModels.Responses
{
    public class LobbyMetadataResponse
    {
        public string LobbyId { get; set; }

        public int PlayerCount { get; set; }
        public bool GameInProgress { get; set; }

        //public GameModeMetadata GameModeSettings { get; set; }

        public int? SelectedGameMode { get; set; }

        public LobbyMetadataResponse()
        {
            //empty
        }
    }
}
