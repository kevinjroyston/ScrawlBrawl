import { Component, HostListener} from '@angular/core';
import {throttle} from 'app/utils/throttle'
import { Router } from '@angular/router'; 
import { trigger, style, animate, transition } from '@angular/animations';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss'],
  animations: [
    trigger('simpleFadeAnimation', [
      transition(':enter' , [
        style({ opacity: 0 }),
        animate('250ms 3400ms', 
          style({opacity: 1})
        )
      ])
    ])
  ]
})
export class NavMenuComponent {
  isExpanded = false;
  isHomePage = true;
  isPastHero = false;

  constructor(private router: Router){
    router.events.subscribe((val) => { 
      this.checkRoute()
    });
  }

  @HostListener('window:scroll', ['$event'])
  @throttle(200)
  onScroll(event: any){
      if (document.documentElement.scrollTop >= event.target['scrollingElement'].clientHeight - 50) {
        this.isPastHero = true;
      } else {
        this.isPastHero = false;
      }
  }

  checkRoute = () => {
    this.router.url === '/' ? this.isHomePage = true : this.isHomePage = false
  }

  toggle = () => {
    this.isExpanded = !this.isExpanded;
  }

  redirectLink = (route: string) => {
    this.isExpanded = false;
    this.router.navigate(['/' + route])
  }
}
