import { Component, Input } from '@angular/core';

@Component({
    selector: 'scrawlbrawl-logo-horizontal',
    templateUrl: '../../../../../../src/assets/svgs/scrawlbrawl-horizontal.svg',
    styleUrls: ['../svg.component.scss']
})
export class ScrawlBrawlLogoHorizontal {
    @Input()strokeColor : string = '#7c7c7c'
}