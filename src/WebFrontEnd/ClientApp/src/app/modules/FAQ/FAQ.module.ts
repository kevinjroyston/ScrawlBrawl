import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import {QandAComponent} from './components/QandA/QandA.component'
import { FAQComponent } from './pages/faq/faq.component';
import {FAQRoutingModule} from './faq.routing'

@NgModule({
  declarations: [
    FAQComponent,
    QandAComponent
  ],
  imports: [
    FAQRoutingModule,
    SharedModule
  ],
})
export class FAQModule { }