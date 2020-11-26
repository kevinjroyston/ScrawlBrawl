import { Component, Input } from '@angular/core';


@Component({
  selector: 'QandA',
  templateUrl: './QandA.component.html',
  styleUrls: ['./QandA.component.css']
})

export class QandAComponent {

  @Input() QandA;

  constructor() {
 
  }
}