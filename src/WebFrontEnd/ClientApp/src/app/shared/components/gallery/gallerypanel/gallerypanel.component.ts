import { Component, ViewEncapsulation, Input, AfterViewInit, ViewChild, ElementRef, HostListener, Output, EventEmitter, Inject } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import {DrawingDirective,DrawingPromptMetadata} from '@shared/components/drawingdirective.component';
import { FixedAsset } from '@core/http/fixedassets';
import {NotificationService} from '@core/services/notification.service';
import Galleries from '@core/models/gallerytypes';

@Component({
    selector: 'gallerypanel',
    templateUrl: './gallerypanel.component.html',
    styleUrls: ['./gallerypanel.component.scss'],
    encapsulation: ViewEncapsulation.Emulated
})
export class GalleryPanel implements ControlValueAccessor, AfterViewInit {
    
    @Output() closeSheet = new EventEmitter<string>();
    @Input() set drawingDirective(value: DrawingDirective) { this._drawingDirective = value; this.loadMostRecentDrawing() }
             get drawingDirective(): DrawingDirective { return this._drawingDirective}
    @Input() set drawingOptions(value: DrawingPromptMetadata){ this._drawingOptions = value; this.loadMostRecentDrawing() }
             get drawingOptions(): DrawingPromptMetadata {return this._drawingOptions}
    @Input() set galleryPanelType(value: Galleries.GalleryPanelType){ this._galleryPanelType = value; this.loadMostRecentDrawing() }   /* Favorites | Recent | Sample */
             get galleryPanelType(): Galleries.GalleryPanelType {return this._galleryPanelType}
    
    private _drawingType : string;
    private _drawingDirective: DrawingDirective;
    private _drawingOptions: DrawingPromptMetadata;
    private _galleryPanelType: Galleries.GalleryPanelType;
    public gallery: Galleries.GalleryDrawing[] = [];
    private galleryType: Galleries.GalleryType;
    private deleteOnNextClick: boolean = false;
    set drawingType(value: string){ this.setDrawingType(value) } 
    get drawingType(): string {return this._drawingType}
    
    constructor(@Inject(FixedAsset) private fixedAsset: FixedAsset, public notificationService : NotificationService) {
    }

    private loadTheGallery(){
        if (this.drawingType) {
            if (this.galleryPanelType == Galleries.GalleryPanelType.SAMPLES) {
                this.fetchGallerySamples(); 
            } else {
                this.loadGalleryImages();
            }
        }
    }

    private saveTheGallery(){
        this.saveGalleryToLocalStorage(); /* in the future we will support saving to DB */
    }

    private loadGalleryImages(){
        this.loadLocalGalleryImages(); /* in the future we will support loading from DB */
    }

    private setDrawingType(typ){
       if ((typ) && (typ != this._drawingType)){
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

    private loadMostRecentDrawing(){
        // if the gallery option galleryAutoLoadMostRecent is true, then take the last "recent" image and put it on the canvas
        // this will get called multiple times until all of the variables are populated
        if (this.galleryPanelType && this.drawingDirective && this.drawingOptions && this.drawingOptions.galleryOptions) {
          if ((this.drawingOptions.galleryOptions.galleryAutoLoadMostRecent) && (this.galleryPanelType==Galleries.GalleryPanelType.RECENT) && (this.gallery.length > 0)) {
              this.drawingDirective.loadImageString(this.gallery[this.gallery.length-1].image);
          }
        }
    }

    private maxGallerySize():number {
        // find the max based on gallerytype and panel type
        if (this.galleryPanelType != Galleries.GalleryPanelType.RECENT) {
          return this.galleryType.maxLocalFavorites
        } else {
          return this.galleryType.maxLocalRecent
        }
    }

    storeImageInGallery(imgStr, onDestroy = false){
        if ((imgStr=='') || (this.drawingType == Galleries.GalleryPanelType.SAMPLES)) {return}  // can't write to samples
        let alreadyInList : boolean=false;
        this.gallery.forEach((drawing,index) => {
            if (drawing.image == imgStr){
                alreadyInList=true;
                if (!onDestroy) {
                    this.notificationService.addMessage("Image is already in favorites", null, {panelClass: ['error-snackbar']});
                }
                return
            }
        })
        if (!alreadyInList){
            /* if we are at max length for this gallery type, error if favorites, delete the first (oldest) image if recent */
            if (this.gallery.length >= this.maxGallerySize())  {
                if (this.galleryPanelType == Galleries.GalleryPanelType.FAVORITES) {
                    this.notificationService.addMessage("You can only save "+this.maxGallerySize()+" favorites.", null, {panelClass: ['error-snackbar']});
                    return;
                }
                this.gallery.splice(0, 1);
            }
            var drawing : Galleries.GalleryDrawing = { image:imgStr }
            this.gallery.push(drawing);

            this.saveTheGallery();
            this.notificationService.addMessage("Image successfully saved.", null, {panelClass: ['success-snackbar']});
        }
    }

    removeImageFromGallery(imgStr){
        if (imgStr!='') {
            let placeInList:number=-1;
            this.gallery.forEach(function (drawing,index){if (drawing.image==imgStr){placeInList=index; return}})

            if (placeInList >= 0){
                this.gallery.splice(placeInList, 1);
                this.saveTheGallery();
                this.notificationService.addMessage("Image successfully deleted.", null, {panelClass: ['success-snackbar']});
            }
        }
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
        console.log('hello')
        this.fixedAsset.fetchFixedAsset(this.fixedAsset.determineGalleryURI(this.drawingType)).subscribe({
            next: (data) => {
                if (data) {
                    this.gallery = JSON.parse(data);
                }
            },
        });
    }

    /************ Local storage routines ***********/
    private localStorageGalleryName():string{
       return 'Gallery-'+this.drawingType+'-'+this.galleryPanelType;
    }

    private loadLocalGalleryImages(){
        if (this.drawingType) {
            var storedGallery = localStorage.getItem(this.localStorageGalleryName());

            if (storedGallery) {
                this.gallery = JSON.parse(storedGallery);
            } 
        }
    }

    private saveGalleryToLocalStorage(){
        if (this.drawingType) {
            var data = JSON.stringify(this.gallery); 
            localStorage.setItem(this.localStorageGalleryName(),data);
        }
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
            .then(()=>{alert("Your gallery is on the clipboard.")})
            .catch(e => console.error(e));
    }


    addClipboardToGallery(){
        /* note firefox needs: dom.events.asyncClipboard.dataTransfer   set to true in about:config */
        navigator.clipboard.readText().then(text => {
            var rawGallery: Galleries.GalleryDrawing[];
                rawGallery =  JSON.parse(text);
                if ((rawGallery) && (rawGallery.length > 0)) {
                    rawGallery.forEach((galleryDrawing)=>{this.storeImageInGallery(galleryDrawing.image)});
                }
        }).catch(err => {
            console.log('Something went wrong', err);
        });
    }
    
}