import { Component, OnInit, Inject} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { GameModeList } from '@core/http/gamemodelist';
import GameplayPrompts from '@core/models/gameplay' 
import GameModes from '@core/models/gamemodes'

interface GameInfoDialogData {
  selectedGameModeId: string
  proceedToGameSettings: () => void
}

@Component({
  selector: 'app-game-info-dialog',
  templateUrl: './gameinfo-dialog.component.html',
  styleUrls: ['./gameinfo-dialog.component.scss']
})
export class GameInfoDialogComponent implements OnInit {
  selectedGameModeId: string;
  proceedToGameSettings: () => void
  public showInstructions: boolean = false;
  defTutorialMetadata: GameplayPrompts.TutorialMetadata = { hideClasses:['Tut-Title','Tut-Chaotic-Colors','Tut-Chaotic-TwoPlayer']}

  constructor(
    private dialogRef: MatDialogRef<GameInfoDialogComponent>,
    @Inject(GameModeList) public gameModeList: GameModeList,
    @Inject(MAT_DIALOG_DATA) public data: GameInfoDialogData) {
    this.selectedGameModeId = data.selectedGameModeId;
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
