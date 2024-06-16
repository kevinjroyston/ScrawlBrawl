import { BrowserModule } from '@angular/platform-browser';
import { NgModule, Provider, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MSALInstanceFactory, MSALGuardConfigFactory, MSALInterceptorConfigFactory } from './app-config';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { IPublicClientApplication, PublicClientApplication, InteractionType, BrowserCacheLocation } from '@azure/msal-browser';
import { MsalGuard, MsalInterceptor, MsalBroadcastService, MsalInterceptorConfiguration, MsalModule, MsalService, MSAL_GUARD_CONFIG, MSAL_INSTANCE, MSAL_INTERCEPTOR_CONFIG, MsalGuardConfiguration } from '@azure/msal-angular';


import { CoreModule } from '@core/core.module';
import { SharedModule } from '@shared/shared.module';
import { AppComponent } from './app.component';
import { NavMenuComponent } from '@layout/nav-menu/nav-menu.component';
import { FooterComponent } from '@layout/footer/footer.component'
import { AppRoutingModule } from './app.routing';
import { environment } from 'environments/environment';
import { FooterService } from '@layout/footer/footer.service';
import { NavMenuService } from '@layout/nav-menu/nav-menu.service';
import { MatTabsModule } from '@angular/material/tabs';
import { ResizableModule } from 'angular-resizable-element';


export const providers: Provider[] = (<Provider[]>[
  { provide: MSAL_INSTANCE, useFactory: MSALInstanceFactory },
  { provide: MSAL_GUARD_CONFIG, useFactory: MSALGuardConfigFactory },
  { provide: MSAL_INTERCEPTOR_CONFIG, useFactory: MSALInterceptorConfigFactory },
  MsalService, MsalGuard, MsalBroadcastService
]).concat(environment.enableMsal ? [
  {
    provide: HTTP_INTERCEPTORS,
    useClass: MsalInterceptor,
    multi: true
  }
] : [])


@NgModule({ declarations: [
        AppComponent,
        NavMenuComponent,
        FooterComponent
    ],
    bootstrap: [AppComponent],
    schemas: [CUSTOM_ELEMENTS_SCHEMA], imports: [BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
        CoreModule,
        SharedModule,
        MatTabsModule,
        BrowserAnimationsModule,
        AppRoutingModule,
        MsalModule,
        ResizableModule], providers: [provideHttpClient(withInterceptorsFromDi())] })
export class AppModule { }
