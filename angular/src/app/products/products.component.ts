import { Component, OnInit } from '@angular/core';
import { AppState } from '../app.service';
import { CartService } from '../../services/cart.service';
import { ApiService } from '../../services/api.service';
import { RouterModule } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { ParamMap } from '@angular/router';
import { ProductItemComponent } from './productitem/productitem.component';

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
export class ProductsComponent implements OnInit {
  /**
   * Set our default values
   */
  public localState = { value: '' };

  public products: any[] = [];
  /**
   * TypeScript public modifiers
   */
  public for: any;
  public type: any;
  constructor(public appState: AppState, private cartServ: CartService,
              private apiService: ApiService, private route: ActivatedRoute) {
      this.for = route.snapshot.paramMap.get('productFor');
      this.type = route.snapshot.paramMap.get('productType');
  }

public ngOnInit() {
 this.GetProducts();
}
public GetProducts() {
    this.apiService.get('SubCategory/' + this.for + 's/' + this.type).then(
      (response: any) => {
      console.log(response);
      this.products = response.data;
      },
      (error: any) => {
      console.log(error);
      }
    );
}
}
