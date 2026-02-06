import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const router = inject(Router);

    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            let errorMessage = 'Bir hata oluştu';

            if (error.error instanceof ErrorEvent) {
                errorMessage = `Hata: ${error.error.message}`;
            } else {
                switch (error.status) {
                    case 401:
                        localStorage.removeItem('token');
                        localStorage.removeItem('refreshToken');
                        router.navigate(['/login']);
                        errorMessage = 'Oturum süreniz doldu. Lütfen tekrar giriş yapın.';
                        break;
                    case 403:
                        errorMessage = 'Bu işlem için yetkiniz yok.';
                        break;
                    case 404:
                        errorMessage = 'İstenen kaynak bulunamadı.';
                        break;
                    case 500:
                        errorMessage = 'Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyin.';
                        break;
                    case 0:
                        errorMessage = 'Sunucuya bağlanılamadı. İnternet bağlantınızı kontrol edin.';
                        break;
                    default:
                        errorMessage = error.error?.message || `Hata kodu: ${error.status}`;
                }
            }

            return throwError(() => ({ ...error, userMessage: errorMessage }));
        })
    );
};
