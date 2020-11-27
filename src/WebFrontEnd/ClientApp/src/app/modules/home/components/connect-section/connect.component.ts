import { Component, OnInit } from '@angular/core';
import { socialLinks } from '@layout/footer/footer';

@Component({
  selector: 'home-connect-with-us',
  templateUrl: './connect.component.html',
  styleUrls: ['../../pages/home/home.component.scss','./connect.component.scss']
})
export class ConnectWithUsComponent implements OnInit {
  socialLinks: any

  constructor() { 
    this.socialLinks = socialLinks;
  }

  ngOnInit() {
  }

}
