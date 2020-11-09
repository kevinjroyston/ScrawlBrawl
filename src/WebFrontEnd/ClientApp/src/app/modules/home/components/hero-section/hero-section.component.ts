import { Component, OnInit } from '@angular/core';
import {
  trigger,
  state,
  style,
  animate,
  transition
} from '@angular/animations';

@Component({
  selector: 'home-hero-section',
  templateUrl: './hero-section.component.html',
  styleUrls: ['../../pages/home/home.component.scss', './hero-section.component.scss'],
  animations: [
    trigger('simpleFadeAnimation', [
      state('slowin', style({opacity: 1})),
      transition(':enter', [
        style({opacity: 0}),
        animate(1000)
      ]),
      state('fastin', style({opacity: 1})),
      transition(':enter', [
        style({opacity: 0}),
        animate(500)
      ])
    ]),
  ]
})
export class HeroSectionComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
