import { NgModule } from '@angular/core';
import { HomeComponent } from './pages/home/home.component';
import {FooterComponent} from '@layout/footer/footer.component'
import { SharedModule } from '@shared/shared.module';
import { HomeRoutingModule } from './home.routing';

@NgModule({
  declarations: [
    HomeComponent,
    FooterComponent
  ],
  imports: [
    SharedModule,
    HomeRoutingModule
  ]
})
export class HomeModule { }
