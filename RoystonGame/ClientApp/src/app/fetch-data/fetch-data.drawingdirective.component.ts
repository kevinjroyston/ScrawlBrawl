import { Directive, ElementRef, HostListener, forwardRef, Input, Output, EventEmitter } from '@angular/core';

declare var angular: any;
@Directive({
    selector: '[appDrawing]',
})

export class DrawingDirective {
    userIsDrawing:boolean;
    lastX:number;
    lastY: number;
  ctx: any;
  lastActionWasStoreImage: boolean = false;
    @Input() lineColor: string;
    @Input() lineWidth: number;
    @Input() premadeDrawing: string;
  @Input() eraserMode: boolean;
    @Output() drawingEmitter = new EventEmitter();
    defaultLineColor: string = "rgba(87,0,132,255)";
    element;

  undoArray: string[] = [];



    constructor(element: ElementRef) {
        console.log("Instantiating canvas");
        this.element = element.nativeElement;
        this.ctx = element.nativeElement.getContext('2d');
        this.userIsDrawing = false;
    }

    ngOnInit() {
        console.log("Clearing canvas");
        this.ctx.clearRect(0, 0, this.ctx.canvas.width, this.ctx.canvas.height);

        if (this.premadeDrawing) {
            var img = new Image;
            var ctx = this.ctx;
            img.onload = function () {
                console.log("Drawing premade drawing to canvas");
                ctx.drawImage(img, 0, 0, ctx.canvas.width, ctx.canvas.height);
            };
            console.log("Loading premade drawing");
            img.src = this.premadeDrawing;
      }
      this.storeImage();
    }

  storeImage() {
    let imgStr = this.element.toDataURL();
    // write the data to the emitter
    this.drawingEmitter.emit(imgStr);

    // store it for an undo
    if (this.undoArray.length >= 20) { this.undoArray.shift(); }
    this.undoArray.push(imgStr);
    this.lastActionWasStoreImage = true;
  }

  onPerformUndo() {
    if (this.undoArray.length > 0) {
      if ((this.lastActionWasStoreImage) && (this.undoArray.length>1)) { this.undoArray.pop(); }  // the first call to undo would have the value we just stored, we want to go one back
      this.lastActionWasStoreImage = false;
      var img = new Image;
      var ctx = this.ctx;
      img.onload = function () {
          console.log("Drawing undo to canvas");
          ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);
          ctx.drawImage(img, 0, 0, ctx.canvas.width, ctx.canvas.height);
        };
      console.log("Loading undo drawing");
      let imgStr = (this.undoArray.length == 1) ? this.undoArray[0] : this.undoArray.pop();
      img.src = imgStr;
      this.drawingEmitter.emit(imgStr);
    }

  }
    @HostListener('mousedown', ['$event'])
    @HostListener('touchstart', ['$event'])
    onmousedown(event) {
        console.log("down/start");
//        event.preventDefault(); not needed since canvas does not care about mouse movements, and preventing affects gestures

        if (document.activeElement instanceof HTMLElement)  // pull focus from any input so it hides the keyboard on mobile
          document.activeElement.blur();
       
        [this.lastX, this.lastY] = this.getCoords(event);

        // begins new line
        //this.ctx.beginPath();
        this.drawCircle(this.lastX, this.lastY);

        this.userIsDrawing = true;
    }

    @HostListener('mousemove', ['$event'])
    @HostListener('touchmove', ['$event'])
    onmousemove(event) {
        //event.preventDefault();  not needed since canvas does not care about mouse movements, and preventing affects gestures
        if (this.userIsDrawing) {
            // get current mouse position
            let [currentX, currentY] = this.getCoords(event);

            // draw
            this.drawLine(this.lastX, this.lastY, currentX, currentY);
            this.drawCircle(currentX, currentY);

            // set current coordinates to last one
            this.lastX = currentX;
            this.lastY = currentY;
        }
    }


  @HostListener('document:gesturestart')
  @HostListener('document:gestureend')
  ongesturestart() {
    console.log("gesture start");
    if (this.userIsDrawing) {
      // stop drawing
      this.userIsDrawing = false;
     // this.storeImage();   we might want to call store image, we might want to revert / undo drawings since gesture started, needs testing
    }
  }

  @HostListener('document:mouseup')
  @HostListener('document:touchend')
    @HostListener('mouseup')
    @HostListener('touchend')
    onmouseup() {
      console.log("up/end");
      if (this.userIsDrawing) {
//        event.preventDefault(); not needed since canvas does not care about mouse movements, and preventing affects gestures

        // stop drawing
        this.userIsDrawing = false;
        this.storeImage();
      }
    }

    @HostListener('mouseleave')
    @HostListener('touchleave')
    onmouseleave() {
      console.log("mouseleave");
/*  we are now stopping the drawing on a global mouseup / touch end
      if (this.userIsDrawing) {
        event.preventDefault();

        // stop drawing
        this.userIsDrawing = false;
        this.storeImage();
      }

*/
    }

    drawLine(lX, lY, cX, cY): void {
        if (this.eraserMode)
            this.ctx.globalCompositeOperation = "destination-out";
        // line from
        this.ctx.beginPath();
        this.ctx.moveTo(lX, lY);
        // to
        this.ctx.lineTo(cX, cY);
        // color
        this.ctx.strokeStyle = this.getColor();
        //this.ctx.lineJoin = "round";
        this.ctx.lineWidth = this.lineWidth;
        // draw it
        this.ctx.stroke();
        this.ctx.globalCompositeOperation = "source-over";
    }

    getColor(): string {
        if (this.lineColor && this.lineColor != "") {
            return this.lineColor;
        }
        return this.defaultLineColor;
    }

    drawCircle(x, y): void {
        if (this.eraserMode)
           this.ctx.globalCompositeOperation = "destination-out";
        this.ctx.beginPath();
        var radius = this.lineWidth/2.1; // Arc radius
        var startAngle = 0; // Starting point on circle
        var endAngle = Math.PI * 2; // End point on circle

        this.ctx.moveTo(x, y);
        this.ctx.strokeStyle = this.getColor();
        this.ctx.lineWidth = 0;
        this.ctx.arc(x, y, radius, startAngle, endAngle);
        this.ctx.fillStyle = this.getColor();
        this.ctx.fill();
        this.ctx.globalCompositeOperation = "source-over";
    }

    getCoords(event) {
        let currentX : number, currentY : number;
        if (event.changedTouches) { // only for touch
            currentX = event.changedTouches[0].pageX - this.element.offsetLeft - this.element.clientLeft;
            currentY = event.changedTouches[0].pageY - this.element.offsetTop - this.element.clientTop;

        }
        else if (event.offsetX !== undefined) {
            currentX = event.offsetX;
            currentY = event.offsetY;
        }
        else {
            currentX = event.layerX - event.currentTarget.offsetLeft;
            currentY = event.layerY - event.currentTarget.offsetTop;
        }

        return [currentX * (this.element.width / this.element.clientWidth), currentY * (this.element.height / this.element.clientHeight)];
    }
}
