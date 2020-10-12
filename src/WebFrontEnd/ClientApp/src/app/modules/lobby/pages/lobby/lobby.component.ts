import { Component, Inject, ViewEncapsulation } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { API } from '@core/http/api';
import { NgForm } from '@angular/forms';
import { BroadcastService, MsalService } from '@azure/msal-angular';

@Component({
    selector: 'app-lobby-management',
    templateUrl: './lobby.component.html',
    styleUrls: ['./lobby.component.css'],

    encapsulation: ViewEncapsulation.None
})

export class LobbyManagementComponent {
    public api: API;
    public lobby!: LobbyMetadata;
    public gameModes!: GameModeMetadata[];
    public error: string = "";
    public gameModeCollapse = false;

    constructor(
        private http: HttpClient,
        @Inject('userId') userId: string,
        @Inject(MsalService) authService: MsalService,
        @Inject(BroadcastService) broadcastService: BroadcastService
    )
    {
        this.api = new API(http, userId, authService, broadcastService)
        this.getGames().then(()=>this.onGetLobby())
    }

    async onStartLobby() {
        var body = new ConfigureLobbyRequest();
        body.gameMode = this.lobby.selectedGameMode;
        body.options = JSON.parse(JSON.stringify(this.gameModes[this.lobby.selectedGameMode].options, ['value']));
        var bodyString = JSON.stringify(body);
        console.log(bodyString);
        this.error = "";
        await this.api.request({ type: "Lobby", path: "Configure", body: bodyString }).subscribe({
            next: async () => {
                if (this.error == "") {
                    await this.api.request({ type: "Lobby", path: "Start" }).subscribe({
                        next: async () => { await this.onGetLobby() },
                        error: async (error) => { this.error = error.error; await this.onGetLobby() }
                    })
                }
            },
            error: async (error) => { this.error = error.error; await this.onGetLobby(); }
        })
    }

    async onGetLobby() {
        this.api.request({ type: "Lobby", path: "Get" }).subscribe({
            next: async (result) => {
                this.lobby = result as LobbyMetadata;
                if (this.lobby != null && this.lobby.selectedGameMode != null && this.lobby.gameModeSettings != null) {
                    this.lobby.gameModeSettings.options.forEach((value: GameModeOptionResponse, index: number, array: GameModeOptionResponse[]) => {
                        if (value != null && value.value != null) {
                            this.gameModes[this.lobby.selectedGameMode].options[index].value = value.value;
                        }
                    });
                }
            },
            error: () => { this.lobby = null; }
        });
    }

    async onCreateLobby() {
        await this.api.request({ type: "Lobby", path: "Create" }).subscribe({
            next: async (result) => {
                this.lobby = result as LobbyMetadata;
                await this.onGetLobby()
            },
            error: async (error) => { this.error = error.error; await this.onGetLobby(); }
        });
    }

    async onDeleteLobby() {
        await this.api.request({ type: "Lobby", path: "Delete" }).subscribe({
            next: async () => { await this.onGetLobby() },
            error: async (error) => { this.error = error.error; await this.onGetLobby(); }
        });
    }

    async getGames() {
        await this.api.request({ type: "Lobby", path: "Games" }).subscribe({
            next: (result) => { this.gameModes = result as GameModeMetadata[] }
        });
    }

    onSelectGameMode(game: number) {
        this.lobby.selectedGameMode = game;
        this.gameModeCollapse = true;
        this.error = "";
    }
    refreshGameModes() {
        if (this.gameModes.length <= 0) {
            this.getGames();
        }
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
