import { Component, OnInit } from '@angular/core';
import { trigger, style, animate, query, stagger, transition } from '@angular/animations';

@Component({
  selector: 'home-hero-section',
  templateUrl: './hero-section.component.html',
  styleUrls: ['../../pages/home/home.component.scss', './hero-section.component.scss'],
  animations: [
    trigger('simpleFadeAnimation', [
      transition(':enter' , [
        query(".cast, h2, #mobile-play", [
          style({ opacity: 0 }),
          stagger('750ms', [
            animate('900ms', style( { opacity: 1}))
          ]
        )])
      ])
    ])
  ]
})
export class HeroSectionComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
