import { Component, Input, OnInit } from '@angular/core';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import {GameInfoDialogComponent} from '../../../../lobby/components/gameinfo-dialog/gameinfo-dialog.component';
import GameModes from '@core/models/gamemodes'


@Component({
  selector: 'app-gamemode',
  templateUrl: './gamemode.component.html',
  styleUrls: ['./gamemode.component.scss']
})
export class GamemodeComponent implements OnInit {

  @Input() gameModeID: string;
  @Input() gameId: number;
  @Input() gameModes: GameModes.GameModeMetadata[];
  svgMapping: {}

  constructor(private matDialog: MatDialog)
  {

//    this.svgMapping = temporarySVGMapping;
  }
  openGameInfoDialog = (event, game: number) => {
    const dialogConfig = new MatDialogConfig();
    dialogConfig.data = {
      gameModes: this.gameModes,
      selectedGameMode: this.gameId,
      proceedToGameSettings: null,
    }
    this.matDialog.open(GameInfoDialogComponent, dialogConfig);
    event.stopPropagation();
}


  ngOnInit() {
  }

}
