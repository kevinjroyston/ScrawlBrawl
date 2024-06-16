
import { NgModule } from '@angular/core';
import {MatLegacyDialogModule as MatDialogModule} from '@angular/material/legacy-dialog';
import {MatLegacyTooltipModule as MatTooltipModule} from '@angular/material/legacy-tooltip';
import { MatLegacyRadioModule as MatRadioModule } from '@angular/material/legacy-radio';
import {MatLegacyProgressSpinnerModule as MatProgressSpinnerModule} from '@angular/material/legacy-progress-spinner';
import {MatBottomSheetModule} from '@angular/material/bottom-sheet';
import {MatLegacySnackBarModule as MatSnackBarModule} from '@angular/material/legacy-snack-bar';
import {MatLegacyTabsModule as MatTabsModule} from '@angular/material/legacy-tabs';

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
