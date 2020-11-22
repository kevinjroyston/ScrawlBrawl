import { Component } from '@angular/core';
import { FooterService } from './footer.service';
import {footerLinksOrder, footerLinks, socialLinks} from './footer'

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.scss']
})
export class FooterComponent {
  sections: any;
  sectionsOrder: any;
  socialLinks: any;

  constructor(public footerService : FooterService) { 
    this.sections = footerLinks;
    this.sectionsOrder = footerLinksOrder;
    this.socialLinks = socialLinks;
  }
}
