import { Component, Inject } from '@angular/core';
import { MatBottomSheet } from '@angular/material/bottom-sheet';
import GameplayPrompts from '@core/models/gameplay'
import { FormBuilder, FormGroup } from '@angular/forms';
import { API } from '@core/http/api';

@Component({
  selector: 'app-join-game',
  templateUrl: './join-game.component.html',
  styleUrls: ['../../../fetch-data/pages/fetch-data/fetch-data.component.scss']
})
export class JoinGameComponent {
  public userPrompt: GameplayPrompts.UserPrompt;
  public userForm;
  private userPromptTimerId;
  private formBuilder: FormBuilder;
  private autoSubmitTimerId;

  constructor(formBuilder: FormBuilder, private _colorPicker: MatBottomSheet, @Inject(API) private api: API) {
    this.formBuilder = formBuilder;
    this.fetchUserPrompt();
  }

  async onSubmit(userSubmitData, autoSubmit = false) {
    // Clear auto refresh timer as well as auto submit timer since we will be calling get after
    // the submission and both will get set there
    if (this.userPromptTimerId) {
      clearTimeout(this.userPromptTimerId);
      this.userPromptTimerId = null;
    }
    if (this.autoSubmitTimerId) {
      clearTimeout(this.autoSubmitTimerId);
      this.autoSubmitTimerId = null;
    }

    // Populate IDs.
    userSubmitData.id = this.userPrompt.id;
    for (let i = 0; i < userSubmitData.subForms.length; i++) {
        userSubmitData.subForms[i].id = this.userPrompt.subPrompts[i].id;
        if (this.userPrompt.subPrompts[i].selector && !userSubmitData.subForms[i].selector) {
            userSubmitData.subForms[i].selector="0";
        }
    }

    var body = JSON.stringify(userSubmitData);
    console.warn('Submitting response', body);

    await this.api.request({ type: "Game", path: autoSubmit ? "AutoFormSubmit" : "FormSubmit", body: body }).subscribe({
        next: async (data) => {
            console.log("POST Request is successful ", data);
            this.fetchUserPrompt();
        },
        error: async (error) => {
            console.log("Error", error);
            if (error && error.error) {
                this.userPrompt.error = error.error;
            }
        }
    });
  }

  async fetchUserPrompt() {
    console.log("fetchUserPrompt: userPromptTimerId="+this.userPromptTimerId);
    if (this.userPromptTimerId) {
      clearTimeout(this.userPromptTimerId);
      this.userPromptTimerId = null;
    }

    // fetch the current content from the server
    await this.api.request({ type: "Game", path: "CurrentContent"}).subscribe({
        next: async data => {
          const prompt = data as GameplayPrompts.UserPrompt;
          this.userPrompt = prompt;
          let subFormCount: number = 0;
          if (prompt && prompt.subPrompts && prompt.subPrompts.length > 0) {
              subFormCount = prompt.subPrompts.length;
          }
          this.userForm = this.formBuilder.group({
              id: '',
              subForms: this.formBuilder.array(this.makeSubForms(subFormCount))
          });
        },
        error: async (error) => {
            console.error(error);
        }
    });
}

createSubForm(): FormGroup {
  return this.formBuilder.group({
      id: '',
      dropdownChoice: '',
      radioAnswer: '',
      shortAnswer: '',
      color: '',
      drawing: '',
      slider: '',
      selector: '',
  });
}

makeSubForms(len: number): FormGroup[] {
  let arr:FormGroup[] = [];
  for (let i = 0; i < len; i++) {
      arr.push(this.createSubForm());
  }
  return arr;
}

}
