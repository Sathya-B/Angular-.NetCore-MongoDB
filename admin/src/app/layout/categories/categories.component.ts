import { Component, OnInit } from '@angular/core';
import { routerTransition } from '../../router.animations';
import { ApiService } from '../../shared/services/api.service';

@Component({
    selector: 'app-categories',
    templateUrl: './categories.component.html',
    styleUrls: ['./categories.component.scss'],
    animations: [routerTransition()]
})
export class CategoriesComponent implements OnInit {
    public categories: any = [];
    public showAdd: boolean = false;
    constructor(private apiService: ApiService) {

    }
    ngOnInit() {
        this.apiService.get('Category').then(
            (response: any) => {
                this.categories = response.data;
            },
            (error: any) => {
                console.log(error);
            }
        );
    }
    cancelClicked(clicked: any) {
        this.showAdd = false;
    }
    addClicked() {
        this.showAdd = true;
    }
}
