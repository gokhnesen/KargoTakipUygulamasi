import { Routes } from '@angular/router';
import { Home } from './features/home/home';
import { Login } from './features/login/login';
import { CargoTrackingComponent } from './features/cargo-tracking/cargo-tracking.component';
import { CargoManagementComponent } from './features/cargo-management/cargo-management.component';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'login', component: Login },
  { path: 'track', component: CargoTrackingComponent },
  { path: 'cargo-management', component: CargoManagementComponent },



    { path: '**', redirectTo: '' },

];
