import { Directive, ElementRef, HostListener, forwardRef, Input, Output, EventEmitter } from '@angular/core';
import { FixedAsset } from '@core/http/fixedassets';
import { Inject } from '@angular/core';

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
    
*/

@Directive({
  selector: "[gameAsset]",
})

export class GameAssetDirective {
  private _gameAssetID: string;
  @Input() gameAssetClass: string;
  @Input() gameAssetType: string;
  @Input() set gameAssetID(value: string) { this._gameAssetID = value; this.loadGameAsset() }
            get gameAssetID(): string { return this._gameAssetID}
  element;

  constructor(
    @Inject(FixedAsset) private fixedAsset: FixedAsset,
    element: ElementRef
  ) {
    this.element = element.nativeElement;
  }
  
  ngOnInit() {
  }

  loadGameAsset() {
    if (this.element.nodeName == "IMG") {
      this.element.src = this.fixedAsset.determineImageAssetURI(this.gameAssetClass,this.gameAssetID,this.gameAssetType );
    } else {
      const uri = this.fixedAsset.determineGameTextAssetURI(this.gameAssetClass,this.gameAssetID,this.gameAssetType )

      this.fixedAsset.fetchFixedAsset(uri).subscribe({
        next: (x) => {
          this.element.innerHTML = x;
        },
      });
    }
  }
}
