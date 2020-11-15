import { Directive, ElementRef, HostListener, forwardRef, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';


interface SelectorPromptMetadata {
  widthInPx: number;
  heightInPx: number;
  imageList: string[];
}

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
      // Default to 0, needs a delay due to buggy form implementation.
//      setTimeout(()=>fn('0'), 200)
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

    this.mainImg.style.setProperty('--n', '100');
    this.rightImg.style.setProperty('--n', '100');
    this.leftImg.style.setProperty('--n', '100');

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

  @HostListener('mousemove', ['$event'])
  @HostListener('touchmove', ['$event'])
  onmousemove(event) {
    if (this.inSwipe) {
      event.preventDefault();
      if (this.mainImg.width > 300) {
        return false;
      }

      let newX = this.getX(event);
      newX -= this.lastX;

      if (newX > 0) {
        if (newX > 200) newX = 200;
        newX = Math.round(49 * (newX / 200));  // between 0 and 50
        console.log(newX);
        this.mainImg.style.setProperty('--n', 100 - newX);
        this.rightImg.style.setProperty('--n', 100 - newX);
        this.leftImg.style.setProperty('--n', 100 + 2*newX);
      }
      else {
        if (newX < 0) {
          newX = -newX;
          if (newX > 200) newX = 200;
          newX = Math.round(49 * (newX / 200));
          console.log(-newX);
          this.mainImg.style.setProperty('--n', 100 - newX);
          this.leftImg.style.setProperty('--n', 100 - newX);
          this.rightImg.style.setProperty('--n', 100 + 2*newX);
        }
      }
    }
  }

  @HostListener('mousedown', ['$event'])
  @HostListener('touchstart', ['$event'])
  onmousedown(event) {
    console.log("down/start");  // NEW CODE FOR OUR MOUSE HANDLING
    if (event.target == this.mainImg) {
      event.preventDefault();
      this.inSwipe = true;
      console.log("swipe started");

      this.lastX = this.getX(event);
    }
  }

  @HostListener('mouseup', ['$event'])
  @HostListener('touchend', ['$event'])
  onmouseup(event) {
    console.log("up/end");
    if (this.inSwipe) {
      this.inSwipe = false;
      console.log("swipe ended");

      event.preventDefault();
      this.mainImg.style.setProperty('--n', '100');
      this.rightImg.style.setProperty('--n', '100');
      this.leftImg.style.setProperty('--n', '100');

      let newX = this.getX(event);
      this.setImages(Math.sign(this.lastX - newX))
    }
  }

  getX(event): number {
    if (event.changedTouches) {
      // for touch event
      return event.changedTouches[0].pageX;
    }
    else {
      return event.offsetX !== undefined ? event.offsetX : event.layerX;
    }
  }
}