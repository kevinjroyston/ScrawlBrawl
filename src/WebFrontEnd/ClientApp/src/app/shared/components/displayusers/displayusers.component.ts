import { Component, ElementRef, Inject, ViewEncapsulation, Input,Output,EventEmitter } from '@angular/core';
import GameplayPrompts from '@core/models/gameplay'


@Component({
  selector: 'displayusers',
  templateUrl: './displayusers.component.html',
  styleUrls: ['./displayusers.component.css'],
})
export class DisplayUsers {
  @Input() displayUsersParameters:GameplayPrompts.UserListPromptMetadata;
  element;

  constructor(element: ElementRef) {
    this.element = element.nativeElement;
  }
}
