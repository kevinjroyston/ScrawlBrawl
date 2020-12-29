import { NgModule } from '@angular/core';
import { FetchDataComponent, Safe } from './pages/fetch-data/fetch-data.component';
import { SharedModule } from '@shared/shared.module';
import { FetchDataRoutingModule } from './fetch-data.routing';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material/bottom-sheet';

@NgModule({
  declarations: [
    FetchDataComponent,
    Safe
  ],
  imports: [
    SharedModule,
    FetchDataRoutingModule
  ],
  providers: [
    { provide: MatBottomSheetRef, useValue: {} },
    { provide: MAT_BOTTOM_SHEET_DATA, useValue: {} }
  ],
})
export class FetchDataModule { }