import { Component, Inject, ViewChild} from '@angular/core';
import {Observable} from 'rxjs'
import { API } from '@core/http/api';
import { GameAssetDirective } from '@shared/components/gameassetdirective.component';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import {GamemodeDialogComponent} from '../../components/gamemode-dialog/gamemode-dialog.component';
import {GameInfoDialogComponent} from '../../components/gameinfo-dialog/gameinfo-dialog.component';
import {ErrorService} from '../../services/error.service'
import Lobby from '@core/models/lobby'
import GameModes from '@core/models/gamemodes'

@Component({
    selector: 'app-lobby-management',
    templateUrl: './lobby.component.html',
    styleUrls: ['./lobby.component.scss'],
})

export class LobbyManagementComponent {
    public lobby!: Lobby.LobbyMetadata;
    public gameModes!: GameModes.GameModeMetadata[];
    public error: string;
    @ViewChild(GameAssetDirective) gameAssetDirective;

    constructor(@Inject(API) private api: API, private matDialog: MatDialog, public errorService: ErrorService)
    {
        this.getGames().then(() => this.onGetLobby())
    }

    async onGetLobby() {
        this.api.request({ type: "Lobby", path: "Get" }).subscribe({
            next: async (result) => {
                this.lobby = result as Lobby.LobbyMetadata;
                
                if (this.lobby != null && this.lobby.selectedGameMode != null && this.lobby.gameModeSettings != null) {
                    this.lobby.gameModeSettings.options.forEach((value: GameModes.GameModeOptionResponse, index: number, array: GameModes.GameModeOptionResponse[]) => {
                        if (value != null && value.value != null) {
                            this.gameModes[this.lobby.selectedGameMode].options[index].value = value.value;
                        }
                    });
                }
            },
            error: () => { this.lobby = null;}
        });
    }

    onCreateLobby = async () => {
        await this.api.request({ type: "Lobby", path: "Create" }).subscribe({
            next: async (result) => {
                this.lobby = result as Lobby.LobbyMetadata;
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
            next: (result) => { this.gameModes = result as GameModes.GameModeMetadata[] }
        });
    }

    isGameModeSelected(game: number){
        return game === this.lobby.selectedGameMode;
    }

    onSelectGameMode(game: number) {
        this.lobby.selectedGameMode === game ? this.lobby.selectedGameMode = null : this.lobby.selectedGameMode = game
        if (this.lobby.selectedGameMode !== null) {
            this.openGameSettingsDialog()
        }
        this.error = null;
    }

    openGameInfoDialog = (event, game: number) => {
        const dialogConfig = new MatDialogConfig();
        dialogConfig.data = {
            gameModes: this.gameModes,
            selectedGameMode: game,
            proceedToGameSettings: () => this.proceedToGameSettings(game)
        }
        this.matDialog.open(GameInfoDialogComponent, dialogConfig);
        event.stopPropagation();
    }

    proceedToGameSettings = (game: number) => {
        this.lobby.selectedGameMode = game;
        this.openGameSettingsDialog()
    }

    openGameSettingsDialog = () => {
        const dialogConfig = new MatDialogConfig();
        dialogConfig.data = {
            gameModes: this.gameModes,
            lobby: this.lobby,
            onGetLobby: () => this.onGetLobby()
        }
        this.matDialog.open(GamemodeDialogComponent, dialogConfig);
    }

    refreshGameModes() {
        if (this.gameModes.length <= 0) {
            this.getGames();
        }
    }
}