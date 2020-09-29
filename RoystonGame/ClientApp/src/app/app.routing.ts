import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { environment } from '../environments/environment';
import {
    MsalGuard,
} from '@azure/msal-angular';

const guards: any[] = environment.enableMsal ? [MsalGuard] : [];

const routes: Routes = [
    { 
        path: '', 
        loadChildren: () => import('./modules/home/home.module').then(m => m.HomeModule)
    },
    {
        path: 'about',
        loadChildren: () => import('./modules/about/about.module').then(m => m.AboutModule)
    },
    {
        path: 'feedback',
        loadChildren: () => import('./modules/feedback/feedback.module').then(m => m.FeedbackModule)
    },
    {
        path: 'game/play',
        loadChildren: () => import('./modules/fetch-data/fetch-data.module').then(m => m.FetchDataModule)
    },
    {
        path: 'lobby/manage',
        canActivate: [guards],
        loadChildren: () => import('./modules/lobby/lobby.module').then(m => m.LobbyModule)
    },
    {
        path: 'user/manage',
        loadChildren: () => import('./modules/user/user.module').then(m => m.UserModule)
    }
    //{ path: 'admin', component: AdminComponent} //not yet made
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { useHash: true })],
  exports: [RouterModule],
  providers: []
})
export class AppRoutingModule {}