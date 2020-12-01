import { Directive, ElementRef, HostListener, forwardRef, Input, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
/*
Usage:
  The root folder for DIV (html) game assets is /assets/GameAssets/
  The root folder for IMG (svg!) game assets is /assets/GameAssets/game-images/
  gameAssetClass is used to specify a SUB-FOLDER under the root level
  gameAssetType is used to specify the end of the filename - defaults to "logo" for images and "description" for text
   format: (root folder)/[gameAssetClass]/[gameAssetId]-[gameAssetType].[html|svg]
   example: <div gameAsset [gameAssetClass]="'homepage-previews'" [gameAssetID]="'Mimic'">
    would load:  /assets/GameAssets/homepage-previews/Mimic-description.html

  Currently valid gameAssetClass (folders)
    homepage-previews  (text)
    lobby-descriptions  (text)
    
  important!  Set gameAssetID as the LAST item in your tag.  As soon as it is set it will attempt to load
*/

@Directive({
    selector: '[gameAsset]',
})

export class GameAssetDirective {
  private _gameAssetID: string;
  @Input() gameAssetClass: string;
  @Input() gameAssetType: string;
  @Input() set gameAssetID(value: string) { this._gameAssetID = value; this.loadGameAsset() }
            get gameAssetID(): string { return this._gameAssetID}
  http: HttpClient;
  element;

  constructor(http: HttpClient, element: ElementRef) {
    console.log("Instantiating gameAsset " + element.nativeElement.nodeName);
    this.element = element.nativeElement;
    this.http = http;
  }
  
  ngOnInit() {
    // console.log("OnInit gameAsset " + this.gameAssetID);  no longer needed due to setter on gameAssetID
    // this.loadGameAsset();
  }

  loadGameAsset() {
    console.log("gameAsset load " + this.gameAssetID);

    if ((!this._gameAssetID) || (this._gameAssetID=="undefined")) {this._gameAssetID=""}
    if (this.element.nodeName == 'IMG') {
      this.element.src = this.determineImageAssetDestination();
    }
    else {
      const url = this.determineDescriptionDestination();
      this.http.get(url, { observe: "body", responseType: "text" }).subscribe(
        data => {
          this.element.innerHTML = data;
        })
    }
  }

  supportsSVG(gameId){return ((gameId!='BodySwap')&&(gameId!='FriendQuiz'))}
  determineImageAssetDestination() {
    return '/assets/GameAssets/game-images/'+(this.gameAssetClass ? this.gameAssetClass+'/' : '') + this.gameAssetID + "-" 
       + (this.gameAssetType ? this.gameAssetType : 'logo') + (this.supportsSVG(this.gameAssetID) ? '.svg' : '.png');
  }

  determineDescriptionDestination() {
    return '/assets/GameAssets/'+(this.gameAssetClass ? this.gameAssetClass+'/' : '') + this.gameAssetID + "-" + (this.gameAssetType ? this.gameAssetType : 'description') + ".html";
  } 
}
