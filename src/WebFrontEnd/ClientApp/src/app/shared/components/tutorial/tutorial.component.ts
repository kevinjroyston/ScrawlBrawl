import { Component, ViewEncapsulation,AfterViewInit, Input, OnInit } from '@angular/core';
import GameplayPrompts from '@core/models/gameplay';
import * as cssTweaks from "app/utils/csstweaks";

@Component({
  selector: 'app-tutorial',
  templateUrl: './tutorial.component.html',
  styleUrls: ['./tutorial.component.scss'],
  encapsulation: ViewEncapsulation.None,
})
export class TutorialComponent implements OnInit,AfterViewInit {
  @Input() gameModeIdString: string;
  @Input() tutorialMetadata: GameplayPrompts.TutorialMetadata;
  constructor() { }

 
  ngAfterViewInit(){
    console.log('hiding tutorial classes');
    if (this.tutorialMetadata && this.tutorialMetadata.hideClasses) {
      this.tutorialMetadata.hideClasses.forEach(function (hideClass){
        console.log('hiding: '+hideClass);
        cssTweaks.setCSSDisplayForSelector("."+hideClass,"none"); }
      )
    }
  }  
  ngOnInit() {
  }

}
