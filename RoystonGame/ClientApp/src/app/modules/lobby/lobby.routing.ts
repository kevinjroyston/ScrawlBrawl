import { Routes, RouterModule } from '@angular/router';
import {LobbyManagementComponent as LobbyComponent} from './pages/lobby/lobby.component'

const routes : Routes = [
    {
        path: '',
        component: LobbyComponent
    }
]

export const LobbyRoutes = RouterModule.forChild(routes);