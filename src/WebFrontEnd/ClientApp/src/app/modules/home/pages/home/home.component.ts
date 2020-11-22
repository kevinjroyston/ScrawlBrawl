import { Component, OnInit } from '@angular/core';
import { FooterService } from '@layout/footer/footer.service';

@Component({
  selector: 'home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  constructor(public footerService: FooterService) {

  }

  ngOnInit () {
    this.footerService.show();
    console.log(this.footerService.visible)
  }

  ngOnDestroy () {
    this.footerService.hide();
  }
}
