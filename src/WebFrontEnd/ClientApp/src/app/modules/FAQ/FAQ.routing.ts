import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import {FAQComponent} from './pages/faq/faq.component'

const routes: Routes = [
  {
    path: '',
    component: FAQComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class FAQRoutingModule { }