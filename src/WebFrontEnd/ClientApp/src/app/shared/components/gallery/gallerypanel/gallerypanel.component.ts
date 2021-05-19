import { Component, ViewEncapsulation, Input, AfterViewInit, ViewChild, ElementRef, HostListener, Output, EventEmitter, Inject } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import {DrawingDirective,DrawingPromptMetadata} from '@shared/components/drawingdirective.component';
import { FixedAsset } from '@core/http/fixedassets';
import {NotificationService} from '@core/services/notification.service';
import Galleries from '@core/models/gallerytypes';
import { GalleryService } from '@core/services/gallery.service';

@Component({
    selector: 'gallerypanel',
    templateUrl: './gallerypanel.component.html',
    styleUrls: ['./gallerypanel.component.scss'],
    encapsulation: ViewEncapsulation.Emulated
})
export class GalleryPanel implements ControlValueAccessor, AfterViewInit {
    
    @Output() closeSheet = new EventEmitter<string>();
    @Input() public drawingDirective: DrawingDirective;
    @Input() public drawingOptions: DrawingPromptMetadata;
    @Input() public galleryPanelType: Galleries.GalleryPanelType;
    
    private _drawingType : string;
    public gallery: Galleries.GalleryDrawing[] = [];
    private galleryType: Galleries.GalleryType;
    private deleteOnNextClick: boolean = false;
    set drawingType(value: string){ this.setDrawingType(value) } 
    get drawingType(): string {return this._drawingType}
    
    constructor(@Inject(FixedAsset) private fixedAsset: FixedAsset, public notificationService : NotificationService, private galleryService: GalleryService) {
    }

    private loadTheGallery(){
        if (this.drawingType) {
            if (this.galleryPanelType === Galleries.GalleryPanelType.SAMPLES) {
                this.fetchGallerySamples(); 
            } else {
                this.gallery = this.galleryService.LoadGalleryImages(this.drawingType, this.galleryPanelType);
            }
        }
    }

    private setDrawingType(typ){
       if ((typ) && (typ !== this._drawingType)){
            this.clearExistingGallery();
            this._drawingType  = typ;
            this.galleryType = Galleries.galleryFromDrawingType(typ);
            this.loadTheGallery();
       }
    }

    ngAfterViewInit(){
        if (this.drawingOptions && this.drawingOptions.galleryOptions) {
            this.drawingType = this.drawingOptions.drawingType;
        }
    }
    deleteDrawing(drawing:string){
        this.galleryService.RemoveImageFromGallery(this.drawingType, this.galleryPanelType, drawing);
        this.gallery = this.galleryService.LoadGalleryImages(this.drawingType, this.galleryPanelType);
    }
    
    ngOnDestroy() {
    }

    writeValue(obj: any): void {
    }

    registerOnChange(fn: any) : void {

    }

    registerOnTouched(fn: any): void {
    }

    /************ the following routines deal with the images in the GalleryPictured div  *************/

    @HostListener('click', ['$event.target'])
    onclick(img) {
            if (!img.src) { return } 
            this.drawingDirective.loadImageString(img.src);
            this.closeSheet.next();
            event.preventDefault;
    }

    private clearExistingGallery(){
        if (this.gallery.length > 0){
            this.gallery.length = 0;
        }
    }

    /************ Fetch Samples from the fixed assets ***********/
    private fetchGallerySamples(){
        this.fixedAsset.fetchFixedAsset(this.fixedAsset.determineGalleryURI(this.drawingType)).subscribe({
            next: (data) => {
                if (data) {
                    this.gallery = JSON.parse(data);
                }
            },
        });
    }

    /****************** TEMPORARY ROUTINES UNTIL WE COME UP WITH A DELETE DESIGN **********/
    toggleDeleteNextClickedImage(){
        this.deleteOnNextClick = !this.deleteOnNextClick;
    }
    cancelDeleteNextClickedImage(){
        this.deleteOnNextClick = false;
    }
    /*********** clipboard functions for our testing purposes - these buttons are hidden on production server  */

    putGalleryOnClipboard(){
        var data = JSON.stringify(this.gallery); 
        navigator.clipboard.writeText(data)
            .then(()=>{
                this.notificationService.addMessage("Your gallery is on the clipboard.", null, {panelClass: ['success-snackbar']});
            })
            .catch(e => console.error(e));
    }


    addClipboardToGallery(){
        /* note firefox needs: dom.events.asyncClipboard.dataTransfer   set to true in about:config */
        navigator.clipboard.readText().then(text => {
            var rawGallery: Galleries.GalleryDrawing[];
                rawGallery =  JSON.parse(text);
                if ((rawGallery) && (rawGallery.length > 0)) {
                    rawGallery.forEach((galleryDrawing)=>{this.galleryService.AddImageToGallery(false, this.drawingType, this.galleryPanelType, galleryDrawing.image)});
                }
        }).catch(err => {
            console.log('Something went wrong', err);
        });
    }
    
}