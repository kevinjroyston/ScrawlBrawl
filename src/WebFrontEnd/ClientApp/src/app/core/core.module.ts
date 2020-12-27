import { APP_INITIALIZER, NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ThemeService } from './services/theme.service';
import { StorageService } from './services/storage.service';
import { NotificationService } from './services/notification.service';

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
    NotificationService,
    {
      provide: APP_INITIALIZER, 
      useFactory: themeFactory, 
      deps: [ThemeService], 
      multi: true
    }
  ]
})
export class CoreModule { }
