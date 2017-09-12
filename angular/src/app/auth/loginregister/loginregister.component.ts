import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import {ApiService} from '../../../services/api.service';
import { FormGroup, FormControl, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute, Params, Router, ParamMap } from '@angular/router';
import { AppState } from '../../app.service';

@Component({
  selector: 'app-loginregister',
  templateUrl: './loginregister.component.html',
  styleUrls: ['./loginregister.component.css']
})
export class LoginRegisterComponent {

  constructor(private appState: AppState, private apiService: ApiService, private route: ActivatedRoute, private router: Router) { }

  onSignin(form: NgForm) {
    const loginDetails = form.value;
    console.log(loginDetails);

      this.apiService.post('login',loginDetails,undefined,"http://192.168.0.111:5001/api/auth/").subscribe(
      (response: any) => {
        if(response.value.code == "999")
        {
        localStorage.setItem("JWT", JSON.stringify(response.value.data));
        this.appState.set('loggedIn', true);
         this.router.navigate(['/']);
        }
      },
      (error: any) => {
        var errormsg = JSON.parse(error._body);
        if(errormsg.code == "400")
        {
          window.alert("Please enter correct credentials");
        }        
      }
    )

  }

}
