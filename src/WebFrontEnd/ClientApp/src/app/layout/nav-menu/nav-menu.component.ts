import { Component, HostListener} from '@angular/core';
import {throttle} from 'app/utils/throttle'
import { Router, ActivatedRoute } from '@angular/router'; 
import { trigger, style, animate, transition } from '@angular/animations';
import { Location } from '@angular/common';
import { NavMenuService } from './nav-menu.service';
import { ThemeService } from '@core/services/theme.service' 

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss'],
  animations: [
    trigger('simpleFadeAnimation', [
      transition(':enter' , [
        style({ opacity: 0 }),
        animate('250ms 1000ms', 
          style({opacity: 1})
        )
      ])
    ])
  ]
})
export class NavMenuComponent {
  isExpanded = false;
  isHomePage = true;
  isLobbyPage = false;
  isPastHero = false;

  constructor(private router: Router, public activatedRoute: ActivatedRoute, public location: Location, public NavMenuService : NavMenuService, private ThemeService: ThemeService ){
    this.checkRoute(location.prepareExternalUrl(location.path()));
    router.events.subscribe((val) => { 
      this.checkRoute(this.router.url)
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

  checkRoute = (url: string) : void => {
    ((url === '/') || (url.toLowerCase() == '/home')) ? this.isHomePage = true : this.isHomePage = false;
    
    ((url === '/lobby') || (url.toLowerCase() == '/lobby')) ? this.isLobbyPage = true : this.isLobbyPage = false;
  }

  toggle = () => {
    this.isExpanded = !this.isExpanded;
  }

  toggleTheme = () => {
    this.ThemeService.toggleTheme();
  }

  redirectLink = (route: string) => {
    this.isExpanded = false;
    this.router.navigate(['/' + route])
  }
}
