import { Component, OnInit, Input } from '@angular/core';
import { NgForm } from '@angular/forms';


@Component({
  selector: '[app-categoryitem]',
  templateUrl: './categoryitem.component.html',
  styleUrls: ['./categoryitem.component.scss']
})

export class CategoryItemComponent implements OnInit {
public showUpdate: boolean = false;
@Input('app-categoryitem') category: any = {};
  constructor() { }

  ngOnInit() {
    console.log(this.category);
  }
  updateClicked() {
    this.showUpdate = true;
  }
  cancelClicked(clicked: any) {
    if(clicked === true) {
    this.showUpdate = false;
    }
  }

}
