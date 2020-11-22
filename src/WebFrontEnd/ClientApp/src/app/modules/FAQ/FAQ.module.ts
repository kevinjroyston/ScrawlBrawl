import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import {QandAComponent} from './components/QandA/QandA.component'
import { FAQComponent } from './pages/FAQ/FAQ.component';
import {FAQRoutingModule} from './FAQ.routing'

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