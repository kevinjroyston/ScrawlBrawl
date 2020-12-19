import { Component, ViewEncapsulation, Input, AfterViewInit, ViewChild, ElementRef, HostListener  } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import {DrawingDirective,DrawingPromptMetadata} from '@shared/components/drawingdirective.component';
import {GalleryPanel} from '@shared/components/gallerypanel/gallerypanel.component';
import {MatTabsModule} from '@angular/material/tabs';
import { environment } from '../../../../environments/environment';

import Galleries from '@core/models/gallerytypes';

@Component({
    selector: 'gallerytool',
    templateUrl: './gallerytool.component.html',
    styleUrls: ['./gallerytool.component.scss'],
    encapsulation: ViewEncapsulation.Emulated
})


export class GalleryTool implements AfterViewInit {
    @Input() drawingOptions: DrawingPromptMetadata;
    @Input() drawingDirective: DrawingDirective;
    @Input() galleryEditor: boolean = false;
    @ViewChild("galleryFavorites") galleryFavorites: GalleryPanel;
    @ViewChild("galleryRecent") galleryRecent: GalleryPanel;
    @ViewChild("gallerySamples") gallerySamples: GalleryPanel;
    @ViewChild("currentImage") galleryImageCurrent: ElementRef;

    private _galleryId : string;
    set galleryId(value: string){ this.setGalleryId(value) } 
    get galleryId(): string {return this._galleryId}

    onChange;
    lastDrawingChange: string = "";
    galleryOptions = environment.galleryOptions;
    currentTab:GalleryPanel;

    constructor() {
    }

    storeMostRecentDrawing(){
        if (this.lastDrawingChange != "") {
            this.galleryRecent.storeImageInGallery(this.lastDrawingChange);
            this.lastDrawingChange = "";
        }
    }
    
    private setGalleryId(id){
        if (id == this._galleryId) { return }

        this.storeMostRecentDrawing(); /* we navigated to a different gallery, save what they were working on in last gallery */

        if (this.drawingDirective && this.drawingDirective.ctx){ /* clear the drawing */
            this.drawingDirective.ctx.clearRect(0, 0, this.drawingDirective.ctx.canvas.width, this.drawingDirective.ctx.canvas.height);
        }
        this._galleryId = id;
        this.galleryFavorites.galleryId = id;
        this.galleryRecent.galleryId = id;
        this.gallerySamples.galleryId = id;
    }

    onTabClick(index){
        switch (index){
            case 0 : this.currentTab = this.gallerySamples; break;
            case 1 : this.currentTab = this.galleryRecent; break;
            case 2 : this.currentTab = this.galleryFavorites; break;
        }
        this.currentTab.cancelDeleteNextClickedImage();
    }

    ngOnInit() {
        this.currentTab = this.gallerySamples;
    }

    ngOnDestroy() {
        this.storeMostRecentDrawing();
    }

    ngAfterViewInit(){
    }

    onDrawingChange(event){
        this.lastDrawingChange = event;
      
        if (this.galleryImageCurrent) {
            this.galleryImageCurrent.nativeElement.src = event;
        }
    }
    onCurrentBtnClick(){
        this.galleryFavorites.storeImageInGallery(this.lastDrawingChange)
      }
  
    registerOnChange(fn: any): void {
        this.onChange = fn;
    }

    registerOnTouched(fn: any): void {
    }

    onDeleteClicked(){
        this.currentTab.toggleDeleteNextClickedImage();
    }

    favoritesToClipboard(){
        this.galleryFavorites.putGalleryOnClipboard();
    }
    favoritesFromClipboard(){
        this.galleryFavorites.addClipboardToGallery();
    }
    
}

