import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { FormGroup, FormControl, FormArray, Validators } from '@angular/forms';
import {ApiService} from '../../../services/api.service';
import { ActivatedRoute, Params, Router, ParamMap } from '@angular/router';
import {ToastMsgService} from '../../../services/toastmsg.service';
import { authServer } from '../../config/configuration';

@Component({
  selector: 'createaccount',
  templateUrl: './createaccount.component.html',
  styleUrls: ['./createaccount.component.scss']
})
export class CreateAccountComponent {

constructor(private apiService: ApiService, private route: ActivatedRoute, private router: Router, private toastmsg: ToastMsgService){

}

onSubmit(form: NgForm){
       
    const userDetails = form.value;
    this.apiService.post('/register',userDetails,undefined,authServer).then(
      (response: any) => {
        if(response.message == "User Registered")
        {
         this.toastmsg.popToast("success","Success","Account Created")
         localStorage.setItem("UserName", userDetails.PhoneNumber);
         this.router.navigate(['../verify', userDetails.PhoneNumber,"createaccount"],{ relativeTo: this.route});
        }
      })
      .catch(
      (error: any) => {
        var errormsg = JSON.parse(error._body);
        if(errormsg.message == "User Already Registered")
        {
          this.toastmsg.popToast("info","Msg","User already Registered")
        }        
      }
    )
}
}
