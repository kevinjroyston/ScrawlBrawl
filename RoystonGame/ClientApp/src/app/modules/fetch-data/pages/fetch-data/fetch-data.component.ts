import { Component, Inject, ViewEncapsulation, Pipe } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { FormBuilder, FormGroup, FormControl, Validators, FormArray } from '@angular/forms';
import { Time } from '@angular/common';
import { DomSanitizer, SafeHtml, SafeStyle, SafeScript, SafeUrl, SafeResourceUrl } from '@angular/platform-browser';
import { MatSlider } from '@angular/material/slider';
import { MsalService, BroadcastService } from '@azure/msal-angular';
import { API } from '@core/http/api';
import { isNullOrUndefined } from 'util';

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
    encapsulation: ViewEncapsulation.Emulated
})

export class FetchDataComponent
{
    public api: API;
    public userPrompt: UserPrompt;
    public userForm;
    public http: HttpClient;
    public baseUrl: string;
    private formBuilder: FormBuilder;
    private userPromptTimerId;
    private autoSubmitTimerId;
    private currentPromptId;
    private userId: string;

    constructor(
        http: HttpClient,
        formBuilder: FormBuilder,
        @Inject('BASE_URL') baseUrl: string,
        @Inject('userId') userId: string,
        @Inject(MsalService) authService: MsalService,
        @Inject(BroadcastService) broadcastService: BroadcastService
    )
    {
      this.api = new API(http, baseUrl, userId, authService, broadcastService)
      this.formBuilder = formBuilder;
      this.http = http;
      this.baseUrl = baseUrl;
      this.userId = userId;
      this.fetchUserPrompt();
    }

    async fetchUserPrompt() {
        if (this.userPromptTimerId) {
          clearTimeout(this.userPromptTimerId);
        }

        // fetch the current content from the server
        await this.api.request({ type: "Game", path: "CurrentContent"}).subscribe({
            next: async data => {
                var prompt = data as UserPrompt;

                // Too lazy to figure out how to properly deserialize things.
                prompt.autoSubmitAtTime = isNullOrUndefined(prompt.autoSubmitAtTime) ? null : new Date(prompt.autoSubmitAtTime);
                prompt.currentServerTime = isNullOrUndefined(prompt.currentServerTime) ? null : new Date(prompt.currentServerTime);

                console.log('Fetched Prompt', prompt);

                // if the current content has the same as id as the current, return
                if (this.userPrompt && this.userPrompt.id == prompt.id) {
                    this.refreshUserPromptTimer(prompt.refreshTimeInMs);
                    return;
                }

                // If you have reached this far it means we have switched to a new prompt, time to cleanup!

                // Clear the autosubmit timer
                if (this.autoSubmitTimerId) {
                  clearTimeout(this.autoSubmitTimerId);
                }

                // Start a new autosubmit timer
                if (prompt && !isNullOrUndefined(prompt.autoSubmitAtTime)) {
                    this.autoSubmitUserPromptTimer(prompt.autoSubmitAtTime.getTime() - prompt.currentServerTime.getTime());
                }

                // Clear whatever was in the old form.
                if (this.userForm) {
                    this.userForm.reset();
                }
                // Store the new user prompt and populate the corresponding formControls
                this.userPrompt = prompt;
                let subFormCount: number = 0;
                if (prompt && prompt.subPrompts && prompt.subPrompts.length > 0) {
                    subFormCount = prompt.subPrompts.length;
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
            },
            error: async (error) => {
                console.error(error);
            }
        });
    }
    refreshUserPromptTimer(ms: number): void {
        this.userPromptTimerId = setTimeout(() => this.fetchUserPrompt(), ms);
    }
    autoSubmitUserPromptTimer(ms: number): void {
        if (ms <= 0) {
            this.onSubmit(this.userForm?.value, true)
            return;
        }
        this.autoSubmitTimerId = setTimeout(() => this.onSubmit(this.userForm?.value, true), ms);
    }

  async onSubmit(userSubmitData, autoSubmit = false) {
        // Clear auto refresh timer as well as auto submit timer since we will be calling get after
        // the submission and both will get set there
        if (this.userPromptTimerId) {
          clearTimeout(this.userPromptTimerId);
        }
        if (this.autoSubmitTimerId) {
          clearTimeout(this.autoSubmitTimerId);
        }

        // Populate IDs.
        userSubmitData.id = this.userPrompt.id;
        for (let i = 0; i < userSubmitData.subForms.length; i++) {
            userSubmitData.subForms[i].id = this.userPrompt.subPrompts[i].id;
        }

        var body = JSON.stringify(userSubmitData);
        console.warn('Submitting response', body);
        var response;

        await this.api.request({ type: "Game", path: autoSubmit ? "AutoFormSubmit" : "FormSubmit", body: body }).subscribe({
            next: async (data) => {
                console.log("POST Request is successful ", data);
                this.fetchUserPrompt();
            },
            error: async (error) => {
                console.log("Error", error);
                this.fetchUserPrompt();
                if (error && error.error) {
                    this.userPrompt.error = error.error;
                }
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
interface UserPrompt {
    id: string;
    refreshTimeInMs: number;
    currentServerTime: Date;
    autoSubmitAtTime: Date;
    submitButton: boolean;
    title: string;
    description: string;
    subPrompts: SubPrompt[];
    error: string;
}
interface SubPrompt {
    id: string;
    prompt: string;
    color: string;
    stringList: string[];
    dropdown: string[];    
    answers: string[];
    colorPicker: boolean;
    shortAnswer: boolean;
    drawing: DrawingPromptMetadata;
    slider: SliderPromptMetadata;
    selector: SelectorPromptMetadata;
}
interface DrawingPromptMetadata {
    colorList: string[];
    widthInPx: number;
    heightInPx: number;
    premadeDrawing: string;
    canvasBackground: string;
}
interface SliderPromptMetadata {
  min: number;
  max: number;
  value: string;
  ticks: number[];
  range: boolean;
  ticksLabels: string[];
}
interface SelectorPromptMetadata {
  widthInPx: number;
  heightInPx: number;
  imageList: string[];
}
