import { Component, Inject, ViewEncapsulation, Input } from '@angular/core';


@Component({
  selector: 'scrawlbutton',
  templateUrl: './scrawlbutton.component.html',
  styleUrls: ['./scrawlbutton.component.css']
})
export class ScrawlButtonComponent {
  @Input() label: string;
  @Input() color: string = 'blue';
  @Input() width: string = '174px';

  constructor() {
       }

}
