import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import { LobbyManagementComponent as LobbyComponent } from './pages/lobby/lobby.component';
import { LobbyRoutes } from './lobby.routing';
import {GamemodeDialogComponent} from './components/gamemode-dialog/gamemode-dialog.component'
import {GameInfoDialogComponent} from './components/gameinfo-dialog/gameinfo-dialog.component'

@NgModule({
  imports: [
    LobbyRoutes,
    SharedModule
  ],
  declarations: [
    LobbyComponent,
    GamemodeDialogComponent,
    GameInfoDialogComponent
  ],
})
export class LobbyModule { }
