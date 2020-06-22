import { Component, Inject, ViewEncapsulation } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { API } from '../api';
import { NgForm } from '@angular/forms';

@Component({
    selector: 'app-lobby-management',
    templateUrl: './lobby-management.component.html',
    styleUrls: ['./lobby-management.component.css'],

    encapsulation: ViewEncapsulation.None
})

export class LobbyManagementComponent {
    public api: API;
    public baseUrl: string;
    public lobby!: LobbyMetadata;
    public gameModes!: GameModeMetadata[];
    public error: string = "";
    public gameModeCollapse = false;

    constructor(
        private http: HttpClient,
        @Inject('BASE_URL') baseUrl: string) {
        this.baseUrl = baseUrl;
        this.api = new API(http, baseUrl)
        this.getGames().then(()=>this.onGetLobby())
    }

    async onConfigure() {
        // TODO: clean up promise/move .then usage to api.ts
        // TODO: configuration error handling
        var body = new ConfigureLobbyRequest();
        body.gameMode = this.lobby.selectedGameMode;
        body.options = JSON.parse(JSON.stringify(this.gameModes[this.lobby.selectedGameMode].options, ['value']));
        var bodyString = JSON.stringify(body); 
        console.log(bodyString);
        this.error = "";
        await this.api.request({ type: "Lobby", path: "Configure", body: bodyString })
            .catch(error => this.error = error.error)
            .then(async () =>
            {
                if (this.error == "") {
                    await this.onGetLobby()
                }
            });
    }

    async onStartLobby() {
        await this.onConfigure().then(async () => {
            if (this.error == "") {
                await this.api.request({ type: "Lobby", path: "Start" })
                    .catch(error => this.error = error.error)
                    .then(async () => {
                        if (this.error == "") {
                            await this.onGetLobby()
                        }
                    });
            }
        });
    }

    async onGetLobby() {
        await this.api.request({ type: "Lobby", path: "Get" })
            .catch(() => {this.lobby = null})
            .then((result) =>
            {
                this.lobby = result;
                if (this.lobby != null && this.lobby.selectedGameMode != null && this.lobby.gameModeSettings != null)
                {
                    this.lobby.gameModeSettings.options.forEach((value: GameModeOptionResponse, index: number, array: GameModeOptionResponse[]) => {
                        if (value != null && value.value != null) {
                            this.gameModes[this.lobby.selectedGameMode].options[index].value = value.value;
                        }
                    });
                }
            });
    }

    async onCreateLobby() {
        await this.api.request({ type: "Lobby", path: "Create" }).then(async (result) => {
            this.lobby = result;
            console.log(`current lobby: ${this.lobby}`);
            await this.onGetLobby();
        });
    }

    async onDeleteLobby() {
        await this.api.request({ type: "Lobby", path: "Delete" }).then(async (result) => {
            await this.onGetLobby();
        });
    }

    async getGames() {
        await this.api.request({ type: "Lobby", path: "Games" }).then((result) =>
        {
            this.gameModes = result
        });
    }

    onSelectGameMode(game: number) {
        this.lobby.selectedGameMode = game;
        this.gameModeCollapse = true;
        this.error = "";
    }
}

enum ResponseType {
    Boolean = 0,
    Integer,
    Text,
}
class LobbyMetadata {
    lobbyId: string;
    isGameInProgress: boolean;
    playerCount: number;
    gameModeSettings: GameModeMetadata;
    selectedGameMode: number;
}
class ConfigureLobbyRequest {
    gameMode: number;
    // Response object when serialized only contains the value.
    options: GameModeOptionResponse[];
}
class GameModeOptionResponse {
    description: string;
    responseType: ResponseType;
    // Response
    value: any;
}
class GameModeMetadata {
    minPlayers: number;
    maxPlayers: number;
    title: string;
    description: string;
    options: GameModeOptionResponse[];
}
