import { Component, Input } from '@angular/core';

@Component({
  selector: 'simplelink',
  templateUrl: './link.component.html',
  styleUrls: ['../simplebutton.component.scss', './link.component.scss']
})
export class SimpleLinkComponent{
  @Input() size: string = 'medium';
  @Input() type: string = 'button';
  @Input() variant: string = 'plain';
  @Input() color: string = 'blue';
  @Input() disabled: boolean = false;
  @Input() link: string;
}
