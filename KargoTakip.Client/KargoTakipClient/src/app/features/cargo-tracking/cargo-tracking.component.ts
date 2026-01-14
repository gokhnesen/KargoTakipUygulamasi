import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Cargo } from '../../shared/Cargo';
import { CargoMovement } from '../../shared/CargoMovement';
import { CargoStatus } from '../../shared/CargoStatus';
import { CargoServices } from '../../core/services/cargoServices.service';
import { CargoDetailsComponent } from './cargo-details/cargo-details.component';

@Component({
  selector: 'app-cargo-tracking',
  standalone: true,
  imports: [CommonModule, FormsModule, CargoDetailsComponent],
  templateUrl: './cargo-tracking.component.html',
  styleUrl: './cargo-tracking.component.scss'
})
export class CargoTrackingComponent implements OnInit {
  trackingNumber: string = '';
  cargo: Cargo | null = null;
  movements: CargoMovement[] = [];
  loading: boolean = false;
  error: string = '';
  isModalOpen: boolean = false;

  constructor(
    private cargoService: CargoServices,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      console.log('1. URL Parametreleri Geldi:', params);
      const trackingNo = params['tracking'];
      
      if (trackingNo) {
        console.log('2. Takip Numarası Tespit Edildi:', trackingNo);
        this.trackingNumber = trackingNo;
        setTimeout(() => {
          this.trackCargo(); 
        }, 100);
      }
    });
  }

  trackCargo(event?: Event): void {
    if (event) {
      event.preventDefault();
    }
    
    console.log('3. trackCargo Metodu Çalıştı. No:', this.trackingNumber);
    console.log('Event:', event);

    if (!this.trackingNumber.trim()) {
      this.error = 'Lütfen takip numarası giriniz.';
      console.log('Hata: Takip numarası boş');
      return;
    }

    this.loading = true;
    this.error = '';
    this.cargo = null;
    this.isModalOpen = false;

    console.log('API çağrısı yapılıyor...');

    this.cargoService.trackCargo(this.trackingNumber).subscribe({
      next: (response) => {
        console.log('4. API Başarılı Yanıt Döndü:', response);
        
        if (!response) {
          this.error = 'Kargo bulunamadı.';
          this.loading = false;
          console.log('Hata: Response boş');
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

        console.log('5. Cargo set:', this.cargo);
        console.log('6. Movements set:', this.movements);

        this.loading = false;
        
        console.log('7. isModalOpen değiştiriliyor: false -> true');
        this.isModalOpen = true;
        
        console.log('8. isModalOpen yeni değer:', this.isModalOpen);
        
        // Zorla change detection tetikle
        setTimeout(() => {
          console.log('9. Timeout sonrası isModalOpen:', this.isModalOpen);
        }, 100);
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

  closeModal(): void {
    this.isModalOpen = false;
  }

  getStatusText(status: CargoStatus): string {
    switch (status) {
      case CargoStatus.Created: return 'Oluşturuldu';
      case CargoStatus.InTransit: return 'Yolda';
      case CargoStatus.InWarehouse: return 'Depoda';
      case CargoStatus.OutForDelivery: return 'Dağıtımda';
      case CargoStatus.Delivered: return 'Teslim Edildi';
      case CargoStatus.Cancelled: return 'İptal Edildi';
      case CargoStatus.Returned: return 'İade Edildi';
      default: return 'Bilinmiyor';
    }
  }

  getStatusClass(status: CargoStatus): string {
    switch (status) {
      case CargoStatus.Created: return 'status-created';
      case CargoStatus.InTransit: return 'status-transit';
      case CargoStatus.InWarehouse: return 'status-warehouse';
      case CargoStatus.OutForDelivery: return 'status-delivery';
      case CargoStatus.Delivered: return 'status-delivered';
      case CargoStatus.Cancelled: return 'status-cancelled';
      case CargoStatus.Returned: return 'status-returned';
      default: return '';
    }
  }
}
