import { Component, OnInit, Output, ViewChild } from '@angular/core';
import { AppState } from '../app.service';
import { RouterModule } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { ParamMap } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { EventEmitter } from '@angular/core';
import { RelatedComponent } from './related/related.component';
import { ColorSizeStockComponent } from './colorsizestock/colorsizestock.component';
import { ToastMsgService } from '../../services/toastmsg.service';
import * as CartModel from '../../models/cart.model';

@Component({
  selector: 'variants',
  /**
   * We need to tell Angular's Dependency Injection which providers are in our app.
   */
  providers: [
  ],
  /**
   * Our list of styles in our component. We may add more to compose many styles together.
   */
  styleUrls: ['./variants.component.css'],
  /**
   * Every Angular template is first compiled by the browser before Angular runs it's compiler.
   */
  templateUrl: './variants.component.html'

})
export class VariantsComponent implements OnInit {
  /**
   * Set our default values
   */
  public localState = { value: '' };

  public variants: any;

  public design: string;

  public for: string;

  public type: string;

  public selectedColor: string;

  public selectedVariant: string;

  public relatedItems: any;

  /**
   * TypeScript public modifiers
   */

  @ViewChild(ColorSizeStockComponent) public css: ColorSizeStockComponent;

  constructor(private cartService: CartService, private route: ActivatedRoute,
              private toastMsg: ToastMsgService) {
    this.for = route.snapshot.paramMap.get('productFor');
    this.type = route.snapshot.paramMap.get('productType');
    this.design = route.snapshot.paramMap.get('productDesign');
  }

public ngOnInit() {
this.variants = JSON.parse(localStorage.getItem(this.for + '-' + this.type + '-' + this.design));
console.log(this.variants);
this.relatedItems = findLocalItems(this.for);
}

public checked(event: any, svariant?: any) {
  this.selectedColor = event.target.id;
  this.selectedVariant = svariant;
}
public isChecked(color: string) {
  if (this.selectedColor === color) {
    return true;
  } else {
    return false;
  }
}

public variantItemClicked(variantItem: any) {
this.variants = variantItem;
this.selectedVariant = null;
this.selectedColor = null;
this.css.selectedSize = null;
}

public addToCart() {
  let cartItem: CartModel.CartItem;
  cartItem = this.css.itemToCart;
  cartItem.productQuantity = this.css.quantity;
  this.cartService.cartItems.listOfProducts.push(cartItem);
  this.toastMsg.popToast('success','Success','Item Added to Bag!')
  if (localStorage.getItem('UserName') !== undefined) {
    console.log('cart');
    console.log(cartItem);
    this.cartService.refreshCart();
  }
}
}
function findLocalItems (query) {
  let results = [];
  for (let i in localStorage) {
    if (localStorage.hasOwnProperty(i)) {
      if (i.match(query) || (!query && typeof i === 'string')) {
        results.push({key: i, val: JSON.parse(localStorage.getItem(i))});
      }
    }
  }
  return results;
}
