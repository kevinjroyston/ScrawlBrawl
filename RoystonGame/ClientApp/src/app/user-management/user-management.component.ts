import { Component, Inject, ViewEncapsulation } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { API } from '../api';
import { NgForm } from '@angular/forms';

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
        @Inject('userId') userId: string) {
        this.api = new API(http, baseUrl, userId);
    }

    async onDeleteUser() {
        await this.api.request({ type: "User", path: "Delete" });
    }
}
