import { Component, Inject, OnInit } from '@angular/core';
import { API } from '@core/http/api';
import GameModes from '@core/models/gamemodes'
import { trigger, transition, style, animate } from "@angular/animations";
import { GameModeList } from '@core/http/gamemodelist';

@Component({
  selector: 'app-gamemode-list',
  templateUrl: './gamemode-list.component.html',
  styleUrls: ['../../../pages/home/home.component.scss', './gamemode-list.component.scss']
})
export class GamemodeListComponent implements OnInit {

  constructor (@Inject(GameModeList) public gameModeList: GameModeList) {}

  ngOnInit() {
  }

}
