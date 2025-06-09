import { Routes } from '@angular/router';
import { HomeComponent } from './home.component';
import { BooksComponent } from './books.component';
import { AdminBooksComponent } from './admin-books.component';
import { LoginComponent } from './login.component';
import { RegisterComponent } from './register.component';
import { ForgotPasswordComponent } from './forgot-password.component';
import { ResetPasswordComponent } from './reset-password.component';
import {BookDetailsComponent} from './book-details.component';
import { ConfirmEmailComponent } from './confirm-email.component';
import { BorrowRecordsComponent } from './borrow-records.component';  
import { AdminFinesComponent } from './admin-fines.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'books', component: BooksComponent },
  { path: 'books/:id', component: BookDetailsComponent },
  { path: 'admin', component: AdminBooksComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'forgot-password', component: ForgotPasswordComponent },
  { path: 'reset-password', component: ResetPasswordComponent },
  { path: 'confirm-email', component: ConfirmEmailComponent },
  {path : 'borrow-records',component:BorrowRecordsComponent},
  {path:'admin-fines',component:AdminFinesComponent},
  // ...add other routes here as needed
];
