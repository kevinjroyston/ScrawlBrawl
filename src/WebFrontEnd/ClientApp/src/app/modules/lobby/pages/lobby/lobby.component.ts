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
import { CommonoptionsDialogComponent } from '@modules/lobby/components/commonoptions-dialog/commonoptions-dialog.component';
import { NotificationService } from '@core/services/notification.service';

@Component({
    selector: 'app-lobby-management',
    templateUrl: './lobby.component.html',
    styleUrls: ['./lobby.component.scss'],
})

export class LobbyManagementComponent {
    public lobby!: Lobby.LobbyMetadata;
    public userLobby!: Lobby.LobbyMetadata; /* user has joined this lobby, but user is NOT lobby owner */
    public error: string;
    public inAGame: boolean = false;

    private ownerIsPlayingGame = false;

    @ViewChild(GameAssetDirective) gameAssetDirective;

    constructor(@Inject(UnityViewer) private unityViewer:UnityViewer, @Inject(GameModeList) public gameModeList: GameModeList, @Inject(API) private api: API, 
    @Inject('BASE_FRONTEND_URL') private baseFrontEndUrl: string,private matDialog: MatDialog, public errorService: ErrorService, private router: Router,
    @Inject(NotificationService) private notificationService: NotificationService)
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
                setTimeout(() => window.scrollTo(0, 0), 200);
            },
            error: () => { 
                this.lobby = null;
                this.onGetUserLobby()
                document.body.classList.remove('makeRoomForToolbar');
                setTimeout(() => window.scrollTo(0, 0), 200);
            }
        });
    }

    async onGetUserLobby() {
        this.api.request({ type: "User", path: "GetLobby" }).subscribe({
            next: async (result) => {
                this.userLobby = result as Lobby.LobbyMetadata;
                this.inAGame = (this.userLobby != null);
            },
            error: () => { this.inAGame = false }
        });
    }

    putViewerLinkOnClipboard(){
        navigator.clipboard.writeText(this.baseFrontEndUrl+"viewer/index.html?lobby="+this.lobby.lobbyId)
            .then(()=>{
                this.notificationService.addMessage("The link is on the clipboard.", null, {panelClass: ['success-snackbar']});
            })
            .catch(e => console.error(e));
    }

    launchJoinLobbyPage() {
        this.router.navigate(['/join'],{queryParams: { lobby:this.lobby.lobbyId}})
    }
    
    putGameLinkOnClipboard(){
        navigator.clipboard.writeText(this.baseFrontEndUrl+"join?lobby="+this.lobby.lobbyId)
        .then(()=>{
            this.notificationService.addMessage("The link is on the clipboard.", null, {panelClass: ['success-snackbar']});
        })
            .catch(e => console.error(e));
    }

    onCreateLobby = async () => {
        await this.api.request({ type: "Lobby", path: "Create"}).subscribe({
            next: async (result) => {
                this.ownerIsPlayingGame = false;
                this.lobby = result as Lobby.LobbyMetadata;
                await this.onGetLobby()
            },
            error: async (error) => { this.error = error.error; await this.onGetLobby(); }
        });
    }

    onCreateAndJoinLobby = async (joinLobbyRequest) => {
        var bodyString = JSON.stringify(joinLobbyRequest);
        await this.api.request({ type: "Lobby", path: "CreateAndJoin", body: bodyString}).subscribe({
            next: async (result) => {
                this.ownerIsPlayingGame = true;
                this.lobby = result as Lobby.LobbyMetadata;
                await this.onGetLobby()
            },
            error: async (error) => { this.error = error.error; await this.onGetLobby(); }
        });
    }

    async onDeleteUser() {
        await this.api.request({ type: "User", path: "Delete" }).subscribe({
            next: async data => {
                console.log("Left lobby");
                this.userLobby = null;
                this.inAGame = false;
            },
            error: async (error) => {
                console.error(error);
            }
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
        this.lobby.selectedGameMode = game;        
        this.openGameSettingsDialog();
        this.error = null;
    }

    onRerouteToPlay(){
        this.router.navigate(['/join'])
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
            onGetLobby: () => this.onGetLobby(),
            onNext: (data: Map<Lobby.GameDuration, number>) => this.openGameStartDialog(data)
        }
        this.matDialog.open(GamemodeDialogComponent, dialogConfig);
    }
    openGameStartDialog = (gameDurationEstimatesInMinutes: Map<Lobby.GameDuration, number>) => {
        const dialogConfig = new MatDialogConfig();
        dialogConfig.data = {
            lobby: this.lobby,
            onGetLobby: () => this.onGetLobby(),
            durationEstimates: gameDurationEstimatesInMinutes,
            launchURL: this.ownerIsPlayingGame?this.baseFrontEndUrl+"play":"",
        }
        this.matDialog.open(CommonoptionsDialogComponent, dialogConfig);
    }

    showInstructions = () => {
        console.log('hello')
        this.matDialog.open(LobbyInstructionsDialogComponent)
    }
    refreshGameModes() {}
}