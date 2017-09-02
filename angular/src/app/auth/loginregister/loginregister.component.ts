import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'app-loginregister',
  templateUrl: './loginregister.component.html',
  styleUrls: ['./loginregister.component.css']
})
export class LoginRegisterComponent {

  constructor() { }

  onSignin(form: NgForm) {
    const email = form.value.email;
    const password = form.value.password;
  }

}
