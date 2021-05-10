import GameModes from './gamemodes'

namespace Lobby {
    export class LobbyMetadata {
        lobbyId: string;
        isGameInProgress: boolean;
        playerCount: number;
        gameModeSettings: GameModes.GameModeMetadata;
        selectedGameMode: number;
    }

    export class ConfigureLobbyRequest {
        gameMode: number;
        // Response object when serialized only contains the value.
        options: GameModes.GameModeOptionResponse[];
    }
    export class ConfigureLobbyResponse {
        lobbyId: string;
        playerCount: number;
        gameDurationEstimatesInMinutes: Map<GameDuration, number>;
    }

    export enum GameDuration{        
        Short,
        Normal,
        Extended,
    }

    export class StartLobbyRequest {
        showTutorial: boolean;
        gameDuration: GameDuration;
        timerEnabled: boolean;
    }
}

export default Lobby