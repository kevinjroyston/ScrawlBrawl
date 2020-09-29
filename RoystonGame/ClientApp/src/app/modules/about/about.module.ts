import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import { AboutComponent } from './pages/about/about.component';
import {AboutRoutingModule} from './about.routing'


@NgModule({
  declarations: [
    AboutComponent
  ],
  imports: [
    AboutRoutingModule,
    SharedModule
  ],
})
export class AboutModule { }
