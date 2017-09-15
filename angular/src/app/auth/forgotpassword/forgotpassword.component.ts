import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { FormGroup, FormControl, FormArray, Validators } from '@angular/forms';
import {ApiService} from '../../../services/api.service';
import {ActivatedRoute, Router} from '@angular/router';
import { AppState } from '../../app.service';
import {ToastMsgService} from '../../../services/toastmsg.service';
import { authServer } from '../../config/configuration';

@Component({
  selector: 'forgotpassword',
  templateUrl: './forgotpassword.component.html',
  styleUrls: ['./forgotpassword.component.css']
})
export class ForgotPasswordComponent {
public PhoneNumber: string;
constructor(private apiService: ApiService, private activatedRoute: ActivatedRoute, private router: Router,  public appState: AppState, private toastmsg: ToastMsgService){
 
}

onSubmit(form: NgForm){
    const phoneNumber = form.value;    
    console.log(phoneNumber);
    this.apiService.post('/forgotpassword',phoneNumber,undefined, authServer).then(
      (response: any) => {
          console.log(response);
        if(response.code == undefined){
          throw response.error;
        }
        if(response.message == "Success")
        {
         localStorage.setItem("UserName", phoneNumber.PhoneNumber);
         this.router.navigate(['../verify', phoneNumber.PhoneNumber, "forgotpassword"],{ relativeTo: this.activatedRoute});
        }
      })
      .catch(
      (error: any) => {        
        if(error.code == "404")
        {
         this.toastmsg.popToast("error","Error","User not Registered")
        }    
      }
    )
}
}