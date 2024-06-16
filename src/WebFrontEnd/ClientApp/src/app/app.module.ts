import { BrowserModule } from '@angular/platform-browser';
import { NgModule, Provider, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';


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
        ResizableModule], providers: [provideHttpClient(withInterceptorsFromDi()),FooterService, NavMenuService] })
export class AppModule { }
