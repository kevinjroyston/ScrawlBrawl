import { BrowserModule } from '@angular/platform-browser';
import { NgModule, Provider } from '@angular/core';
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
import { NavMenuComponent } from '@layout/nav-menu/nav-menu.component';
import { FooterComponent } from '@layout/footer/footer.component'
import { Configuration } from 'msal';
import { AppRoutingModule } from './app.routing';
import { environment } from 'environments/environment';
import { FooterService } from '@layout/footer/footer.service';
import { NavMenuService } from '@layout/nav-menu/nav-menu.service';

function MSALConfigFactory(): Configuration {
    return msalConfig;
}

function MSALAngularConfigFactory(): MsalAngularConfiguration {
    return msalAngularConfig;
}

export const providers : Provider[] = (<Provider[]>[
  { 
    provide: MSAL_CONFIG,
    useFactory: MSALConfigFactory
  },
  {
    provide: MSAL_CONFIG_ANGULAR,
    useFactory: MSALAngularConfigFactory
  },
  MsalService
]).concat(environment.enableMsal ? [
  {
    provide: HTTP_INTERCEPTORS,
    useClass: MsalInterceptor,    
    multi: true
  }
]:[])


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
  providers: providers.concat([FooterService, NavMenuService]),
  bootstrap: [AppComponent]
})
export class AppModule { }
