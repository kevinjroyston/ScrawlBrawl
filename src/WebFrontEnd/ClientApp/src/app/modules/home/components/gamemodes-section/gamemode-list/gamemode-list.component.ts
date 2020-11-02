import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-gamemode-list',
  templateUrl: './gamemode-list.component.html',
  styleUrls: ['./gamemode-list.component.scss']
})
export class GamemodeListComponent implements OnInit {
  gameModes: string[]

  constructor() { 
    this.gameModes = ['BodyBuilder', 'Imposter', 'ChaoticCoop', 'BodySwap', 'FriendQuiz', 'Mimic'];
  }

  ngOnInit() {
  }
}
