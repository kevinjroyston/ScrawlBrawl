import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule, Routes } from '@angular/router';
import { UiSwitchModule } from 'ngx-ui-switch';
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
import { SelectorDirective } from './fetch-data/fetch-data.selectordirective.component';
import { FeedbackComponent} from './feedback/feedback.component';
import { Configuration } from 'msal';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

const appRoutes: Routes = [
    { path: 'lobby/manage', component: LobbyManagementComponent, canActivate: [MsalGuard] },
    { path: '', component: FetchDataComponent },
    { path: 'feedback', component: FeedbackComponent}
    //{ path: 'admin', component: AdminComponent} //not yet made
];

export const protectedResourceMap: [string, string[]][] = [
    ['https://login.microsoftonline.com', ['api://f62ed1b5-3f4f-4c23-925a-0d27767707c6/ManageLobby']],
    ['/lobby/manage', ['api://f62ed1b5-3f4f-4c23-925a-0d27767707c6/ManageLobby']],
    ['/Lobby/Games', ['api://f62ed1b5-3f4f-4c23-925a-0d27767707c6/ManageLobby']],
    ['/Lobby/Get', ['api://f62ed1b5-3f4f-4c23-925a-0d27767707c6/ManageLobby']],
    ['/Lobby/Create', ['api://f62ed1b5-3f4f-4c23-925a-0d27767707c6/ManageLobby']],
    ['/Lobby/Configure', ['api://f62ed1b5-3f4f-4c23-925a-0d27767707c6/ManageLobby']],
    ['/Lobby/Delete', ['api://f62ed1b5-3f4f-4c23-925a-0d27767707c6/ManageLobby']],
    ['/Lobby/Start', ['api://f62ed1b5-3f4f-4c23-925a-0d27767707c6/ManageLobby']],
];

const isIE = window.navigator.userAgent.indexOf("MSIE ") > -1 || window.navigator.userAgent.indexOf("Trident/") > -1;

function MSALConfigFactory(): Configuration {
    return {
        auth: {
            clientId: 'f62ed1b5-3f4f-4c23-925a-0d27767707c6',
            authority: "https://login.microsoftonline.com/common/",
            validateAuthority: true,
            navigateToLoginRequestUrl: true,
        },
        cache: {
            cacheLocation: "localStorage",
            storeAuthStateInCookie: isIE, // set to true for IE 11
        },
    };
}

function MSALAngularConfigFactory(): MsalAngularConfiguration {
    return {
        popUp: !isIE,
        consentScopes: [
            "api://f62ed1b5-3f4f-4c23-925a-0d27767707c6/ManageLobby"
        ],
        unprotectedResources: ["/currentContent", "/FormSubmit"],
        protectedResourceMap,
        extraQueryParameters: {}
    };
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
    Safe,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    ColorPickerModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    UiSwitchModule,
    RouterModule.forRoot(appRoutes),
    MsalModule,
    NgbModule
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
