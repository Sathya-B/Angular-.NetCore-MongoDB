import { Routes } from '@angular/router';
import { HomeComponent } from './home';
import { AboutComponent } from './about';
import { ProductsComponent} from './products/products.component';
import {VariantsComponent} from './variants/variants.component';
import { NoContentComponent } from './no-content';
import {LoginRegisterComponent} from './auth/loginregister/loginregister.component';

import { DataResolver } from './app.resolver';

export const ROUTES: Routes = [
  { path: '',      component: HomeComponent },
  { path: 'home',  component: HomeComponent },
  { path: 'about', component: AboutComponent },
  { path: 'loginregister', component: LoginRegisterComponent },
  { path: 'products/:productFor/:productType', component: ProductsComponent},
  { path: 'products/:productFor/:productType/variants/:productDesign', component: VariantsComponent},
  { path: 'barrel', loadChildren: './+barrel#BarrelModule'},
  { path: '**',    component: NoContentComponent },
];
