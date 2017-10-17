import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: '[app-productitem]',
  templateUrl: './productitem.component.html',
  styleUrls: ['./productitem.component.scss']
})
export class ProductitemComponent implements OnInit {
@Input('app-productitem') product: any = {};
public showUpdate: boolean = false;
public showAdd: boolean = false;
  constructor() { }

  ngOnInit() {
    console.log(this.product);
  }
  updateClicked() {
    this.showUpdate = true;
  }
  cancelClicked(clicked: any) {
    if(clicked === true) {
    this.showUpdate = false;
    }
  }
  addClicked(){
  this.showAdd = true;
  }
}
