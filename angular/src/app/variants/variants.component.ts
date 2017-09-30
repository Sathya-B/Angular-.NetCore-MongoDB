import { Component, OnInit, Output, ViewChild } from '@angular/core';
import { AppState } from '../app.service';
import { RouterModule, Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { ParamMap } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { WishListService } from '../../services/wishlist.service';
import { EventEmitter } from '@angular/core';
import { RelatedComponent } from './related/related.component';
import { ColorSizeStockComponent } from './colorsizestock/colorsizestock.component';
import { ToastMsgService } from '../../services/toastmsg.service';
import * as CartModel from '../../models/cart.model';
import * as WishListModel from '../../models/wishlist.model';

@Component({
  selector: 'variants',
  providers: [
  ],
  styleUrls: ['./variants.component.css'],
  templateUrl: './variants.component.html'

})
export class VariantsComponent implements OnInit {
  public localState = { value: '' };

  public variants: any;

  public design: string;

  public for: string;

  public type: string;

  public selectedColor: string;

  public selectedVariant: any;

  public initItem: any;

  public relatedItems: any;

  @ViewChild(ColorSizeStockComponent) public css: ColorSizeStockComponent;

  constructor(private cartService: CartService, private route: ActivatedRoute,
              private toastMsg: ToastMsgService, private wishListService: WishListService,
              private router: Router) {
    this.for = route.snapshot.paramMap.get('productFor');
    this.type = route.snapshot.paramMap.get('productType');
    this.design = route.snapshot.paramMap.get('productDesign');
  }

public ngOnInit() {
this.variants = JSON.parse(localStorage.getItem(this.for + '-' + this.type + '-' + this.design));
this.initItem = this.variants;
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
  this.router.navigate(['/addedtocart']);
  if (localStorage.getItem('UserName') !== undefined) {
    this.cartService.refreshCart();
  }
}

public addToWishList() {
  let wishListItem: WishListModel.WishListItem;
  wishListItem = this.css.itemToCart;
  if( wishListItem !== undefined) {
  wishListItem.productQuantity = 1;
  this.wishListService.wishListItems.listOfProducts.push(wishListItem);  
  if (localStorage.getItem('UserName') !== undefined) {
    this.wishListService.refreshList();
  }
  this.router.navigate(['/addedtowishlist']);
  } else {
    this.toastMsg.popToast('info','Info','Please Select a Colour and Size.')
  }
}
public isBagDisabled() {
  if (!(Number(this.css.remainingQty) >= 1)) {
    return true;
  }
  else {
    return false;
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
