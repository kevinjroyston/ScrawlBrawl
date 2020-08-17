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
    @Input() lineColor: string;
    @Input() lineWidth: number;
    @Input() premadeDrawing: string;
    @Input() eraserMode: boolean;
    @Output() drawingEmitter = new EventEmitter();
    defaultLineColor: string = "rgba(87,0,132,255)";
    element;

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
    }

    @HostListener('mousedown', ['$event'])
    @HostListener('touchstart', ['$event'])
    onmousedown(event) {
        console.log("down/start");
        event.preventDefault();
       
        [this.lastX, this.lastY] = this.getCoords(event);

        // begins new line
        //this.ctx.beginPath();
        this.drawCircle(this.lastX, this.lastY);

        this.userIsDrawing = true;
    }

    @HostListener('mousemove', ['$event'])
    @HostListener('touchmove', ['$event'])
    onmousemove(event) {
        event.preventDefault();
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

    @HostListener('mouseup')
    @HostListener('touchend')
    onmouseup() {
        console.log("up/end");
        event.preventDefault();

        // stop drawing
        this.userIsDrawing = false;
        this.drawingEmitter.emit(this.element.toDataURL());
    }

    @HostListener('mouseleave')
    onmouseleave() {
        console.log("mouseleave");
        event.preventDefault();

        // stop drawing
        this.userIsDrawing = false;
        this.drawingEmitter.emit(this.element.toDataURL());
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
