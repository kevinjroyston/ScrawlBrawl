using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.StateGroups;
using RoystonGame.TV.GameModes.TimsGames.FriendQuiz.DataModels;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.TimsGames.FriendQuiz
{
    public class FriendQuizGameMode: IGameMode
    {
        private Random Rand { get; } = new Random();
        private GameState Setup { get; set; }
        public FriendQuizGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            int maxQuestions = (int)gameModeOptions[(int)GameModeOptionsEnum.AnswerTimerLength].ValueParsed;
            float setupTimerLength = (int)gameModeOptions[(int)GameModeOptionsEnum.AnswerTimerLength].ValueParsed;
            float answerTimerLength = (int)gameModeOptions[(int)GameModeOptionsEnum.AnswerTimerLength].ValueParsed;
            float votingTimerLength = (int)gameModeOptions[(int)GameModeOptionsEnum.AnswerTimerLength].ValueParsed;
            
        }
    }
}
