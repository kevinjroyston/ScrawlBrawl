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
}

export default Lobby