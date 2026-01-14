import { Component, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CargoServices } from '../../core/services/cargoServices.service';
import { Cargo } from '../../shared/Cargo';
import { CargoMovement } from '../../shared/CargoMovement';
import { CargoStatus } from '../../shared/CargoStatus';
import { CargoDetailsComponent } from "../cargo-tracking/cargo-details/cargo-details.component";

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, CargoDetailsComponent],
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

  searchCargo() { 
    console.log('searchCargo çağrıldı, trackingCode:', this.trackingCode);
    
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
        console.log('API Başarılı Yanıt:', response);
        
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
        
        console.log('Sonuçlar gösteriliyor, showResults:', this.showResults);
      },
      error: (err) => {
        console.error('API Hatası:', err);
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

  getStatusText(status: CargoStatus): string {
    const statusMap: Record<CargoStatus, string> = {
      [CargoStatus.Created]: 'Oluşturuldu',
      [CargoStatus.InTransit]: 'Yolda',
      [CargoStatus.InWarehouse]: 'Depoda',
      [CargoStatus.OutForDelivery]: 'Dağıtımda',
      [CargoStatus.Delivered]: 'Teslim Edildi',
      [CargoStatus.Cancelled]: 'İptal Edildi',
      [CargoStatus.Returned]: 'İade Edildi'
    };
    return statusMap[status] || 'Bilinmiyor';
  }

  getStatusIcon(status: CargoStatus): string {
    const iconMap: Record<CargoStatus, string> = {
      [CargoStatus.Created]: '📦',
      [CargoStatus.InTransit]: '🚚',
      [CargoStatus.InWarehouse]: '🏢',
      [CargoStatus.OutForDelivery]: '🚛',
      [CargoStatus.Delivered]: '✅',
      [CargoStatus.Cancelled]: '❌',
      [CargoStatus.Returned]: '↩️'
    };
    return iconMap[status] || '📋';
  }
  
  goToLogin() {
    this.router.navigate(['/login']);
  }

 // ...existing code...
  @HostListener('document:keydown.escape')
  handleEscapeKey() {
    if (this.showResults) {
      this.closeResults();
    }
  }
// ...existing code...
}