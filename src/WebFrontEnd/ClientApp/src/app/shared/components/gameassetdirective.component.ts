import { Directive, ElementRef, HostListener, forwardRef, Input, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, Subscriber } from 'rxjs';

declare var angular: any;
@Directive({
    selector: '[gameAsset]',
})

export class GameAssetDirective {
  private _gameID: string;
  @Input() set gameID(value: string) { this._gameID = value; this.loadGame() }
            get gameID(): string { return this._gameID}
  http: HttpClient;
    element;

  constructor(http: HttpClient, element: ElementRef) {

    console.log("Instantiating gameAsset " + element.nativeElement.nodeName + this.gameID);
    this.element = element.nativeElement;
    this.http = http;
  }
  
  ngOnInit() {
    console.log("OnInit gameAsset " + this.gameID);
    this.loadGame();
  }

  loadGame() {

    if (this.element.nodeName == 'IMG') {
//      this.element.src = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAC0AAAAsCAAAAADULW7jAAAALHRFWHRDcmVhdGlvbiBUaW1lAE1vbiAyOCBTZXAgMjAyMCAxNzozNDozMiAtMDgwMF + 1bzkAAAAHdElNRQfkCR0AJyV / o3glAAAACXBIWXMAAAsSAAALEgHS3X78AAAABGdBTUEAALGPC / xhBQAAA4JJREFUOMuVlctrE1EUxi / owj9AaV3pUt2kioILQVwo4kKrC99EXAhaRS3tILhRpNIWH4iC + KCvdPABsUURNbZRWq0VtYp9xEdtFJ9NJ20yTebOOfdO1Htn2mbSTLF + qzD55cuZc757LjH / R8TrIXCLw3RpwO7gW4R / 0cCZZPgzQsgzbj + BKWk0Br4LS4BjZAY5BvKTSXN4Fw2p88R3lwHQMuFdRgWd1pMpN + 6ieReZSUjEAqoIWhGuNClEvWnrOZlTQI4aOEGPStrwplHbSgpmk / YMLRd0OUVDwrqZT4OsznpMZheQ7cPg0KCPW0 + U7tDI0sgBjCOksIDc + 10j6BpuW4 / aMIzxNo3Jxr3n + jmzesmsQnI1Ezu5oiLGUs47CpPEcNKpx6Z5EykkvoZYhknXl5xRjTKQ3inZmLimafoEDew0KSpaSDY + pOnQxedyLHKmYKQM0RfJalrC5f2UzPP5fPNJLVo8mydZblJzlMrSAI + KyUKfb8HiHywnGEAd5 / jYRJ2egBULFJFFc1cOstw0mpKOJ6m7J0KM91ctWxOenFNRdjxh5PQbmCX0h / 0cYnmhFilEhkL2L4iEv74Reh0xLI8TAOZIXGpU4oJmbWuWLBVaXDGM + bDR322rdxAljYndy1fbKgxbeTT + ettjq / udSMz / 0hOVLJlOJdm3THusBcBJbzneQT6o5a0FQD2J3N1BZzosemn3vieTcMDPj8NdGmLOaQBrKOjftHPLnqHcyaP + qKX1YbhPZ67JA3Qc2rDT79 + x61fONAET4VahUNtnV6r4i3Xb / H7 / trU33IkFxjj2PGgReEvoK2ZzcqVYGK8 / 3EaNzqYeeRrslTXwtI + a0baQdH + NWe / 7qzYX + 4NDFruzp + RAL0czKQ52tL6uphOZ3hcOtdz / wLKnWL91vDbKkfeXlislt62R5gvNI7yzTm0MfGOA2puOvjS4N4SBYgEalw8qSklHpvXEqROtmZ46VW14IA4CAkXI2z781X5FKa1O4PXqM9XXYbQ5oKr17 / nYt5M320hVqaIc6LJMmzb5x3pVDTTp6L1ju0sU5eBVimM00lCDqtYNMG86sres9NAn0W + HBvY9EGis / eJNQ / rm4SPt4mXHaXH / XAu0T7GRxV9 / G7THEqw8Wxm0hxSPUZjylmJ2TFikqqIqwuyg5IbS8waEaEcUYNq3K + Ns2rfr1PoLmvqkPcS07D8AAAAASUVORK5CYII=";
      this.element.src = "/assets/GameAssets/" + this.gameID + "-logo.png";
    }
    else {
      let url: string;
      url = "/assets/GameAssets/" + this.gameID + "-lobby-description.html";
      this.element.innerHTML = "";
      console.log(url);
      /*
            console.log(http.get(url, { observe: "body", responseType: "text" }));
            result = http.get(url, { observe: "body", responseType: "text" }).toString();
      -*/
      this.http.get(url, { observe: "body", responseType: "text" }).subscribe(
        data => {
//          console.log("received:" + result);
          this.element.innerHTML = data;
        })


    }
    }


}
