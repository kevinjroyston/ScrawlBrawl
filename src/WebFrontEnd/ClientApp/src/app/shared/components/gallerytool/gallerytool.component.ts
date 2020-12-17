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

    galleryId: string;
    onChange;
    lastDrawingChange: string = "";
    galleryOptions = environment.galleryOptions;

    constructor() {
    }

    storeMostRecent(){
        if (this.lastDrawingChange != "") {
            this.galleryRecent.storeImageInGallery(this.lastDrawingChange);
            this.lastDrawingChange = "";
        }
    }
    setGalleryId(galId){
        if (galId != this.galleryId) {
            this.storeMostRecent(); /* we navigated to a different gallery, save what they were working on in last gallery */

            if (this.drawingDirective && this.drawingDirective.ctx){
                this.drawingDirective.ctx.clearRect(0, 0, this.drawingDirective.ctx.canvas.width, this.drawingDirective.ctx.canvas.height);
            }
            this.galleryId = galId;
            this.galleryFavorites.setGalleryId(galId);
            this.galleryRecent.setGalleryId(galId);
            this.gallerySamples.setGalleryId(galId);
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
        this.lastDrawingChange = event;
      
        if (this.galleryImageCurrent) {
            this.galleryImageCurrent.nativeElement.src = event;
        }
//        this.onChange(event)         
        
    }
    onCurrentBtnClick(){
        this.galleryFavorites.storeImageInGallery(this.lastDrawingChange)
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

