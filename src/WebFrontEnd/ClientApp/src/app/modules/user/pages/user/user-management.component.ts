import { Component, Inject, ViewEncapsulation } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { API } from '@core/http/api';
import { FormControl, NgForm } from '@angular/forms';
import { MsalService, BroadcastService } from '@azure/msal-angular';
import Galleries from '@core/models/gallerytypes';

@Component({
    selector: 'app-user-management',
    templateUrl: './user-management.component.html',
    styleUrls: ['./user-management.component.css'],
})

export class UserManagementComponent {
    public userForm;
    
    galleryName;
    galleryTypes:Galleries.GalleryType[] = Galleries.galleryTypes;
    currentGalleryType = this.galleryTypes[0].galleryId;
    dropdownChoice = new FormControl(this.currentGalleryType);
    
    constructor(@Inject(API) private api: API)
    {
        
    }

    dropDownChanged(){
        if (this.dropdownChoice){
            this.currentGalleryType = this.dropdownChoice.value;
        }
    }
    async onDeleteUser() {
        await this.api.request({ type: "User", path: "Delete" }).subscribe({
            next: async data => {
                console.log("Left lobby");
            },
            error: async (error) => {
                console.error(error);
            }
        });
    }
}
