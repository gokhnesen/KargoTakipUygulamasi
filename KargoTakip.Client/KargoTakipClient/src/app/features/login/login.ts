import { Component, ViewChildren, QueryList, ElementRef, AfterViewInit } from '@angular/core';
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
export class Login implements AfterViewInit {
  @ViewChildren('otpDigit') otpDigitRefs!: QueryList<ElementRef<HTMLInputElement>>;

  step: 'credentials' | 'otp' = 'credentials';
  email: string = '';
  password: string = '';
  otpCode: string = '';
  loginToken: string = '';
  error: string = '';
  loading: boolean = false;
  otpLoading: boolean = false;
  showResendLink: boolean = false;
  resendMessage: string = '';
  resendLoading: boolean = false;
  readonly otpLength = 6;
  otpIndices = [0, 1, 2, 3, 4, 5];

  constructor(
    private router: Router,
    private loginService: LoginServices
  ) {}

  ngAfterViewInit(): void {}

  getOtpChar(i: number): string {
    return this.otpCode[i] ?? '';
  }

  onOtpInput(e: Event, i: number): void {
    const input = e.target as HTMLInputElement;
    let raw = input.value.replace(/\D/g, '');
    if (raw.length > 1) {
      raw = raw.slice(0, this.otpLength);
      this.otpCode = raw;
      input.value = this.getOtpChar(i);
      const nextIdx = Math.min(raw.length, this.otpLength - 1);
      this.focusOtpDigit(nextIdx);
      return;
    }
    const char = raw.slice(-1);
    this.otpCode = this.otpCode.substring(0, i) + char + this.otpCode.substring(i + 1).slice(0, this.otpLength - i - 1);
    if (char && i < this.otpLength - 1) this.focusOtpDigit(i + 1);
  }

  onOtpKeydown(e: KeyboardEvent, i: number): void {
    if (e.key === 'Backspace' && !this.getOtpChar(i) && i > 0) {
      this.otpCode = this.otpCode.substring(0, i - 1) + this.otpCode.substring(i);
      this.focusOtpDigit(i - 1);
    }
  }

  focusOtpDigit(i: number): void {
    setTimeout(() => this.otpDigitRefs?.get(i)?.nativeElement?.focus(), 0);
  }

  login() {
    if (!this.email || !this.password) {
      this.error = 'Lütfen tüm alanları doldurun.';
      return;
    }

    this.loading = true;
    this.error = '';

    this.loginService.login(this.email, this.password).subscribe({
      next: (response) => {
        this.loading = false;
        if (response.requiresOtp && response.loginToken) {
          this.step = 'otp';
          this.loginToken = response.loginToken;
          this.otpCode = '';
          this.error = '';
          setTimeout(() => this.focusOtpDigit(0), 150);
        } else if (response.token) {
          sessionStorage.setItem('token', response.token);
          if (response.refreshToken) {
            sessionStorage.setItem('refreshToken', response.refreshToken);
          }
          this.router.navigate(['/cargo-management']);
        }
      },
      error: (error) => {
        const msg = error.error?.message || '';
        this.error = msg || (error.status === 401 ? 'E-posta veya şifre hatalı.' : 'Bir hata oluştu. Lütfen tekrar deneyin.');
        this.showResendLink = error.status === 401 && (msg.includes('onaylanmadi') || msg.includes('onaylanmamış'));
        this.loading = false;
      }
    });
  }

  verifyOtp() {
    const code = this.otpCode?.trim().replace(/\s/g, '') ?? '';
    if (!this.loginToken || !code) {
      this.error = 'Lütfen e-postanıza gelen 6 haneli kodu girin.';
      return;
    }

    this.otpLoading = true;
    this.error = '';

    this.loginService.verifyLoginOtp(this.loginToken, code).subscribe({
      next: () => {
        this.otpLoading = false;
        this.router.navigate(['/cargo-management']);
      },
      error: (error) => {
        this.otpLoading = false;
        this.error = error.error?.message || 'Kod hatalı veya süresi dolmuş. Tekrar giriş yapıp yeni kod alabilirsiniz.';
      }
    });
  }

  backToCredentials() {
    this.step = 'credentials';
    this.loginToken = '';
    this.otpCode = '';
    this.error = '';
  }

  goBack() {
    this.router.navigate(['/']);
  }

  resendConfirmationEmail() {
    if (!this.email?.trim()) return;
    this.resendLoading = true;
    this.resendMessage = '';
    this.loginService.resendConfirmationEmail(this.email).subscribe({
      next: (res) => {
        this.resendMessage = res.message || 'Onay linki e-posta adresinize gönderildi.';
        this.showResendLink = false;
        this.resendLoading = false;
      },
      error: (err) => {
        this.resendMessage = err.error?.message || 'Onay e-postası gönderilemedi.';
        this.resendLoading = false;
      }
    });
  }
}