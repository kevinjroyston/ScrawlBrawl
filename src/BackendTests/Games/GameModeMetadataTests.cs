using Backend.GameInfrastructure.DataModels;
using Common.DataModels.Enums;
using Common.DataModels.Requests.LobbyManagement;
using Common.DataModels.Responses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackendTests.Games
{
    [TestClass]
    public class GameModeMetadataTests
    {
        // Not an actual test. Just useful for tweaking game durations.
        [TestMethod]
        public void PrintGameDurationEstimatesToConsole()
        {
            var moqEnv = new Moq.Mock<IWebHostEnvironment>();
            moqEnv.Setup(env => env.EnvironmentName).Returns("Microsoft.Extensions.Hosting.EnvironmentName.Production");
            InMemoryConfiguration inMemoryConfig = new InMemoryConfiguration(moqEnv.Object);
            inMemoryConfig.UpdateConfiguration();

            foreach (GameModeMetadataHolder holder in inMemoryConfig.GameModes)
            {
                GameModeMetadata metadata = holder.GameModeMetadata;
                var defaultParams = metadata.Options.Select(option =>
                {
                    var optionRequest = new ConfigureLobbyRequest.GameModeOptionRequest
                    {
                        Value = option.DefaultValue.ToString()
                    };
                    optionRequest.ParseValue(option, out string err);
                    return optionRequest;
                }).ToList();

                Console.WriteLine($"GameMode: {metadata.GameIdString}");
                for (int i = metadata.MinPlayers.Value; i < 40; i++)
                {
                    var estimates = metadata.GetGameDurationEstimates(i, defaultParams);
                    Console.WriteLine($"Players:{i} \ts:{estimates[GameDuration.Short].TotalMinutes},\t m:{estimates[GameDuration.Normal].TotalMinutes},\t l:{estimates[GameDuration.Extended].TotalMinutes}");
                }
            }
        }
    }
}
