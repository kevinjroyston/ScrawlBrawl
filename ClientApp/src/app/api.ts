
import { HttpClient, HttpHeaders } from '@angular/common/http';

//TODO: Change all "any" types to concrete, importable data types.
//TODO: clean up this class to simplify usage / logging of responses/errors to consoles is not done correctly.

type APIRequest = LobbyRequest; // | DummyRequest

type LobbyRequest = {
    type: "Lobby", 
    path: "Get" | "Create" | "Configure" | "Start" | "Games" | "Delete", 
    body?: {}
}

export class API {
    private http: HttpClient
    private baseUrl: string
    private httpOptions = {
        headers: new HttpHeaders({
            'Content-Type': 'application/json',
        })
    };

    constructor(http: HttpClient, baseUrl: string) {
        this.http = http;
        this.baseUrl = baseUrl;
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
            default: "return"
        }
        return await p//.then(this.logRequest(r)).catch(this.logError(r))    
    } 
    
    async Get(r: APIRequest) {
        return await this.http.get(this.getAPIPath(r)).toPromise()
    }

    async Post(r: APIRequest): Promise<any> {
        return await this.http.post(this.getAPIPath(r), r.body, this.httpOptions).toPromise()
    }

    getAPIPath = (r: APIRequest): string => {
        return `${this.baseUrl}${r.type}/${r.path}`;
    }
}
