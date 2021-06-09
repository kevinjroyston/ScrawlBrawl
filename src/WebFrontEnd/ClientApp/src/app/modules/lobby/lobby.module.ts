import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import { LobbyManagementComponent as LobbyComponent } from './pages/lobby/lobby.component';
import { LobbyRoutes } from './lobby.routing';
import {GamemodeDialogComponent} from './components/gamemode-dialog/gamemode-dialog.component'
import {GameInfoDialogComponent} from './components/gameinfo-dialog/gameinfo-dialog.component'
import { ErrorService } from '@modules/lobby/services/error.service';
import { LobbyInstructionsDialogComponent } from './components/lobbyinstructions-dialog/lobbyinstructions-dialog.component';
import { CreateLobbyComponent } from './pages/lobby/create-lobby/create-lobby.component';
import { CommonoptionsDialogComponent } from './components/commonoptions-dialog/commonoptions-dialog.component';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material/bottom-sheet';

@NgModule({
  imports: [
    LobbyRoutes,
    SharedModule
  ],
  declarations: [
    LobbyComponent,
    GamemodeDialogComponent,
    CommonoptionsDialogComponent,
    GameInfoDialogComponent,
    CreateLobbyComponent,
    LobbyInstructionsDialogComponent
  ],
  providers: [
    ErrorService,
    { provide: MatBottomSheetRef, useValue: {} },
    { provide: MAT_BOTTOM_SHEET_DATA, useValue: {} }
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class LobbyModule { }
