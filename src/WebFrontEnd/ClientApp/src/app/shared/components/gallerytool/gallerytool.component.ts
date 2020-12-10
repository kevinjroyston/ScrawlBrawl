import { Component, ViewEncapsulation, Input, AfterViewInit, ViewChild, ElementRef, HostListener  } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import {DrawingDirective} from '@shared/components/drawingdirective.component';
import {GalleryPanel} from '@shared/components/gallerypanel/gallerypanel.component';
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

    @ViewChild("galleryFavorites") galleryFavorites: GalleryPanel;
    @ViewChild("galleryRecent") galleryRecent: GalleryPanel;
    @ViewChild("gallerySamples") gallerySamples: GalleryPanel;
    @ViewChild("currentImage") galleryImageCurrent: ElementRef;

    galleryType: string;
    gameId: string;
    onChange;
    lastImageChange: string = "";

    constructor() {
    }

    
    ngOnInit() {
    }

    ngOnDestroy() {
        if (this.lastImageChange != "") {
            this.galleryRecent.saveImageAsRecent(this.lastImageChange);
        }
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

    favoritesToClipboard(){
        this.galleryFavorites.putGalleryOnClipboard();
    }
    favoritesFromClipboard(){
        this.galleryFavorites.addClipboardToGallery();
    }
    
}

interface DrawingPromptMetadata {
    colorList: string[];
    widthInPx: number;
    heightInPx: number;
    premadeDrawing: string;
    canvasBackground: string;
    localStorageId: string;
    galleryType: string;
    gameId: string;  /* should we be getting this from fetch-data? */
}
