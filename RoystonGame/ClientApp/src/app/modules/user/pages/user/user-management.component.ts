import { Component, Inject, ViewEncapsulation } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { API } from '@core/http/api';
import { NgForm } from '@angular/forms';
import { MsalService, BroadcastService } from '@azure/msal-angular';

@Component({
    selector: 'app-user-management',
    templateUrl: './user-management.component.html',
    styleUrls: ['./user-management.component.css'],

    encapsulation: ViewEncapsulation.None
})

export class UserManagementComponent {
    public api: API;

    constructor(
        private http: HttpClient,
        @Inject('BASE_URL') baseUrl: string,
        @Inject('userId') userId: string,
        @Inject(MsalService) authService: MsalService,
        @Inject(BroadcastService) broadcastService: BroadcastService
    ) {
        // TODO. Broadcast API through DI so dont have to pass through 4 values.
        this.api = new API(http, baseUrl, userId, authService, broadcastService);
    }

    async onDeleteUser() {
        await this.api.request({ type: "User", path: "Delete" });
    }
}
