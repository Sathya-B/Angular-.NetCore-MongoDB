import { Component, Input } from '@angular/core';
import { CartService } from '../../../services/cart.service';

@Component({
  selector: 'cartitem',
  templateUrl: './cartitem.component.html',
  styleUrls: ['./cartitem.component.scss']
})
export class CartItemComponent {

@Input() public item: any;

@Input() public itemIndex: number;

constructor(private cartService: CartService) {

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
}
