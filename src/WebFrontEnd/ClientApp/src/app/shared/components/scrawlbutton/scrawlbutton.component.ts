import { Component, ElementRef, Inject, ViewEncapsulation, Input,Output,EventEmitter } from '@angular/core';

@Component({
  selector: 'scrawlbutton',
  templateUrl: './scrawlbutton.component.html',
  styleUrls: ['./scrawlbutton.component.css'],
})
export class ScrawlButtonComponent {
  @Input() color: string = 'blue';
  @Input() textColor: string;
  @Input() link: string;
  @Output() onClick = new EventEmitter<MouseEvent>();
  element;

  constructor(element: ElementRef) {
    this.element = element.nativeElement;
  }

  onClickButton(event){
    this.onClick.emit(event)
  } 
}
