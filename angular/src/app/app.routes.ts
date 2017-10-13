import { Routes } from '@angular/router';
import { HomeComponent } from './home';
import { ProductsComponent } from './products/products.component';
import { VariantsComponent } from './variants/variants.component';
import { VariantsResolver } from './variants/variants.resolver';
import { OrderListResolver } from './orderlist/orderlist.resolver';
import { NoContentComponent } from './no-content';
import { AuthGuard } from './auth/authguard/authguard';
import { LoginRegisterComponent } from './auth/loginregister/loginregister.component';
import { CreateAccountComponent } from './auth/createaccount/createaccount.component';
import { CheckEmailComponent } from './auth/createaccount/checkemail/checkemail.component';
import { VerifyEmailComponent } from './auth/createaccount/verifyemail/verifyemail.component';
import { VerificationComponent } from './auth/verification/verification.component';
import { ForgotPasswordComponent } from './auth/forgotpassword/forgotpassword.component';
import { UpdatePasswordComponent } from './auth/updatepassword/updatepassword.component';
import { ChangePasswordComponent } from './auth/changepassword/changepassword.component';
import { GetEmailComponent } from './auth/loginregister/facebook/getemail/getemail.component';
import { CartComponent } from './cart/cart.component';
import { CheckOutComponent } from './checkout/checkout.component';
import { MyAccountComponent } from './account/myaccount.component';
import { AddressComponent } from './account/address/address.component';
import { OrderListComponent } from './orderlist/orderlist.component';
import { WishListComponent } from './wishlist/wishlist.component';
import { DataResolver } from './app.resolver';
import { AboutComponent } from './policies/about/about.component';
import { ContactComponent } from './policies/contact/contact';
import { CancellationComponent } from './policies/cancellation/cancellation';
import { DeliveryComponent } from './policies/delivery/delivery';
import { DisclaimerComponent } from './policies/disclaimer/disclaimer';
import { PrivacyComponent } from './policies/privacy/privacy';
import { TermsComponent } from './policies/termsandconditions/terms';
import { AddedToCartComponent } from './message/addedtocart/addedtocart';
import { AddedToWishListComponent } from './message/addedtowishlist/addedtowishlist';

export const ROUTES: Routes = [
  { path: '',      component: HomeComponent },
  { path: 'home',  component: HomeComponent },
  { path: 'about', component: AboutComponent },
  { path: 'contact', component: ContactComponent },
  { path: 'loginregister', component: LoginRegisterComponent },
  { path: 'createaccount', component: CreateAccountComponent },
  { path: 'checkemail', component: CheckEmailComponent },
  { path: 'verifyemail/:userName/:otp/:update', component: VerifyEmailComponent },
  { path: 'forgotpassword', component: ForgotPasswordComponent },
  { path: 'changepassword', component: ChangePasswordComponent },
  { path: 'getemail', component: GetEmailComponent},
  { path: 'verify/:PhoneNumber/:action', component: VerificationComponent },
  { path: 'updatepassword/:userName', component: UpdatePasswordComponent },
  { path: 'products/:productFor/:productType', component: ProductsComponent },
  { path: 'products/:productFor/:productType/variants/:productDesign',
    component: VariantsComponent, resolve: { item: VariantsResolver }},
  { path: 'cart', component: CartComponent },
  { path: 'checkout', component: CheckOutComponent, canActivate: [AuthGuard] },
  { path: 'myaccount', component: MyAccountComponent, canActivate: [AuthGuard] },
  { path: 'address', component: AddressComponent, canActivate: [AuthGuard] },
  { path: 'orderlist', component: OrderListComponent, canActivate: [AuthGuard],
     resolve: { orders: OrderListResolver } },
  { path: 'wishlist', component: WishListComponent },
  { path: 'cancellation', component: CancellationComponent },
  { path: 'delivery', component: DeliveryComponent },
  { path: 'disclaimer', component: DisclaimerComponent },
  { path: 'privacy', component: PrivacyComponent },
  { path: 'terms', component: TermsComponent },
  { path: 'addedtocart', component: AddedToCartComponent},
  { path: 'addedtowishlist', component: AddedToWishListComponent},
  { path: '**',    component: NoContentComponent },
];
