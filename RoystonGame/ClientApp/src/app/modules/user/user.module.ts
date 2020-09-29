import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import { UserManagementComponent} from './pages/user/user-management.component';
import { UserRoutes} from './user.routing'

@NgModule({
  imports: [
    UserRoutes,
    SharedModule
  ],
  declarations: [UserManagementComponent]
})
export class UserModule { }
