
import { NgModule } from '@angular/core';

import { MatRadioModule } from '@angular/material/radio';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@NgModule({
    imports: [
        MatRadioModule,
        MatFormFieldModule,
        MatInputModule
    ],
    exports: [
        MatRadioModule,
        MatFormFieldModule,
        MatInputModule
    ]
})

export class MaterialModule { }
