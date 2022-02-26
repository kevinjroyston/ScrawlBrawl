using Microsoft.Extensions.Hosting;
using Backend.Games.BriansGames.BattleReady;
using Backend.Games.BriansGames.TwoToneDrawing;
using System.Collections.Generic;
using Backend.Games.KevinsGames.Mimic;
using Backend.Games.TimsGames.FriendQuiz;
using Backend.Games.BriansGames.ImposterDrawing;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Backend.Games.KevinsGames.TextBodyBuilder.Game;
using Backend.Games.KevinsGames.LateToArtClass;

namespace Backend.GameInfrastructure.DataModels
{
    public class InMemoryConfiguration
    {
        private IWebHostEnvironment Environment { get; }
        #region GameModes

        private IReadOnlyList<GameModeMetadataHolder> InternalGameModes { get; } = new List<GameModeMetadataHolder>
        {
            new GameModeMetadataHolder()
            {
                GameModeMetadata = ImposterDrawingGameMode.GameModeMetadata,
                GameModeInstantiator = (lobby, options, standardOptions) => new ImposterDrawingGameMode(lobby, options, standardOptions)
            },
            new GameModeMetadataHolder()
            {
                GameModeMetadata = TwoToneDrawingGameMode.GameModeMetadata,
                GameModeInstantiator = (lobby, options, standardOptions) => new TwoToneDrawingGameMode(lobby, options, standardOptions)
            },
            new GameModeMetadataHolder()
            {
                GameModeMetadata = BattleReadyGameMode.GameModeMetadata,
                GameModeInstantiator = (lobby, options, standardOptions) => new BattleReadyGameMode(lobby, options, standardOptions)
            },
            new GameModeMetadataHolder()
            {
                GameModeMetadata = MimicGameMode.GameModeMetadata,
                GameModeInstantiator = (lobby, options, standardOptions) => new MimicGameMode(lobby, options, standardOptions)
            },
            new GameModeMetadataHolder()
            {
                GameModeMetadata = FriendQuizGameMode.GameModeMetadata,
                GameModeInstantiator = (lobby, options, standardOptions) => new FriendQuizGameMode(lobby, options, standardOptions)
            },
            new GameModeMetadataHolder()
            {
                GameModeMetadata = TextBodyBuilderGameMode.GameModeMetadata,
                GameModeInstantiator = (lobby, options, standardOptions) => new TextBodyBuilderGameMode(lobby, options, standardOptions)
            },
            new GameModeMetadataHolder()
            {
                GameModeMetadata = LateToArtClassGameMode.GameModeMetadata,
                GameModeInstantiator = (lobby, options, standardOptions) => new LateToArtClassGameMode(lobby, options, standardOptions)
            },
            
            #region BodyBuilder (Removed)
            /*new GameModeMetadataHolder()
            {
                GameModeMetadata = BodyBuilderGameMode.GameModeMetadata,
                GameModeInstantiator = (lobby, options) => new BodyBuilderGameMode(lobby, options)
            },*/
            #endregion
        }.AsReadOnly();
        public IReadOnlyList<GameModeMetadataHolder> GameModes { get; private set; }
        #endregion
        public InMemoryConfiguration(IWebHostEnvironment env)
        {
            this.Environment = env;
            this.UpdateConfiguration();
        }
    public void UpdateConfiguration()
        {
            this.GameModes = this.InternalGameModes.Where(e => e.GameModeMetadata.Attributes.ProductionReady || !this.Environment.IsProduction()).ToList().AsReadOnly();

        }
    }
}
