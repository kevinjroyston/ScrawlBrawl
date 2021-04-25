
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, Subject, Subscriber } from 'rxjs';
import { AuthError, InteractionRequiredAuthError } from 'msal';
import { MsalService, MsalBroadcastService } from '@azure/msal-angular';
import { environment } from 'environments/environment';
import { Inject, Injectable } from '@angular/core';
import { tokenRequest } from 'app/app-config';


//TODO: Change all "any" types to concrete, importable data types.
//TODO: clean up this class to simplify usage / logging of responses/errors to consoles is not done correctly.

type APIRequest = LobbyRequest | UserRequest | GameRequest;

type UserRequest = {
    type: "User",
    path: "Delete" | "Get",
    body?: {}
}

type LobbyRequest = {
    type: "Lobby",
    path: "Get" | "Create" | "Configure" | "Start" | "Games" | "Delete" | "Join",
    body?: {}
}

type GameRequest = {
    type: "Game",
    path: "CurrentContent" | "FormSubmit" | "AutoFormSubmit",
    body?: {}
}

@Injectable({
    providedIn: 'root'
})
export class API {

    private getHttpOptions = {
        params: {
            id: "--Set by constructor--"
        }, 
    };
    private postHttpOptions = {
        headers: new HttpHeaders({
            'Content-Type': 'application/json',
        }),
        params: {
            id: "--Set by constructor--"
        }
    };

    // TODO: instantiate api via dependency injection / make it injectable.
    constructor(
        @Inject(HttpClient) private http: HttpClient,
        @Inject('userId')userId: string,
        @Inject(MsalService) private authService: MsalService,
        @Inject('BASE_API_URL') private baseUrl: string)
    {
        this.postHttpOptions.params.id = userId;
        this.getHttpOptions.params.id = userId;
    }


    logRequest = (r: APIRequest) => {
    return (data: any) => {
        console.log(`[APIResponse] ${r.type}/${r.path}`, data)
        }
    }

    logError = (r: APIRequest) => {
    return (data: any) => {
        console.log(`[APIResponse] ${r.type}/${r.path}`, data)
        }
    }

    request = (r: APIRequest): Observable<Object> => {
        console.log(`[APIRequest] ${r.type}/${r.path}`, r.body)
        let obs: Observable<Object> = null;
        switch (r.type) {
            case "Lobby":
                switch (r.path) {
                    case "Get":
                    case "Start":
                    case "Delete":
                    case "Games": obs = this.Get(r);
                        break;
                    case "Configure":
                    case "Join": obs = this.Post(r);
                        break;
                    case "Create": obs = this.Post(r);
                        break;
                    default: return;
                }
                break;
            case "User":
                switch (r.path) {
                    case "Get": obs = this.Get(r);
                        break;
                    case "Delete": obs = this.Get(r);
                        break;
                    default: return;
                }
                break;
            case "Game":
                switch (r.path) {
                    case "CurrentContent": obs = this.Get(r);
                        break;
                    case "FormSubmit":
                    case "AutoFormSubmit": obs = this.Post(r);
                        break;
                    default: return;
                }
                break;
            default: "return"
        }
        return obs;
    } 

    Get(r: APIRequest): Observable<Object> {
        var url = this.getAPIPath(r);

        // I do not like this sam I am.
        var observable = new Observable(subscriber =>
        {
            this.http.get(url, this.getHttpOptions).subscribe(
            {
                next: (x) => {
                    subscriber.next(x);
                },
                error: (err: unknown) => {
                    // If there is an interaction required error,
                    // call one of the interactive methods and then make the request again.
                    if ((err as AuthError).errorCode !== undefined && InteractionRequiredAuthError.isInteractionRequiredError((err as AuthError).errorCode)) {
                        this.authService.acquireTokenPopup(
                            this.getScopesForEndpoint(url)
                        ).subscribe({
                            next: () => {
                                this.http.get(url, this.getHttpOptions).subscribe({
                                    next: (x) => subscriber.next(x),
                                    error: (err) => subscriber.error(err),
                                    complete: () => subscriber.complete()
                                });
                            },
                            error: (err) =>  subscriber.error(err)
                        });
                    }
                    else if ((err as HttpErrorResponse).status !== undefined && [401, 403].includes((err as HttpErrorResponse).status))
                    {
                        this.authService.acquireTokenPopup(
                            this.getScopesForEndpoint(url)
                        ).subscribe({
                            next: () => {
                                this.http.get(url, this.getHttpOptions).subscribe({
                                    next: (x) => subscriber.next(x),
                                    error: (err) => subscriber.error(err),
                                    complete: () => subscriber.complete()
                                });
                            },
                            error: (err) =>  subscriber.error(err)
                        });
                    }
                    else
                    {
                        subscriber.error(err);
                    }
                },
                complete: () => {
                    subscriber.complete();
                }
            });
        });
        return observable;
    }

    Post(r: APIRequest): Observable<Object> {
        var url = this.getAPIPath(r);

        // I do not like this sam I am.
        var observable = new Observable(subscriber => {
            this.http.post(url, r.body, this.postHttpOptions).subscribe(
                {
                    next: (x) => {
                        subscriber.next(x);
                    },
                    error: (err: AuthError) => {
                        // If there is an interaction required error,
                        // call one of the interactive methods and then make the request again.
                        if (InteractionRequiredAuthError.isInteractionRequiredError(err.errorCode)) {
                            this.authService.acquireTokenPopup(
                                this.getScopesForEndpoint(url)
                            ).subscribe({
                                next: () => {
                                    this.http.get(url, this.getHttpOptions).subscribe({
                                        next: (x) => subscriber.next(x),
                                        error: (err) => subscriber.error(err),
                                        complete: () => subscriber.complete()
                                    });
                                },
                                error: (err) =>  subscriber.error(err)
                            });
                        } else {
                            subscriber.error(err);
                        }
                    },
                    complete: () => {
                        subscriber.complete();
                    }
                });
        });
        return observable;
    }    

    getAPIPath = (r: APIRequest): string => {
        return `${this.baseUrl}api/v1/${r.type}/${r.path}`;
    }
    getScopesForEndpoint(url): any {
        return tokenRequest;
    }
}
