import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import { LobbyManagementComponent as LobbyComponent } from './pages/lobby/lobby.component';
import { LobbyRoutes } from './lobby.routing';

@NgModule({
  imports: [
    LobbyRoutes,
    SharedModule
  ],
  declarations: [LobbyComponent]
})
export class LobbyModule { }
