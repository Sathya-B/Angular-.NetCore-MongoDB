import { Component, OnInit, Input, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-category-insert-update',
  templateUrl: './category-insert-update.component.html',
  styleUrls: ['./category-insert-update.component.scss']
})
export class CategoryInsertUpdateComponent implements OnInit {

@Input() category: any = { categoryItem: {}};
@Output() cancelButtonClicked = new EventEmitter<any>();
  constructor() { }

  ngOnInit() {
  }

cancelClicked(){
  this.cancelButtonClicked.emit(true);
}
}
