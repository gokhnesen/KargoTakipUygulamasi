import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LoginServices } from '../../core/services/loginServices.service';

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
  error: string = '';
  loading: boolean = false;

  constructor(
    private router: Router,
    private loginService: LoginServices
  ) {}

  login() {
    if (!this.email || !this.password) {
      this.error = 'Lütfen tüm alanları doldurun.';
      return;
    }

    this.loading = true;
    this.error = '';

    this.loginService.login(this.email, this.password).subscribe({
      next: (response) => {
        localStorage.setItem('token', response.token);
        localStorage.setItem('refreshToken', response.refreshToken);        
        this.router.navigate(['/cargo-management']);
      },
      error: (error) => {
        this.error = error.status === 401 
          ? 'E-posta veya şifre hatalı.' 
          : 'Bir hata oluştu. Lütfen tekrar deneyin.';
        this.loading = false;
      }
    });
  }

  goBack() {
    this.router.navigate(['/']);
  }
}