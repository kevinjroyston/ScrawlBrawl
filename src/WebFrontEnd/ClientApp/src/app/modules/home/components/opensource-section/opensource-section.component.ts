import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'home-open-source',
  templateUrl: './opensource-section.component.html',
  styleUrls: ['../../pages/home/home.component.scss', './opensource-section.component.scss']
})
export class OpensourceSectionComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

  onGithubRedirect() {
    window.location.href='https://github.com/kevinjroyston/ScrawlBrawl';
  }
}
