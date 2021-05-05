import { Component, Inject, ViewChild} from '@angular/core';
import {Observable} from 'rxjs'
import { API } from '@core/http/api';
import { GameAssetDirective } from '@shared/components/gameassetdirective.component';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import {GamemodeDialogComponent} from '../../components/gamemode-dialog/gamemode-dialog.component';
import {GameInfoDialogComponent} from '../../components/gameinfo-dialog/gameinfo-dialog.component';
import {LobbyInstructionsDialogComponent} from '../../components/lobbyinstructions-dialog/lobbyinstructions-dialog.component';
import {ErrorService} from '../../services/error.service'
import { Router, ActivatedRoute } from '@angular/router'; 
import Lobby from '@core/models/lobby'
import GameModes from '@core/models/gamemodes'
import { GameModeList } from '@core/http/gamemodelist';
import { UnityComponent } from '@shared/components/unity/unity.component';
import { UnityViewer } from '@core/http/viewerInjectable';

@Component({
    selector: 'app-lobby-management',
    templateUrl: './lobby.component.html',
    styleUrls: ['./lobby.component.scss'],
})

export class LobbyManagementComponent {
    public lobby!: Lobby.LobbyMetadata;
    public error: string;
    @ViewChild(GameAssetDirective) gameAssetDirective;

    constructor(@Inject(UnityViewer) private unityViewer:UnityViewer, @Inject(GameModeList) public gameModeList: GameModeList, @Inject(API) private api: API, 
    @Inject('BASE_FRONTEND_URL') private baseFrontEndUrl: string,private matDialog: MatDialog, public errorService: ErrorService, private router: Router)
    {
/*        this.getGames().then(() => this.onGetLobby()) */
        this.onGetLobby()
    }

    async onGetLobby() {
        this.api.request({ type: "Lobby", path: "Get" }).subscribe({
            next: async (result) => {
                this.lobby = result as Lobby.LobbyMetadata;
                if (this.lobby != null) {
                    this.unityViewer.UpdateLobbyId(this.lobby.lobbyId);
                }
                
                if (this.lobby != null && this.lobby.selectedGameMode != null && this.lobby.gameModeSettings != null) {
                    this.lobby.gameModeSettings.options.forEach((value: GameModes.GameModeOptionResponse, index: number, array: GameModes.GameModeOptionResponse[]) => {
                        if (value != null && value.value != null) {
                            this.gameModeList.gameModes[this.lobby.selectedGameMode].options[index].value = value.value;
                        }
                    });
                }
            },
            error: () => { this.lobby = null;}
        });
    }

    putViewerLinkOnClipboard(){
        navigator.clipboard.writeText(this.baseFrontEndUrl+"viewer/index.html?lobby="+this.lobby.lobbyId)
            .then(()=>{alert("The link is on the clipboard.")})
            .catch(e => console.error(e));
    }

    launchJoinLobbyPage() {
        this.router.navigate(['/join'],{queryParams: { lobby:this.lobby.lobbyId}})
    }
    
    putGameLinkOnClipboard(){
        navigator.clipboard.writeText(this.baseFrontEndUrl+"play?lobby="+this.lobby.lobbyId)
            .then(()=>{alert("The link is on the clipboard.")})
            .catch(e => console.error(e));
    }

    launchViewer(){
        window.open('/viewer/index.html?lobby='+this.lobby.lobbyId,'_blank');
    }
    
    onCreateLobby = async (joinLobbyRequest) => {
        var bodyString = JSON.stringify(joinLobbyRequest);
        await this.api.request({ type: "Lobby", path: "Create", body: bodyString}).subscribe({
            next: async (result) => {
                this.lobby = result as Lobby.LobbyMetadata;
                await this.onGetLobby()
            },
            error: async (error) => { this.error = error.error; await this.onGetLobby(); }
        });
    }

    async onDeleteLobby() {
        if (!confirm("Are you sure you want to disband this lobby?")) return false;
        await this.api.request({ type: "Lobby", path: "Delete" }).subscribe({
            next: async () => { await this.onGetLobby() },
            error: async (error) => { this.error = error.error; await this.onGetLobby(); }
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
            selectedGameModeId: this.gameModeList.gameModes[game].gameIdString,
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
            lobby: this.lobby,
            onGetLobby: () => this.onGetLobby()
        }
        this.matDialog.open(GamemodeDialogComponent, dialogConfig);
    }

    showInstructions = () => {
        console.log('hello')
        this.matDialog.open(LobbyInstructionsDialogComponent)
    }
    refreshGameModes() {}
}