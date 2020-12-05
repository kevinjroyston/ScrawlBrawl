import { APP_INITIALIZER, NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ThemeService, StorageService } from './services/theme.service';

export function themeFactory(themeService: ThemeService) {
  return () => themeService.setThemeInit();
}


@NgModule({
  imports: [
    CommonModule
  ],
  declarations: [],
  providers: [
    ThemeService,
    StorageService,
    {provide: APP_INITIALIZER, useFactory: themeFactory, deps: [ThemeService], multi: true},
  ]
})
export class CoreModule { }
