import { Component, OnInit, Inject } from '@angular/core';
import {Subscription} from 'rxjs'
import { API } from '@core/http/api';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import Lobby from '@core/models/lobby';
import GameModes from '@core/models/gamemodes';
import { ErrorService } from '@modules/lobby/services/error.service';
import { GameModeList } from '@core/http/gamemodelist';
import { SliderPromptMetadata } from '@shared/components/slider/slider.component';

interface GameModeDialogData {
  lobby: Lobby.LobbyMetadata
  error: string
  onGetLobby: () => void,
  durationEstimates: Map<Lobby.GameDuration,number>,
  launchURL: string
}

@Component({
  selector: 'app-commonoptions-dialog',
  templateUrl: './commonoptions-dialog.component.html',
  styleUrls: ['./commonoptions-dialog.component.scss']
})
export class CommonoptionsDialogComponent implements OnInit {
  lobby: Lobby.LobbyMetadata
  error: string
  launchURL: string
  onGetLobby: () => void
  errorSubscription: Subscription
  showTutorial: boolean = true;
  timerEnabled: boolean = true;
  gameDuration: Lobby.GameDuration = Lobby.GameDuration.Normal;
  durationEstimates: Map<Lobby.GameDuration, number>;
  estimatedDurationSliderParameters: SliderPromptMetadata =  {
    min: 0,
    max: 2,
    value: [1],
    ticks: [0,1,2],
    range: false,
    enabled: true,
    showTooltip: "hide",
    ticksLabels: ["Short", "Normal", "Extended"],
    rangeHighlights: null
  }

  constructor(
    private dialogRef: MatDialogRef<CommonoptionsDialogComponent>,
    public errorService: ErrorService,
    @Inject(GameModeList) public gameModeList: GameModeList,
    @Inject(MAT_DIALOG_DATA) public data: GameModeDialogData,
    @Inject(API) private api: API) {
    this.lobby = data.lobby;
    this.launchURL = data.launchURL;
    this.onGetLobby = data.onGetLobby;
    this.durationEstimates = data.durationEstimates;
    this.estimatedDurationSliderParameters.ticksLabels=[
      this.durationEstimates["Short"] + "m",
      this.durationEstimates["Normal"] + "m",
      this.durationEstimates["Extended"] + "m"];
  }

  ngOnInit() {
    this.errorSubscription = this.errorService.errorObservable.subscribe((error) => {
      this.error = error;
    })
    this.updateEstimatedDuration(this.gameDuration);
  }

  ngOnDestroy() {
    this.errorSubscription.unsubscribe();
  }

  onStart() : void {
    var body = new Lobby.StartLobbyRequest();
    body.gameDuration = this.gameDuration;
    body.showTutorial = this.showTutorial;
    body.timerEnabled = this.timerEnabled;
    var bodyString = JSON.stringify(body);
    let startLobbyRequest = this.api.request({ type: "Lobby", path: "Start", body: bodyString });

    startLobbyRequest.subscribe({
        next: () => {
          // TODO: open gameplay tab & viewer tab?
          this.closeDialog();
          if (this.launchURL && (this.launchURL!='')) {
            window.scrollTo(0,0);
            window.open(this.launchURL,'SBLaunch')  
          }
        },
        error: (error) => { 
            this.error = error.error; 
            this.errorService.announceError(error.error); 
            this.onGetLobby(); 
        }
    })
  }

  updateEstimatedDuration(duration:number){
    this.gameDuration = duration;
  }

  closeDialog() {
    this.dialogRef.close();
  }
}
