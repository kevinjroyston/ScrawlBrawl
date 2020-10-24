import { Component, OnInit, Inject} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import GameModes from '../../interfaces/gamemodes';

interface GameInfoDialogData {
  gameModes: GameModes.GameModeMetadata
  selectedGameMode: number
}

@Component({
  selector: 'app-game-info-dialog',
  templateUrl: './gameinfo-dialog.component.html',
  styleUrls: ['./gameinfo-dialog.component.scss']
})
export class GameInfoDialogComponent implements OnInit {
  gameModes: GameModes.GameModeMetadata
  selectedGameMode: number

  constructor(
    private dialogRef: MatDialogRef<GameInfoDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: GameInfoDialogData) {
    this.gameModes = data.gameModes;
    this.selectedGameMode = data.selectedGameMode;
  }

  ngOnInit() {
  }

  close() {
    this.dialogRef.close();
  }
}
