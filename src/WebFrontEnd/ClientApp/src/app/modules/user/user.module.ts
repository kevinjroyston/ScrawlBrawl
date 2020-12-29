import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import { UserManagementComponent} from './pages/user/user-management.component';
import { UserRoutes} from './user.routing'
import {MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA} from '@angular/material/bottom-sheet';

@NgModule({
  imports: [
    UserRoutes,
    SharedModule
  ],
  declarations: [UserManagementComponent],
  providers: [
    { provide: MatBottomSheetRef, useValue: {} },
    { provide: MAT_BOTTOM_SHEET_DATA, useValue: {} }
  ]
})
export class UserModule { }
