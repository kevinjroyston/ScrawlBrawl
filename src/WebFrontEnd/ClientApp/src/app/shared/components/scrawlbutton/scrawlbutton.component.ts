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
  @Input() link: string = '#';
  @Output() onClick = new EventEmitter<MouseEvent>();
  element;

  constructor(element: ElementRef) {
    this.element = element.nativeElement;
       }
        onClickButton(event){
        this.onClick.emit(event)
      }
    /*    
       ngOnInit() {
        console.log("scrawlbutton init");
      }
      ngAfterViewInit() {
      }
*/      
}
