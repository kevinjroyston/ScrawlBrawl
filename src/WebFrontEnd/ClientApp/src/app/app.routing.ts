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
        path: 'home', 
        loadChildren: () => import('./modules/home/home.module').then(m => m.HomeModule)
    },

    {
        path: 'about',
        loadChildren: () => import('./modules/about/about.module').then(m => m.AboutModule)
    },
    {
        path: 'questions',
        loadChildren: () => import('./modules/questions/questions.module').then(m => m.QuestionsModule)
    }, 
    {
        path: 'feedback',
        loadChildren: () => import('./modules/feedback/feedback.module').then(m => m.FeedbackModule)
    },
    {
        path: 'join',
        loadChildren: () => import('./modules/join-game/join-game.module').then(m => m.JoinGameModule)
    },
    {
        path: 'play',
        loadChildren: () => import('./modules/fetch-data/fetch-data.module').then(m => m.FetchDataModule)
    },
    {
        path: 'lobby',
        canActivate: guards,
        loadChildren: () => import('./modules/lobby/lobby.module').then(m => m.LobbyModule)
    },
    {
        path: 'user',
        loadChildren: () => import('./modules/user/user.module').then(m => m.UserModule)
    },
    //{ path: 'admin', component: AdminComponent} //not yet made
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { relativeLinkResolution: 'legacy' })],
  exports: [RouterModule],
  //Put guard in here?
  providers: []
})
export class AppRoutingModule {}