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
    @Input() drawingPrompt: DrawingPromptMetadata;
    @Input() drawingDirective: DrawingDirective;
    @Input() galleryEditor: boolean = false;
    @ViewChild("galleryFavorites") galleryFavorites: GalleryPanel;
    @ViewChild("galleryRecent") galleryRecent: GalleryPanel;
    @ViewChild("gallerySamples") gallerySamples: GalleryPanel;
    @ViewChild("currentImage") galleryImageCurrent: ElementRef;

    galleryType: string;
    gameId: string;
    onChange;
    lastImageChange: string = "";
    galleryOptions = environment.galleryOptions;

    constructor() {
    }

    storeMostRecent(){
        if (this.lastImageChange != "") {
            this.galleryRecent.saveImageAsRecent(this.lastImageChange);
        }
    }
    setGalleryType(galType){
        if (galType != this.galleryType) {
            this.storeMostRecent();
            this.drawingDirective.ctx.clearRect(0, 0, this.drawingDirective.ctx.canvas.width, this.drawingDirective.ctx.canvas.height);
            this.galleryType = galType;
            this.galleryFavorites.setGalleryType(galType);
            this.galleryRecent.setGalleryType(galType);
            this.gallerySamples.setGalleryType(galType);
        }
    }

    ngOnInit() {
    }

    ngOnDestroy() {
        this.storeMostRecent();
    }

    ngAfterViewInit(){
    }

    onDrawingChange(event){
        this.lastImageChange = event;
      
        if (this.galleryImageCurrent) {
            this.galleryImageCurrent.nativeElement.src = event;
        }
//        this.onChange(event)         
        
    }
    onCurrentBtnClick(){
        this.galleryFavorites.saveImageAsFavorite(this.lastImageChange)
      }
  
    registerOnChange(fn: any): void {
        this.onChange = fn;
    }

    registerOnTouched(fn: any): void {
    }

    loadMostRecent(){
//        this.galleryRecent.loadMostRecent();
    }
    favoritesToClipboard(){
        this.galleryFavorites.putGalleryOnClipboard();
    }
    favoritesFromClipboard(){
        this.galleryFavorites.addClipboardToGallery();
    }
    
}

