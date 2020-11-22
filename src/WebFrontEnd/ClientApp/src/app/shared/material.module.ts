
import { NgModule } from '@angular/core';
import {MatDialogModule} from '@angular/material/dialog';
import {MatTooltipModule} from '@angular/material/tooltip';
import { MatRadioModule } from '@angular/material/radio';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {MatBottomSheetModule} from '@angular/material/bottom-sheet'

@NgModule({
    imports: [
        MatRadioModule,
        MatDialogModule,
        MatTooltipModule,
        MatProgressSpinnerModule,
        MatBottomSheetModule
    ],
    exports: [
        MatRadioModule,
        MatDialogModule,
        MatTooltipModule,
        MatProgressSpinnerModule,
        MatBottomSheetModule
    ]
})

export class MaterialModule { }
