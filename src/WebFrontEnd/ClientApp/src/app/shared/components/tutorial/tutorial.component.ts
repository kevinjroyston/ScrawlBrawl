import { Component, ViewEncapsulation, Input, OnInit } from '@angular/core';
import GameplayPrompts from '@core/models/gameplay';

@Component({
  selector: 'app-tutorial',
  templateUrl: './tutorial.component.html',
  styleUrls: ['./tutorial.component.scss'],
  encapsulation: ViewEncapsulation.None,
})
export class TutorialComponent implements OnInit {
  @Input() gameModeIdString: string;
  @Input() tutorialMetadata: GameplayPrompts.TutorialMetadata;
  constructor() { }

  ngOnInit() {
  }

}
