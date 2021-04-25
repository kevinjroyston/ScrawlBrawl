import { Inject, Injectable } from "@angular/core";
import { Router } from "@angular/router";
import User from "@core/models/user";
import { API } from "./api";

@Injectable({
  providedIn: 'root'
})
export class UserManager{
  constructor(@Inject(API) private api: API, private router : Router) {
  }
  public async getUserDataAndRedirect(){
      let userRequest = await this.api.request({ 
        type: "User", 
        path: "Get"
      });
  
      userRequest.subscribe({
        next: async (data : User) => {
          if (data.LobbyId !== null){ // If we have a lobby id, go to play page.
              if (this.router.url !== '/play'){
                this.router.navigate(['/play']); // navigate to play page if not on play page.
              }
          }else{
            if (this.router.url !== '/join'){
              this.router.navigate(['/join']) // navigate to join page if not already on join page.
            }
          }
        },
        error: async (error) => {
          console.log(error);
        }
      });
    }
}