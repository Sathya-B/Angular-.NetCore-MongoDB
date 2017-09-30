import { Component, Output, EventEmitter } from '@angular/core';
import { apiUrl } from '../../config/configuration';
import { ApiService } from '../../../services/api.service';
import { AddressService } from '../../../services/address.service';

@Component({
    selector: 'addressbook',
    templateUrl: './addressbook.component.html'
})
export class AddressBookComponent {
    @Output() public addAddress = new EventEmitter<boolean>();    
    public addresses: any;
    constructor(private apiService: ApiService, public addressService: AddressService) {
    
    }
    public addNewAddress() {
        this.addAddress.emit(true);
    }
}
