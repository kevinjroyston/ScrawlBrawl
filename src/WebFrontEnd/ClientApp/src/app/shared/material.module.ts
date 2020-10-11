
import { NgModule } from '@angular/core';

import { MatRadioModule } from '@angular/material/radio';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import {MatCardModule} from '@angular/material/card';

@NgModule({
    imports: [
        MatRadioModule,
        MatFormFieldModule,
        MatInputModule,
        MatCardModule
    ],
    exports: [
        MatRadioModule,
        MatFormFieldModule,
        MatInputModule,
        MatCardModule
    ]
})

export class MaterialModule { }
