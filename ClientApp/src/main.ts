import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

export function getBaseUrl() {
    return document.getElementsByTagName('base')[0].href;
}

export function getUserIdOverrideQueryParameter() {
    const params = location.search.slice(1).split('&').reduce((acc, s) => {
        const [k, v] = s.split('=')
        return Object.assign(acc, { [k]: v })
    }, {})
    return params["idOverride"];
}

const providers = [
  { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] },
  { provide: 'userIdOverride', useFactory: getUserIdOverrideQueryParameter, deps: [] }
];

if (environment.production) {
  enableProdMode();
}

platformBrowserDynamic(providers).bootstrapModule(AppModule)
  .catch(err => console.log(err));
