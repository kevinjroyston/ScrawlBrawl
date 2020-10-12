import { Routes, RouterModule } from '@angular/router';
import {UserManagementComponent} from './pages/user/user-management.component'

const routes : Routes = [
    {
        path: '',
        component: UserManagementComponent
    }
]

export const UserRoutes = RouterModule.forChild(routes);