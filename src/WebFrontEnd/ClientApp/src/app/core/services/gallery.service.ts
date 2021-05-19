import { Injectable } from "@angular/core";
import Galleries from "@core/models/gallerytypes";
import { NotificationService } from "./notification.service";
import { StorageService } from "./storage.service";


@Injectable()
export class GalleryService {
    
    constructor(private storageService:StorageService, private notificationService:NotificationService){
    }
    
    private localStorageGalleryName(drawingType: string, galleryPanelType: Galleries.GalleryPanelType):string{
        return 'Gallery-'+drawingType+'-'+galleryPanelType;
     }
 
     public LoadGalleryImages(drawingType: string, galleryPanelType: Galleries.GalleryPanelType):Galleries.GalleryDrawing[] {
         if (drawingType) {
             var storedGallery = this.storageService.get(this.localStorageGalleryName(drawingType, galleryPanelType));
 
             if (storedGallery) {
                 return JSON.parse(storedGallery);
             } 
         }
         return [];
     }
 
     public SaveGalleryImages(drawingType: string, galleryPanelType: Galleries.GalleryPanelType, gallery: Galleries.GalleryDrawing[]){
         if (drawingType) {
             var data = JSON.stringify(gallery); 
             this.storageService.set(this.localStorageGalleryName(drawingType, galleryPanelType),data);
         }
     }

     public AddImageToGallery(onDestroy: boolean, drawingType: string, galleryPanelType: Galleries.GalleryPanelType, imgStr: string){
        if ((imgStr=='') || (drawingType === Galleries.GalleryPanelType.SAMPLES)) {return}  // can't write to samples
        var gallery = this.LoadGalleryImages(drawingType, galleryPanelType);
        var galleryType = Galleries.galleryFromDrawingType(drawingType);
        let alreadyInList : boolean=false;
        gallery.forEach((drawing,index) => {
            if (drawing.image === imgStr){
                alreadyInList=true;
                if (!onDestroy) {
                    this.notificationService.addMessage("Image is already in favorites", null, {panelClass: ['error-snackbar']});
                }
                return
            }
        })
        if (!alreadyInList){
            /* if we are at max length for this gallery type, error if favorites, delete the first (oldest) image if recent */
            if (gallery.length >= this.maxGallerySize(galleryType, galleryPanelType))  {
                if (galleryPanelType === Galleries.GalleryPanelType.FAVORITES) {
                    this.notificationService.addMessage(`You can only save ${this.maxGallerySize(galleryType, galleryPanelType)} favorites.`, null, {panelClass: ['error-snackbar']});
                    return;
                }
                gallery.splice(0, 1);
            }
            var drawing : Galleries.GalleryDrawing = { image:imgStr }
            gallery.push(drawing);

            this.SaveGalleryImages(drawingType,galleryPanelType,gallery);
            this.notificationService.addMessage(`Image successfully saved to ${galleryPanelType}.`, null, {panelClass: ['success-snackbar']});
        }
     }

     private maxGallerySize(galleryType: Galleries.GalleryType, galleryPanelType: Galleries.GalleryPanelType):number {
        // find the max based on gallerytype and panel type
        if (galleryPanelType !== Galleries.GalleryPanelType.RECENT) {
          return galleryType.maxLocalFavorites
        } else {
          return galleryType.maxLocalRecent
        }
    }

    RemoveImageFromGallery(drawingType: string, galleryPanelType: Galleries.GalleryPanelType, imgStr: string){
        if (imgStr!='') {
            let placeInList:number=-1;
            var gallery = this.LoadGalleryImages(drawingType,galleryPanelType);
            gallery.forEach(function (drawing,index){if (drawing.image==imgStr){placeInList=index; return}})

            if (placeInList >= 0){
                gallery.splice(placeInList, 1);
                this.SaveGalleryImages(drawingType,galleryPanelType,gallery);
                this.notificationService.addMessage(`Image successfully deleted from ${galleryPanelType}`, null, {panelClass: ['success-snackbar']});
            }
        }
    }

     

    public GetMostRecentDrawing(drawingType:string):string{        
        var gallery = this.LoadGalleryImages(drawingType, Galleries.GalleryPanelType.RECENT)
        if (!gallery || gallery.length <= 0){
            return null;
        }
        return gallery[gallery.length-1].image;
    }
}
