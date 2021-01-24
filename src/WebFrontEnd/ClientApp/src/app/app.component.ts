import { Component, OnInit } from '@angular/core';
import { MsalService, MsalBroadcastService } from '@azure/msal-angular';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {

    title = 'app';
    constructor(
        private msalService: MsalService,
        private broadcastService: MsalBroadcastService) {
    }
    ngOnInit() {
/*        
        this.broadcastService.subscribe('msal:acquireTokenSuccess', (payload) => {
            console.log('access token acquired at: ' + new Date().toString());
            console.log(payload);
        });

        this.broadcastService.subscribe('msal:acquireTokenFailure', (payload) => {
            console.log('access token acquisition fails');
            console.log(payload);
        });
*/        
    }

    onActivate = (event) => {
        window.scroll(0,0)
    }
}
