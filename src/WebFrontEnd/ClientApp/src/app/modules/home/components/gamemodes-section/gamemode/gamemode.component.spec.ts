/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { GamemodeComponent } from './gamemode.component';

describe('GamemodeComponent', () => {
  let component: GamemodeComponent;
  let fixture: ComponentFixture<GamemodeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GamemodeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GamemodeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
