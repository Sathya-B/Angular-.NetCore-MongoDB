import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { FormGroup, FormControl, FormArray, Validators } from '@angular/forms';
import {ApiService} from '../../../services/api.service';
import {ActivatedRoute, Router} from '@angular/router';
import { AppState } from '../../app.service';

@Component({
  selector: 'forgotpassword',
  templateUrl: './forgotpassword.component.html',
  styleUrls: ['./forgotpassword.component.css']
})
export class ForgotPasswordComponent {
public PhoneNumber: string;
constructor(private apiService: ApiService, private activatedRoute: ActivatedRoute, private router: Router,  public appState: AppState){
 
}

onSubmit(form: NgForm){
    const phoneNumber = form.value;    
    console.log(phoneNumber);
    this.apiService.post('forgotpassword',phoneNumber,undefined,"http://192.168.0.111:5001/api/auth/").subscribe(
      (response: any) => {
          console.log(response);
        if(response.message == "Success")
        {
         this.router.navigate(['../verify', phoneNumber.PhoneNumber, "forgotpassword"],{ relativeTo: this.activatedRoute});
        }
      },
      (error: any) => {
        console.log(error);
      }
    )
}
}