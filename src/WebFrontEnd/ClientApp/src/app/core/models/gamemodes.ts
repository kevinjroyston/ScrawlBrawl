namespace GameModes {
    export class GameModeOptionResponse {
        description: string;
        responseType: ResponseType;
        // Response
        value: any;
        minValue:number;
        maxValue:number;
    }
    
    export class GameModeMetadata {
        maxPlayers: number;
        title: string;
        gameIdString: string;
        description: string;
        options: GameModeOptionResponse[];
    }
}

export default GameModes
