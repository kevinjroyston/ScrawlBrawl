using Microsoft.Extensions.Hosting;
using Backend.Games.BriansGames.BattleReady;
using Backend.Games.BriansGames.BodyBuilder;
using Backend.Games.BriansGames.TwoToneDrawing;
using System.Collections.Generic;
using Backend.Games.KevinsGames.Mimic;
using Backend.Games.TimsGames.FriendQuiz;
using Backend.Games.BriansGames.ImposterDrawing;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Backend.Games.BriansGames.HintHint;

namespace Backend.GameInfrastructure.DataModels
{
    public class InMemoryConfiguration
    {
        private IWebHostEnvironment Environment { get; }
        #region GameModes

        private IReadOnlyList<GameModeMetadataHolder> InternalGameModes { get; } = new List<GameModeMetadataHolder>
        {
            #region Imposter Syndrome (OLD Removed)
            /*
            new GameModeMetadata
            {
                Title = "Imposter Syndrome",
                Description = "Come up with a difference only you'll be able to spot!",
                MinPlayers = 4,
                MaxPlayers = null,
                GameModeInstantiator = (lobby, options) => new OneOfTheseThingsIsNotLikeTheOtherOneGameMode(lobby, options),
                Options = new List<GameModeOptionResponse>
                {
                    new GameModeOptionResponse
                    {
                        Description = "Max total drawings per player.",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 6,
                        MinValue = 3
                    },
                }
            },*/
            #endregion
            #region Imposter Syndrome Text (Removed)
            /*new GameModeMetadata
            {
                Title = "Imposter Syndrome (Text)",
                Description = "Come up with a difference only you'll be able to spot!",
                MinPlayers = 4,
                MaxPlayers = null,
                GameModeInstantiator = (lobby, options) => new ImposterTextGameMode(lobby, options),
                Options = new List<GameModeOptionResponse>
                {
                    new GameModeOptionResponse
                    {
                        Description = "Length of the game (10 for longest 1 for shortest 0 for no timer)",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 5,
                        MinValue = 0,
                        MaxValue = 10,
                    }
                }
            },*/
            #endregion
            new GameModeMetadataHolder()
            {
                GameModeMetadata = ImposterDrawingGameMode.GameModeMetadata,
                GameModeInstantiator = (lobby, options) => new ImposterDrawingGameMode(lobby, options)
            },
            new GameModeMetadataHolder()
            {
                GameModeMetadata = TwoToneDrawingGameMode.GameModeMetadata,
                GameModeInstantiator = (lobby, options) => new TwoToneDrawingGameMode(lobby, options)
            },
            new GameModeMetadataHolder()
            {
                GameModeMetadata = BodyBuilderGameMode.GameModeMetadata,
                GameModeInstantiator = (lobby, options) => new BodyBuilderGameMode(lobby, options)
            },
            new GameModeMetadataHolder()
            {
                GameModeMetadata = BattleReadyGameMode.GameModeMetadata,
                GameModeInstantiator = (lobby, options) => new BattleReadyGameMode(lobby, options)
            },
            new GameModeMetadataHolder()
            {
                GameModeMetadata = MimicGameMode.GameModeMetadata,
                GameModeInstantiator = (lobby, options) => new MimicGameMode(lobby, options)
            },
            new GameModeMetadataHolder()
            {
                GameModeMetadata = FriendQuizGameMode.GameModeMetadata,
                GameModeInstantiator = (lobby, options) => new FriendQuizGameMode(lobby, options)
            },
            new GameModeMetadataHolder()
            {
                GameModeMetadata = HintGameMode.GameModeMetadata,
                GameModeInstantiator = (lobby, options) => new HintGameMode(lobby, options)
            }
            
            #region StoryTime (Removed)
            /*new GameModeMetadata
            {
                Title = "StoryTime",
                Description = "Work together to make the best story that fits set of rapidly changing genres",
                MinPlayers = 3,
                MaxPlayers = null,
                GameModeInstantiator = (lobby, options) => new StoryTimeGameMode(lobby, options),
                Options = new List<GameModeOptionResponse>
                {
                    new GameModeOptionResponse
                    {
                        Description = "Number of players asked to write each round",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 2,
                        MinValue = 2,
                        MaxValue = 30,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Number of rounds",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 10,
                        MinValue = 2,
                        MaxValue = 60,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "length of timer for writing",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 45,
                        MinValue = 10,
                        MaxValue = 120,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "length of timer for voting",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 30,
                        MinValue = 5,
                        MaxValue = 60,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "character limit for sentences",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 100,
                        MinValue = 50,
                        MaxValue = 200,
                    },
                }
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
