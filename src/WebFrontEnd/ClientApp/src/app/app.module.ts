import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { msalConfig, msalAngularConfig } from './app-config';

import {
    MsalModule,
    MsalInterceptor,
    MSAL_CONFIG,
    MSAL_CONFIG_ANGULAR,
    MsalService,
    BroadcastService,
    MsalAngularConfiguration
} from '@azure/msal-angular';

import { CoreModule } from '@core/core.module';
import { SharedModule } from '@shared/shared.module';
import { AppComponent } from './app.component';
import { NavMenuComponent } from './layout/nav-menu/nav-menu.component';
import { FooterComponent} from './layout/footer/footer.component';
import { Configuration } from 'msal';
import { AppRoutingModule } from './app.routing';

function MSALConfigFactory(): Configuration {
    return msalConfig;
}

function MSALAngularConfigFactory(): MsalAngularConfiguration {
//  if (!msalAngularConfig.unprotectedResources.includes("assets/BodyBuilder")) {
    // iterate here through all our games
    msalAngularConfig.unprotectedResources.push("assets/BodyBuilder");
    msalAngularConfig.unprotectedResources.push("assets/Mimic");
//  }
    return msalAngularConfig;
}

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    FooterComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    CoreModule,
    SharedModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    HttpClientModule,
    MsalModule,
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
