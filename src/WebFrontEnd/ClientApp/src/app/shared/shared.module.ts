import { NgModule,  CUSTOM_ELEMENTS_SCHEMA} from '@angular/core';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { CommonModule } from '@angular/common';
import { MaterialModule } from './material.module';
import { DrawingBoard } from './components/drawingboard/drawingboard.component'
import { Slider } from './components/slider/slider.component';
import { NgxBootstrapSliderModule } from 'ngx-bootstrap-slider';
import { ColorPickerModule } from 'ngx-color-picker';
import { UiSwitchModule } from 'ngx-ui-switch';
import { DrawingDirective } from './components/drawingdirective.component';
import { ScrawlButtonComponent } from './components/scrawlbutton/scrawlbutton.component';
import { SelectorDirective } from './components/selectordirective.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SimpleButtonComponent } from './components/simplebutton/simplebutton.component'
import {FooterModule} from '@layout/footer/footer.module'
import { RouterModule } from '@angular/router';
import {IconButtonComponent} from './components/iconbutton/iconbutton.component'
import {GameAssetDirective} from './components/gameassetdirective.component'

@NgModule({
  imports: [
    CommonModule,
    MaterialModule,
    NgxBootstrapSliderModule,
    NgbModule,
    ColorPickerModule,
    UiSwitchModule,
    FooterModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule
  ],
  declarations: [
    DrawingBoard,
    Slider,
    ScrawlButtonComponent,
    SimpleButtonComponent,
    DrawingDirective,
    SelectorDirective,
    GameAssetDirective,
    IconButtonComponent
  ],
  exports: [
    CommonModule,
    MaterialModule,
    DrawingBoard,
    NgxBootstrapSliderModule,
    NgbModule,
    ColorPickerModule,
    UiSwitchModule,
    SimpleButtonComponent,
    FooterModule,
    Slider,
    FormsModule,
    ReactiveFormsModule,
    DrawingDirective,
    GameAssetDirective,
    ScrawlButtonComponent,
    SelectorDirective,
    FooterModule,
    IconButtonComponent
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class SharedModule { }
