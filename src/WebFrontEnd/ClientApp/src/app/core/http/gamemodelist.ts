
import { Inject, Injectable } from '@angular/core';
import { API } from '@core/http/api';
import GameModes from '@core/models/gamemodes'

//Used to pull down items that are not expected to change during the session (game assets, game list, etc)

@Injectable({
    providedIn: 'root'
})
export class GameModeList {
    private _gameModes: ReadonlyArray<GameModes.GameModeMetadata>=[];
    public get gameModes():ReadonlyArray<GameModes.GameModeMetadata> {return this._gameModes}

    gameModeIndexFromGameId(gameId):number{
        let result = -1;
        this._gameModes.forEach(function (gameMode,index){if (gameMode.gameIdString==gameId){result=index; return}})
        return result;
    }

    gameModeFromGameId(gameId):GameModes.GameModeMetadata{
        let index = this.gameModeIndexFromGameId(gameId);
        if (index < 0) {return null}
        return this._gameModes[index];
    }

    constructor(@Inject(API) private api: API) { 
        this.getGames();
         }


    getGames = async () => {
            await this.api.request({ type: "Lobby", path: "Games" }).subscribe({
            next: (result) => { this._gameModes = result as GameModes.GameModeMetadata[]; }
        });
    }    
}