/***************************************************************************************************
 * Load `$localize` onto the global scope - used if i18n tags appear in Angular templates.
 */
//import '@angular/localize/init';
import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';


export function getUserIdOverrideQueryParameter() {
  const params = location.search.slice(1).split('&').reduce((acc, s) => {
    const [k, v] = s.split('=')
    return Object.assign(acc, { [k]: v })
  }, {})
  return params["idOverride"];
}

export function getUserIdQueryParameter() {
    const idKeyName: string = 'roystonGameUserId';
    var userId = localStorage.getItem(idKeyName);
    if (getUserIdOverrideQueryParameter())
    {
      userId = getUserIdOverrideQueryParameter();
    }
    if (!userId) {
        userId = genHexString(50);
        localStorage.setItem(idKeyName, userId);
    }
    return userId;
}

function genHexString(len) {
    let output = '';
    for (let i = 0; i < len; ++i) {
        output += (Math.floor(Math.random() * 16)).toString(16);
    }
    return output;
}
export function getBaseFrontendUrl() {
  if (environment.overrideUrlsWithBrowserHost){
    return document.getElementsByTagName('base')[0].href;
  }
  else
  {
    return environment.frontendUrl;
  }
}
export function getBaseBackendUrl() {
  if (environment.overrideUrlsWithBrowserHost){
    return document.getElementsByTagName('base')[0].href;
  }
  else
  {
    return environment.backendApiUrl;
  }
}
const providers = [
  { provide: 'BASE_FRONTEND_URL', useFactory: getBaseFrontendUrl, deps: [] },
  { provide: 'BASE_API_URL', useFactory: getBaseBackendUrl, deps: [] },
  { provide: 'userId', useFactory: getUserIdQueryParameter, deps: [] }
];

if (environment.production) {
  //enableProdMode();
}

platformBrowserDynamic(providers).bootstrapModule(AppModule)
  .catch(err => console.log(err));
