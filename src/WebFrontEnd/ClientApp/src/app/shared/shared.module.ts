import { NgModule,  CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
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
import { SimpleButtonComponent } from './components/simplebuttons/button/button.component'
import { RouterModule } from '@angular/router';
import { IconButtonComponent} from './components/iconbutton/iconbutton.component'
import { GameAssetDirective} from './components/gameassetdirective.component'
//SVGs Import/Export these as module later
import { GreenMoustacheBrushSVG } from './components/svgs/characters/green-moustache-brush';
import { RedPencilOnBluePencilSVG } from './components/svgs/characters/red-pencil-on-blue-pencil';
import { ComingSoonSVG } from './components/svgs/characters/coming-soon'
import { ScrawlBrawlLogoHorizontal } from './components/svgs/branding/scrawlbrawl-horizontal';
import { EraserSVG } from './components/svgs/icons/eraser';
import { ScrawlBrawlLogoVertical } from './components/svgs/branding/scrawlbrawl-vertical';
import { GreenBrushCharacter } from './components/svgs/characters/greenbrush';
import { FeatherCharacter } from './components/svgs/characters/feather';
import { RedPencilCharacter } from './components/svgs/characters/redpencil';
import { BluePencilCharacter } from './components/svgs/characters/bluepencil';
import { YellowCrayonCharacter } from './components/svgs/characters/yellowcrayon';
import { BlackPenCharacter } from './components/svgs/characters/blackpen';
import { TwitchIcon } from './components/svgs/socials/twitch';
import { TwitterIcon } from './components/svgs/socials/twitter';
import { YoutubeIcon } from './components/svgs/socials/youtube';
import { FacebookIcon } from './components/svgs/socials/facebook';
import { ColorPickerComponent } from './components/colorpicker/colorpicker.component';
import { SimpleLinkComponent } from './components/simplebuttons/link/link.component';
import {MatTabsModule} from '@angular/material/tabs';
import {GalleryTool} from '@shared/components/gallerytool/gallerytool.component';
import {GalleryPanel} from '@shared/components/gallerypanel/gallerypanel.component';

@NgModule({
  imports: [
    CommonModule,
    MaterialModule,
    NgxBootstrapSliderModule,
    NgbModule,
    ColorPickerModule,
    UiSwitchModule,
    FormsModule,
    MatTabsModule,
    ReactiveFormsModule,
    RouterModule
  ],
  declarations: [
    DrawingBoard,
    Slider,
    ColorPickerComponent,
    ScrawlButtonComponent,
    SimpleButtonComponent,
    SimpleLinkComponent,
    DrawingDirective,
    GalleryPanel,
    GalleryTool,
    SelectorDirective,
    GameAssetDirective,
    IconButtonComponent,
    GreenMoustacheBrushSVG,
    RedPencilOnBluePencilSVG,
    ScrawlBrawlLogoHorizontal,
    ScrawlBrawlLogoVertical,
    EraserSVG,
    ComingSoonSVG,
    GreenBrushCharacter,
    FeatherCharacter,
    RedPencilCharacter,
    BluePencilCharacter,
    YellowCrayonCharacter,
    BlackPenCharacter,
    TwitchIcon,
    TwitterIcon,
    YoutubeIcon,
    FacebookIcon
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
    SimpleLinkComponent,
    Slider,
    FormsModule,
    ReactiveFormsModule,
    DrawingDirective,
    GameAssetDirective,
    ScrawlButtonComponent,
    SelectorDirective,
    IconButtonComponent,
    GreenMoustacheBrushSVG,
    RedPencilOnBluePencilSVG,
    EraserSVG,
    ComingSoonSVG,
    ScrawlBrawlLogoHorizontal,
    ScrawlBrawlLogoVertical,
    GreenBrushCharacter,
    FeatherCharacter,
    RedPencilCharacter,
    BluePencilCharacter,
    YellowCrayonCharacter,
    BlackPenCharacter,
    TwitchIcon,
    TwitterIcon,
    YoutubeIcon,
    FacebookIcon
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class SharedModule { }
