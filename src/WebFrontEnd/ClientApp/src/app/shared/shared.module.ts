import { NgModule } from '@angular/core';
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
import {ButtonComponent} from './components/button/button.component'
import {FooterModule} from '@layout/footer/footer.module'

@NgModule({
  imports: [
    CommonModule,
    MaterialModule,
    NgxBootstrapSliderModule,
    NgbModule,
    ColorPickerModule,
    UiSwitchModule,
    FormsModule,
    ReactiveFormsModule,
    FooterModule
  ],
  declarations: [
    DrawingBoard,
    Slider,
    ScrawlButtonComponent,
    DrawingDirective,
    SelectorDirective
  ],
  exports: [
    CommonModule,
    MaterialModule,
    DrawingBoard,
    NgxBootstrapSliderModule,
    NgbModule,
    ColorPickerModule,
    UiSwitchModule,
    ButtonComponent,
    Slider,
    FormsModule,
    ReactiveFormsModule,
    DrawingDirective,
    ScrawlButtonComponent,
    SelectorDirective,
    FooterModule
  ]
})
export class SharedModule { }
