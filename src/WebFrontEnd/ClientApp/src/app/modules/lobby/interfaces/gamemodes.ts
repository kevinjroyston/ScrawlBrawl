namespace GameModes {
    export class GameModeOptionResponse {
        description: string;
        responseType: ResponseType;
        // Response
        value: any;
    }
    
    export class GameModeMetadata {
        minPlayers: number;
        maxPlayers: number;
        title: string;
        gameIdString: string;
        description: string;
        options: GameModeOptionResponse[];
    }
}

export default GameModes
