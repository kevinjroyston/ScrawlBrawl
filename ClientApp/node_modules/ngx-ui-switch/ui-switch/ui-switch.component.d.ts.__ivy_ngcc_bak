import { ChangeDetectorRef, EventEmitter, OnDestroy } from '@angular/core';
import { ControlValueAccessor } from '@angular/forms';
import { UiSwitchModuleConfig } from './ui-switch.config';
import { Observable } from 'rxjs';
export declare class UiSwitchComponent implements ControlValueAccessor, OnDestroy {
    private cdr;
    private _checked;
    private _disabled;
    private _reverse;
    private _loading;
    private _beforeChange;
    size: any;
    color: any;
    switchOffColor: any;
    switchColor: any;
    defaultBgColor: any;
    defaultBoColor: any;
    checkedLabel: any;
    uncheckedLabel: any;
    checkedTextColor: any;
    uncheckedTextColor: any;
    beforeChange: Observable<boolean>;
    set checked(v: boolean);
    get checked(): boolean;
    set disabled(v: boolean);
    get disabled(): boolean;
    set reverse(v: boolean);
    get reverse(): boolean;
    set loading(v: boolean);
    get loading(): boolean;
    /**
     * Emits changed value
     */
    change: EventEmitter<boolean>;
    /**
     * Emits DOM event
     */
    changeEvent: EventEmitter<MouseEvent>;
    /**
     * Emits changed value
     */
    valueChange: EventEmitter<boolean>;
    constructor(config: UiSwitchModuleConfig | undefined, cdr: ChangeDetectorRef);
    getColor(flag?: string): any;
    onClick(event: MouseEvent): void;
    onToggle(event: MouseEvent): void;
    writeValue(obj: any): void;
    registerOnChange(fn: any): void;
    registerOnTouched(fn: any): void;
    setDisabledState(isDisabled: boolean): void;
    private onTouchedCallback;
    private onChangeCallback;
    ngOnDestroy(): void;
}
