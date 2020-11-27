import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import { AnswerComponent } from './components/answer/answer.component'
import { QuestionsComponent } from './pages/questions/questions.component';
import { QuestionsRoutingModule } from './questions.routing'

@NgModule({
  declarations: [
    QuestionsComponent,
    AnswerComponent
  ],
  imports: [
    QuestionsRoutingModule,
    SharedModule
  ],
})
export class QuestionsModule { }