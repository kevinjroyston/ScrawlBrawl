import { Component, Inject, ViewEncapsulation, Pipe } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { FormBuilder, FormGroup, FormControl, Validators, FormArray } from '@angular/forms';
import { Time } from '@angular/common';
import { DomSanitizer, SafeHtml, SafeStyle, SafeScript, SafeUrl, SafeResourceUrl } from '@angular/platform-browser';



@Pipe({ name: 'safe' })
export class Safe {
    constructor(private _sanitizer: DomSanitizer) { }

    public transform(value: string, type: string): SafeHtml | SafeStyle | SafeScript | SafeUrl | SafeResourceUrl {
        switch (type) {
            case 'html':
                return this._sanitizer.bypassSecurityTrustHtml(value);
            case 'style':
                return this._sanitizer.bypassSecurityTrustStyle(value);
            case 'script':
                return this._sanitizer.bypassSecurityTrustScript(value);
            case 'url':
                return this._sanitizer.bypassSecurityTrustUrl(value);
            case 'resourceUrl':
                return this._sanitizer.bypassSecurityTrustResourceUrl(value);
            default:
                throw new Error(`Unable to bypass security for invalid type: ${type}`);
        }
    }
}

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html',
    styleUrls: ['./fetch-data.component.css'],

    encapsulation: ViewEncapsulation.None
})
export class FetchDataComponent
{
    public userPrompt: UserPrompt;
    public userForm;
    public http: HttpClient;
    public baseUrl: string;
    private formBuilder: FormBuilder;
    private userPromptTimerId;
    private currentPromptId;

    constructor(
        http: HttpClient,
        formBuilder: FormBuilder,
        @Inject('BASE_URL') baseUrl: string)
    {
      this.formBuilder = formBuilder;
      this.http = http;
      this.baseUrl = baseUrl;
      this.fetchUserPrompt();
    }

    fetchUserPrompt(): void {
      // poor attempt at removing race condition on submit + regular fetch cycle.
      // might actually work if my understanding of setTmeout is correct.
      if (this.userPromptTimerId) {
        clearTimeout(this.userPromptTimerId);
      }
      // fetch the current content from the server
      this.http.get<UserPrompt>(this.baseUrl + 'currentContent').subscribe(result => {
        // if the current content has the same as id as the current, return
        if (this.userPrompt && this.userPrompt.id == result.id) {
          this.refreshUserPromptTimer(this.userPrompt.refreshTimeInMs);
          return;
          }

        // Clear whatever was in the old form.
        if (this.userForm) {
            this.userForm.reset();
        }
        // Store the new user prompt and populate the corresponding formControls
        this.userPrompt = result;
        let subFormCount: number = 0;
        if (result && result.subPrompts && result.subPrompts.length > 0) {
            subFormCount = result.subPrompts.length;
        }
        this.userForm = this.formBuilder.group({
            id: '',
            subForms: this.formBuilder.array(this.makeSubForms(subFormCount))
        });
        // reset the timer to call again
        this.refreshUserPromptTimer(this.userPrompt.refreshTimeInMs);

        // Reset the form again because you are a bad coder
        if (this.userForm) {
            this.userForm.reset();
        }
      }, error => {
        console.error(error);
        this.refreshUserPromptTimer(1000);
      });
    }
    refreshUserPromptTimer(ms:number):void {
        this.userPromptTimerId = setTimeout(() => this.fetchUserPrompt(), ms);
    }

    onSubmit(userSubmitData) {
      const httpOptions = {
        headers: new HttpHeaders({
            'Content-Type': 'application/json',
        })
      };
      // Populate IDs.
      userSubmitData.id = this.userPrompt.id;
      for (let i = 0; i < userSubmitData.subForms.length; i++) {
          userSubmitData.subForms[i].id = this.userPrompt.subPrompts[i].id;
      }

      var body = JSON.stringify(userSubmitData);
        console.warn('Submitting response', body);
        var response;
        this.http.post(this.baseUrl + "FormSubmit", body, httpOptions).subscribe(
            data => {
                console.log("POST Request is successful ", data);
                this.fetchUserPrompt();
            },
            error => {
                console.log("Error", error);
                this.fetchUserPrompt();
            });
    }

    createSubForm(): FormGroup {
        return this.formBuilder.group({
            id: '',
            radioAnswer: '',
            shortAnswer: '',
            color: '',
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
    refreshTimeInMs: number;
    submitButton: boolean;
    title: string;
    description: string;
    subPrompts: SubPrompt[];
}
interface SubPrompt {
    id: string;
    prompt: string;
    stringList: string[];
    answers: string[];
    colorPicker: boolean;
    shortAnswer: boolean;
    drawing: DrawingPromptMetadata;
}
interface DrawingPromptMetadata {
    color: string;
    widthInPx: number;
    heightInPx: number;
    premadeDrawing: string;
    canvasBackground: string;
}
