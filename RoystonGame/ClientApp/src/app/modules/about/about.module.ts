import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import {MemberComponent} from './components/member/member.component'
import { AboutComponent } from './pages/about/about.component';
import {AboutRoutingModule} from './about.routing'

@NgModule({
  declarations: [
    AboutComponent,
    MemberComponent
  ],
  imports: [
    AboutRoutingModule,
    SharedModule
  ],
})
export class AboutModule { }
