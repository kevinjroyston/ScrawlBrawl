import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import {FeedbackRoutingModule} from './feedback.routing'
import { FeedbackComponent } from './pages/feedback/feedback.component';

@NgModule({
  declarations: [FeedbackComponent],
  imports: [
    SharedModule,
    FeedbackRoutingModule
  ],
})
export class FeedbackModule { }
