import { Component, HostListener} from '@angular/core';
import {throttle} from 'app/utils/throttle'
import { Router } from '@angular/router'; 

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss']
})
export class NavMenuComponent {
  isExpanded = false;
  isHomePage = true;
  isPastHero = false;

  constructor(private router: Router){
    router.events.subscribe((val) => {
      // see also 
      this.checkRoute()
    });
  }

  @HostListener('window:scroll', ['$event'])
  @throttle(200)
  onScroll(event: any) {
      // visible height + pixel scrolled >= total height 
      if (document.documentElement.scrollTop >= event.target['scrollingElement'].clientHeight - 50) {
        this.isPastHero = true;
      } else {
        this.isPastHero = false;
      }
  }

  checkRoute = () => {
    this.router.url === '/' ? this.isHomePage = true : this.isHomePage = false
  }
  
  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }
}
