import { NgModule } from '@angular/core';
import { FetchDataComponent, Safe } from './pages/fetch-data/fetch-data.component';
import { SharedModule } from '@shared/shared.module';
import { FetchDataRoutingModule } from './fetch-data.routing';

@NgModule({
  declarations: [
    FetchDataComponent,
    Safe
  ],
  imports: [
    SharedModule,
    FetchDataRoutingModule
  ]
})
export class FetchDataModule { }