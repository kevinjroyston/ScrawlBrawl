
import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { FixedAsset } from '@core/http/fixedassets';
import { Observable } from 'rxjs';

//Used to pull down items that are not expected to change during the session (game assets, game list, etc)

@Injectable({
    providedIn: 'root'
})
export class Suggestions {

    constructor( @Inject(FixedAsset) private fixedAsset: FixedAsset)
        {  }

        private nextSuggestion = 0;
        private indexArray;

        // TODO: use a random function that supports a seed, store seed, size
        shuffleArray(array):void {
            for (var i = array.length - 1; i > 0; i--) {
                var j = Math.floor(Math.random() * (i + 1));
                var temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }

        nextSuggestionIndex(len):number{
            if (len==0) return 0;

            if (!this.indexArray || this.indexArray.length != len) {    // prepare a randomized index array 
               this.indexArray = Array.from(Array(len).keys());         // create an index list 0,1,2,3,4  etc.
               this.shuffleArray(this.indexArray)                       // randomize the list
            }
            if (this.nextSuggestion >= len) {
                this.nextSuggestion = 0;
            }
            return this.indexArray[this.nextSuggestion++];
        }

        pickOneEntry(suggestionList):string {
            var suggestions = JSON.parse(suggestionList);
            if (!suggestions || suggestions.length==0) { return "" }

            return JSON.stringify(suggestions[this.nextSuggestionIndex(suggestions.length)]);
        }

       fetchSuggestion(suggestionKey):Observable<any>{
       var observable = new Observable(subscriber => {

        this.fixedAsset.fetchFixedAsset(this.fixedAsset.determineSuggestionURI(suggestionKey)).subscribe(
            {
                next: (x) => {
                    // x has the entire list, pick one
                    subscriber.next(this.pickOneEntry(x));
                },
                error: (err) => {
                        subscriber.error(err); 
                },
                complete: () => {
                    subscriber.complete();
                }
            });
       });
        return observable;
    }

}

