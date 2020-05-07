import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule, Routes } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { FetchDataComponent, Safe } from './fetch-data/fetch-data.component';
import { DrawingDirective } from './fetch-data/fetch-data.drawingdirective.component';
import { ColorPickerModule } from 'ngx-color-picker';
import { LobbyManagementComponent } from './lobby-management/lobby-management.component';

const appRoutes: Routes = [
    { path: 'Lobby/Manage', component: LobbyManagementComponent },
    { path: '', component: FetchDataComponent },
];


@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    FetchDataComponent,
    DrawingDirective,
    LobbyManagementComponent,
    Safe,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    ColorPickerModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forRoot(appRoutes)
  ],
  exports: [
    ColorPickerModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
