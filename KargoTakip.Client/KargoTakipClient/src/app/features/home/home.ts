import { Component, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CargoServices } from '../../core/services/cargoServices.service';
import { Cargo } from '../../shared/Cargo';
import { CargoMovement } from '../../shared/CargoMovement';
import { CargoStatus } from '../../shared/CargoStatus';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './home.html',
  styleUrls: ['./home.scss']
})
export class Home {
  trackingCode: string = '';
  cargo: Cargo | null = null;
  movements: CargoMovement[] = [];
  loading: boolean = false;
  error: string = '';
  showResults: boolean = false;
  
  CargoStatus = CargoStatus;
  
  constructor(
    private router: Router,
    private cargoService: CargoServices,
    private cdr: ChangeDetectorRef
  ) {}

  convertToTurkeyTime(date: Date | string | undefined): Date | undefined {
    if (!date) return undefined;
    const d = new Date(date);
    d.setHours(d.getHours() + 3);
    return d;
  }

  searchCargo() {
    if (!this.trackingCode.trim()) {
      this.error = 'Lütfen takip numarası giriniz.';
      return;
    }
    
    this.loading = true;
    this.error = '';
    this.cargo = null;
    this.showResults = false;

    this.cargoService.trackCargo(this.trackingCode.trim()).subscribe({
      next: (response) => {
        if (!response) {
          this.error = 'Kargo bulunamadı.';
          this.loading = false;
          return;
        }

        this.cargo = {
          trackingNumber: response.trackingNumber,
          status: response.currentStatus,
          currentLocation: response.currentLocation,
          sender: {
            fullName: response.sender.fullName,
            address: response.sender.address,
            phoneNumber: response.sender.phoneNumber
          },
          receiver: {
            fullName: response.receiver.fullName,
            address: response.receiver.address,
            phoneNumber: response.receiver.phoneNumber,
            email: response.receiver.email
          }
        } as Cargo;
        
        this.movements = (response.movements || []).map((m: any) => ({
          status: m.status,
          statusText: m.statusText,
          location: m.location,
          description: m.description,
          date: m.date
        }));

        this.loading = false;
        this.showResults = true;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = err.status === 404 
          ? 'Kargo bulunamadı. Lütfen takip numarasını kontrol ediniz.' 
          : 'Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.';
        this.loading = false;
      }
    });
  }

  closeResults(): void {
    this.showResults = false;
    this.trackingCode = '';
    this.cargo = null;
    this.movements = [];
  }

  getStatusText(status?: CargoStatus): string {
    if (status === undefined) {
      return 'Bilinmiyor';
    }
    const statusMap: Record<CargoStatus, string> = {
      [CargoStatus.Received]: 'Teslim Alındı',
      [CargoStatus.Processing]: 'İşleniyor',
      [CargoStatus.InTransit]: 'Yolda',
      [CargoStatus.OutForDelivery]: 'Dağıtımda',
      [CargoStatus.Delivered]: 'Teslim Edildi',
      [CargoStatus.Undelivered]: 'Teslim Edilemedi'
    };
    return statusMap[status] || 'Bilinmiyor';
  }

  getStatusIcon(status: CargoStatus): string {
    const iconMap: Record<CargoStatus, string> = {
      [CargoStatus.Received]: '📦',
      [CargoStatus.Processing]: '⚙️',
      [CargoStatus.InTransit]: '🚚',
      [CargoStatus.OutForDelivery]: '🚛',
      [CargoStatus.Delivered]: '✅',
      [CargoStatus.Undelivered]: '❌'
    };
    return iconMap[status] || '📋';
  }
  
  goToLogin() {
    this.router.navigate(['/login']);
  }

  @HostListener('document:keydown.escape')
  handleEscapeKey() {
    if (this.showResults) {
      this.closeResults();
    }
  }
}