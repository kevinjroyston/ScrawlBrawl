import { Component, Inject, Input, ViewEncapsulation } from '@angular/core';


@Component({
  selector: 'aboutus-member',
  templateUrl: './member.component.html',
  styleUrls: ['./member.component.css']
})
export class MemberComponent {
  @Input() member;

  constructor() {

  }
}