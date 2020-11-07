import { NgModule } from '@angular/core';
import { HomeComponent } from './pages/home/home.component';
import { GamemodeListComponent} from './components/gamemodes-section/gamemode-list/gamemode-list.component'
import { GamemodeComponent} from './components/gamemodes-section/gamemode/gamemode.component'
import { SharedModule } from '@shared/shared.module';
import { HomeRoutingModule } from './home.routing';

@NgModule({
  declarations: [
    HomeComponent,
    GamemodeComponent,
    GamemodeListComponent
  ],
  imports: [
    SharedModule,
    HomeRoutingModule
  ]
})
export class HomeModule { }
