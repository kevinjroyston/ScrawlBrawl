
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, Subject, Subscriber } from 'rxjs';
import { AuthError, InteractionRequiredAuthError } from 'msal';
import { MsalService, BroadcastService } from '@azure/msal-angular';
import HttpEvent from 'msal/lib-commonjs/telemetry/HttpEvent';

//TODO: Change all "any" types to concrete, importable data types.
//TODO: clean up this class to simplify usage / logging of responses/errors to consoles is not done correctly.

type APIRequest = LobbyRequest | UserRequest | GameRequest;

type UserRequest = {
    type: "User",
    path: "Delete",
    body?: {}
}

type LobbyRequest = {
    type: "Lobby",
    path: "Get" | "Create" | "Configure" | "Start" | "Games" | "Delete",
    body?: {}
}

type GameRequest = {
    type: "Game",
    path: "CurrentContent" | "FormSubmit" | "AutoFormSubmit",
    body?: {}
}

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
    constructor(private http: HttpClient, private baseUrl: string,  userId: string, private authService: MsalService, private broadcastService: BroadcastService) {
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
                    case "Create": obs = this.Post(r);
                        break;
                    default: return;
                }
                break;
            case "User":
                switch (r.path) {
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
                        this.authService.acquireTokenPopup({
                            scopes: this.authService.getScopesForEndpoint(url)
                        }).then(() => {
                            this.http.get(url, this.getHttpOptions).subscribe({
                                next: (x) => subscriber.next(x),
                                error: (err) => subscriber.error(err),
                                complete: () => subscriber.complete()
                            });
                        });
                    }
                    else if ((err as HttpErrorResponse).status !== undefined && [401, 403].includes((err as HttpErrorResponse).status))
                    {
                        this.authService.acquireTokenPopup({
                            scopes: this.authService.getScopesForEndpoint(url)
                        }).then(() => {
                            this.http.get(url, this.getHttpOptions).subscribe({
                                next: (x) => subscriber.next(x),
                                error: (err) => subscriber.error(err),
                                complete: () => subscriber.complete()
                            });
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
                            this.authService.acquireTokenPopup({
                                scopes: this.authService.getScopesForEndpoint(url)
                            }).then(() => {
                                this.http.post(url, r.body, this.postHttpOptions).subscribe({
                                    next: (x) => subscriber.next(x),
                                    error: (err) => subscriber.error(err),
                                    complete: () => subscriber.complete()
                                });
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
}
