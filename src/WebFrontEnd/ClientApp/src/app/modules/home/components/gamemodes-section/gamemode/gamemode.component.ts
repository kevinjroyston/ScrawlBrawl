import { Component, Input, Inject, OnInit } from '@angular/core';
import { MatDialog,  MatDialogConfig } from '@angular/material/dialog';
import {GameInfoDialogComponent} from '../../../../lobby/components/gameinfo-dialog/gameinfo-dialog.component';
import { GameModeList } from '@core/http/gamemodelist';
import GameModes from '@core/models/gamemodes'
@Component({
  selector: 'app-gamemode',
  templateUrl: './gamemode.component.html',
  styleUrls: ['./gamemode.component.scss']
})
export class GamemodeComponent implements OnInit {

  
  @Input() gameModeID: string;

  constructor(@Inject(GameModeList) public gameModeList: GameModeList, private matDialog: MatDialog)
  {
  }

  openGameInfoDialog = (event) => {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.data = {
      selectedGameModeId: this.gameModeID,
      proceedToGameSettings: null,
    }
    this.matDialog.open(GameInfoDialogComponent, dialogConfig);
    event.stopPropagation();
}

  ngOnInit() {
  }

}
