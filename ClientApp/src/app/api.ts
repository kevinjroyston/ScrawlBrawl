
import { HttpClient, HttpHeaders } from '@angular/common/http';

//TODO: Change all "any" types to concrete, importable data types.
//TODO: clean up this class to simplify usage / logging of responses/errors to consoles is not done correctly.

type APIRequest = LobbyRequest | UserRequest;

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

export class API {
    private http: HttpClient
    private baseUrl: string

    private getHttpOptions = {
        params: {
            id: "--Set by constructor--"
        }
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
    constructor(http: HttpClient, baseUrl: string,  userId: string) {
        this.http = http;
        this.baseUrl = baseUrl;
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

    request = async (r: APIRequest): Promise<any> => {
        console.log(`[APIRequest] ${r.type}/${r.path}`, r.body)
        let p: Promise<any> = null;
        switch (r.type) {
            case "Lobby":
                switch (r.path) {
                    case "Get":
                    case "Start":
                    case "Delete":
                    case "Games": p = this.Get(r);
                        break;
                    case "Configure":
                    case "Create": p = this.Post(r);
                        break;
                    default: return;
                }
            case "User":
                switch (r.path) {
                    case "Delete": p = this.Get(r);
                        break;
                    default: return;
                }
            default: "return"
        }
        return await p;
    } 
    
    async Get(r: APIRequest) {
        return await this.http.get(this.getAPIPath(r), this.getHttpOptions).toPromise();
    }

    async Post(r: APIRequest): Promise<any> {
        return await this.http.post(this.getAPIPath(r), r.body, this.postHttpOptions).toPromise()
    }

    getAPIPath = (r: APIRequest): string => {
        return `${this.baseUrl}${r.type}/${r.path}`;
    }
}
