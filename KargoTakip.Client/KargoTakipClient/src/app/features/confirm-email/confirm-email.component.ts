import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { LoginServices } from '../../core/services/loginServices.service';

@Component({
  selector: 'app-confirm-email',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './confirm-email.component.html',
  styleUrls: ['./confirm-email.component.scss']
})
export class ConfirmEmailComponent implements OnInit {
  loading = true;
  success = false;
  error = '';
  message = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private loginService: LoginServices
  ) {}

  ngOnInit(): void {
    const userId = this.route.snapshot.queryParamMap.get('userId');
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!userId || !token) {
      this.loading = false;
      this.error = 'Geçersiz onay linki. Eksik parametreler.';
      return;
    }

    this.loginService.confirmEmail(userId, token).subscribe({
      next: (res) => {
        this.loading = false;
        this.success = res.success ?? true;
        this.message = res.message || 'E-posta adresiniz onaylandı. Giriş yapabilirsiniz.';
      },
      error: (err) => {
        this.loading = false;
        this.success = false;
        this.error = err.error?.message || 'E-posta onayı yapılamadı. Link süresi dolmuş veya geçersiz olabilir.';
      }
    });
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }

  goHome(): void {
    this.router.navigate(['/']);
  }
}
