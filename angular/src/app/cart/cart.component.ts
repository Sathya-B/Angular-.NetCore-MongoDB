import { Component, OnInit, OnDestroy } from '@angular/core';
import { CartItem } from 'cartitem/cartitem.component';
import { CartService } from '../../services/cart.service';

@Component({
  selector: 'cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.scss']
})
export class CartComponent implements OnInit, OnDestroy {

public cartItems: any;

constructor(private cartService: CartService) {

}

public ngOnInit() {
 this.cartItems = this.cartService.cartItems.listOfProducts;

}
public getCartTotal() {
      return this.cartService.cartItems.listOfProducts.reduce((acc, item) => {
            return acc + (Number(item.productPrice) * item.productQuantity);
        }, 0);
}

public ngOnDestroy() {
    this.cartService.refreshCart();
}
}
