import { Component, Inject, OnInit } from '@angular/core';
import { API } from '@core/http/api';
import GameModes from '@core/models/gamemodes'

@Component({
  selector: 'app-gamemode-list',
  templateUrl: './gamemode-list.component.html',
  styleUrls: ['../../../pages/home/home.component.scss', './gamemode-list.component.scss']
})
export class GamemodeListComponent implements OnInit {
  gameModes: GameModes.GameModeMetadata[];
  currentGameMode: number = 0;

  constructor(@Inject(API) private api: API) { 
    this.getGames();
  }

  ngOnInit() {
  }

  getGames = async () => {
    await this.api.request({ type: "Lobby", path: "Games" }).subscribe({
        next: (result) => { this.gameModes = result as GameModes.GameModeMetadata[];console.log(this.gameModes) }
    });
  }

  getNext = () => {
    const next = this.currentGameMode + 1;
    return next === this.gameModes.length ? 0 : next
  }

  getPrevious = () => {
    const previous = this.currentGameMode - 1;
    return previous < 0 ? this.gameModes.length - 1: previous
  }

  onNextGameMode = () => {
    this.currentGameMode = this.getNext();
  }

  onPreviousGameMode = () => {
    this.currentGameMode = this.getPrevious();
  }
}
