import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule, Routes } from '@angular/router';
import { UiSwitchModule } from 'ngx-ui-switch';
import { MaterialModule } from './material.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { msalConfig, msalAngularConfig } from './app-config';

import {
    MsalModule,
    MsalGuard,
    MsalInterceptor,
    MSAL_CONFIG,
    MSAL_CONFIG_ANGULAR,
    MsalService,
    MsalAngularConfiguration
} from '@azure/msal-angular';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { FetchDataComponent, Safe } from './fetch-data/fetch-data.component';
import { DrawingDirective } from './fetch-data/fetch-data.drawingdirective.component';
import { ColorPickerModule } from 'ngx-color-picker';
import { LobbyManagementComponent } from './lobby-management/lobby-management.component';
import { UserManagementComponent } from './user-management/user-management.component';
import { SelectorDirective } from './fetch-data/fetch-data.selectordirective.component';
import { FeedbackComponent} from './feedback/feedback.component';
import { DrawingBoard } from './drawingboard/drawingboard.component';
import { Configuration } from 'msal';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

const appRoutes: Routes = [
    { path: 'lobby/manage', component: LobbyManagementComponent, canActivate: [MsalGuard] },
    { path: 'user/manage', component: UserManagementComponent },
    { path: '', component: FetchDataComponent },
    { path: 'feedback', component: FeedbackComponent}
    //{ path: 'admin', component: AdminComponent} //not yet made
];

function MSALConfigFactory(): Configuration {
    return msalConfig;
}

function MSALAngularConfigFactory(): MsalAngularConfiguration {
    return msalAngularConfig;
}

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    FetchDataComponent,
    FeedbackComponent,
    DrawingDirective,
    SelectorDirective,
    LobbyManagementComponent,
    UserManagementComponent,
    DrawingBoard,
    Safe,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    BrowserAnimationsModule,
    ColorPickerModule,
    FormsModule,
    HttpClientModule,
    MaterialModule,
    MsalModule,
    NgbModule,
    ReactiveFormsModule,
    RouterModule.forRoot(appRoutes),
    UiSwitchModule,
  ],
  exports: [
    ColorPickerModule
  ],
    providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: MsalInterceptor,
      multi: true
    },
    {
      provide: MSAL_CONFIG,
      useFactory: MSALConfigFactory
    },
    {
      provide: MSAL_CONFIG_ANGULAR,
      useFactory: MSALAngularConfigFactory
    },
    MsalService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
