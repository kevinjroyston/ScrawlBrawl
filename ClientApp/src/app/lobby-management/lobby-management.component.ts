import { Component, Inject, ViewEncapsulation } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Component({
    selector: 'app-lobby-management',
    templateUrl: './lobby-management.component.html',
    styleUrls: ['./lobby-management.component.css'],

    encapsulation: ViewEncapsulation.None
})
export class LobbyManagementComponent {
    public baseUrl: string;

    constructor(
        private http: HttpClient,
        @Inject('BASE_URL') baseUrl: string) {
        this.baseUrl = baseUrl;

        // Temporary for testing.
        this.configure();
    }

    configure() {
        const httpOptions = {
            headers: new HttpHeaders({
                'Content-Type': 'application/json',
            })
        };
        // TODO: build these from user input and use actual classes instead of this garbage
        var body = JSON.stringify({ "GameMode": 2, "Options": [] });
        console.warn('Submitting response', body);
        this.http.post(this.baseUrl + "Lobby/Configure", body, httpOptions).subscribe(
            data => {
                console.log("POST Request is successful ", data);
            },
            error => {
                console.log("Error", error);
                // TODO: show this string to user.
            });
    }
}
