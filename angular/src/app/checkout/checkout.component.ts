import { Component, ViewChild } from '@angular/core';
import { AddressService } from '@services/address.service';
import { ToastMsgService } from '@services/toastmsg.service';
import { ApiService } from '@services/api.service';
import { OrderComponent } from '@app/cart/order/order.component';
import { CartService } from '@services/cart.service';
import * as Config from '@app/config/configuration';
import * as Util from '@shared/utils/utils.ts';
import { Router } from '@angular/router';

@Component({
  selector: 'checkout',
  templateUrl: './checkout.component.html'
})

export class CheckOutComponent {

  public newDeliveryAddress: boolean = true;
  public sameAsDeliveryAddress: boolean = true;

  constructor(private addressService: AddressService, private toastmsg: ToastMsgService,
    private apiService: ApiService, private cartService: CartService, private route: Router) {
    this.addressService.getAddresses();
  }

  public isAddressAvailable() {
    return true;
  }
  public check(type: boolean) {
    this.newDeliveryAddress = type;
  }

  public checkBilling(type: boolean) {
    this.sameAsDeliveryAddress = type;
  }

  public addNewAddressClicked(addNew: boolean) {
    this.newDeliveryAddress = addNew;
  }

  public addressAdded(event: boolean) {
    this.newDeliveryAddress = !event;
    this.toastmsg.popToast('success', 'Success', 'Address added.');
  }
  public confirmAndPay() {
    if (this.addressService.addressItems.listOfAddress.length == 0) {
    this.toastmsg.popToast('error', 'Error', 'Please add Delivery and Billing Address.');
    } else {

      let userName = localStorage.getItem('UserName');
      if (userName !== undefined) {
        let postOder = { totalAmount: 0, couponDiscount: 0, estimatedTax: 0 };
        postOder.totalAmount = this.cartService.cartItems.listOfProducts.reduce((acc, item) => {
          return acc + (Number(item.productPrice) * item.productQuantity);
        }, 0);
        postOder.estimatedTax = postOder.totalAmount * Config.tax.estimatedTax;
        postOder.totalAmount = postOder.totalAmount + postOder.estimatedTax;

        this.apiService.post('Order/placeorder/' + userName,
          postOder, { useAuth: true }, undefined).then(
          (response: any) => {
            if (response.code === '200') {
              let postData = Util.xwwwfurlenc(response.data);
              this.apiService.formpost('https://test.payu.in/_payment', response.data, 'post');
              return true;
            } else {
              throw response.error;
            }
          })
          .catch(
          (error: any) => {
            console.log(error);
            if (error.code === '403') {
              this.toastmsg.popToast('error', 'Error', 'Available stock is less than selected Quantity. Please reduce quantity and try again.');
              this.route.navigate(['/cart']);
            }
          }
          );
      }
    }
  }
}
