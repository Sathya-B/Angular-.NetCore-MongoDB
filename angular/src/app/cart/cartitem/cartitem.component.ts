import { Component, Input, OnInit } from '@angular/core';
import { CartService } from '@services/cart.service';
import { WishListService } from '@services/wishlist.service';
import { Router } from '@angular/router';

@Component({
  selector: 'cartitem',
  templateUrl: './cartitem.component.html',
  styleUrls: ['./cartitem.component.scss']
})
export class CartItemComponent implements OnInit {

@Input() public item: any;

@Input() public itemIndex: number;

constructor(public cartService: CartService, public wishListService: WishListService,
            private router: Router) {
}

public ngOnInit() {
}
public totalAmount() {
  return this.item.productQuantity * this.item.productPrice;
  }
public addOne() {
  if (this.item.productQuantity < 10) {
    this.item.productQuantity++;
    }
  }
public reduceOne() {
  if (this.item.productQuantity > 1) {
     this.item.productQuantity--;
    }
  }
public removeItem() {
  this.cartService.cartItems.listOfProducts.splice(this.itemIndex, 1);
}
public addToWishList() {
  this.wishListService.wishListItems.listOfProducts.push(this.item);
  this.removeItem();
  this.router.navigate(['/addedtowishlist']);
}
public viewItem(){
  let viewItemUrl = './products/' + this.item.productFor + '/' + this.item.productType + '/variants/' + this.item.productDesign;
  this.router.navigate([viewItemUrl]);
}
}
