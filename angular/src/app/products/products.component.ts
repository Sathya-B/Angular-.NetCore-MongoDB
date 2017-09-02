import { Component, OnInit } from '@angular/core';
import { AppState } from '../app.service';
import { DataServ } from '../../services/data.service';
import {ApiService} from '../../services/api.service';
import { RouterModule } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { ParamMap } from '@angular/router';
import { ProductItemComponent } from './productitem/productitem.component';
import {Product} from '../../models/product.model';


@Component({
  selector: 'products',  
  /**
   * We need to tell Angular's Dependency Injection which providers are in our app.
   */
  providers: [
  ],
  /**
   * Our list of styles in our component. We may add more to compose many styles together.
   */
  styleUrls: ['./products.component.css'],
  /**
   * Every Angular template is first compiled by the browser before Angular runs it's compiler.
   */
  templateUrl: './products.component.html'

})
export class ProductsComponent implements OnInit{
  /**
   * Set our default values
   */
  public localState = { value: '' };

  public products: Product[] = [];
  /**
   * TypeScript public modifiers
   */
  public for: any;
  public type: any;
  constructor(public appState: AppState, private dataServ: DataServ, private apiService: ApiService, private route: ActivatedRoute) {
      this.for = route.snapshot.paramMap.get('productFor');
      this.type = route.snapshot.paramMap.get('productType');
  }

ngOnInit(){
 this.GetProducts();
}
GetProducts(){
    this.apiService.get('SubCategory/'+this.for + "s/" + this.type ).subscribe(
      (response: any) => {
        console.log(response);
        this.products = response.result;
        
      },
      (error: any) => {
        console.log(error);
      }
    )
}
}
