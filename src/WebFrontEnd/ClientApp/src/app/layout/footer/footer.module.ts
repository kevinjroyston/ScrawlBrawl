import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router'
import { CommonModule } from '@angular/common';

import {FooterComponent} from './footer.component'


@NgModule({
  imports: [RouterModule, CommonModule],
  declarations: [
    FooterComponent
  ],
  exports: [
      FooterComponent,
  ]
})
export class FooterModule { }
