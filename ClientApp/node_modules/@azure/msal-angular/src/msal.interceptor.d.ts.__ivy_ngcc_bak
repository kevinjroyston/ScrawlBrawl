import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from "@angular/common/http";
import { Observable } from "rxjs";
import { MsalService } from "./msal.service";
import { BroadcastService } from "./broadcast.service";
export declare class MsalInterceptor implements HttpInterceptor {
    private auth;
    private broadcastService;
    constructor(auth: MsalService, broadcastService: BroadcastService);
    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>>;
}
