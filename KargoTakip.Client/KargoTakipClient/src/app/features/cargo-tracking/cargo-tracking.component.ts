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
      const trackingNo = params['tracking'];
      
      if (trackingNo) {
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

    if (!this.trackingNumber.trim()) {
      this.error = 'Lütfen takip numarası giriniz.';
      return;
    }

    this.loading = true;
    this.error = '';
    this.cargo = null;

    this.cargoService.trackCargo(this.trackingNumber).subscribe({
      next: (response) => {
        if (!response) {
          this.error = 'Kargo bulunamadı.';
          this.loading = false;
          return;
        }

        this.cargo = {
          id: response.id,
          trackingNumber: response.trackingNumber,
          status: response.status,
          currentLocation: response.currentLocation,
          senderName: response.senderName,
          senderAddress: response.senderAddress,
          senderPhoneNumber: response.senderPhoneNumber,
          receiverName: response.receiverName,
          receiverSurname: response.receiverSurname,
          receiverAddress: response.receiverAddress,
          receiverPhoneNumber: response.receiverPhoneNumber,
          receiverEmail: response.receiverEmail,
          movements: response.movements || [],
          createdDate: response.createdDate,
          updatedDate: response.updatedDate
        } as Cargo;
        
        this.movements = response.movements || [];

        this.loading = false;
        
        this.router.navigate(['/cargo-tracking', this.trackingNumber]);
      },
      error: (err) => {
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
