import { Component, OnInit } from '@angular/core';
import { AddressService } from '../../services/address.service';
import { ToastMsgService } from '../../services/toastmsg.service';

@Component({
  selector: 'checkout',
  templateUrl: './checkout.component.html'
})

export class CheckOutComponent implements OnInit{

public newDeliveryAddress: boolean = true;
public sameAsDeliveryAddress: boolean = true;

constructor(private addressService: AddressService, private toastmsg: ToastMsgService){
 this.addressService.getAddresses();
}

public ngOnInit(){
}
public isAddressAvailable(){
  return true;
}
public check(type: boolean){
this.newDeliveryAddress = type;
console.log(this.newDeliveryAddress);
}

public checkBilling(type: boolean){
this.sameAsDeliveryAddress = type;
console.log(this.sameAsDeliveryAddress);
}

public addNewAddressClicked(addNew: boolean) {
  this.newDeliveryAddress = addNew;
}

public addressAdded(event: boolean){
  this.newDeliveryAddress = !event;
  this.toastmsg.popToast('success','Success','Address added.')
}
}
