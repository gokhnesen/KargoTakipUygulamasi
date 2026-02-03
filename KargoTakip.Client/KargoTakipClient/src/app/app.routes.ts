import { Routes } from '@angular/router';
import { Home } from './features/home/home';
import { Login } from './features/login/login';
import { CargoTrackingComponent } from './features/cargo-tracking/cargo-tracking.component';
import { CargoManagementComponent } from './features/cargo-management/cargo-management.component';
import { ConfirmEmailComponent } from './features/confirm-email/confirm-email.component';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'login', component: Login },
  { path: 'confirm-email', component: ConfirmEmailComponent },
  { path: 'track', component: CargoTrackingComponent },
  { path: 'cargo-management', component: CargoManagementComponent },
  { path: '**', redirectTo: '' },
];
