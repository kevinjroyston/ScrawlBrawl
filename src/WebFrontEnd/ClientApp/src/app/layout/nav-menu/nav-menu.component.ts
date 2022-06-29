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

  public launchedPlayWindow = null;
  public launchedHostWindow = null;

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

  tryToFocusWindow(win, lookFor){
    /* note, this rarely works because a window.open causes a full page load on the other tab
        causing that tab to forget any window handles it had stored */
    if(win != null && !win.closed){
      try {            
          if (win.location.href.indexOf(lookFor) >= 0) { /* we only ever launch to the play or lobby page right now */
              win.focus();            
              console.log("refocused "+lookFor);
              return true;
          }
      }
    catch (error) {console.log("Launched window had navigated, reopening")}
    return false;
  }    

  }
  redirectLink = (route: string) => {
    this.isExpanded = false;

    /* if window.name is set, we are in a lobby */
    if ((route.toLowerCase() == 'lobby') && (window.name=='_SBPlay')) {
      if (!this.tryToFocusWindow(this.launchedHostWindow, 'lobby')){
          this.launchedHostWindow = window.open('/'+route,'_SBHost','');
      }
      return
    }

    if (((route.toLowerCase() == 'play')||(route.toLowerCase() == 'join')) && (window.name=='_SBHost')) {
      if (!this.tryToFocusWindow(this.launchedPlayWindow, 'play')){
        this.launchedPlayWindow = window.open('/'+route,'_SBPlay','');
      }
      return
    }

    this.router.navigate(['/' + route])
  }
}
