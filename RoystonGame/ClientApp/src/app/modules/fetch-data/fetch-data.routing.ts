import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import {FetchDataComponent} from './pages/fetch-data/fetch-data.component';

export const routes: Routes = [
  {
    path: '',
    component: FetchDataComponent
  },
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class FetchDataRoutingModule { }
