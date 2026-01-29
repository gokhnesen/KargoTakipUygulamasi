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
  ) { }


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
          id: '',
          trackingNumber: response.trackingNumber,
          senderName: response.sender.fullName,
          senderAddress: response.sender.address,
          senderPhoneNumber: response.sender.phoneNumber,
          receiverName: response.receiver.fullName,
          receiverSurname: '',
          receiverAddress: response.receiver.address,
          receiverPhoneNumber: response.receiver.phoneNumber,
          receiverEmail: response.receiver.email,
          status: response.currentStatus,
          currentLocation: response.currentLocation,
          createdDate: new Date(),
          movements: (response.movements || []).map((m: any) => ({
            id: '',
            cargoId: '',
            newStatus: m.status,
            location: m.location,
            description: m.description,
            createdDate: m.date ? new Date(m.date) : new Date(),
            updatedDate: m.date ? new Date(m.date) : new Date(),
            isDeleted: false,
            status: m.status,
            statusText: m.statusText,
            date: m.date
          }))
        };

        this.movements = this.cargo?.movements || [];
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