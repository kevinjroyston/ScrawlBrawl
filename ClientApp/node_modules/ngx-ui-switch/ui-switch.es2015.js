import { ChangeDetectorRef, Component, EventEmitter, HostListener, Inject, InjectionToken, Input, NgModule, Optional, Output, forwardRef } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import 'rxjs';
import { CommonModule } from '@angular/common';

/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes,extraRequire,missingOverride,missingReturn,unusedPrivateMembers,uselessCode} checked by tsc
 */
/** @type {?} */
const UI_SWITCH_OPTIONS = new InjectionToken('UI_SWITCH_OPTIONS');

/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes,extraRequire,missingOverride,missingReturn,unusedPrivateMembers,uselessCode} checked by tsc
 */
/** @type {?} */
const UI_SWITCH_CONTROL_VALUE_ACCESSOR = {
    provide: NG_VALUE_ACCESSOR,
    /* tslint:disable-next-line: no-use-before-declare */
    useExisting: forwardRef((/**
     * @return {?}
     */
    () => UiSwitchComponent)),
    multi: true,
};
class UiSwitchComponent {
    /**
     * @param {?=} config
     * @param {?=} cdr
     */
    constructor(config = {}, cdr) {
        this.cdr = cdr;
        /**
         * Emits changed value
         */
        this.change = new EventEmitter();
        /**
         * Emits DOM event
         */
        this.changeEvent = new EventEmitter();
        /**
         * Emits changed value
         */
        this.valueChange = new EventEmitter();
        this.onTouchedCallback = (/**
         * @param {?} v
         * @return {?}
         */
        (v) => { });
        this.onChangeCallback = (/**
         * @param {?} v
         * @return {?}
         */
        (v) => { });
        this.size = (config && config.size) || 'medium';
        this.color = config && config.color;
        this.switchOffColor = config && config.switchOffColor;
        this.switchColor = config && config.switchColor;
        this.defaultBgColor = config && config.defaultBgColor;
        this.defaultBoColor = config && config.defaultBoColor;
        this.checkedLabel = config && config.checkedLabel;
        this.uncheckedLabel = config && config.uncheckedLabel;
        this.checkedTextColor = config && config.checkedTextColor;
        this.uncheckedTextColor = config && config.uncheckedTextColor;
    }
    /**
     * @param {?} v
     * @return {?}
     */
    set checked(v) {
        this._checked = v !== false;
    }
    /**
     * @return {?}
     */
    get checked() {
        return this._checked;
    }
    /**
     * @param {?} v
     * @return {?}
     */
    set disabled(v) {
        this._disabled = v !== false;
    }
    /**
     * @return {?}
     */
    get disabled() {
        return this._disabled;
    }
    /**
     * @param {?} v
     * @return {?}
     */
    set reverse(v) {
        this._reverse = v !== false;
    }
    /**
     * @return {?}
     */
    get reverse() {
        return this._reverse;
    }
    /**
     * @param {?} v
     * @return {?}
     */
    set loading(v) {
        this._loading = v !== false;
    }
    /**
     * @return {?}
     */
    get loading() {
        return this._loading;
    }
    /**
     * @param {?=} flag
     * @return {?}
     */
    getColor(flag = '') {
        if (flag === 'borderColor') {
            return this.defaultBoColor;
        }
        if (flag === 'switchColor') {
            if (this.reverse) {
                return !this.checked ? this.switchColor : this.switchOffColor || this.switchColor;
            }
            return this.checked ? this.switchColor : this.switchOffColor || this.switchColor;
        }
        if (flag === 'checkedTextColor') {
            return this.reverse ? this.uncheckedTextColor : this.checkedTextColor;
        }
        if (flag === 'uncheckedTextColor') {
            return this.reverse ? this.checkedTextColor : this.uncheckedTextColor;
        }
        if (this.reverse) {
            return !this.checked ? this.color : this.defaultBgColor;
        }
        return this.checked ? this.color : this.defaultBgColor;
    }
    /**
     * @param {?} event
     * @return {?}
     */
    onClick(event) {
        if (this.disabled) {
            return;
        }
        this.checked = !this.checked;
        // Component events
        this.change.emit(this.checked);
        this.valueChange.emit(this.checked);
        this.changeEvent.emit(event);
        // value accessor callbacks
        this.onChangeCallback(this.checked);
        this.onTouchedCallback(this.checked);
        this.cdr.markForCheck();
    }
    /**
     * @param {?} event
     * @return {?}
     */
    onToggle(event) {
        if (this.beforeChange) {
            this._beforeChange = this.beforeChange.subscribe((/**
             * @param {?} confirm
             * @return {?}
             */
            (confirm) => {
                if (confirm) {
                    this.onClick(event);
                }
            }));
        }
        else {
            this.onClick(event);
        }
    }
    /**
     * @param {?} obj
     * @return {?}
     */
    writeValue(obj) {
        if (obj !== this.checked) {
            this.checked = !!obj;
        }
        if (this.cdr) {
            this.cdr.markForCheck();
        }
    }
    /**
     * @param {?} fn
     * @return {?}
     */
    registerOnChange(fn) {
        this.onChangeCallback = fn;
    }
    /**
     * @param {?} fn
     * @return {?}
     */
    registerOnTouched(fn) {
        this.onTouchedCallback = fn;
    }
    /**
     * @param {?} isDisabled
     * @return {?}
     */
    setDisabledState(isDisabled) {
        this.disabled = isDisabled;
    }
    /**
     * @return {?}
     */
    ngOnDestroy() {
        if (this._beforeChange) {
            this._beforeChange.unsubscribe();
        }
    }
}
UiSwitchComponent.decorators = [
    { type: Component, args: [{
                selector: 'ui-switch',
                template: "<button\n  type=\"button\"\n  class=\"switch\"\n  role=\"switch\"\n  [attr.aria-checked]=\"checked\"\n  [class.checked]=\"checked\"\n  [class.disabled]=\"disabled\"\n  [class.loading]=\"loading\"\n  [class.switch-large]=\"size === 'large'\"\n  [class.switch-medium]=\"size === 'medium'\"\n  [class.switch-small]=\"size === 'small'\"\n  [style.background-color]=\"getColor()\"\n  [style.border-color]=\"getColor('borderColor')\"\n>\n  <label class=\"switch-pane\" *ngIf=\"checkedLabel || uncheckedLabel\">\n    <span\n      [attr.aria-label]=\"this.checkedLabel\"\n      class=\"switch-label-checked\"\n      [style.color]=\"getColor('checkedTextColor')\"\n      >{{ this.checkedLabel }}</span\n    >\n    <span\n      [attr.aria-label]=\"this.uncheckedLabel\"\n      class=\"switch-label-unchecked\"\n      [style.color]=\"getColor('uncheckedTextColor')\"\n      >{{ this.uncheckedLabel }}</span\n    >\n  </label>\n  <small [style.background]=\"getColor('switchColor')\">\n    <ng-content></ng-content>\n  </small>\n</button>\n",
                providers: [UI_SWITCH_CONTROL_VALUE_ACCESSOR]
            }] }
];
/** @nocollapse */
UiSwitchComponent.ctorParameters = () => [
    { type: undefined, decorators: [{ type: Inject, args: [UI_SWITCH_OPTIONS,] }, { type: Optional }] },
    { type: ChangeDetectorRef }
];
UiSwitchComponent.propDecorators = {
    size: [{ type: Input }],
    color: [{ type: Input }],
    switchOffColor: [{ type: Input }],
    switchColor: [{ type: Input }],
    defaultBgColor: [{ type: Input }],
    defaultBoColor: [{ type: Input }],
    checkedLabel: [{ type: Input }],
    uncheckedLabel: [{ type: Input }],
    checkedTextColor: [{ type: Input }],
    uncheckedTextColor: [{ type: Input }],
    beforeChange: [{ type: Input }],
    checked: [{ type: Input }],
    disabled: [{ type: Input }],
    reverse: [{ type: Input }],
    loading: [{ type: Input }],
    change: [{ type: Output }],
    changeEvent: [{ type: Output }],
    valueChange: [{ type: Output }],
    onToggle: [{ type: HostListener, args: ['click', ['$event'],] }]
};

/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes,extraRequire,missingOverride,missingReturn,unusedPrivateMembers,uselessCode} checked by tsc
 */
class UiSwitchModule {
    /**
     * @param {?} config
     * @return {?}
     */
    static forRoot(config) {
        return {
            ngModule: UiSwitchModule,
            providers: [
                { provide: UI_SWITCH_OPTIONS, useValue: config || {} }
            ]
        };
    }
}
UiSwitchModule.decorators = [
    { type: NgModule, args: [{
                declarations: [
                    UiSwitchComponent
                ],
                imports: [
                    CommonModule,
                    FormsModule
                ],
                exports: [
                    FormsModule,
                    UiSwitchComponent
                ]
            },] }
];

/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes,extraRequire,missingOverride,missingReturn,unusedPrivateMembers,uselessCode} checked by tsc
 */

/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes,extraRequire,missingOverride,missingReturn,unusedPrivateMembers,uselessCode} checked by tsc
 */
/**
 * Generated bundle index. Do not edit.
 */

export { UiSwitchComponent, UiSwitchModule, UI_SWITCH_OPTIONS as ɵa };
//# sourceMappingURL=ui-switch.es2015.js.map
