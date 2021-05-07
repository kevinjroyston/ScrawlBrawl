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
  onGetLobby: () => void
  onNext: (data: Map<Lobby.GameDuration, number>) => void
}

@Component({
  selector: 'gamemode-dialog',
  templateUrl: './gamemode-dialog.component.html',
  styleUrls: ['./gamemode-dialog.component.scss']
})
export class GamemodeDialogComponent implements OnInit {
  lobby: Lobby.LobbyMetadata
  error: string
  onGetLobby: () => void
  onNext: (data: Map<Lobby.GameDuration, number>) =>void
  errorSubscription: Subscription

  constructor(
    private dialogRef: MatDialogRef<GamemodeDialogComponent>,
    public errorService: ErrorService,
    @Inject(GameModeList) public gameModeList: GameModeList,
    @Inject(MAT_DIALOG_DATA) public data: GameModeDialogData,
    @Inject(API) private api: API) {
    this.lobby = data.lobby;
    this.onGetLobby = data.onGetLobby;
    this.onNext = data.onNext;
    if (gameModeList.gameModes[this.lobby.selectedGameMode].options.length == 0){
      this.onGameModeNext();
    }
  }

  ngOnInit() {
    this.errorSubscription = this.errorService.errorObservable.subscribe((error) => {
      this.error = error;
    })
  }

  ngOnDestroy() {
    this.errorSubscription.unsubscribe();
  }

  onGameModeNext() : void {
    var body = new Lobby.ConfigureLobbyRequest();
    body.gameMode = this.lobby.selectedGameMode;
    body.options = JSON.parse(JSON.stringify(this.gameModeList.gameModes[this.lobby.selectedGameMode].options, ['value']));
    var bodyString = JSON.stringify(body);

    let configureRequest = this.api.request({ type: "Lobby", path: "Configure", body: bodyString });

    configureRequest.subscribe({
        next: (data) => {
          let response = data as Lobby.ConfigureLobbyResponse;
          this.closeDialog(); // needed?
          this.onNext(response.gameDurationEstimatesInMinutes);
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
