import { Component, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormControl, FormGroup } from '@angular/forms';
import { API } from '@core/http/api';
import GameplayPrompts from '@core/models/gameplay'
import User from '@core/models/user'
import { UserManager } from '@core/http/userManager';

@Component({
  selector: 'app-join-game',
  templateUrl: './join-game.component.html',
  styleUrls: ['../../../fetch-data/pages/fetch-data/fetch-data.component.scss']
})
export class JoinGameComponent {
  form = new FormGroup({
    DisplayName: new FormControl(''),
    LobbyID: new FormControl(''),
    SelfPortrait: new FormControl(''),
  });
  error : any;
  user: User;

  constructor(@Inject(API) private api: API, private router : Router, @Inject(UserManager) userManager) {
    userManager.getUserDataAndRedirect();
  }

  onSubmit = async () => {
    let requestBody = this.form.value
    let lobbyJoinRequest = await this.api.request({ 
      type: "Lobby", 
      path: "Join", 
      body: JSON.stringify(requestBody)
    })

    lobbyJoinRequest.subscribe({
        next: async () => {
          this.router.navigate(['/play']);
        },
        error: async (error) => {
          const firstError = Object.keys(error.error.errors)[0];
          const firstErrorMessage = error.error.errors[firstError][0]
          this.error = firstErrorMessage;
        }
    });
  }
}
