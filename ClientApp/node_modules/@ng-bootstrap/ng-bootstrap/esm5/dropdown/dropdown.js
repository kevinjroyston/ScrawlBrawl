import { __decorate, __extends, __param } from "tslib";
import { ChangeDetectorRef, ContentChild, ContentChildren, Directive, ElementRef, EventEmitter, forwardRef, Inject, Input, NgZone, AfterContentInit, OnDestroy, Output, QueryList, Renderer2, SimpleChanges, Optional } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { fromEvent, Subject } from 'rxjs';
import { take } from 'rxjs/operators';
import { positionElements } from '../util/positioning';
import { ngbAutoClose } from '../util/autoclose';
import { Key } from '../util/key';
import { NgbDropdownConfig } from './dropdown-config';
var NgbNavbar = /** @class */ (function () {
    function NgbNavbar() {
    }
    NgbNavbar = __decorate([
        Directive({ selector: '.navbar' })
    ], NgbNavbar);
    return NgbNavbar;
}());
export { NgbNavbar };
/**
 * A directive you should put on a dropdown item to enable keyboard navigation.
 * Arrow keys will move focus between items marked with this directive.
 *
 * @since 4.1.0
 */
var NgbDropdownItem = /** @class */ (function () {
    function NgbDropdownItem(elementRef) {
        this.elementRef = elementRef;
        this._disabled = false;
    }
    Object.defineProperty(NgbDropdownItem.prototype, "disabled", {
        get: function () { return this._disabled; },
        set: function (value) {
            this._disabled = value === '' || value === true; // accept an empty attribute as true
        },
        enumerable: true,
        configurable: true
    });
    NgbDropdownItem.ctorParameters = function () { return [
        { type: ElementRef }
    ]; };
    __decorate([
        Input()
    ], NgbDropdownItem.prototype, "disabled", null);
    NgbDropdownItem = __decorate([
        Directive({ selector: '[ngbDropdownItem]', host: { 'class': 'dropdown-item', '[class.disabled]': 'disabled' } })
    ], NgbDropdownItem);
    return NgbDropdownItem;
}());
export { NgbDropdownItem };
/**
 * A directive that wraps dropdown menu content and dropdown items.
 */
var NgbDropdownMenu = /** @class */ (function () {
    function NgbDropdownMenu(dropdown) {
        this.dropdown = dropdown;
        this.placement = 'bottom';
        this.isOpen = false;
    }
    NgbDropdownMenu.ctorParameters = function () { return [
        { type: undefined, decorators: [{ type: Inject, args: [forwardRef(function () { return NgbDropdown; }),] }] }
    ]; };
    __decorate([
        ContentChildren(NgbDropdownItem)
    ], NgbDropdownMenu.prototype, "menuItems", void 0);
    NgbDropdownMenu = __decorate([
        Directive({
            selector: '[ngbDropdownMenu]',
            host: {
                '[class.dropdown-menu]': 'true',
                '[class.show]': 'dropdown.isOpen()',
                '[attr.x-placement]': 'placement',
                '(keydown.ArrowUp)': 'dropdown.onKeyDown($event)',
                '(keydown.ArrowDown)': 'dropdown.onKeyDown($event)',
                '(keydown.Home)': 'dropdown.onKeyDown($event)',
                '(keydown.End)': 'dropdown.onKeyDown($event)',
                '(keydown.Enter)': 'dropdown.onKeyDown($event)',
                '(keydown.Space)': 'dropdown.onKeyDown($event)'
            }
        }),
        __param(0, Inject(forwardRef(function () { return NgbDropdown; })))
    ], NgbDropdownMenu);
    return NgbDropdownMenu;
}());
export { NgbDropdownMenu };
/**
 * A directive to mark an element to which dropdown menu will be anchored.
 *
 * This is a simple version of the `NgbDropdownToggle` directive.
 * It plays the same role, but doesn't listen to click events to toggle dropdown menu thus enabling support
 * for events other than click.
 *
 * @since 1.1.0
 */
var NgbDropdownAnchor = /** @class */ (function () {
    function NgbDropdownAnchor(dropdown, _elementRef) {
        this.dropdown = dropdown;
        this._elementRef = _elementRef;
        this.anchorEl = _elementRef.nativeElement;
    }
    NgbDropdownAnchor.prototype.getNativeElement = function () { return this._elementRef.nativeElement; };
    NgbDropdownAnchor.ctorParameters = function () { return [
        { type: undefined, decorators: [{ type: Inject, args: [forwardRef(function () { return NgbDropdown; }),] }] },
        { type: ElementRef }
    ]; };
    NgbDropdownAnchor = __decorate([
        Directive({
            selector: '[ngbDropdownAnchor]',
            host: { 'class': 'dropdown-toggle', 'aria-haspopup': 'true', '[attr.aria-expanded]': 'dropdown.isOpen()' }
        }),
        __param(0, Inject(forwardRef(function () { return NgbDropdown; })))
    ], NgbDropdownAnchor);
    return NgbDropdownAnchor;
}());
export { NgbDropdownAnchor };
/**
 * A directive to mark an element that will toggle dropdown via the `click` event.
 *
 * You can also use `NgbDropdownAnchor` as an alternative.
 */
var NgbDropdownToggle = /** @class */ (function (_super) {
    __extends(NgbDropdownToggle, _super);
    function NgbDropdownToggle(dropdown, elementRef) {
        return _super.call(this, dropdown, elementRef) || this;
    }
    NgbDropdownToggle_1 = NgbDropdownToggle;
    var NgbDropdownToggle_1;
    NgbDropdownToggle.ctorParameters = function () { return [
        { type: undefined, decorators: [{ type: Inject, args: [forwardRef(function () { return NgbDropdown; }),] }] },
        { type: ElementRef }
    ]; };
    NgbDropdownToggle = NgbDropdownToggle_1 = __decorate([
        Directive({
            selector: '[ngbDropdownToggle]',
            host: {
                'class': 'dropdown-toggle',
                'aria-haspopup': 'true',
                '[attr.aria-expanded]': 'dropdown.isOpen()',
                '(click)': 'dropdown.toggle()',
                '(keydown.ArrowUp)': 'dropdown.onKeyDown($event)',
                '(keydown.ArrowDown)': 'dropdown.onKeyDown($event)',
                '(keydown.Home)': 'dropdown.onKeyDown($event)',
                '(keydown.End)': 'dropdown.onKeyDown($event)'
            },
            providers: [{ provide: NgbDropdownAnchor, useExisting: forwardRef(function () { return NgbDropdownToggle_1; }) }]
        }),
        __param(0, Inject(forwardRef(function () { return NgbDropdown; })))
    ], NgbDropdownToggle);
    return NgbDropdownToggle;
}(NgbDropdownAnchor));
export { NgbDropdownToggle };
/**
 * A directive that provides contextual overlays for displaying lists of links and more.
 */
var NgbDropdown = /** @class */ (function () {
    function NgbDropdown(_changeDetector, config, _document, _ngZone, _elementRef, _renderer, ngbNavbar) {
        var _this = this;
        this._changeDetector = _changeDetector;
        this._document = _document;
        this._ngZone = _ngZone;
        this._elementRef = _elementRef;
        this._renderer = _renderer;
        this._closed$ = new Subject();
        this._bodyContainer = null;
        /**
         * Defines whether or not the dropdown menu is opened initially.
         */
        this._open = false;
        /**
         * An event fired when the dropdown is opened or closed.
         *
         * The event payload is a `boolean`:
         * * `true` - the dropdown was opened
         * * `false` - the dropdown was closed
         */
        this.openChange = new EventEmitter();
        this.placement = config.placement;
        this.container = config.container;
        this.autoClose = config.autoClose;
        this.display = ngbNavbar ? 'static' : 'dynamic';
        this._zoneSubscription = _ngZone.onStable.subscribe(function () { _this._positionMenu(); });
    }
    NgbDropdown.prototype.ngAfterContentInit = function () {
        var _this = this;
        this._ngZone.onStable.pipe(take(1)).subscribe(function () {
            _this._applyPlacementClasses();
            if (_this._open) {
                _this._setCloseHandlers();
            }
        });
    };
    NgbDropdown.prototype.ngOnChanges = function (changes) {
        if (changes.container && this._open) {
            this._applyContainer(this.container);
        }
        if (changes.placement && !changes.placement.isFirstChange) {
            this._applyPlacementClasses();
        }
    };
    /**
     * Checks if the dropdown menu is open.
     */
    NgbDropdown.prototype.isOpen = function () { return this._open; };
    /**
     * Opens the dropdown menu.
     */
    NgbDropdown.prototype.open = function () {
        if (!this._open) {
            this._open = true;
            this._applyContainer(this.container);
            this.openChange.emit(true);
            this._setCloseHandlers();
        }
    };
    NgbDropdown.prototype._setCloseHandlers = function () {
        var _this = this;
        var anchor = this._anchor;
        ngbAutoClose(this._ngZone, this._document, this.autoClose, function () { return _this.close(); }, this._closed$, this._menu ? [this._menuElement.nativeElement] : [], anchor ? [anchor.getNativeElement()] : [], '.dropdown-item,.dropdown-divider');
    };
    /**
     * Closes the dropdown menu.
     */
    NgbDropdown.prototype.close = function () {
        if (this._open) {
            this._open = false;
            this._resetContainer();
            this._closed$.next();
            this.openChange.emit(false);
            this._changeDetector.markForCheck();
        }
    };
    /**
     * Toggles the dropdown menu.
     */
    NgbDropdown.prototype.toggle = function () {
        if (this.isOpen()) {
            this.close();
        }
        else {
            this.open();
        }
    };
    NgbDropdown.prototype.ngOnDestroy = function () {
        this._resetContainer();
        this._closed$.next();
        this._zoneSubscription.unsubscribe();
    };
    NgbDropdown.prototype.onKeyDown = function (event) {
        var _this = this;
        // tslint:disable-next-line:deprecation
        var key = event.which;
        var itemElements = this._getMenuElements();
        var position = -1;
        var itemElement = null;
        var isEventFromToggle = this._isEventFromToggle(event);
        if (!isEventFromToggle && itemElements.length) {
            itemElements.forEach(function (item, index) {
                if (item.contains(event.target)) {
                    itemElement = item;
                }
                if (item === _this._document.activeElement) {
                    position = index;
                }
            });
        }
        // closing on Enter / Space
        if (key === Key.Space || key === Key.Enter) {
            if (itemElement && (this.autoClose === true || this.autoClose === 'inside')) {
                // Item is either a button or a link, so click will be triggered by the browser on Enter or Space.
                // So we have to register a one-time click handler that will fire after any user defined click handlers
                // to close the dropdown
                fromEvent(itemElement, 'click').pipe(take(1)).subscribe(function () { return _this.close(); });
            }
            return;
        }
        // opening / navigating
        if (isEventFromToggle || itemElement) {
            this.open();
            if (itemElements.length) {
                switch (key) {
                    case Key.ArrowDown:
                        position = Math.min(position + 1, itemElements.length - 1);
                        break;
                    case Key.ArrowUp:
                        if (this._isDropup() && position === -1) {
                            position = itemElements.length - 1;
                            break;
                        }
                        position = Math.max(position - 1, 0);
                        break;
                    case Key.Home:
                        position = 0;
                        break;
                    case Key.End:
                        position = itemElements.length - 1;
                        break;
                }
                itemElements[position].focus();
            }
            event.preventDefault();
        }
    };
    NgbDropdown.prototype._isDropup = function () { return this._elementRef.nativeElement.classList.contains('dropup'); };
    NgbDropdown.prototype._isEventFromToggle = function (event) {
        return this._anchor.getNativeElement().contains(event.target);
    };
    NgbDropdown.prototype._getMenuElements = function () {
        var menu = this._menu;
        if (menu == null) {
            return [];
        }
        return menu.menuItems.filter(function (item) { return !item.disabled; }).map(function (item) { return item.elementRef.nativeElement; });
    };
    NgbDropdown.prototype._positionMenu = function () {
        var menu = this._menu;
        if (this.isOpen() && menu) {
            this._applyPlacementClasses(this.display === 'dynamic' ?
                positionElements(this._anchor.anchorEl, this._bodyContainer || this._menuElement.nativeElement, this.placement, this.container === 'body') :
                this._getFirstPlacement(this.placement));
        }
    };
    NgbDropdown.prototype._getFirstPlacement = function (placement) {
        return Array.isArray(placement) ? placement[0] : placement.split(' ')[0];
    };
    NgbDropdown.prototype._resetContainer = function () {
        var renderer = this._renderer;
        var menuElement = this._menuElement;
        if (menuElement) {
            var dropdownElement = this._elementRef.nativeElement;
            var dropdownMenuElement = menuElement.nativeElement;
            renderer.appendChild(dropdownElement, dropdownMenuElement);
            renderer.removeStyle(dropdownMenuElement, 'position');
            renderer.removeStyle(dropdownMenuElement, 'transform');
        }
        if (this._bodyContainer) {
            renderer.removeChild(this._document.body, this._bodyContainer);
            this._bodyContainer = null;
        }
    };
    NgbDropdown.prototype._applyContainer = function (container) {
        if (container === void 0) { container = null; }
        this._resetContainer();
        if (container === 'body') {
            var renderer = this._renderer;
            var dropdownMenuElement = this._menuElement.nativeElement;
            var bodyContainer = this._bodyContainer = this._bodyContainer || renderer.createElement('div');
            // Override some styles to have the positionning working
            renderer.setStyle(bodyContainer, 'position', 'absolute');
            renderer.setStyle(dropdownMenuElement, 'position', 'static');
            renderer.setStyle(bodyContainer, 'z-index', '1050');
            renderer.appendChild(bodyContainer, dropdownMenuElement);
            renderer.appendChild(this._document.body, bodyContainer);
        }
    };
    NgbDropdown.prototype._applyPlacementClasses = function (placement) {
        var menu = this._menu;
        if (menu) {
            if (!placement) {
                placement = this._getFirstPlacement(this.placement);
            }
            var renderer = this._renderer;
            var dropdownElement = this._elementRef.nativeElement;
            // remove the current placement classes
            renderer.removeClass(dropdownElement, 'dropup');
            renderer.removeClass(dropdownElement, 'dropdown');
            menu.placement = this.display === 'static' ? null : placement;
            /*
            * apply the new placement
            * in case of top use up-arrow or down-arrow otherwise
            */
            var dropdownClass = placement.search('^top') !== -1 ? 'dropup' : 'dropdown';
            renderer.addClass(dropdownElement, dropdownClass);
            var bodyContainer = this._bodyContainer;
            if (bodyContainer) {
                renderer.removeClass(bodyContainer, 'dropup');
                renderer.removeClass(bodyContainer, 'dropdown');
                renderer.addClass(bodyContainer, dropdownClass);
            }
        }
    };
    NgbDropdown.ctorParameters = function () { return [
        { type: ChangeDetectorRef },
        { type: NgbDropdownConfig },
        { type: undefined, decorators: [{ type: Inject, args: [DOCUMENT,] }] },
        { type: NgZone },
        { type: ElementRef },
        { type: Renderer2 },
        { type: NgbNavbar, decorators: [{ type: Optional }] }
    ]; };
    __decorate([
        ContentChild(NgbDropdownMenu, { static: false })
    ], NgbDropdown.prototype, "_menu", void 0);
    __decorate([
        ContentChild(NgbDropdownMenu, { read: ElementRef, static: false })
    ], NgbDropdown.prototype, "_menuElement", void 0);
    __decorate([
        ContentChild(NgbDropdownAnchor, { static: false })
    ], NgbDropdown.prototype, "_anchor", void 0);
    __decorate([
        Input()
    ], NgbDropdown.prototype, "autoClose", void 0);
    __decorate([
        Input('open')
    ], NgbDropdown.prototype, "_open", void 0);
    __decorate([
        Input()
    ], NgbDropdown.prototype, "placement", void 0);
    __decorate([
        Input()
    ], NgbDropdown.prototype, "container", void 0);
    __decorate([
        Input()
    ], NgbDropdown.prototype, "display", void 0);
    __decorate([
        Output()
    ], NgbDropdown.prototype, "openChange", void 0);
    NgbDropdown = __decorate([
        Directive({ selector: '[ngbDropdown]', exportAs: 'ngbDropdown', host: { '[class.show]': 'isOpen()' } }),
        __param(2, Inject(DOCUMENT)),
        __param(6, Optional())
    ], NgbDropdown);
    return NgbDropdown;
}());
export { NgbDropdown };
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiZHJvcGRvd24uanMiLCJzb3VyY2VSb290Ijoibmc6Ly9AbmctYm9vdHN0cmFwL25nLWJvb3RzdHJhcC8iLCJzb3VyY2VzIjpbImRyb3Bkb3duL2Ryb3Bkb3duLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7QUFBQSxPQUFPLEVBQ0wsaUJBQWlCLEVBQ2pCLFlBQVksRUFDWixlQUFlLEVBQ2YsU0FBUyxFQUNULFVBQVUsRUFDVixZQUFZLEVBQ1osVUFBVSxFQUNWLE1BQU0sRUFDTixLQUFLLEVBQ0wsTUFBTSxFQUNOLGdCQUFnQixFQUNoQixTQUFTLEVBQ1QsTUFBTSxFQUNOLFNBQVMsRUFDVCxTQUFTLEVBQ1QsYUFBYSxFQUNiLFFBQVEsRUFDVCxNQUFNLGVBQWUsQ0FBQztBQUN2QixPQUFPLEVBQUMsUUFBUSxFQUFDLE1BQU0saUJBQWlCLENBQUM7QUFDekMsT0FBTyxFQUFDLFNBQVMsRUFBRSxPQUFPLEVBQWUsTUFBTSxNQUFNLENBQUM7QUFDdEQsT0FBTyxFQUFDLElBQUksRUFBQyxNQUFNLGdCQUFnQixDQUFDO0FBRXBDLE9BQU8sRUFBNEIsZ0JBQWdCLEVBQUMsTUFBTSxxQkFBcUIsQ0FBQztBQUNoRixPQUFPLEVBQUMsWUFBWSxFQUFDLE1BQU0sbUJBQW1CLENBQUM7QUFDL0MsT0FBTyxFQUFDLEdBQUcsRUFBQyxNQUFNLGFBQWEsQ0FBQztBQUVoQyxPQUFPLEVBQUMsaUJBQWlCLEVBQUMsTUFBTSxtQkFBbUIsQ0FBQztBQUdwRDtJQUFBO0lBQ0EsQ0FBQztJQURZLFNBQVM7UUFEckIsU0FBUyxDQUFDLEVBQUMsUUFBUSxFQUFFLFNBQVMsRUFBQyxDQUFDO09BQ3BCLFNBQVMsQ0FDckI7SUFBRCxnQkFBQztDQUFBLEFBREQsSUFDQztTQURZLFNBQVM7QUFHdEI7Ozs7O0dBS0c7QUFFSDtJQVlFLHlCQUFtQixVQUFtQztRQUFuQyxlQUFVLEdBQVYsVUFBVSxDQUF5QjtRQVQ5QyxjQUFTLEdBQUcsS0FBSyxDQUFDO0lBUytCLENBQUM7SUFOMUQsc0JB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Q0FBQzs4Q0FBZ0M7SUFDYjtRQUFqRSxZQUFZLENBQUMsZUFBZSxFQUFFLEVBQUMsSUFBSSxFQUFFLFVBQVUsRUFBRSxNQUFNLEVBQUUsS0FBSyxFQUFDLENBQUM7cURBQWtDO0lBQ2pEO1FBQWpELFlBQVksQ0FBQyxpQkFBaUIsRUFBRSxFQUFDLE1BQU0sRUFBRSxLQUFLLEVBQUMsQ0FBQztnREFBb0M7SUFVNUU7UUFBUixLQUFLLEVBQUU7a0RBQTJDO0lBS3BDO1FBQWQsS0FBSyxDQUFDLE1BQU0sQ0FBQzs4Q0FBZTtJQWVwQjtRQUFSLEtBQUssRUFBRTtrREFBMkI7SUFRMUI7UUFBUixLQUFLLEVBQUU7a0RBQTBCO0lBVXpCO1FBQVIsS0FBSyxFQUFFO2dEQUErQjtJQVM3QjtRQUFULE1BQU0sRUFBRTttREFBMEM7SUFuRXhDLFdBQVc7UUFEdkIsU0FBUyxDQUFDLEVBQUMsUUFBUSxFQUFFLGVBQWUsRUFBRSxRQUFRLEVBQUUsYUFBYSxFQUFFLElBQUksRUFBRSxFQUFDLGNBQWMsRUFBRSxVQUFVLEVBQUMsRUFBQyxDQUFDO1FBdUV0QixXQUFBLE1BQU0sQ0FBQyxRQUFRLENBQUMsQ0FBQTtRQUV2RixXQUFBLFFBQVEsRUFBRSxDQUFBO09BeEVKLFdBQVcsQ0F1VHZCO0lBQUQsa0JBQUM7Q0FBQSxBQXZURCxJQXVUQztTQXZUWSxXQUFXIiwic291cmNlc0NvbnRlbnQiOlsiaW1wb3J0IHtcbiAgQ2hhbmdlRGV0ZWN0b3JSZWYsXG4gIENvbnRlbnRDaGlsZCxcbiAgQ29udGVudENoaWxkcmVuLFxuICBEaXJlY3RpdmUsXG4gIEVsZW1lbnRSZWYsXG4gIEV2ZW50RW1pdHRlcixcbiAgZm9yd2FyZFJlZixcbiAgSW5qZWN0LFxuICBJbnB1dCxcbiAgTmdab25lLFxuICBBZnRlckNvbnRlbnRJbml0LFxuICBPbkRlc3Ryb3ksXG4gIE91dHB1dCxcbiAgUXVlcnlMaXN0LFxuICBSZW5kZXJlcjIsXG4gIFNpbXBsZUNoYW5nZXMsXG4gIE9wdGlvbmFsXG59IGZyb20gJ0Bhbmd1bGFyL2NvcmUnO1xuaW1wb3J0IHtET0NVTUVOVH0gZnJvbSAnQGFuZ3VsYXIvY29tbW9uJztcbmltcG9ydCB7ZnJvbUV2ZW50LCBTdWJqZWN0LCBTdWJzY3JpcHRpb259IGZyb20gJ3J4anMnO1xuaW1wb3J0IHt0YWtlfSBmcm9tICdyeGpzL29wZXJhdG9ycyc7XG5cbmltcG9ydCB7UGxhY2VtZW50LCBQbGFjZW1lbnRBcnJheSwgcG9zaXRpb25FbGVtZW50c30gZnJvbSAnLi4vdXRpbC9wb3NpdGlvbmluZyc7XG5pbXBvcnQge25nYkF1dG9DbG9zZX0gZnJvbSAnLi4vdXRpbC9hdXRvY2xvc2UnO1xuaW1wb3J0IHtLZXl9IGZyb20gJy4uL3V0aWwva2V5JztcblxuaW1wb3J0IHtOZ2JEcm9wZG93bkNvbmZpZ30gZnJvbSAnLi9kcm9wZG93bi1jb25maWcnO1xuXG5ARGlyZWN0aXZlKHtzZWxlY3RvcjogJy5uYXZiYXInfSlcbmV4cG9ydCBjbGFzcyBOZ2JOYXZiYXIge1xufVxuXG4vKipcbiAqIEEgZGlyZWN0aXZlIHlvdSBzaG91bGQgcHV0IG9uIGEgZHJvcGRvd24gaXRlbSB0byBlbmFibGUga2V5Ym9hcmQgbmF2aWdhdGlvbi5cbiAqIEFycm93IGtleXMgd2lsbCBtb3ZlIGZvY3VzIGJldHdlZW4gaXRlbXMgbWFya2VkIHdpdGggdGhpcyBkaXJlY3RpdmUuXG4gKlxuICogQHNpbmNlIDQuMS4wXG4gKi9cbkBEaXJlY3RpdmUoe3NlbGVjdG9yOiAnW25nYkRyb3Bkb3duSXRlbV0nLCBob3N0OiB7J2NsYXNzJzogJ2Ryb3Bkb3duLWl0ZW0nLCAnW2NsYXNzLmRpc2FibGVkXSc6ICdkaXNhYmxlZCd9fSlcbmV4cG9ydCBjbGFzcyBOZ2JEcm9wZG93bkl0ZW0ge1xuICBzdGF0aWMgbmdBY2NlcHRJbnB1dFR5cGVfZGlzYWJsZWQ6IGJvb2xlYW4gfCAnJztcblxuICBwcml2YXRlIF9kaXNhYmxlZCA9IGZhbHNlO1xuXG4gIEBJbnB1dCgpXG4gIHNldCBkaXNhYmxlZCh2YWx1ZTogYm9vbGVhbikge1xuICAgIHRoaXMuX2Rpc2FibGVkID0gPGFueT52YWx1ZSA9PT0gJycgfHwgdmFsdWUgPT09IHRydWU7ICAvLyBhY2NlcHQgYW4gZW1wdHkgYXR0cmlidXRlIGFzIHRydWVcbiAgfVxuXG4gIGdldCBkaXNhYmxlZCgpOiBib29sZWFuIHsgcmV0dXJuIHRoaXMuX2Rpc2FibGVkOyB9XG5cbiAgY29uc3RydWN0b3IocHVibGljIGVsZW1lbnRSZWY6IEVsZW1lbnRSZWY8SFRNTEVsZW1lbnQ+KSB7fVxufVxuXG4vKipcbiAqIEEgZGlyZWN0aXZlIHRoYXQgd3JhcHMgZHJvcGRvd24gbWVudSBjb250ZW50IGFuZCBkcm9wZG93biBpdGVtcy5cbiAqL1xuQERpcmVjdGl2ZSh7XG4gIHNlbGVjdG9yOiAnW25nYkRyb3Bkb3duTWVudV0nLFxuICBob3N0OiB7XG4gICAgJ1tjbGFzcy5kcm9wZG93bi1tZW51XSc6ICd0cnVlJyxcbiAgICAnW2NsYXNzLnNob3ddJzogJ2Ryb3Bkb3duLmlzT3BlbigpJyxcbiAgICAnW2F0dHIueC1wbGFjZW1lbnRdJzogJ3BsYWNlbWVudCcsXG4gICAgJyhrZXlkb3duLkFycm93VXApJzogJ2Ryb3Bkb3duLm9uS2V5RG93bigkZXZlbnQpJyxcbiAgICAnKGtleWRvd24uQXJyb3dEb3duKSc6ICdkcm9wZG93bi5vbktleURvd24oJGV2ZW50KScsXG4gICAgJyhrZXlkb3duLkhvbWUpJzogJ2Ryb3Bkb3duLm9uS2V5RG93bigkZXZlbnQpJyxcbiAgICAnKGtleWRvd24uRW5kKSc6ICdkcm9wZG93bi5vbktleURvd24oJGV2ZW50KScsXG4gICAgJyhrZXlkb3duLkVudGVyKSc6ICdkcm9wZG93bi5vbktleURvd24oJGV2ZW50KScsXG4gICAgJyhrZXlkb3duLlNwYWNlKSc6ICdkcm9wZG93bi5vbktleURvd24oJGV2ZW50KSdcbiAgfVxufSlcbmV4cG9ydCBjbGFzcyBOZ2JEcm9wZG93bk1lbnUge1xuICBwbGFjZW1lbnQ6IFBsYWNlbWVudCB8IG51bGwgPSAnYm90dG9tJztcbiAgaXNPcGVuID0gZmFsc2U7XG5cbiAgQENvbnRlbnRDaGlsZHJlbihOZ2JEcm9wZG93bkl0ZW0pIG1lbnVJdGVtczogUXVlcnlMaXN0PE5nYkRyb3Bkb3duSXRlbT47XG5cbiAgY29uc3RydWN0b3IoQEluamVjdChmb3J3YXJkUmVmKCgpID0+IE5nYkRyb3Bkb3duKSkgcHVibGljIGRyb3Bkb3duKSB7fVxufVxuXG4vKipcbiAqIEEgZGlyZWN0aXZlIHRvIG1hcmsgYW4gZWxlbWVudCB0byB3aGljaCBkcm9wZG93biBtZW51IHdpbGwgYmUgYW5jaG9yZWQuXG4gKlxuICogVGhpcyBpcyBhIHNpbXBsZSB2ZXJzaW9uIG9mIHRoZSBgTmdiRHJvcGRvd25Ub2dnbGVgIGRpcmVjdGl2ZS5cbiAqIEl0IHBsYXlzIHRoZSBzYW1lIHJvbGUsIGJ1dCBkb2Vzbid0IGxpc3RlbiB0byBjbGljayBldmVudHMgdG8gdG9nZ2xlIGRyb3Bkb3duIG1lbnUgdGh1cyBlbmFibGluZyBzdXBwb3J0XG4gKiBmb3IgZXZlbnRzIG90aGVyIHRoYW4gY2xpY2suXG4gKlxuICogQHNpbmNlIDEuMS4wXG4gKi9cbkBEaXJlY3RpdmUoe1xuICBzZWxlY3RvcjogJ1tuZ2JEcm9wZG93bkFuY2hvcl0nLFxuICBob3N0OiB7J2NsYXNzJzogJ2Ryb3Bkb3duLXRvZ2dsZScsICdhcmlhLWhhc3BvcHVwJzogJ3RydWUnLCAnW2F0dHIuYXJpYS1leHBhbmRlZF0nOiAnZHJvcGRvd24uaXNPcGVuKCknfVxufSlcbmV4cG9ydCBjbGFzcyBOZ2JEcm9wZG93bkFuY2hvciB7XG4gIGFuY2hvckVsO1xuXG4gIGNvbnN0cnVjdG9yKEBJbmplY3QoZm9yd2FyZFJlZigoKSA9PiBOZ2JEcm9wZG93bikpIHB1YmxpYyBkcm9wZG93biwgcHJpdmF0ZSBfZWxlbWVudFJlZjogRWxlbWVudFJlZjxIVE1MRWxlbWVudD4pIHtcbiAgICB0aGlzLmFuY2hvckVsID0gX2VsZW1lbnRSZWYubmF0aXZlRWxlbWVudDtcbiAgfVxuXG4gIGdldE5hdGl2ZUVsZW1lbnQoKSB7IHJldHVybiB0aGlzLl9lbGVtZW50UmVmLm5hdGl2ZUVsZW1lbnQ7IH1cbn1cblxuLyoqXG4gKiBBIGRpcmVjdGl2ZSB0byBtYXJrIGFuIGVsZW1lbnQgdGhhdCB3aWxsIHRvZ2dsZSBkcm9wZG93biB2aWEgdGhlIGBjbGlja2AgZXZlbnQuXG4gKlxuICogWW91IGNhbiBhbHNvIHVzZSBgTmdiRHJvcGRvd25BbmNob3JgIGFzIGFuIGFsdGVybmF0aXZlLlxuICovXG5ARGlyZWN0aXZlKHtcbiAgc2VsZWN0b3I6ICdbbmdiRHJvcGRvd25Ub2dnbGVdJyxcbiAgaG9zdDoge1xuICAgICdjbGFzcyc6ICdkcm9wZG93bi10b2dnbGUnLFxuICAgICdhcmlhLWhhc3BvcHVwJzogJ3RydWUnLFxuICAgICdbYXR0ci5hcmlhLWV4cGFuZGVkXSc6ICdkcm9wZG93bi5pc09wZW4oKScsXG4gICAgJyhjbGljayknOiAnZHJvcGRvd24udG9nZ2xlKCknLFxuICAgICcoa2V5ZG93bi5BcnJvd1VwKSc6ICdkcm9wZG93bi5vbktleURvd24oJGV2ZW50KScsXG4gICAgJyhrZXlkb3duLkFycm93RG93biknOiAnZHJvcGRvd24ub25LZXlEb3duKCRldmVudCknLFxuICAgICcoa2V5ZG93bi5Ib21lKSc6ICdkcm9wZG93bi5vbktleURvd24oJGV2ZW50KScsXG4gICAgJyhrZXlkb3duLkVuZCknOiAnZHJvcGRvd24ub25LZXlEb3duKCRldmVudCknXG4gIH0sXG4gIHByb3ZpZGVyczogW3twcm92aWRlOiBOZ2JEcm9wZG93bkFuY2hvciwgdXNlRXhpc3Rpbmc6IGZvcndhcmRSZWYoKCkgPT4gTmdiRHJvcGRvd25Ub2dnbGUpfV1cbn0pXG5leHBvcnQgY2xhc3MgTmdiRHJvcGRvd25Ub2dnbGUgZXh0ZW5kcyBOZ2JEcm9wZG93bkFuY2hvciB7XG4gIGNvbnN0cnVjdG9yKEBJbmplY3QoZm9yd2FyZFJlZigoKSA9PiBOZ2JEcm9wZG93bikpIGRyb3Bkb3duLCBlbGVtZW50UmVmOiBFbGVtZW50UmVmPEhUTUxFbGVtZW50Pikge1xuICAgIHN1cGVyKGRyb3Bkb3duLCBlbGVtZW50UmVmKTtcbiAgfVxufVxuXG4vKipcbiAqIEEgZGlyZWN0aXZlIHRoYXQgcHJvdmlkZXMgY29udGV4dHVhbCBvdmVybGF5cyBmb3IgZGlzcGxheWluZyBsaXN0cyBvZiBsaW5rcyBhbmQgbW9yZS5cbiAqL1xuQERpcmVjdGl2ZSh7c2VsZWN0b3I6ICdbbmdiRHJvcGRvd25dJywgZXhwb3J0QXM6ICduZ2JEcm9wZG93bicsIGhvc3Q6IHsnW2NsYXNzLnNob3ddJzogJ2lzT3BlbigpJ319KVxuZXhwb3J0IGNsYXNzIE5nYkRyb3Bkb3duIGltcGxlbWVudHMgQWZ0ZXJDb250ZW50SW5pdCwgT25EZXN0cm95IHtcbiAgc3RhdGljIG5nQWNjZXB0SW5wdXRUeXBlX2F1dG9DbG9zZTogYm9vbGVhbiB8IHN0cmluZztcbiAgc3RhdGljIG5nQWNjZXB0SW5wdXRUeXBlX2Rpc3BsYXk6IHN0cmluZztcblxuICBwcml2YXRlIF9jbG9zZWQkID0gbmV3IFN1YmplY3Q8dm9pZD4oKTtcbiAgcHJpdmF0ZSBfem9uZVN1YnNjcmlwdGlvbjogU3Vic2NyaXB0aW9uO1xuICBwcml2YXRlIF9ib2R5Q29udGFpbmVyOiBIVE1MRWxlbWVudCB8IG51bGwgPSBudWxsO1xuXG4gIEBDb250ZW50Q2hpbGQoTmdiRHJvcGRvd25NZW51LCB7c3RhdGljOiBmYWxzZX0pIHByaXZhdGUgX21lbnU6IE5nYkRyb3Bkb3duTWVudTtcbiAgQENvbnRlbnRDaGlsZChOZ2JEcm9wZG93bk1lbnUsIHtyZWFkOiBFbGVtZW50UmVmLCBzdGF0aWM6IGZhbHNlfSkgcHJpdmF0ZSBfbWVudUVsZW1lbnQ6IEVsZW1lbnRSZWY7XG4gIEBDb250ZW50Q2hpbGQoTmdiRHJvcGRvd25BbmNob3IsIHtzdGF0aWM6IGZhbHNlfSkgcHJpdmF0ZSBfYW5jaG9yOiBOZ2JEcm9wZG93bkFuY2hvcjtcblxuICAvKipcbiAgICogSW5kaWNhdGVzIHdoZXRoZXIgdGhlIGRyb3Bkb3duIHNob3VsZCBiZSBjbG9zZWQgd2hlbiBjbGlja2luZyBvbmUgb2YgZHJvcGRvd24gaXRlbXMgb3IgcHJlc3NpbmcgRVNDLlxuICAgKlxuICAgKiAqIGB0cnVlYCAtIHRoZSBkcm9wZG93biB3aWxsIGNsb3NlIG9uIGJvdGggb3V0c2lkZSBhbmQgaW5zaWRlIChtZW51KSBjbGlja3MuXG4gICAqICogYGZhbHNlYCAtIHRoZSBkcm9wZG93biBjYW4gb25seSBiZSBjbG9zZWQgbWFudWFsbHkgdmlhIGBjbG9zZSgpYCBvciBgdG9nZ2xlKClgIG1ldGhvZHMuXG4gICAqICogYFwiaW5zaWRlXCJgIC0gdGhlIGRyb3Bkb3duIHdpbGwgY2xvc2Ugb24gaW5zaWRlIG1lbnUgY2xpY2tzLCBidXQgbm90IG91dHNpZGUgY2xpY2tzLlxuICAgKiAqIGBcIm91dHNpZGVcImAgLSB0aGUgZHJvcGRvd24gd2lsbCBjbG9zZSBvbmx5IG9uIHRoZSBvdXRzaWRlIGNsaWNrcyBhbmQgbm90IG9uIG1lbnUgY2xpY2tzLlxuICAgKi9cbiAgQElucHV0KCkgYXV0b0Nsb3NlOiBib29sZWFuIHwgJ291dHNpZGUnIHwgJ2luc2lkZSc7XG5cbiAgLyoqXG4gICAqIERlZmluZXMgd2hldGhlciBvciBub3QgdGhlIGRyb3Bkb3duIG1lbnUgaXMgb3BlbmVkIGluaXRpYWxseS5cbiAgICovXG4gIEBJbnB1dCgnb3BlbicpIF9vcGVuID0gZmFsc2U7XG5cbiAgLyoqXG4gICAqIFRoZSBwcmVmZXJyZWQgcGxhY2VtZW50IG9mIHRoZSBkcm9wZG93bi5cbiAgICpcbiAgICogUG9zc2libGUgdmFsdWVzIGFyZSBgXCJ0b3BcImAsIGBcInRvcC1sZWZ0XCJgLCBgXCJ0b3AtcmlnaHRcImAsIGBcImJvdHRvbVwiYCwgYFwiYm90dG9tLWxlZnRcImAsXG4gICAqIGBcImJvdHRvbS1yaWdodFwiYCwgYFwibGVmdFwiYCwgYFwibGVmdC10b3BcImAsIGBcImxlZnQtYm90dG9tXCJgLCBgXCJyaWdodFwiYCwgYFwicmlnaHQtdG9wXCJgLFxuICAgKiBgXCJyaWdodC1ib3R0b21cImBcbiAgICpcbiAgICogQWNjZXB0cyBhbiBhcnJheSBvZiBzdHJpbmdzIG9yIGEgc3RyaW5nIHdpdGggc3BhY2Ugc2VwYXJhdGVkIHBvc3NpYmxlIHZhbHVlcy5cbiAgICpcbiAgICogVGhlIGRlZmF1bHQgb3JkZXIgb2YgcHJlZmVyZW5jZSBpcyBgXCJib3R0b20tbGVmdCBib3R0b20tcmlnaHQgdG9wLWxlZnQgdG9wLXJpZ2h0XCJgXG4gICAqXG4gICAqIFBsZWFzZSBzZWUgdGhlIFtwb3NpdGlvbmluZyBvdmVydmlld10oIy9wb3NpdGlvbmluZykgZm9yIG1vcmUgZGV0YWlscy5cbiAgICovXG4gIEBJbnB1dCgpIHBsYWNlbWVudDogUGxhY2VtZW50QXJyYXk7XG5cbiAgLyoqXG4gICogQSBzZWxlY3RvciBzcGVjaWZ5aW5nIHRoZSBlbGVtZW50IHRoZSBkcm9wZG93biBzaG91bGQgYmUgYXBwZW5kZWQgdG8uXG4gICogQ3VycmVudGx5IG9ubHkgc3VwcG9ydHMgXCJib2R5XCIuXG4gICpcbiAgKiBAc2luY2UgNC4xLjBcbiAgKi9cbiAgQElucHV0KCkgY29udGFpbmVyOiBudWxsIHwgJ2JvZHknO1xuXG4gIC8qKlxuICAgKiBFbmFibGUgb3IgZGlzYWJsZSB0aGUgZHluYW1pYyBwb3NpdGlvbmluZy4gVGhlIGRlZmF1bHQgdmFsdWUgaXMgZHluYW1pYyB1bmxlc3MgdGhlIGRyb3Bkb3duIGlzIHVzZWRcbiAgICogaW5zaWRlIGEgQm9vdHN0cmFwIG5hdmJhci4gSWYgeW91IG5lZWQgY3VzdG9tIHBsYWNlbWVudCBmb3IgYSBkcm9wZG93biBpbiBhIG5hdmJhciwgc2V0IGl0IHRvXG4gICAqIGR5bmFtaWMgZXhwbGljaXRseS4gU2VlIHRoZSBbcG9zaXRpb25pbmcgb2YgZHJvcGRvd25dKCMvcG9zaXRpb25pbmcjZHJvcGRvd24pXG4gICAqIGFuZCB0aGUgW25hdmJhciBkZW1vXSgvIy9jb21wb25lbnRzL2Ryb3Bkb3duL2V4YW1wbGVzI25hdmJhcikgZm9yIG1vcmUgZGV0YWlscy5cbiAgICpcbiAgICogQHNpbmNlIDQuMi4wXG4gICAqL1xuICBASW5wdXQoKSBkaXNwbGF5OiAnZHluYW1pYycgfCAnc3RhdGljJztcblxuICAvKipcbiAgICogQW4gZXZlbnQgZmlyZWQgd2hlbiB0aGUgZHJvcGRvd24gaXMgb3BlbmVkIG9yIGNsb3NlZC5cbiAgICpcbiAgICogVGhlIGV2ZW50IHBheWxvYWQgaXMgYSBgYm9vbGVhbmA6XG4gICAqICogYHRydWVgIC0gdGhlIGRyb3Bkb3duIHdhcyBvcGVuZWRcbiAgICogKiBgZmFsc2VgIC0gdGhlIGRyb3Bkb3duIHdhcyBjbG9zZWRcbiAgICovXG4gIEBPdXRwdXQoKSBvcGVuQ2hhbmdlID0gbmV3IEV2ZW50RW1pdHRlcjxib29sZWFuPigpO1xuXG4gIGNvbnN0cnVjdG9yKFxuICAgICAgcHJpdmF0ZSBfY2hhbmdlRGV0ZWN0b3I6IENoYW5nZURldGVjdG9yUmVmLCBjb25maWc6IE5nYkRyb3Bkb3duQ29uZmlnLCBASW5qZWN0KERPQ1VNRU5UKSBwcml2YXRlIF9kb2N1bWVudDogYW55LFxuICAgICAgcHJpdmF0ZSBfbmdab25lOiBOZ1pvbmUsIHByaXZhdGUgX2VsZW1lbnRSZWY6IEVsZW1lbnRSZWY8SFRNTEVsZW1lbnQ+LCBwcml2YXRlIF9yZW5kZXJlcjogUmVuZGVyZXIyLFxuICAgICAgQE9wdGlvbmFsKCkgbmdiTmF2YmFyOiBOZ2JOYXZiYXIpIHtcbiAgICB0aGlzLnBsYWNlbWVudCA9IGNvbmZpZy5wbGFjZW1lbnQ7XG4gICAgdGhpcy5jb250YWluZXIgPSBjb25maWcuY29udGFpbmVyO1xuICAgIHRoaXMuYXV0b0Nsb3NlID0gY29uZmlnLmF1dG9DbG9zZTtcblxuICAgIHRoaXMuZGlzcGxheSA9IG5nYk5hdmJhciA/ICdzdGF0aWMnIDogJ2R5bmFtaWMnO1xuXG4gICAgdGhpcy5fem9uZVN1YnNjcmlwdGlvbiA9IF9uZ1pvbmUub25TdGFibGUuc3Vic2NyaWJlKCgpID0+IHsgdGhpcy5fcG9zaXRpb25NZW51KCk7IH0pO1xuICB9XG5cbiAgbmdBZnRlckNvbnRlbnRJbml0KCkge1xuICAgIHRoaXMuX25nWm9uZS5vblN0YWJsZS5waXBlKHRha2UoMSkpLnN1YnNjcmliZSgoKSA9PiB7XG4gICAgICB0aGlzLl9hcHBseVBsYWNlbWVudENsYXNzZXMoKTtcbiAgICAgIGlmICh0aGlzLl9vcGVuKSB7XG4gICAgICAgIHRoaXMuX3NldENsb3NlSGFuZGxlcnMoKTtcbiAgICAgIH1cbiAgICB9KTtcbiAgfVxuXG4gIG5nT25DaGFuZ2VzKGNoYW5nZXM6IFNpbXBsZUNoYW5nZXMpIHtcbiAgICBpZiAoY2hhbmdlcy5jb250YWluZXIgJiYgdGhpcy5fb3Blbikge1xuICAgICAgdGhpcy5fYXBwbHlDb250YWluZXIodGhpcy5jb250YWluZXIpO1xuICAgIH1cblxuICAgIGlmIChjaGFuZ2VzLnBsYWNlbWVudCAmJiAhY2hhbmdlcy5wbGFjZW1lbnQuaXNGaXJzdENoYW5nZSkge1xuICAgICAgdGhpcy5fYXBwbHlQbGFjZW1lbnRDbGFzc2VzKCk7XG4gICAgfVxuICB9XG5cbiAgLyoqXG4gICAqIENoZWNrcyBpZiB0aGUgZHJvcGRvd24gbWVudSBpcyBvcGVuLlxuICAgKi9cbiAgaXNPcGVuKCk6IGJvb2xlYW4geyByZXR1cm4gdGhpcy5fb3BlbjsgfVxuXG4gIC8qKlxuICAgKiBPcGVucyB0aGUgZHJvcGRvd24gbWVudS5cbiAgICovXG4gIG9wZW4oKTogdm9pZCB7XG4gICAgaWYgKCF0aGlzLl9vcGVuKSB7XG4gICAgICB0aGlzLl9vcGVuID0gdHJ1ZTtcbiAgICAgIHRoaXMuX2FwcGx5Q29udGFpbmVyKHRoaXMuY29udGFpbmVyKTtcbiAgICAgIHRoaXMub3BlbkNoYW5nZS5lbWl0KHRydWUpO1xuICAgICAgdGhpcy5fc2V0Q2xvc2VIYW5kbGVycygpO1xuICAgIH1cbiAgfVxuXG4gIHByaXZhdGUgX3NldENsb3NlSGFuZGxlcnMoKSB7XG4gICAgY29uc3QgYW5jaG9yID0gdGhpcy5fYW5jaG9yO1xuICAgIG5nYkF1dG9DbG9zZShcbiAgICAgICAgdGhpcy5fbmdab25lLCB0aGlzLl9kb2N1bWVudCwgdGhpcy5hdXRvQ2xvc2UsICgpID0+IHRoaXMuY2xvc2UoKSwgdGhpcy5fY2xvc2VkJCxcbiAgICAgICAgdGhpcy5fbWVudSA/IFt0aGlzLl9tZW51RWxlbWVudC5uYXRpdmVFbGVtZW50XSA6IFtdLCBhbmNob3IgPyBbYW5jaG9yLmdldE5hdGl2ZUVsZW1lbnQoKV0gOiBbXSxcbiAgICAgICAgJy5kcm9wZG93bi1pdGVtLC5kcm9wZG93bi1kaXZpZGVyJyk7XG4gIH1cblxuICAvKipcbiAgICogQ2xvc2VzIHRoZSBkcm9wZG93biBtZW51LlxuICAgKi9cbiAgY2xvc2UoKTogdm9pZCB7XG4gICAgaWYgKHRoaXMuX29wZW4pIHtcbiAgICAgIHRoaXMuX29wZW4gPSBmYWxzZTtcbiAgICAgIHRoaXMuX3Jlc2V0Q29udGFpbmVyKCk7XG4gICAgICB0aGlzLl9jbG9zZWQkLm5leHQoKTtcbiAgICAgIHRoaXMub3BlbkNoYW5nZS5lbWl0KGZhbHNlKTtcbiAgICAgIHRoaXMuX2NoYW5nZURldGVjdG9yLm1hcmtGb3JDaGVjaygpO1xuICAgIH1cbiAgfVxuXG4gIC8qKlxuICAgKiBUb2dnbGVzIHRoZSBkcm9wZG93biBtZW51LlxuICAgKi9cbiAgdG9nZ2xlKCk6IHZvaWQge1xuICAgIGlmICh0aGlzLmlzT3BlbigpKSB7XG4gICAgICB0aGlzLmNsb3NlKCk7XG4gICAgfSBlbHNlIHtcbiAgICAgIHRoaXMub3BlbigpO1xuICAgIH1cbiAgfVxuXG4gIG5nT25EZXN0cm95KCkge1xuICAgIHRoaXMuX3Jlc2V0Q29udGFpbmVyKCk7XG5cbiAgICB0aGlzLl9jbG9zZWQkLm5leHQoKTtcbiAgICB0aGlzLl96b25lU3Vic2NyaXB0aW9uLnVuc3Vic2NyaWJlKCk7XG4gIH1cblxuICBvbktleURvd24oZXZlbnQ6IEtleWJvYXJkRXZlbnQpIHtcbiAgICAvLyB0c2xpbnQ6ZGlzYWJsZS1uZXh0LWxpbmU6ZGVwcmVjYXRpb25cbiAgICBjb25zdCBrZXkgPSBldmVudC53aGljaDtcbiAgICBjb25zdCBpdGVtRWxlbWVudHMgPSB0aGlzLl9nZXRNZW51RWxlbWVudHMoKTtcblxuICAgIGxldCBwb3NpdGlvbiA9IC0xO1xuICAgIGxldCBpdGVtRWxlbWVudDogSFRNTEVsZW1lbnQgfCBudWxsID0gbnVsbDtcbiAgICBjb25zdCBpc0V2ZW50RnJvbVRvZ2dsZSA9IHRoaXMuX2lzRXZlbnRGcm9tVG9nZ2xlKGV2ZW50KTtcblxuICAgIGlmICghaXNFdmVudEZyb21Ub2dnbGUgJiYgaXRlbUVsZW1lbnRzLmxlbmd0aCkge1xuICAgICAgaXRlbUVsZW1lbnRzLmZvckVhY2goKGl0ZW0sIGluZGV4KSA9PiB7XG4gICAgICAgIGlmIChpdGVtLmNvbnRhaW5zKGV2ZW50LnRhcmdldCBhcyBIVE1MRWxlbWVudCkpIHtcbiAgICAgICAgICBpdGVtRWxlbWVudCA9IGl0ZW07XG4gICAgICAgIH1cbiAgICAgICAgaWYgKGl0ZW0gPT09IHRoaXMuX2RvY3VtZW50LmFjdGl2ZUVsZW1lbnQpIHtcbiAgICAgICAgICBwb3NpdGlvbiA9IGluZGV4O1xuICAgICAgICB9XG4gICAgICB9KTtcbiAgICB9XG5cbiAgICAvLyBjbG9zaW5nIG9uIEVudGVyIC8gU3BhY2VcbiAgICBpZiAoa2V5ID09PSBLZXkuU3BhY2UgfHwga2V5ID09PSBLZXkuRW50ZXIpIHtcbiAgICAgIGlmIChpdGVtRWxlbWVudCAmJiAodGhpcy5hdXRvQ2xvc2UgPT09IHRydWUgfHwgdGhpcy5hdXRvQ2xvc2UgPT09ICdpbnNpZGUnKSkge1xuICAgICAgICAvLyBJdGVtIGlzIGVpdGhlciBhIGJ1dHRvbiBvciBhIGxpbmssIHNvIGNsaWNrIHdpbGwgYmUgdHJpZ2dlcmVkIGJ5IHRoZSBicm93c2VyIG9uIEVudGVyIG9yIFNwYWNlLlxuICAgICAgICAvLyBTbyB3ZSBoYXZlIHRvIHJlZ2lzdGVyIGEgb25lLXRpbWUgY2xpY2sgaGFuZGxlciB0aGF0IHdpbGwgZmlyZSBhZnRlciBhbnkgdXNlciBkZWZpbmVkIGNsaWNrIGhhbmRsZXJzXG4gICAgICAgIC8vIHRvIGNsb3NlIHRoZSBkcm9wZG93blxuICAgICAgICBmcm9tRXZlbnQoaXRlbUVsZW1lbnQsICdjbGljaycpLnBpcGUodGFrZSgxKSkuc3Vic2NyaWJlKCgpID0+IHRoaXMuY2xvc2UoKSk7XG4gICAgICB9XG4gICAgICByZXR1cm47XG4gICAgfVxuXG4gICAgLy8gb3BlbmluZyAvIG5hdmlnYXRpbmdcbiAgICBpZiAoaXNFdmVudEZyb21Ub2dnbGUgfHwgaXRlbUVsZW1lbnQpIHtcbiAgICAgIHRoaXMub3BlbigpO1xuXG4gICAgICBpZiAoaXRlbUVsZW1lbnRzLmxlbmd0aCkge1xuICAgICAgICBzd2l0Y2ggKGtleSkge1xuICAgICAgICAgIGNhc2UgS2V5LkFycm93RG93bjpcbiAgICAgICAgICAgIHBvc2l0aW9uID0gTWF0aC5taW4ocG9zaXRpb24gKyAxLCBpdGVtRWxlbWVudHMubGVuZ3RoIC0gMSk7XG4gICAgICAgICAgICBicmVhaztcbiAgICAgICAgICBjYXNlIEtleS5BcnJvd1VwOlxuICAgICAgICAgICAgaWYgKHRoaXMuX2lzRHJvcHVwKCkgJiYgcG9zaXRpb24gPT09IC0xKSB7XG4gICAgICAgICAgICAgIHBvc2l0aW9uID0gaXRlbUVsZW1lbnRzLmxlbmd0aCAtIDE7XG4gICAgICAgICAgICAgIGJyZWFrO1xuICAgICAgICAgICAgfVxuICAgICAgICAgICAgcG9zaXRpb24gPSBNYXRoLm1heChwb3NpdGlvbiAtIDEsIDApO1xuICAgICAgICAgICAgYnJlYWs7XG4gICAgICAgICAgY2FzZSBLZXkuSG9tZTpcbiAgICAgICAgICAgIHBvc2l0aW9uID0gMDtcbiAgICAgICAgICAgIGJyZWFrO1xuICAgICAgICAgIGNhc2UgS2V5LkVuZDpcbiAgICAgICAgICAgIHBvc2l0aW9uID0gaXRlbUVsZW1lbnRzLmxlbmd0aCAtIDE7XG4gICAgICAgICAgICBicmVhaztcbiAgICAgICAgfVxuICAgICAgICBpdGVtRWxlbWVudHNbcG9zaXRpb25dLmZvY3VzKCk7XG4gICAgICB9XG4gICAgICBldmVudC5wcmV2ZW50RGVmYXVsdCgpO1xuICAgIH1cbiAgfVxuXG4gIHByaXZhdGUgX2lzRHJvcHVwKCk6IGJvb2xlYW4geyByZXR1cm4gdGhpcy5fZWxlbWVudFJlZi5uYXRpdmVFbGVtZW50LmNsYXNzTGlzdC5jb250YWlucygnZHJvcHVwJyk7IH1cblxuICBwcml2YXRlIF9pc0V2ZW50RnJvbVRvZ2dsZShldmVudDogS2V5Ym9hcmRFdmVudCkge1xuICAgIHJldHVybiB0aGlzLl9hbmNob3IuZ2V0TmF0aXZlRWxlbWVudCgpLmNvbnRhaW5zKGV2ZW50LnRhcmdldCBhcyBIVE1MRWxlbWVudCk7XG4gIH1cblxuICBwcml2YXRlIF9nZXRNZW51RWxlbWVudHMoKTogSFRNTEVsZW1lbnRbXSB7XG4gICAgY29uc3QgbWVudSA9IHRoaXMuX21lbnU7XG4gICAgaWYgKG1lbnUgPT0gbnVsbCkge1xuICAgICAgcmV0dXJuIFtdO1xuICAgIH1cbiAgICByZXR1cm4gbWVudS5tZW51SXRlbXMuZmlsdGVyKGl0ZW0gPT4gIWl0ZW0uZGlzYWJsZWQpLm1hcChpdGVtID0+IGl0ZW0uZWxlbWVudFJlZi5uYXRpdmVFbGVtZW50KTtcbiAgfVxuXG4gIHByaXZhdGUgX3Bvc2l0aW9uTWVudSgpIHtcbiAgICBjb25zdCBtZW51ID0gdGhpcy5fbWVudTtcbiAgICBpZiAodGhpcy5pc09wZW4oKSAmJiBtZW51KSB7XG4gICAgICB0aGlzLl9hcHBseVBsYWNlbWVudENsYXNzZXMoXG4gICAgICAgICAgdGhpcy5kaXNwbGF5ID09PSAnZHluYW1pYycgP1xuICAgICAgICAgICAgICBwb3NpdGlvbkVsZW1lbnRzKFxuICAgICAgICAgICAgICAgICAgdGhpcy5fYW5jaG9yLmFuY2hvckVsLCB0aGlzLl9ib2R5Q29udGFpbmVyIHx8IHRoaXMuX21lbnVFbGVtZW50Lm5hdGl2ZUVsZW1lbnQsIHRoaXMucGxhY2VtZW50LFxuICAgICAgICAgICAgICAgICAgdGhpcy5jb250YWluZXIgPT09ICdib2R5JykgOlxuICAgICAgICAgICAgICB0aGlzLl9nZXRGaXJzdFBsYWNlbWVudCh0aGlzLnBsYWNlbWVudCkpO1xuICAgIH1cbiAgfVxuXG4gIHByaXZhdGUgX2dldEZpcnN0UGxhY2VtZW50KHBsYWNlbWVudDogUGxhY2VtZW50QXJyYXkpOiBQbGFjZW1lbnQge1xuICAgIHJldHVybiBBcnJheS5pc0FycmF5KHBsYWNlbWVudCkgPyBwbGFjZW1lbnRbMF0gOiBwbGFjZW1lbnQuc3BsaXQoJyAnKVswXSBhcyBQbGFjZW1lbnQ7XG4gIH1cblxuICBwcml2YXRlIF9yZXNldENvbnRhaW5lcigpIHtcbiAgICBjb25zdCByZW5kZXJlciA9IHRoaXMuX3JlbmRlcmVyO1xuICAgIGNvbnN0IG1lbnVFbGVtZW50ID0gdGhpcy5fbWVudUVsZW1lbnQ7XG4gICAgaWYgKG1lbnVFbGVtZW50KSB7XG4gICAgICBjb25zdCBkcm9wZG93bkVsZW1lbnQgPSB0aGlzLl9lbGVtZW50UmVmLm5hdGl2ZUVsZW1lbnQ7XG4gICAgICBjb25zdCBkcm9wZG93bk1lbnVFbGVtZW50ID0gbWVudUVsZW1lbnQubmF0aXZlRWxlbWVudDtcblxuICAgICAgcmVuZGVyZXIuYXBwZW5kQ2hpbGQoZHJvcGRvd25FbGVtZW50LCBkcm9wZG93bk1lbnVFbGVtZW50KTtcbiAgICAgIHJlbmRlcmVyLnJlbW92ZVN0eWxlKGRyb3Bkb3duTWVudUVsZW1lbnQsICdwb3NpdGlvbicpO1xuICAgICAgcmVuZGVyZXIucmVtb3ZlU3R5bGUoZHJvcGRvd25NZW51RWxlbWVudCwgJ3RyYW5zZm9ybScpO1xuICAgIH1cbiAgICBpZiAodGhpcy5fYm9keUNvbnRhaW5lcikge1xuICAgICAgcmVuZGVyZXIucmVtb3ZlQ2hpbGQodGhpcy5fZG9jdW1lbnQuYm9keSwgdGhpcy5fYm9keUNvbnRhaW5lcik7XG4gICAgICB0aGlzLl9ib2R5Q29udGFpbmVyID0gbnVsbDtcbiAgICB9XG4gIH1cblxuICBwcml2YXRlIF9hcHBseUNvbnRhaW5lcihjb250YWluZXI6IG51bGwgfCAnYm9keScgPSBudWxsKSB7XG4gICAgdGhpcy5fcmVzZXRDb250YWluZXIoKTtcbiAgICBpZiAoY29udGFpbmVyID09PSAnYm9keScpIHtcbiAgICAgIGNvbnN0IHJlbmRlcmVyID0gdGhpcy5fcmVuZGVyZXI7XG4gICAgICBjb25zdCBkcm9wZG93bk1lbnVFbGVtZW50ID0gdGhpcy5fbWVudUVsZW1lbnQubmF0aXZlRWxlbWVudDtcbiAgICAgIGNvbnN0IGJvZHlDb250YWluZXIgPSB0aGlzLl9ib2R5Q29udGFpbmVyID0gdGhpcy5fYm9keUNvbnRhaW5lciB8fCByZW5kZXJlci5jcmVhdGVFbGVtZW50KCdkaXYnKTtcblxuICAgICAgLy8gT3ZlcnJpZGUgc29tZSBzdHlsZXMgdG8gaGF2ZSB0aGUgcG9zaXRpb25uaW5nIHdvcmtpbmdcbiAgICAgIHJlbmRlcmVyLnNldFN0eWxlKGJvZHlDb250YWluZXIsICdwb3NpdGlvbicsICdhYnNvbHV0ZScpO1xuICAgICAgcmVuZGVyZXIuc2V0U3R5bGUoZHJvcGRvd25NZW51RWxlbWVudCwgJ3Bvc2l0aW9uJywgJ3N0YXRpYycpO1xuICAgICAgcmVuZGVyZXIuc2V0U3R5bGUoYm9keUNvbnRhaW5lciwgJ3otaW5kZXgnLCAnMTA1MCcpO1xuXG4gICAgICByZW5kZXJlci5hcHBlbmRDaGlsZChib2R5Q29udGFpbmVyLCBkcm9wZG93bk1lbnVFbGVtZW50KTtcbiAgICAgIHJlbmRlcmVyLmFwcGVuZENoaWxkKHRoaXMuX2RvY3VtZW50LmJvZHksIGJvZHlDb250YWluZXIpO1xuICAgIH1cbiAgfVxuXG4gIHByaXZhdGUgX2FwcGx5UGxhY2VtZW50Q2xhc3NlcyhwbGFjZW1lbnQ/OiBQbGFjZW1lbnQgfCBudWxsKSB7XG4gICAgY29uc3QgbWVudSA9IHRoaXMuX21lbnU7XG4gICAgaWYgKG1lbnUpIHtcbiAgICAgIGlmICghcGxhY2VtZW50KSB7XG4gICAgICAgIHBsYWNlbWVudCA9IHRoaXMuX2dldEZpcnN0UGxhY2VtZW50KHRoaXMucGxhY2VtZW50KTtcbiAgICAgIH1cblxuICAgICAgY29uc3QgcmVuZGVyZXIgPSB0aGlzLl9yZW5kZXJlcjtcbiAgICAgIGNvbnN0IGRyb3Bkb3duRWxlbWVudCA9IHRoaXMuX2VsZW1lbnRSZWYubmF0aXZlRWxlbWVudDtcblxuICAgICAgLy8gcmVtb3ZlIHRoZSBjdXJyZW50IHBsYWNlbWVudCBjbGFzc2VzXG4gICAgICByZW5kZXJlci5yZW1vdmVDbGFzcyhkcm9wZG93bkVsZW1lbnQsICdkcm9wdXAnKTtcbiAgICAgIHJlbmRlcmVyLnJlbW92ZUNsYXNzKGRyb3Bkb3duRWxlbWVudCwgJ2Ryb3Bkb3duJyk7XG4gICAgICBtZW51LnBsYWNlbWVudCA9IHRoaXMuZGlzcGxheSA9PT0gJ3N0YXRpYycgPyBudWxsIDogcGxhY2VtZW50O1xuXG4gICAgICAvKlxuICAgICAgKiBhcHBseSB0aGUgbmV3IHBsYWNlbWVudFxuICAgICAgKiBpbiBjYXNlIG9mIHRvcCB1c2UgdXAtYXJyb3cgb3IgZG93bi1hcnJvdyBvdGhlcndpc2VcbiAgICAgICovXG4gICAgICBjb25zdCBkcm9wZG93bkNsYXNzID0gcGxhY2VtZW50LnNlYXJjaCgnXnRvcCcpICE9PSAtMSA/ICdkcm9wdXAnIDogJ2Ryb3Bkb3duJztcbiAgICAgIHJlbmRlcmVyLmFkZENsYXNzKGRyb3Bkb3duRWxlbWVudCwgZHJvcGRvd25DbGFzcyk7XG5cbiAgICAgIGNvbnN0IGJvZHlDb250YWluZXIgPSB0aGlzLl9ib2R5Q29udGFpbmVyO1xuICAgICAgaWYgKGJvZHlDb250YWluZXIpIHtcbiAgICAgICAgcmVuZGVyZXIucmVtb3ZlQ2xhc3MoYm9keUNvbnRhaW5lciwgJ2Ryb3B1cCcpO1xuICAgICAgICByZW5kZXJlci5yZW1vdmVDbGFzcyhib2R5Q29udGFpbmVyLCAnZHJvcGRvd24nKTtcbiAgICAgICAgcmVuZGVyZXIuYWRkQ2xhc3MoYm9keUNvbnRhaW5lciwgZHJvcGRvd25DbGFzcyk7XG4gICAgICB9XG4gICAgfVxuICB9XG59XG4iXX0=