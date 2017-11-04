import { Component } from '@angular/core';
import { CartService } from '@services/cart.service';
import * as Config from '@app/config/configuration';

@Component({
  selector: 'order',
  templateUrl: './order.component.html'
})
export class OrderComponent {

public tax = Config.tax.estimatedTax;
public estimatedTax: number = 0;
constructor(private cartService: CartService) {
}
public getCartTotal() {
      return this.cartService.cartItems.listOfProducts.reduce((acc, item) => {
            return acc + (Number(item.productPrice) * item.productQuantity);
        }, 0);
}
public getEstimatedTax() {
  // console.log('boom');
  // return this.cartService.cartItems.listOfProducts.reduce((acc, item) => {
  //           if( acc == undefined) {
  //             acc = 0;
  //           }
  //           if(item.productType !== 'Gifts') {
  //            return acc + ((Number(item.productPrice) * item.productQuantity) * this.tax);
  //           }
  //       }, 0);
  return 0;
}
}
