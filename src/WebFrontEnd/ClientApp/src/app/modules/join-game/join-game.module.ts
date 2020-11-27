import { NgModule } from '@angular/core';
import { JoinGameComponent} from './pages/join-game/join-game.component';
import { SharedModule } from '@shared/shared.module';
import { JoinGameRoutingModule } from './join-game.routing';

@NgModule({
  declarations: [
    JoinGameComponent
  ],
  imports: [
    SharedModule,
    JoinGameRoutingModule
  ]
})
export class JoinGameModule { }