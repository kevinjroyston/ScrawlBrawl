import { Directive, ElementRef, HostListener, forwardRef, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';


interface SelectorPromptMetadata {
  widthInPx: number;
  heightInPx: number;
  imageList: string[];
}

declare var angular: any;
@Directive({
  selector: '[appSelector]',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      multi: true,
      useExisting: forwardRef(() => SelectorDirective),
    }
  ]
})


export class SelectorDirective implements ControlValueAccessor {

  writeValue(obj: any): void {
    if (obj == null) {  // remove the selector

    } else {

    }
  }
  registerOnChange(fn: any): void {
    this.onChange = fn;
    fn('0');  // default value to zero
  }
  registerOnTouched(fn: any): void {
  }
  setDisabledState?(isDisabled: boolean): void {
  }

  constructor(element: ElementRef) {
    console.log("build Selector");
    this.element = element.nativeElement;
  }


  ngOnInit() {
    console.log("on init - selector");
    console.log(this.selectorParameters);
    this.createRotatorImages(this.selectorParameters.widthInPx, this.selectorParameters.heightInPx);
    for (let i = 0; i < this.selectorParameters.imageList.length; i++) {
      this.addRotatorImage(this.selectorParameters.imageList[i]);
    }
    this.index = 0;
    this.setImages(0);

  }




  @Input() selectorParameters: SelectorPromptMetadata;
  index: number;
  leftImg;
  rightImg;
  mainImg;
  inSwipe: boolean = false;
  images = new Array(0);
  lastX: number;
  lastY: number;


  onChange = function (s) { console.log("selector onchange called before initialization"); };
  element;


  createImage(id, cn, w, h): Element {
    var img = document.createElement('img');
    img.id = id;
    img.width = w;// + "px";
    img.height = h; // + "px";
    img.className = cn;
    this.element.appendChild(img);
    return img;
  }
  createRotatorImages(w, h): void {
    if (w < 10) w = 300;
    if (h < 10) w = 300;


    this.leftImg = this.createImage("leftimg", "leftImg", Math.floor(w / 2), h);
    this.mainImg = this.createImage("mainimg", "mainImg", w, h);
    this.rightImg = this.createImage("rightimg", "rightImg", Math.floor(w / 2), h);

  }
  addRotatorImage(img): void {
    this.images.push(img);
  }
  setImage(img, ind): void {
    if (ind < 0) ind = this.images.length - 1;
    if (ind >= this.images.length) ind = 0;
    img.src = this.images[ind];
  }

  setImages(delta): void {
    this.index += delta;
    if (this.index >= this.images.length) this.index = 0;
    if (this.index < 0) this.index = this.images.length - 1;

    this.setImage(this.leftImg, this.index - 1);
    this.setImage(this.mainImg, this.index);
    this.setImage(this.rightImg, this.index + 1);
    if (delta != 0) this.onChange(this.index.toString());  // write out current index value
  }

  @HostListener('click', ['$event.target'])
  onclick(img) {
    if (img == this.leftImg) this.setImages(-1)
    else if (img == this.rightImg) this.setImages(1)
  }

  @HostListener('mousedown', ['$event'])
  @HostListener('touchstart', ['$event'])
  onmousedown(event) {
    console.log("down/start");  // NEW CODE FOR OUR MOUSE HANDLING
    if (event.target == this.mainImg) {
      event.preventDefault();
      this.inSwipe = true;
      console.log("swipe started");
      if (event.changedTouches) {                      // only for touch
        this.lastX = event.changedTouches[0].pageX - this.element.offsetLeft - this.element.clientLeft;
        this.lastY = event.changedTouches[0].pageY - this.element.offsetTop - this.element.clientTop;
      } else {
        if (event.offsetX !== undefined) {
          this.lastX = event.offsetX;
          this.lastY = event.offsetY;
        } else {
          this.lastX = event.layerX - event.currentTarget.offsetLeft;
          this.lastY = event.layerY - event.currentTarget.offsetTop;
        }
      }
    }

  }

  @HostListener('mouseup', ['$event'])
  @HostListener('touchend', ['$event'])
  onmouseup(event) {
    console.log("up/end");
    if (this.inSwipe) {
      console.log("swipe ended");

      event.preventDefault();
      let newX: number;
      if (event.changedTouches) {                      // only for touch
        newX = event.changedTouches[0].pageX - this.element.offsetLeft - this.element.clientLeft;
      } else {
        if (event.offsetX !== undefined) {
          newX = event.offsetX;
        } else {
          newX = event.layerX - event.currentTarget.offsetLeft;
        }
      }
      this.setImages(Math.sign(newX - this.lastX))
    }

  }
}



