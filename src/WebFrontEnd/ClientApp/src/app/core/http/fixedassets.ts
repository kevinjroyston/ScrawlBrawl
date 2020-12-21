
import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

//Used to pull down items that are not expected to change during the session (game assets, game list, etc)

@Injectable({
    providedIn: 'root'
})
export class FixedAsset {

    constructor(@Inject(HttpClient) private http: HttpClient)
        {  }

    assetList = new Map();

    fetchFixedAsset(uri):Observable<any>{
        // I still do not like this sam I am.
       var observable = new Observable(subscriber => {

        if (this.assetList.has(uri)){  /* if we are in the cache, return from the cache */
            console.log(uri+" returned from cache");
            subscriber.next(this.assetList.get(uri));
            return;
        }

        this.http.get(uri, { observe: "body", responseType: "text" }).subscribe(  /* fetch asset and add to cache */
            {
                next: (x) => {
                    this.assetList.set(uri,x);
                    console.log(uri+" loaded / added to cache")
                    subscriber.next(x);
                },
                error: (err) => {
                        subscriber.error(err);  /* add an entry to cache so we stop trying to load ??? */
                },
                complete: () => {
                    subscriber.complete();
                }
            });
       });
        return observable;
    }

    determineImageAssetURI(gameAssetClass,gameAssetId,gameAssetType){
        return "/assets/GameAssets/game-images/"+(gameAssetClass ? gameAssetClass + "/" : "") + gameAssetId + "-" 
        + (gameAssetType ? gameAssetType : "logo") + ".svg";
    }

    determineGameTextAssetURI(gameAssetClass,gameAssetId,gameAssetType){
        return "/assets/GameAssets/"+(gameAssetClass ? gameAssetClass + "/" : "") + gameAssetId + "-" 
               + (gameAssetType ? gameAssetType : "description") + ".html";
    }


    determineGalleryURI(drawingType):string{
        return '/assets/GameAssets/gallery-samples/samples-'+drawingType+'.json'
    }
}

