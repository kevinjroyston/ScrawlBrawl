import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import { LobbyManagementComponent as LobbyComponent } from './pages/lobby/lobby.component';
import { LobbyRoutes } from './lobby.routing';
import {GamemodeDialogComponent} from './components/gamemode-dialog/gamemode-dialog.component'
import {GameInfoDialogComponent} from './components/gameinfo-dialog/gameinfo-dialog.component'
import { ErrorService } from '@modules/lobby/services/error.service';
import { LobbyInstructionsDialogComponent } from './components/lobbyinstructions-dialog/lobbyinstructions-dialog.component';

@NgModule({
  imports: [
    LobbyRoutes,
    SharedModule
  ],
  declarations: [
    LobbyComponent,
    GamemodeDialogComponent,
    GameInfoDialogComponent,
    LobbyInstructionsDialogComponent
  ],
  providers: [
    ErrorService
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class LobbyModule { }
