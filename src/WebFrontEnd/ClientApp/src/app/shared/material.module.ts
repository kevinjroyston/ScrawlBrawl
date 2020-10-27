
import { NgModule } from '@angular/core';
import {MatDialogModule} from '@angular/material/dialog';
import {MatTooltipModule} from '@angular/material/tooltip';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';


@NgModule({
    imports: [
        MatDialogModule,
        MatTooltipModule,
        MatProgressSpinnerModule
    ],
    exports: [
        MatDialogModule,
        MatTooltipModule,
        MatProgressSpinnerModule
    ]
})

export class MaterialModule { }
