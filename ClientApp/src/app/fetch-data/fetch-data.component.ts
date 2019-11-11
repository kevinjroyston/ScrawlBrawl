import { Component, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { FormBuilder, FormGroup, FormControl, Validators, FormArray } from '@angular/forms';
import { Time } from '@angular/common';


@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent
{
    public userPrompt: UserPrompt;
    public userForm;
    public http: HttpClient;
    public baseUrl: string;
    private formBuilder: FormBuilder;

  constructor(
      http: HttpClient,
      formBuilder: FormBuilder,
      @Inject('BASE_URL') baseUrl: string)
  {
      this.formBuilder = formBuilder;
      //this.userForm = formBuilder.group({ radioAnswer: '' })
     // this.userForm = new FormGroup({ radioAnswer: new FormControl() });

      this.http = http;
      this.baseUrl = baseUrl;
    http.get<UserPrompt>(baseUrl + 'currentContent').subscribe(result => {
        this.userPrompt = result;
        let subFormCount:number = 0;
        if (result && result.subPrompts && result.subPrompts.length > 0) {
            subFormCount = result.subPrompts.length;
        } 
        this.userForm = this.formBuilder.group({
            id: '',
            subForms: this.formBuilder.array(this.makeSubForms(subFormCount))
        });
    }, error => console.error(error));
  }
    onSubmit(userSubmitData) {
      const httpOptions = {
          headers: new HttpHeaders({
              'Content-Type': 'application/json',
          })
        };
      // Pass IDs back
      userSubmitData.id = this.userPrompt.id;
      for (let i = 0; i < userSubmitData.subForms.length; i++) {
          userSubmitData.subForms[i].id = this.userPrompt.subPrompts[i].id;
      }
      var body = JSON.stringify(userSubmitData);
      console.warn('Submitting response', body);
      this.http.post(this.baseUrl +"FormSubmit", body, httpOptions).subscribe((data) => { });
      this.userForm.reset();
    }

    createSubForm(): FormGroup {
        return this.formBuilder.group({
            id: '',
            radioAnswer: '',
            shortAnswer: '',
            drawing: '',
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
interface UserPrompt {
    id: string;
    refreshTime: Time;
    submitButton: boolean;
    title: string;
    description: string;
    subPrompts: SubPrompt[];
}
interface SubPrompt {
    id: string;
    prompt: string;
    answers: string[];
    shortAnswer: boolean;
    drawing: boolean;
}
