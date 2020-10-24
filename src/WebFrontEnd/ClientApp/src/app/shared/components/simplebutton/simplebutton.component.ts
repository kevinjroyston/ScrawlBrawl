import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';

interface Button {
  variant: 'outlined' | 'action' | 'plain'
  size: 'small' | 'medium' | 'large'
  type: 'button' | 'submit'
  color: 'blue' | 'red' | 'green'
  disabled: true | false
}

@Component({
  selector: 'simplebutton',
  templateUrl: './simplebutton.component.html',
  styleUrls: ['./simplebutton.component.scss']
})
export class SimpleButtonComponent implements OnInit {
  @Input() size: string = 'medium';
  @Input() type: string = 'button';
  @Input() variant: string = 'plain';
  @Input() color: string = 'blue';
  @Input() disabled: boolean = false;
  @Output() onClick = new EventEmitter<MouseEvent>();

  constructor(){
  }

  ngOnInit() {
  }

  onClickButton(event){
    this.onClick.emit(event)
  }
}
