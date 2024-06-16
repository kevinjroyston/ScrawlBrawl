import { Component, Inject, ViewEncapsulation, Input, AfterViewInit, ViewChild, ElementRef, HostListener  } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import {DrawingDirective,DrawingPromptMetadata} from '@shared/components/drawingdirective.component';
import {GalleryPanel} from '@shared/components/gallery/gallerypanel/gallerypanel.component';
import {MatLegacyTabsModule as MatTabsModule} from '@angular/material/legacy-tabs';
import {MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA} from '@angular/material/bottom-sheet';
import { environment } from '../../../../../environments/environment';
import Galleries from '@core/models/gallerytypes';

@Component({
    selector: 'gallerytool',
    templateUrl: './gallerytool.component.html',
    styleUrls: ['./gallerytool.component.scss'],
    encapsulation: ViewEncapsulation.Emulated
})
export class GalleryTool implements AfterViewInit {
    drawingOptions: DrawingPromptMetadata;
    drawingDirective: DrawingDirective;
    galleryEditor: boolean = false;
    @ViewChild("galleryFavorites") galleryFavorites: GalleryPanel;
    @ViewChild("galleryRecent") galleryRecent: GalleryPanel;
    @ViewChild("gallerySamples") gallerySamples: GalleryPanel;

    private _drawingType : string;
    set drawingType(value: string){ this.setDrawingType(value) } 
    get drawingType(): string {return this._drawingType}

    galleryOptions = environment.galleryOptions;
    currentTab: GalleryPanel;
    galleryPanelTypes = Galleries.GalleryPanelType;

    constructor(private _bottomSheetRef: MatBottomSheetRef<GalleryTool>, @Inject(MAT_BOTTOM_SHEET_DATA) public data) {
        this.initializeBottomSheet(data);
    }

    private initializeBottomSheet(data) {
        this.drawingOptions = data.drawingOptions;
        this.drawingDirective = data.drawingDirective;
        this.galleryEditor = data.galleryEditor;
    }

    public closeSheet() {
        this._bottomSheetRef.dismiss();
    }

    
    private setDrawingType(typ){
        if (typ == this._drawingType) { return }

        if (this.drawingDirective && this.drawingDirective.ctx){ /* clear the drawing */
            this.drawingDirective.ctx.clearRect(0, 0, this.drawingDirective.ctx.canvas.width, this.drawingDirective.ctx.canvas.height);
            this.drawingDirective.handleClearUndo();
        }
        this._drawingType = typ;
        this.drawingOptions.drawingType = typ;
        this.galleryFavorites.drawingType = typ;
        this.galleryRecent.drawingType = typ;
        this.gallerySamples.drawingType = typ;
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
    }

    ngAfterViewInit(){
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

    @HostListener('window:keydown',['$event'])
    onKeyPress($event: KeyboardEvent) {
        if(($event.ctrlKey || $event.metaKey) && $event.keyCode == 90)
          this.drawingDirective.onPerformUndo();
    }    
}

