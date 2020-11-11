import { NgModule } from '@angular/core';
import { HomeComponent } from './pages/home/home.component';
import { GamemodeListComponent} from './components/gamemodes-section/gamemode-list/gamemode-list.component'
import { GamemodeComponent} from './components/gamemodes-section/gamemode/gamemode.component'
import { HomeActionComponent} from './components/action-section/action/action.component'
import {ConnectWithUsComponent} from './components/connect-section/connect.component'
import {OpensourceSectionComponent} from './components/opensource-section/opensource-section.component'
import { SharedModule } from '@shared/shared.module';
import { HomeRoutingModule } from './home.routing';
import { TrailerSectionComponent } from './components/trailer-section/trailer-section.component';
import { ActionSectionComponent } from './components/action-section/action-section.component';
import { HeroSectionComponent } from './components/hero-section/hero-section.component';
import { QuestionsSectionComponent } from './components/questions-section/questions-section.component';

@NgModule({
  declarations: [
    HomeComponent,
    GamemodeComponent,
    GamemodeListComponent,
    HomeActionComponent,
    ConnectWithUsComponent,
    OpensourceSectionComponent,
    TrailerSectionComponent,
    ActionSectionComponent,
    HeroSectionComponent,
    QuestionsSectionComponent
  ],
  imports: [
    SharedModule,
    HomeRoutingModule
  ]
})
export class HomeModule { }
