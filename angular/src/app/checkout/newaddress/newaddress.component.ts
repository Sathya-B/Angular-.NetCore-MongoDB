import { Component, Input, Output, EventEmitter } from '@angular/core';
import { NgForm } from '@angular/forms';
import { AddressService } from '../../../services/address.service';
import { ToastMsgService } from '../../../services/toastmsg.service';

@Component({
  selector: 'newaddress',
  templateUrl: './newaddress.component.html'
})
export class NewAddressComponent {

@Input() defaultAddress: boolean;
@Input() shippingAddress: boolean;
@Input() billingAddress: boolean;
@Output() addressAdded: EventEmitter<boolean> = new EventEmitter();
constructor(private addressService: AddressService, private toastmsg: ToastMsgService){

}

onSubmit(form: NgForm ){
    console.log(form.value);
    if(form.value.billingAddress == true) {
    this.addressService.addressItems.listOfAddress.forEach((add)=>{
     add.billingAddress = false;           
    }); 
    }
    this.addressService.addressItems.listOfAddress.forEach((add)=>{
     add.shippingAddress = false;
    });
    this.addressService.addressItems.listOfAddress.push(form.value);
    this.addressService.refreshAddressList();
    this.addressAdded.emit(true);   
}
}
