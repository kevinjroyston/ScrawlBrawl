import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import Lobby from '../../interfaces/lobby';
import GameModes from '../../interfaces/gamemodes';

interface GameModeDialogData {
  gameModes: GameModes.GameModeMetadata
  lobby: Lobby.LobbyMetadata
  error: string
  onStart: () => void
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
  onStart: () => void

  constructor(
    private dialogRef: MatDialogRef<GamemodeDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: GameModeDialogData) {
    this.gameModes = data.gameModes;
    this.lobby = data.lobby;
    this.error = data.error;
    this.onStart = data.onStart;
  }

  onStartLobby = () => {
    this.onStart();
  }

  ngOnInit() {
  }

  close() {
    this.dialogRef.close();
  }
}
