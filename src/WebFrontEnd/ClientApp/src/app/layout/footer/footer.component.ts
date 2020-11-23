import { Component, Input, OnInit } from '@angular/core';

const footerLinksOrder = ['Learn More', 'About', 'Contact']

const footerLinks = {
  'Learn More': [
    {url: '/', name: "Tutorial"}
  ],
  'About': [
    {url: '/about', name: "Team"},
    {url: '/', name: "Join Us"}
  ],
  'Contact': [
    {url: '/feedback', name: "Feedback"},
    {url: '/contact', name: "Contact Us"}
  ]
}

const socialLinks = [
  {name: "Discord"},
  {name: "Reddit"},
  {name: "Twitch"},
  {name: "Twitter"}
]

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.css']
})
export class FooterComponent implements OnInit {
  sections: any;
  sectionsOrder: any;
  socialLinks: any;

  constructor() { 
    this.sections = footerLinks;
    this.sectionsOrder = footerLinksOrder;
    this.socialLinks = socialLinks;
  }

  ngOnInit() {
  }

}
