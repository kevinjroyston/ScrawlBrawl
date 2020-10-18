import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';

@Component({
  selector: 'scrawlbrawl-button',
  templateUrl: './button.component.html',
  styleUrls: ['./button.component.css']
})
export class ButtonComponent implements OnInit {
  hovered: boolean
  style: any
  @Input() type: string = 'button';
  @Input() color: string = 'blue';
  @Output() onClick = new EventEmitter<MouseEvent>();

  constructor(){
    this.hovered = false;
  }

  ngOnInit() {
  }

  onClickButton(event){
    this.onClick.emit(event)
  }

  defaultStyle(){
    this.style = {
      'background-color': 'var(--' + this.color + '-primary)',
      'filter': 'brightness(110%)',
      'box-shadow': '0 2px 8px var(--' +this.color+'-tertiary)'
    }
    return this.style;
  }

  hoveredStyle(){
    this.style['filter'] = 'brightness(95%)'
    return this.style;
  }
}
