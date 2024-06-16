
import { NgModule } from '@angular/core';
import {MatDialogModule } from '@angular/material/dialog';
import {MatTooltipModule } from '@angular/material/tooltip';
import { MatRadioModule } from '@angular/material/radio';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {MatBottomSheetModule} from '@angular/material/bottom-sheet';
import {MatSnackBarModule} from '@angular/material/snack-bar';
import {MatTabsModule} from '@angular/material/tabs';

@NgModule({
    imports: [
        MatRadioModule,
        MatDialogModule,
        MatTooltipModule,
        MatProgressSpinnerModule,
        MatBottomSheetModule,
        MatSnackBarModule,
        MatTabsModule
    ],
    exports: [
        MatRadioModule,
        MatDialogModule,
        MatTooltipModule,
        MatProgressSpinnerModule,
        MatBottomSheetModule,
        MatSnackBarModule,
        MatTabsModule
    ]
})

export class MaterialModule { }
