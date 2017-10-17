import { Component, OnInit } from '@angular/core';
import { routerTransition } from '../../router.animations';
import { ApiService } from '../../shared/services/api.service';

@Component({
  selector: 'app-products',
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.scss'],
  animations: [routerTransition()]
})
export class ProductsComponent implements OnInit {
  
  public products: any = [];
  public showAdd: boolean = false;
  constructor(private apiService: ApiService) { }

  ngOnInit() {
  this.apiService.get('Product').then(
      (response: any) => {
      this.products = response.data;
      },
      (error: any) => {
      console.log(error);
      }
    );

  }
  addClicked(){
  this.showAdd = true;
  }
  cancelClicked(clicked: any) {
    if(clicked === true) {
    this.showAdd = false;
    }
  }
}
