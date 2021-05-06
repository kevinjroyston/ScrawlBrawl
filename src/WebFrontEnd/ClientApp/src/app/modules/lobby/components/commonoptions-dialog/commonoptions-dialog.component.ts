import { Component, OnInit, Inject } from '@angular/core';
import {Subscription} from 'rxjs'
import { API } from '@core/http/api';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import Lobby from '@core/models/lobby';
import GameModes from '@core/models/gamemodes';
import { ErrorService } from '@modules/lobby/services/error.service';
import { GameModeList } from '@core/http/gamemodelist';

interface GameModeDialogData {
  lobby: Lobby.LobbyMetadata
  error: string
  onGetLobby: () => void,
  durationEstimates: Map<Lobby.GameDuration,number>,
}

@Component({
  selector: 'app-commonoptions-dialog',
  templateUrl: './commonoptions-dialog.component.html',
  styleUrls: ['./commonoptions-dialog.component.scss']
})
export class CommonoptionsDialogComponent implements OnInit {
  lobby: Lobby.LobbyMetadata
  error: string
  onGetLobby: () => void
  errorSubscription: Subscription
  showTutorial: boolean = true;
  timerEnabled: boolean = true;
  gameDuration: Lobby.GameDuration = Lobby.GameDuration.Normal;
  durationEstimates: Map<Lobby.GameDuration, number>;

  constructor(
    private dialogRef: MatDialogRef<CommonoptionsDialogComponent>,
    public errorService: ErrorService,
    @Inject(GameModeList) public gameModeList: GameModeList,
    @Inject(MAT_DIALOG_DATA) public data: GameModeDialogData,
    @Inject(API) private api: API) {
    this.lobby = data.lobby;
    this.onGetLobby = data.onGetLobby;
    this.durationEstimates = data.durationEstimates;
  }

  ngOnInit() {
    this.errorSubscription = this.errorService.errorObservable.subscribe((error) => {
      this.error = error;
    })
  }

  ngOnDestroy() {
    this.errorSubscription.unsubscribe();
  }

  onStart() : void {
    var body = new Lobby.StartLobbyRequest();
    body.gameDuration = this.gameDuration;
    body.showTutorial = this.showTutorial;
    body.timerEnabled = this.timerEnabled;
    var bodyString = JSON.stringify(body);
    let startLobbyRequest = this.api.request({ type: "Lobby", path: "Start", body: bodyString });

    startLobbyRequest.subscribe({
        next: () => {
          // TODO: open gameplay tab & viewer tab?
          this.closeDialog();
        },
        error: (error) => { 
            this.error = error.error; 
            this.errorService.announceError(error.error); 
            this.onGetLobby(); 
        }
    })
  }

  closeDialog() {
    this.dialogRef.close();
  }
}
