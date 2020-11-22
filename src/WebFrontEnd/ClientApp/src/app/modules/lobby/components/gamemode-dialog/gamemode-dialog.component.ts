import { Component, OnInit, Inject } from '@angular/core';
import {Subscription} from 'rxjs'
import { API } from '@core/http/api';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import Lobby from '@core/models/lobby';
import GameModes from '@core/models/gamemodes';
import { ErrorService } from '@modules/lobby/services/error.service';

interface GameModeDialogData {
  gameModes: GameModes.GameModeMetadata
  lobby: Lobby.LobbyMetadata
  error: string
  onGetLobby: () => void
}

@Component({
  selector: 'gamemode-dialog',
  templateUrl: './gamemode-dialog.component.html',
  styleUrls: ['./gamemode-dialog.component.scss']
})
export class GamemodeDialogComponent implements OnInit {
  gameModes: GameModes.GameModeMetadata
  lobby: Lobby.LobbyMetadata
  error: string
  onGetLobby: () => void
  errorSubscription: Subscription

  constructor(
    private dialogRef: MatDialogRef<GamemodeDialogComponent>,
    public errorService: ErrorService,
    @Inject(MAT_DIALOG_DATA) public data: GameModeDialogData,
    @Inject(API) private api: API) {
    this.gameModes = data.gameModes;
    this.lobby = data.lobby;
    this.onGetLobby = data.onGetLobby;
  }

  ngOnInit() {
    this.errorSubscription = this.errorService.errorObservable.subscribe((error) => {
      this.error = error;
    })
  }

  ngOnDestroy() {
    this.errorSubscription.unsubscribe();
  }

  onStartLobby() : void {
    var body = new Lobby.ConfigureLobbyRequest();
    body.gameMode = this.lobby.selectedGameMode;
    body.options = JSON.parse(JSON.stringify(this.gameModes[this.lobby.selectedGameMode].options, ['value']));
    var bodyString = JSON.stringify(body);

    let configureRequest = this.api.request({ type: "Lobby", path: "Configure", body: bodyString });
    let lobbyRequest = this.api.request({ type: "Lobby", path: "Start" });

    configureRequest.subscribe({
        next: () => {
            lobbyRequest.subscribe({
                next: () => { 
                    this.onGetLobby()
                    this.closeDialog();
                },
                error: (error) => { 
                    this.error = error.error; 
                    this.errorService.announceError(error.error); 
                    this.onGetLobby() 
                }
            })
        },
        error: (error) => { 
            this.error = error.error; 
            this.onGetLobby(); 
        }
    })
  }

  closeDialog() {
    this.dialogRef.close();
  }
}
