import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

export interface AuthErrorResponse {
  message?: string;
  statusCode?: number;
}

@Injectable({
  providedIn: 'root',
})
export class LoginServices {
  private baseUrl: string = `${environment.apiUrl}/Auth`;
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(this.hasToken());
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) { }

  login(email: string, password: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/login`, { email, password }, { withCredentials: true }).pipe(
      tap((response: any) => {
        if (response.success && response.token && !response.requiresOtp) {
          sessionStorage.setItem('token', response.token);
          if (response.refreshToken) {
            sessionStorage.setItem('refreshToken', response.refreshToken);
          }
          this.isAuthenticatedSubject.next(true);
        }
      })
    );
  }

  verifyLoginOtp(loginToken: string, code: string): Observable<{ success: boolean; token: string; refreshToken?: string; expiration?: string; message?: string }> {
    return this.http.post<{ success: boolean; token: string; refreshToken?: string; expiration?: string; message?: string }>(
      `${this.baseUrl}/verify-login-otp`,
      { loginToken, code },
      { withCredentials: true }
    ).pipe(
      tap((response) => {
        if (response.success && response.token) {
          sessionStorage.setItem('token', response.token);
          if (response.refreshToken) {
            sessionStorage.setItem('refreshToken', response.refreshToken);
          }
          this.isAuthenticatedSubject.next(true);
        }
      })
    );
  }

  logout(): void {
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('refreshToken');
    this.isAuthenticatedSubject.next(false);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return sessionStorage.getItem('token');
  }

  private hasToken(): boolean {
    return !!sessionStorage.getItem('token');
  }

  isLoggedIn(): boolean {
    return this.hasToken();
  }

  confirmEmail(userId: string, token: string): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.baseUrl}/confirm-email`, {
      userId,
      token
    });
  }

  resendConfirmationEmail(email: string): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.baseUrl}/resend-confirmation-email`, {
      email
    });
  }
}