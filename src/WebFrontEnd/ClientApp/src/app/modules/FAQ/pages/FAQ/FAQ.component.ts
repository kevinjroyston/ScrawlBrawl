import { Component} from '@angular/core';
import { QandAs } from '../../components/QandA/QandAs';

@Component({
  selector: 'FAQ',
  templateUrl: './faq.component.html',
  styleUrls: ['./faq.component.scss']
})
export class FAQComponent {

    QandAs: any[]

    constructor() {
      this.QandAs = QandAs;
    }
}