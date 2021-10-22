import { Component, HostListener, Inject, ViewEncapsulation, Pipe, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup} from '@angular/forms';
import { DomSanitizer, SafeHtml, SafeStyle, SafeScript, SafeUrl, SafeResourceUrl } from '@angular/platform-browser';
import GameplayPrompts from '@core/models/gameplay' 
import { API } from '@core/http/api';
import { Router } from '@angular/router';
import {MatBottomSheet, MatBottomSheetConfig} from '@angular/material/bottom-sheet';
import { ColorPickerComponent } from '@shared/components/colorpicker/colorpicker.component';
import {DrawingPromptMetadata} from '@shared/components/drawingdirective.component';
import Galleries from '@core/models/gallerytypes';
import { Suggestions } from '@core/http/suggestions';
import { UnityViewer } from '@core/http/viewerInjectable';
import * as drawingUtils from "app/utils/drawingutils";
import { HttpHeaders, HttpParams } from '@angular/common/http';
import { UserManager } from '@core/http/userManager';
import { NotificationService } from '@core/services/notification.service';

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
    styleUrls: ['./fetch-data.component.scss'],
    encapsulation: ViewEncapsulation.Emulated
})

export class FetchDataComponent implements OnDestroy
{
    public userPrompt: GameplayPrompts.UserPrompt;
    public userForm;
    public anythingEverTouched = false;
    public anythingTouchedSinceFetch = false;
    public displayUsersMetadata: GameplayPrompts.UserListMetadata;
    private formBuilder: FormBuilder;
    private userPromptTimerId;
    private autoSubmitTimerId;
    private timerDisplayIntervalId;


    timerDisplay = '';
    timerRemaining = 0;
    timerColor = 'green';
    galleryEditorVisible = false;
    galleryTypes = [...Galleries.galleryTypes]; /* so html page can see the reference.  ... is SPREAD command: https://www.samanthaming.com/tidbits/35-es6-way-to-clone-an-array/ */
    currentDrawingType = this.galleryTypes[0].drawingType;

    constructor(
        formBuilder: FormBuilder,
        private router: Router,
        private _colorPicker: MatBottomSheet,
        @Inject(NotificationService) private notificationService: NotificationService,
        @Inject(API) private api: API,
        @Inject(UnityViewer) private unityViewer:UnityViewer,
        @Inject(UserManager) userManager,
        @Inject(Suggestions) private suggestions: Suggestions)
    {
      userManager.getUserDataAndRedirectToPlayOrJoin();
      this.formBuilder = formBuilder;
      this.fetchUserPrompt();
      router.events.subscribe((val) => {
        if (this.userPromptTimerId) {
            clearTimeout(this.userPromptTimerId);
            this.userPromptTimerId = null;
          }
        if (this.autoSubmitTimerId) {
            this.clearAutoSubmitTimers();
          }
        });
    }
    ngOnDestroy(): void {
        document.body.classList.remove('makeRoomForToolbar');
    }

    handleColorChange = (color: string, subPrompt: number) => {
        this.userPrompt.subPrompts[subPrompt].color = drawingUtils.convertColorToHexRGB(color);
    }

    openColorPicker = (event: MouseEvent, subPrompt: number): void => {
        event.preventDefault();
        const bottomConfig = new MatBottomSheetConfig();
        bottomConfig.data = {
            handleColorChange: (color: string) => this.handleColorChange(color, subPrompt),
            panelClass: 'sb-colorpicker-dialog'
        }
        this._colorPicker.open(ColorPickerComponent, bottomConfig)
    }

    async fetchUserPrompt() {
        console.log("fetchUserPrompt: userPromptTimerId="+this.userPromptTimerId);

        if (this.userPromptTimerId) {
          clearTimeout(this.userPromptTimerId);
          this.userPromptTimerId = null;
        }

        // fetch the current content from the server
        var dirtyBit = this.anythingTouchedSinceFetch;
        this.anythingTouchedSinceFetch = false;
        var thisId = this.userPrompt? this.userPrompt.id : "";

        await this.api.request({ type: "Game", params: {promptId: thisId, db: (dirtyBit ? "True" : "False")}, path: "CurrentContent"}).subscribe({
            next: async data => {
                var currentContent = data as GameplayPrompts.CurrentContent;
                if (currentContent == null) // If no prompt object we have not joined lobby. Redirect accordingly.
                {
                    console.error("Joined lobby but not seeing a prompt.");
                    this.router.navigate(['/join']); // an error occurred, go to the join page
                    return;
                }
                                
                this.displayUsersMetadata = currentContent.userListMetadata;

                // if the current content has the same as id as the current, return
                if (this.userPrompt && this.userPrompt.id == currentContent.promptId) {
                    this.refreshUserPromptTimer(currentContent.refreshTimeInMs);
                    return;
                }

                var prompt = currentContent.userPrompt;

                if (prompt.submitButton) {
                    document.body.classList.add('makeRoomForToolbar');
                } else {
                    document.body.classList.remove('makeRoomForToolbar');
                }
      
                this.unityViewer.UpdateLobbyId(prompt.lobbyId);

                // Too lazy to figure out how to properly deserialize things.
                prompt.autoSubmitAtTime = !prompt.autoSubmitAtTime ? null : new Date(prompt.autoSubmitAtTime);
                prompt.currentServerTime = !prompt.currentServerTime ? null : new Date(prompt.currentServerTime);

                console.log('Fetched Prompt', prompt);

                // If you have reached this far it means we have switched to a new prompt, time to cleanup!

                // Clear the autosubmit timer
                if (this.autoSubmitTimerId) {
                    this.clearAutoSubmitTimers();
                }


                // Start a new autosubmit timer
                if (prompt && prompt.autoSubmitAtTime) {
                    this.timerRemaining = prompt.autoSubmitAtTime.getTime() - prompt.currentServerTime.getTime();
                    this.autoSubmitUserPromptTimer(prompt.autoSubmitAtTime.getTime() - prompt.currentServerTime.getTime());
                }

                // Clear whatever was in the old form.
                if (this.userForm) {
                    this.userForm.reset();
                }

                this.galleryEditorVisible = false;

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

    assignSuggestions(userSubmitData,data): void{
        var suggestion = JSON.parse(data);
        if (!suggestion) {
            this.notificationService.addMessage(`Sorry, you're on your own this round.`, null, {panelClass: ['error-snackbar']});
            return;
        }
        for (let i = 0; i < Math.min(userSubmitData.subForms.length,suggestion.values.length); i++) {
            if (suggestion.values[i] != "") {
                if (this.userPrompt.subPrompts[i].shortAnswer) {
                    this.userForm.controls.subForms.controls[i].patchValue({shortAnswer:suggestion.values[i]});
                }
                if (this.userPrompt.subPrompts[i].colorPicker) {
                    this.userPrompt.subPrompts[i].color = suggestion.values[i];
                    this.userForm.controls.subForms.controls[i].patchValue({colorPicker:suggestion.values[i]});
                }
//                if (this.userPrompt.subPrompts[i].selector) { this.userPrompt.subPrompts[i].selector = suggestion.value[i]) }
//                if (this.userPrompt.subPrompts[i].slider) { this.userPrompt.subPrompts[i].slider.value=suggestion.value[i]; }
            }
        }
    }

    suggestValues(): void {
        this.suggestions.fetchSuggestion(this.userPrompt.suggestion.suggestionKey).subscribe({
            next: (data) => {
                if (data) {
                    this.assignSuggestions(this.userForm?.value,data)
                }
            },
            error: (err) => {
                this.notificationService.addMessage(`Sorry, you're on your own this round.`, null, {panelClass: ['error-snackbar']});
            },
        });
    }

    autoSubmitUserPromptTimer(ms: number): void {
        if (ms <= 0) {
            this.onSubmit(this.userForm?.value, true)
            return;
        }
        this.autoSubmitTimerId = setTimeout(() => this.onSubmit(this.userForm?.value, true), ms);
        this.timerColor = 'var(--green-secondary)';
        this.timerDisplayIntervalId = setInterval(() => 
             {this.timerRemaining-=1000; this.timerDisplay = new Date(this.timerRemaining).toISOString().substr(14, 5);
                if (this.timerRemaining < 10000) this.timerColor = 'var(--red-secondary)'
                else if (this.timerRemaining < 30000) this.timerColor = 'var(--yellow-secondary)';
             }, 1000);
    }

    private clearAutoSubmitTimers(){
        clearTimeout(this.autoSubmitTimerId);
        this.autoSubmitTimerId = null;
        clearInterval(this.timerDisplayIntervalId);
        this.timerDisplayIntervalId = null;
        this.timerDisplay = '';
    }
  shortTermSanitize(ans){
        /* see this line in backend  sanitize.cs
           str.All(" abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.?,'-".Contains); 
        */
       if (ans) {
            ans=ans.replace(/[!\*;:]/g, ".");               // replace some punctuation with a .
            ans=ans.replace(/[\()\[\]\{}<>=\+_]/g, "-");    // replace a few chars with -
            ans=ans.replace(/[^a-zA-Z0-9 .?,\-]/g, "");  // delete the rest of the undesireables
       }
       return ans;
  }

  async onSubmit(userSubmitData, autoSubmit = false) {
        // Clear auto refresh timer as well as auto submit timer since we will be calling get after
        // the submission and both will get set there
        if (this.userPromptTimerId) {
          clearTimeout(this.userPromptTimerId);
          this.userPromptTimerId = null;
        }
        if (this.autoSubmitTimerId) {
            this.clearAutoSubmitTimers();
        }
        if (autoSubmit && !this.anythingEverTouched) return false; // if nothing has been touched, do not autosubmit

        this.anythingEverTouched = false; // reset so future form submits work

        // Populate IDs.
        userSubmitData.id = this.userPrompt.id;
        for (let i = 0; i < userSubmitData.subForms.length; i++) {
            userSubmitData.subForms[i].id = this.userPrompt.subPrompts[i].id;
            if (this.userPrompt.subPrompts[i].shortAnswer) {
                userSubmitData.subForms[i].shortAnswer=this.shortTermSanitize(userSubmitData.subForms[i].shortAnswer);
            }
            if (this.userPrompt.subPrompts[i].colorPicker) {
                userSubmitData.subForms[i].color=drawingUtils.convertColorToRGB(this.userPrompt.subPrompts[i].color);
            }
            if (this.userPrompt.subPrompts[i].selector && !userSubmitData.subForms[i].selector) {
                userSubmitData.subForms[i].selector="0";
            }
            if (this.userPrompt.subPrompts[i].slider && !userSubmitData.subForms[i].slider) {
                userSubmitData.subForms[i].slider = this.userPrompt.subPrompts[i].slider.value;
            }
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

    @HostListener("document:gesturestart")
    @HostListener("document:mouseup")
    @HostListener("mouseup")// are both of these mouseup calls needed?
    @HostListener("mousedown")
    @HostListener("keydown")
    @HostListener("touchstart")
    @HostListener("window:scroll")
    onListener() {
        this.anythingEverTouched = true;
        this.anythingTouchedSinceFetch = true;
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
