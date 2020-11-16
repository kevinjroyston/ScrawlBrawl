
import { NgModule } from '@angular/core';
import {MatDialogModule} from '@angular/material/dialog';
import {MatTooltipModule} from '@angular/material/tooltip';
import { MatRadioModule } from '@angular/material/radio';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';


@NgModule({
    imports: [
        MatRadioModule,
        MatDialogModule,
        MatTooltipModule,
        MatProgressSpinnerModule
    ],
    exports: [
        MatRadioModule,
        MatDialogModule,
        MatTooltipModule,
        MatProgressSpinnerModule
    ]
})

export class MaterialModule { }
