import { Component, OnInit } from '@angular/core';
import { MsalService, BroadcastService } from '@azure/msal-angular';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {

    title = 'app';
    constructor(
        private msalService: MsalService,
        private broadcastService: BroadcastService) {
    }
    ngOnInit() {
        const requestObj = {
            scopes: [
                //"api://f62ed1b5-3f4f-4c23-925a-0d27767707c6/ManageLobby"
            ]
        };

        const sub = this.broadcastService.subscribe('msal:acquireTokenFailure', () => {
            sub.unsubscribe();
            //this.msalService.loginPopup();
            this.msalService.acquireTokenPopup(requestObj);
        });
    }
}
