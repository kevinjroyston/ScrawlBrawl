import { Component, OnInit, Inject } from '@angular/core';
import { FooterService } from '@layout/footer/footer.service';
import { UserManager } from '@core/http/userManager';
import { Router } from "@angular/router";

@Component({
  selector: 'home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  constructor(public footerService: FooterService, @Inject(UserManager) userManager, private router : Router) {
    if (this.router.url.toLowerCase() !== '/home'){
            userManager.rejoinGameIfAlreadyPlaying();
    }
  }

  ngOnInit () {
    this.footerService.show();
    console.log(this.footerService.visible)
  }

  ngOnDestroy () {
    this.footerService.hide();
  }
}
