import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {

    title = 'app';
    ngOnInit() {
    }

    onActivate = (event) => {
        window.scroll(0,0)
    }
}
