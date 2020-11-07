import { Directive, ElementRef, HostListener, forwardRef, Input, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Directive({
    selector: '[gameAsset]',
})

export class GameAssetDirective {
  private _gameID: string;
  @Input() descriptionPreview: boolean = false;
  @Input() set gameID(value: string) { this._gameID = value; this.loadGame() }
            get gameID(): string { return this._gameID}
  http: HttpClient;
  element;

  constructor(http: HttpClient, element: ElementRef) {
    console.log("Instantiating gameAsset " + element.nativeElement.nodeName + this.gameID);
    this.element = element.nativeElement;
    this.http = http;
  }
  
  ngOnInit() {
    console.log("OnInit gameAsset " + this.gameID);
    this.loadGame();
  }

  loadGame() {

    if (this.element.nodeName == 'IMG') {
      this.element.src = "/assets/GameAssets/game-images/" + this.gameID + "-logo.png";
    }
    else {
      const url = this.determineDescriptionDestination() + this.gameID + "-description.html";
      this.http.get(url, { observe: "body", responseType: "text" }).subscribe(
        data => {
          this.element.innerHTML = data;
        })
    }
  }

  determineDescriptionDestination = () => {
    const gameAssetFolder = '/assets/GameAssets/'
    return gameAssetFolder + (this.descriptionPreview ? '/homepage-previews/' : '/lobby-descriptions/')
  } 
}
