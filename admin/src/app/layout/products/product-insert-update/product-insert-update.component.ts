import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-product-insert-update',
  templateUrl: './product-insert-update.component.html',
  styleUrls: ['./product-insert-update.component.scss']
})
export class ProductInsertUpdateComponent implements OnInit {

@Input() product: any = { productItem: {}};
@Output() cancelButtonClicked = new EventEmitter<any>();
  constructor() { }

  ngOnInit() {
  }
  cancelClicked(){
    this.cancelButtonClicked.emit(true);
  }

}
