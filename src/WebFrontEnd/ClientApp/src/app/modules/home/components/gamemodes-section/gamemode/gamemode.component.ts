import { Component, Input, OnInit } from '@angular/core';


@Component({
  selector: 'app-gamemode',
  templateUrl: './gamemode.component.html',
  styleUrls: ['./gamemode.component.scss']
})
export class GamemodeComponent implements OnInit {

  @Input() gameModeID: string;
  svgMapping: {}

  constructor() { 
//    this.svgMapping = temporarySVGMapping;
  }

  ngOnInit() {
  }

}
