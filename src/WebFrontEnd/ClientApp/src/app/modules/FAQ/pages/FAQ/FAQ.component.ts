import { Component, OnInit} from '@angular/core';
import { QandAs } from '../../components/QandA/QandAs';

@Component({
  selector: 'FAQ',
  templateUrl: './FAQ.component.html',
  styleUrls: ['./FAQ.component.scss']
})
export class FAQComponent {

    QandAs: any[]

    constructor() {
      this.QandAs = QandAs;
    }
}