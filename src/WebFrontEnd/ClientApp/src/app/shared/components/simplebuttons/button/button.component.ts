import { Component, Output, EventEmitter, Input } from '@angular/core';

interface Button {
  variant: 'outlined' | 'action' | 'plain'
  size: 'small' | 'medium' | 'large'
  type: 'button' | 'submit'
  color: 'blue' | 'red' | 'green'
  disabled: true | false
}

@Component({
  selector: 'simplebutton',
  templateUrl: './button.component.html',
  styleUrls: ['../simplebutton.component.scss']
})
export class SimpleButtonComponent {
  @Input() size: string = 'medium';
  @Input() type: string = 'button';
  @Input() variant: string = 'plain';
  @Input() color: string = 'blue';
  @Input() disabled: boolean = false;
  @Output() onClick = new EventEmitter<MouseEvent>();

  onClickButton(event){
    this.onClick.emit(event)
  }
}
