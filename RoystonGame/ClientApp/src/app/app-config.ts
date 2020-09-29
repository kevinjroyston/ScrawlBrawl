import { Configuration } from 'msal';
import { MsalAngularConfiguration } from '@azure/msal-angular';
import { environment } from '../environments/environment';

// Copied with minor modifications from: https://github.com/Azure-Samples/active-directory-b2c-javascript-angular-spa/blob/master/src/app/app-config.ts


// this checks if the app is running on IE
export const isIE = window.navigator.userAgent.indexOf('MSIE ') > -1 || window.navigator.userAgent.indexOf('Trident/') > -1;

/** =================== REGIONS ====================
 * 1) B2C policies and user flows
 * 2) Web API configuration parameters
 * 3) Authentication configuration parameters
 * 4) MSAL-Angular specific configuration parameters
 * ================================================= 
*/

// #region 1) B2C policies and user flows
/**
 * Enter here the user flows and custom policies for your B2C application,
 * To learn more about user flows, visit https://docs.microsoft.com/en-us/azure/active-directory-b2c/user-flow-overview
 * To learn more about custom policies, visit https://docs.microsoft.com/en-us/azure/active-directory-b2c/custom-policy-overview
 */
export const b2cPolicies = {
    names: {
        signUpSignIn: "b2c_1_SignUpSignIn",
        resetPassword: "b2c_1_PasswordReset",
    },
    authorities: {
        signUpSignIn: {
            authority: "https://scrawlbrawl.b2clogin.com/scrawlbrawl.onmicrosoft.com/b2c_1_SignUpSignIn"
        },
        resetPassword: {
            authority: "https://scrawlbrawl.b2clogin.com/scrawlbrawl.onmicrosoft.com/b2c_1_PasswordReset"
        }
    }
}
// #endregion


// #region 2) Web API Configuration
/** 
 * Enter here the coordinates of your Web API and scopes for access token request
 * The current application coordinates were pre-registered in a B2C tenant.
 */
export const apiConfig: { b2cScopes: string[], webApi: string } = {
    b2cScopes: ['https://ScrawlBrawl.onmicrosoft.com/5c59c94a-140d-4c49-a4ed-772a55c52d57/LobbyManagement'],
    webApi: 'https://scrawlbrawl.tv'
};
// #endregion



// #region 3) Authentication Configuration
/** 
 * Config object to be passed to Msal on creation. For a full list of msal.js configuration parameters,
 * visit https://azuread.github.io/microsoft-authentication-library-for-js/docs/msal/modules/_configuration_.html
 */
export const msalConfig: Configuration = {
    auth: {
        clientId: "5c59c94a-140d-4c49-a4ed-772a55c52d57",
        authority: b2cPolicies.authorities.signUpSignIn.authority,
        redirectUri:  "https://scrawlbrawl.tv/lobby/manage",
        postLogoutRedirectUri: "https://scrawlbrawl.tv/",
        navigateToLoginRequestUrl: true,
        validateAuthority: false,
    },
    cache: {
        cacheLocation: "localStorage",
        storeAuthStateInCookie: isIE, // Set this to "true" to save cache in cookies to address trusted zones limitations in IE
    },
}

/** 
 * Scopes you enter here will be consented once you authenticate. For a full list of available authentication parameters, 
 * visit https://azuread.github.io/microsoft-authentication-library-for-js/docs/msal/modules/_authenticationparameters_.html
 */
export const loginRequest: { scopes: string[] } = {
    scopes: ['openid', 'profile'],
};

// Scopes you enter will be used for the access token request for your web API
export const tokenRequest: { scopes: string[] } = {
    scopes: apiConfig.b2cScopes
};
// #endregion



// #region 4) MSAL-Angular Configuration
// here you can define the coordinates and required permissions for your protected resources
export const protectedResourceMap: [string, string[]][] = environment.enableMsal ? [
    [apiConfig.webApi, apiConfig.b2cScopes],
    ['lobby/manage', apiConfig.b2cScopes],
    ['api/v1/Lobby/Games', apiConfig.b2cScopes],
    ['api/v1/Lobby/Get', apiConfig.b2cScopes],
    ['api/v1/Lobby/Create', apiConfig.b2cScopes],
    ['api/v1/Lobby/Configure', apiConfig.b2cScopes],
    ['api/v1/Lobby/Delete', apiConfig.b2cScopes],
    ['api/v1/Lobby/Start', apiConfig.b2cScopes],
] : [];

/** 
 * MSAL-Angular specific authentication parameters. For a full list of available options,
 * visit https://github.com/AzureAD/microsoft-authentication-library-for-js/tree/dev/lib/msal-angular#config-options-for-msal-initialization
*/
export const msalAngularConfig: MsalAngularConfiguration = {
    popUp: !isIE,
    consentScopes: [
        ...loginRequest.scopes,
        ...tokenRequest.scopes,
    ],
  unprotectedResources: ["api/v1/Game/CurrentContent", "api/v1/Game/FormSubmit", "api/v1/Game/AutoFormSubmit", "api/v1/User/Delete"], // API calls to these coordinates will NOT activate MSALGuard
    protectedResourceMap,     // Scopes to use
  extraQueryParameters: {}

}
// #endregion
