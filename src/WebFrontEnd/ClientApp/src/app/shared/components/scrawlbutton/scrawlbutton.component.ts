import { Component, ElementRef, Inject, ViewEncapsulation, Input,Output,EventEmitter } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'scrawlbutton',
  templateUrl: './scrawlbutton.component.html',
  styleUrls: ['./scrawlbutton.component.css'],
})
export class ScrawlButtonComponent {
  @Input() label: string = '';
  @Input() color: string = 'blue';
  @Input() width: string = '174px';
  @Input() link: string = '';
  @Output() onclick = new EventEmitter();
  element;

  constructor(element: ElementRef) {
    this.element = element.nativeElement;
       }
/*    
       ngOnInit() {
        console.log("scrawlbutton init");
      }
      ngAfterViewInit() {
      }
*/      
}
