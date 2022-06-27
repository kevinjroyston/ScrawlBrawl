import { Component, ElementRef, Inject, ViewEncapsulation, Input,Output,EventEmitter } from '@angular/core';
import { API } from '@core/http/api';
import GameplayPrompts from '@core/models/gameplay'


@Component({
  selector: 'waitingforgamestart',
  templateUrl: './waitingforgamestart.component.html',
  styleUrls: ['./waitingforgamestart.component.css'],
})
export class WaitingForGameStart {
  @Input() displayUsersParameters:GameplayPrompts.UserListMetadata;
  @Input() waitingForGameStartParameters:GameplayPrompts.WaitingForGameStartMetadata;
  @Input() lobbyId:string; 
  element;

  constructor(element: ElementRef, @Inject(API) private api: API) {
    this.element = element.nativeElement;
  }

  launchViewer(){
    window.open('/viewer/index.html?lobby='+this.lobbyId,'SBViewer');
  }  
  
  async onDeleteUser() {
    if (!confirm("Are you sure you want to leave this lobby?")) return false;
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
