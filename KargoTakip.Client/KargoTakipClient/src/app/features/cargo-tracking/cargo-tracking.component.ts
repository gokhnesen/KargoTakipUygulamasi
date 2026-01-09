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
  CargoStatus = CargoStatus;

  constructor(
    private cargoService: CargoServices,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // URL parametrelerini dinle
    this.route.queryParams.subscribe(params => {
      console.log('1. URL Parametreleri Geldi:', params); // DEBUG
      const trackingNo = params['tracking'];
      
      if (trackingNo) {
        console.log('2. Takip Numarası Tespit Edildi:', trackingNo); // DEBUG
        this.trackingNumber = trackingNo;
        // Küçük bir gecikme ile çağırarak input'un dolmasını garantileyelim
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
    
    console.log('3. trackCargo Metodu Çalıştı. No:', this.trackingNumber); // DEBUG

    if (!this.trackingNumber.trim()) {
      this.error = 'Lütfen takip numarası giriniz.';
      return;
    }

    this.loading = true;
    this.error = '';
    this.cargo = null;

    this.cargoService.trackCargo(this.trackingNumber).subscribe({
      next: (response) => {
        console.log('4. API Başarılı Yanıt Döndü:', response); // DEBUG
        
        // API yanıtını kontrol et, null geliyorsa hata ver
        if (!response) {
          this.error = 'API boş yanıt döndü.';
          this.loading = false;
          return;
        }

        this.cargo = {
          trackingNumber: response.trackingNumber,
          status: response.currentStatus, // Buradaki isimlendirmeye dikkat (API'de 'status' olabilir)
          sender: response.sender,
          receiver: response.receiver
        } as Cargo;
        
        this.movements = response.movements || [];
        this.loading = false;
      },
      error: (err) => {
        console.error('5. API Hatası:', err); // DEBUG
        this.error = 'Kargo bulunamadı veya bir hata oluştu. (Detaylar konsolda)';
        this.loading = false;
      }
    });
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
