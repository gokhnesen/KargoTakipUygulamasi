import { Routes } from '@angular/router';
import { Home } from './features/home/home';
import { Login } from './features/login/login';
import { CargoTrackingComponent } from './features/cargo-tracking/cargo-tracking.component';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'login', component: Login },
  { path: '**', redirectTo: '' },
  { path: 'track', component: CargoTrackingComponent }
];
