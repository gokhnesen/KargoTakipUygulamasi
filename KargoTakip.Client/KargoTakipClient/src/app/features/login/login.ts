import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})
export class Login {
  email: string = '';
  password: string = '';

  constructor(private router: Router) {}

  login() {
    console.log('Email:', this.email);
    console.log('Password:', this.password);
  }

  goBack() {
    this.router.navigate(['/']);
  }
}