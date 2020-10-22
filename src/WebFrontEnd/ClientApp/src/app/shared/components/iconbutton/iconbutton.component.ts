import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'scrawlbrawl-iconbutton',
  templateUrl: './iconbutton.component.html',
  styleUrls: ['./iconbutton.component.scss']
})
export class IconButtonComponent implements OnInit {
  @Input() active: boolean = false;

  constructor() { }

  ngOnInit() {
  }
}
