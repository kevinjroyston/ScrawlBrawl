import { Component} from '@angular/core';
import {members} from '../../components/member/members'
import {Member} from '@core/models/members'

@Component({
  selector: 'about',
  templateUrl: './about.component.html',
  styleUrls: ['./about.component.css']
})
export class AboutComponent {

  members: Member[]

  constructor() {
    this.members = members;
  }

}
