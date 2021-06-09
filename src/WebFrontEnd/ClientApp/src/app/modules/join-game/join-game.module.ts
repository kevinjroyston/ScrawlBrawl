import { NgModule } from '@angular/core';
import { JoinGameComponent} from './pages/join-game/join-game.component';
import { SharedModule } from '@shared/shared.module';
import { JoinGameRoutingModule } from './join-game.routing';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material/bottom-sheet';

@NgModule({
  declarations: [
    JoinGameComponent
  ],
  imports: [
    SharedModule,
    JoinGameRoutingModule
  ],
  providers: [
    { provide: MatBottomSheetRef, useValue: {} },
    { provide: MAT_BOTTOM_SHEET_DATA, useValue: {} }
  ],
})
export class JoinGameModule { }