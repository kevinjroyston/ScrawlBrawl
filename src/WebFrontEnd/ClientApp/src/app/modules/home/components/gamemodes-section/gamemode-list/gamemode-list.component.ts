import { Component, OnInit } from '@angular/core';

const gameModes = ['BodyBuilder', 'Imposter', 'ChaoticCoop', 'BodySwap', 'FriendQuiz', 'Mimic']

@Component({
  selector: 'app-gamemode-list',
  templateUrl: './gamemode-list.component.html',
  styleUrls: ['../../../pages/home/home.component.scss', './gamemode-list.component.scss']
})
export class GamemodeListComponent implements OnInit {
  gameModes: string[];
  currentGameMode: number = 0;

  constructor() { 
    this.gameModes = gameModes;
  }

  ngOnInit() {
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
