import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { apiUrl } from '../../config/configuration';
import { ApiService } from '../../../services/api.service';
import { AddressService } from '../../../services/address.service';

@Component({
    selector: 'addressbook',
    templateUrl: './addressbook.component.html'
})
export class AddressBookComponent implements OnInit {
    @Output() public addAddress = new EventEmitter<boolean>();    
    public addresses: any;
    constructor(private apiService: ApiService, private addressService: AddressService) {
    
    }

    public ngOnInit() {
    //this.addresses = this.addressService.addressItems.listOfAddress;
    }
    public addNewAddress() {
        this.addAddress.emit(true);
    }
}
