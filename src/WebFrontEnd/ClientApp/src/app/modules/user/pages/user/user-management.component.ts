import { Component, Inject, ViewEncapsulation } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router'; 
import { API } from '@core/http/api';
import { MsalService, BroadcastService } from '@azure/msal-angular';
import Galleries from '@core/models/gallerytypes';

@Component({
    selector: 'app-user-management',
    templateUrl: './user-management.component.html',
    styleUrls: ['./user-management.component.scss'],
})

export class UserManagementComponent {
    public userForm;
    
    galleryTypes = [...Galleries.galleryTypes]; /* needed so html page can see it */
    currentDrawingType = this.galleryTypes[0].drawingType;
    
    constructor(@Inject(API) private api: API, private router: Router)
    {
        
    }

    async onDeleteUser() {
        await this.api.request({ type: "User", path: "Delete" }).subscribe({
            next: async data => {
                console.log("Left lobby");
                this.router.navigate(['/play'])
            },
            error: async (error) => {
                console.error(error);
            }
        });
    }
}
