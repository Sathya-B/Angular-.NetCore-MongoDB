import { Component, OnInit, Input, OnChanges } from '@angular/core';
import { CartService } from '@services/cart.service';
import * as Config from '@app/config/configuration';

@Component({
  selector: 'order',
  templateUrl: './order.component.html'
})
export class OrderComponent implements OnInit, OnChanges {

@Input() couponDiscount: any = { value:0, percentage: false};
public couponDiscountAmount: any = 0;
public tax = Config.tax.estimatedTax;
public estimatedTax: number = 0;
public cartTotal: number = 0;
public finalAmount: number = 0;
constructor(private cartService: CartService) {
}
public getCartTotal() {
      return 
}

public ngOnInit() {
  this.updateCart();
  this.cartService.cartUpdated.subscribe(updated => {
    this.updateCart();
  })
}
updateCart(){
this.cartTotal = this.cartService.cartItems.listOfProducts.reduce((acc, item) => {
            return acc + (Number(item.productPrice) * item.productQuantity);
        }, 0);
this.estimatedTax = this.cartService.cartItems.listOfProducts.reduce((acc, item) => {
            if( acc == undefined) {
              acc = 0;
            }
            if(item.productType !== 'Gifts') {
             return acc + ((Number(item.productPrice) * item.productQuantity) * this.tax);
            } else {
              return acc;
            }
        }, 0);
this.finalAmount = this.cartTotal + this.estimatedTax;
if(this.couponDiscount.percentage) {
  this.finalAmount = this.finalAmount  - ((this.couponDiscount.value/100) * this.finalAmount) ;
} else {
  if(this.couponDiscount.value <= this.finalAmount) {
  this.finalAmount = this.finalAmount - this.couponDiscount.value;
  } else {
    this.finalAmount = 0;
    console.log('updatecoupon');
  }
}
}
public ngOnChanges() {
 this.updateCart();
}
}
