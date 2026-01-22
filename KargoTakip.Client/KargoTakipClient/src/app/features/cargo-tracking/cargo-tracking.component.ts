import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Cargo } from '../../shared/Cargo';
import { CargoMovement } from '../../shared/CargoMovement';
import { CargoStatus } from '../../shared/CargoStatus';
import { CargoServices } from '../../core/services/cargoServices.service';

@Component({
  selector: 'app-cargo-tracking',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './cargo-tracking.component.html',
  styleUrl: './cargo-tracking.component.scss'
})
export class CargoTrackingComponent implements OnInit {
  trackingNumber: string = '';
  cargo: Cargo | null = null;
  movements: CargoMovement[] = [];
  loading: boolean = false;
  error: string = '';

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
        
        // Kargo detay sayfasına yönlendir
        console.log('7. Detay sayfasına yönlendiriliyor...');
        this.router.navigate(['/cargo-tracking', this.trackingNumber]);
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

  getStatusText(status: CargoStatus): string {
    switch (status) {
      case CargoStatus.Received: return 'Teslim Alındı';
      case CargoStatus.Processing: return 'İşleniyor';
      case CargoStatus.InTransit: return 'Yolda';
      case CargoStatus.OutForDelivery: return 'Dağıtımda';
      case CargoStatus.Delivered: return 'Teslim Edildi';
      case CargoStatus.Undelivered: return 'Teslim Edilemedi';
      default: return 'Bilinmiyor';
    }
  }

  getStatusClass(status: CargoStatus): string {
    switch (status) {
      case CargoStatus.Received: return 'status-received';
      case CargoStatus.Processing: return 'status-processing';
      case CargoStatus.InTransit: return 'status-transit';
      case CargoStatus.OutForDelivery: return 'status-delivery';
      case CargoStatus.Delivered: return 'status-delivered';
      case CargoStatus.Undelivered: return 'status-undelivered';
      default: return '';
    }
  }
}
