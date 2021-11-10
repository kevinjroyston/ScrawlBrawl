import { Component, EventEmitter, Inject, OnInit, Output } from '@angular/core';
import { Router } from '@angular/router';
import { FormControl, FormGroup } from '@angular/forms';
import { API } from '@core/http/api';
import GameplayPrompts from '@core/models/gameplay'
import User from '@core/models/user'
import { UserManager } from '@core/http/userManager';
import * as localStorage from "app/utils/localstorage";


@Component({
  selector: 'app-create-lobby',
  templateUrl: './create-lobby.component.html',
  styleUrls: ['../../../../fetch-data/pages/fetch-data/fetch-data.component.scss']
})
export class CreateLobbyComponent implements OnInit {
  @Output() onCreateLobby = new EventEmitter();
  @Output() onCreateAndJoinLobby = new EventEmitter();
  public showPrompts = false;
  constructor(@Inject(API) private api: API) {
  }
  ngOnInit() {
  }

  form = new FormGroup({
    DisplayName: new FormControl(this.fetchDisplayName()),
    SelfPortrait: new FormControl(''),
  });
  error : any;
  user: User;

  fetchDisplayName():string {return localStorage.fetchLocalStorage("Join","DisplayName")}

  onCreateAndJoinLobbySubmit = async () => {
    if (!this.showPrompts) { this.showPrompts = true; return false}
    let requestBody = this.form.value;
    requestBody.LobbyId = "temp";    
    localStorage.storeLocalStorage("Join","DisplayName",this.form.controls.DisplayName.value);    
    this.onCreateAndJoinLobby.emit(requestBody);
  }
  onCreateLobbySubmit = async () => {
    if (this.showPrompts) { this.showPrompts = false; return false}
    this.onCreateLobby.emit();
  }
}
