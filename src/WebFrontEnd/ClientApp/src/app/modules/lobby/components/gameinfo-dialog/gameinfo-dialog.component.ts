import { Component, OnInit, Inject} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import GameModes from '../../interfaces/gamemodes';

interface GameInfoDialogData {
  gameModes: GameModes.GameModeMetadata
  selectedGameMode: number
  proceedToGameSettings: () => void
}

@Component({
  selector: 'app-game-info-dialog',
  templateUrl: './gameinfo-dialog.component.html',
  styleUrls: ['./gameinfo-dialog.component.scss']
})
export class GameInfoDialogComponent implements OnInit {
  gameModes: GameModes.GameModeMetadata
  selectedGameMode: number
  proceedToGameSettings: () => void

  constructor(
    private dialogRef: MatDialogRef<GameInfoDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: GameInfoDialogData) {
    this.gameModes = data.gameModes;
    this.selectedGameMode = data.selectedGameMode;
    this.proceedToGameSettings = data.proceedToGameSettings;
  }

  onProceedToSettings = () => {
    this.dialogRef.close();
    this.proceedToGameSettings()
  }

  ngOnInit() {
  }

  close() {
    this.dialogRef.close();
  }
}
