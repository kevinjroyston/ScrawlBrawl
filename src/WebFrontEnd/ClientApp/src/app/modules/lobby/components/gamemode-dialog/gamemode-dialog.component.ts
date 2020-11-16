import { Component, OnInit, Inject } from '@angular/core';
import {Subscription} from 'rxjs'
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import Lobby from '@core/models/lobby';
import GameModes from '@core/models/gamemodes';
import {Observable} from 'rxjs'
import { ErrorService } from '@modules/lobby/services/error.service';

interface GameModeDialogData {
  gameModes: GameModes.GameModeMetadata
  lobby: Lobby.LobbyMetadata
  error: string
  onStart: () => Observable<any>
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
  errorSubscription: Subscription
  onStart: () => Observable<any>

  constructor(
    private dialogRef: MatDialogRef<GamemodeDialogComponent>,
    public errorService: ErrorService,
    @Inject(MAT_DIALOG_DATA) public data: GameModeDialogData) {
    this.gameModes = data.gameModes;
    this.lobby = data.lobby;
    this.onStart = data.onStart;
  }

  ngOnInit() {
    this.errorSubscription = this.errorService.errorObservable.subscribe((error) => {
      this.error = error;
    })
  }

  ngOnDestroy() {
    this.errorSubscription.unsubscribe();
  }

  onStartLobby = () => {
    let request = this.onStart();
    request.subscribe({
      next: () => {
        this.closeDialog();
      }
    })
  }

  closeDialog() {
    this.dialogRef.close();
  }
}
