import { Directive, ElementRef, HostListener, forwardRef } from '@angular/core';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';

@Directive({
    selector: '[appDrawing]',
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            multi: true,
            useExisting: forwardRef(() => DrawingDirective),
        }
    ]
})
export class DrawingDirective implements ControlValueAccessor{
    writeValue(obj: any): void {
        if (obj == null) {
            this.ctx.clearRect(0, 0, this.ctx.canvas.width, this.ctx.canvas.height);
        }
    }
    registerOnChange(fn: any): void {
        this.onChange = fn; 
    }
    registerOnTouched(fn: any): void {

    }
    setDisabledState?(isDisabled: boolean): void {

    }
    constructor(element: ElementRef) {
        this.element = element.nativeElement;
        this.ctx = element.nativeElement.getContext('2d');
        this.drawing = false;
        /*this.mc = new Hammer.Manager(this.element, {
            touchAction: 'all',
            inputClass: Hammer.Input,
            recognizers: [
                [Hammer.Pan, {
                    direction: Hammer.DIRECTION_ALL,
                    threshold: 0,
                }]
            ]
        });

        this.mc.on('panstart', this.onmousedown);
        this.mc.on('panmove', this.onmousemove);
        this.mc.on('panend', this.onmouseup);*/
    }
    drawing:boolean;
    lastX:number;
    lastY: number;
    ctx: any;
    lineWidth: number = 10;
    lineColor = "#570084";
    onChange;
    element;

    @HostListener('mousedown', ['$event'])
    @HostListener('touchstart', ['$event'])
    onmousedown(event) {
        console.log("down/start");
        event.preventDefault();
        if (event.changedTouches) {                      // only for touch
            this.lastX = event.changedTouches[0].clientX;
            this.lastY = event.changedTouches[0].clientY;
        } else {
            if (event.offsetX !== undefined) {
                this.lastX = event.offsetX;
                this.lastY = event.offsetY;
            } else {
                this.lastX = event.layerX - event.currentTarget.offsetLeft;
                this.lastY = event.layerY - event.currentTarget.offsetTop;
            }
        }

        // begins new line
        //this.ctx.beginPath();
        this.drawCircle(this.lastX, this.lastY);

        this.drawing = true;
    }

    @HostListener('mousemove', ['$event'])
    @HostListener('touchmove', ['$event'])
    onmousemove(event) {
        console.log("move");
        event.preventDefault();
        if (this.drawing) {
            // get current mouse position
            var currentX;
            var currentY;
            if (event.changedTouches) {                      // only for touch
                currentX = event.changedTouches[0].clientX;
                currentY = event.changedTouches[0].clientY;
            }else if (event.offsetX !== undefined) {
                currentX = event.offsetX;
                currentY = event.offsetY;
            } else {
                currentX = event.layerX - event.currentTarget.offsetLeft;
                currentY = event.layerY - event.currentTarget.offsetTop;
            }
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
        this.drawing = false;
        this.onChange(this.element.toDataURL());
    }

    @HostListener('mouseleave')
    onmouseleave() {
        console.log("mouseleave");
        event.preventDefault();
        // stop drawing
        this.drawing = false;
        this.onChange(this.element.toDataURL());
    }

    drawLine(lX, lY, cX, cY): void {
        // line from
        this.ctx.beginPath();
        this.ctx.moveTo(lX, lY);
        // to
        this.ctx.lineTo(cX, cY);
        // color
        this.ctx.strokeStyle = this.lineColor;
        //this.ctx.lineJoin = "round";
        this.ctx.lineWidth = this.lineWidth;
        // draw it
        this.ctx.stroke();
    }

    drawCircle(x, y): void {
        this.ctx.beginPath();
        var radius = this.lineWidth/2.1; // Arc radius
        var startAngle = 0; // Starting point on circle
        var endAngle = Math.PI * 2; // End point on circle

        this.ctx.moveTo(x,y);
        this.ctx.strokeStyle = this.lineColor;
        this.ctx.lineWidth = 0;
        this.ctx.arc(x, y, radius, startAngle, endAngle);
        this.ctx.fillStyle = this.lineColor;
        this.ctx.fill();
    }
}
