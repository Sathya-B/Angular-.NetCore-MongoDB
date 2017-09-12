import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { FormGroup, FormControl, FormArray, Validators } from '@angular/forms';
import {ApiService} from '../../../services/api.service';
import { ActivatedRoute, Params, Router, ParamMap } from '@angular/router';

@Component({
  selector: 'createaccount',
  templateUrl: './createaccount.component.html',
  styleUrls: ['./createaccount.component.scss']
})
export class CreateAccountComponent {

constructor(private apiService: ApiService, private route: ActivatedRoute, private router: Router){

}

onSubmit(form: NgForm){
       
    const userDetails = form.value;
    this.apiService.post('register',userDetails,undefined,"http://192.168.0.111:5001/api/auth/").subscribe(
      (response: any) => {
        if(response.message == "User Registered")
        {
         this.router.navigate(['../verify', userDetails.PhoneNumber,"createaccount"],{ relativeTo: this.route});
        }
      },
      (error: any) => {
        var errormsg = JSON.parse(error._body);
        if(errormsg.message == "User Already Registered")
        {
          window.alert("User Already Registered");
        }        
      }
    )
}
}
