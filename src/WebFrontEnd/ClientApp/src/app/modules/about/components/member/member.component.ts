import { Component, Inject, Input, ViewEncapsulation } from '@angular/core';


@Component({
  selector: 'aboutus-member',
  templateUrl: './member.component.html',
  styleUrls: ['./member.component.scss']
})
export class MemberComponent {
  @Input() member;

  constructor() {

  }
}