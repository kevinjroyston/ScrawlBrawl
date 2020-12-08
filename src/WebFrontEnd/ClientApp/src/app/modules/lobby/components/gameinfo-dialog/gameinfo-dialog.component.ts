import { Component, OnInit, Inject} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { GameModeList } from '@core/http/gamemodelist';

interface GameInfoDialogData {
  selectedGameMode: number
  proceedToGameSettings: () => void
}

@Component({
  selector: 'app-game-info-dialog',
  templateUrl: './gameinfo-dialog.component.html',
  styleUrls: ['./gameinfo-dialog.component.scss']
})
export class GameInfoDialogComponent implements OnInit {
  selectedGameMode: number
  proceedToGameSettings: () => void

  constructor(
    private dialogRef: MatDialogRef<GameInfoDialogComponent>,
    @Inject(GameModeList) public gameModeList: GameModeList,
    @Inject(MAT_DIALOG_DATA) public data: GameInfoDialogData) {
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
