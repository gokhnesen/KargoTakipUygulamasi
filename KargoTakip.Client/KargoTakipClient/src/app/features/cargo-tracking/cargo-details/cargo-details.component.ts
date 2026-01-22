import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CargoServices } from '../../../core/services/cargoServices.service';
import { Cargo } from '../../../shared/Cargo';
import { CargoMovement } from '../../../shared/CargoMovement';
import { CargoStatus } from '../../../shared/CargoStatus';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-cargo-details',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './cargo-details.component.html',
  styleUrl: './cargo-details.component.scss'
})
export class CargoDetailsComponent implements OnInit {
  cargo: Cargo | null = null;
  movements: CargoMovement[] = [];
  loading = false;
  error: string | null = null;
  successMessage: string | null = null;
  validationError: string | null = null;

  showUpdateModal = false;
  updateForm: Partial<CargoMovement> = {
    cargoId: '',
    newStatus: CargoStatus.InTransit,
    location: '',
    description: ''
  };

  cargoStatuses = [
    { value: CargoStatus.Received, label: 'Teslim Alındı' },
    { value: CargoStatus.Processing, label: 'İşleniyor' },
    { value: CargoStatus.InTransit, label: 'Yolda' },
    { value: CargoStatus.OutForDelivery, label: 'Dağıtımda' },
    { value: CargoStatus.Delivered, label: 'Teslim Edildi' },
    { value: CargoStatus.Undelivered, label: 'Teslim Edilemedi' }
  ];

  constructor(
    private route: ActivatedRoute,
    private cargoServices: CargoServices
  ) {}

  ngOnInit(): void {
    const trackingNumber = this.route.snapshot.paramMap.get('trackingNumber');
    if (trackingNumber) {
      this.loadCargoDetails(trackingNumber);
    }
  }

  loadCargoDetails(trackingNumber: string): void {
    this.loading = true;
    this.error = null;

    this.cargoServices.trackCargo(trackingNumber).subscribe({
      next: (data) => {
        this.cargo = data.cargo;
        this.movements = data.movements || [];
        this.updateForm.cargoId = this.cargo?.id || '';
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Kargo bilgileri yüklenirken bir hata oluştu.';
        this.loading = false;
        console.error('Hata:', err);
      }
    });
  }

  openUpdateModal(): void {
    if (this.cargo) {
      this.updateForm = {
        cargoId: this.cargo.id,
        newStatus: this.cargo.status,
        location: '',
        description: ''
      };
      this.showUpdateModal = true;
      this.validationError = null;
      this.successMessage = null;
    }
  }

  closeUpdateModal(): void {
    this.showUpdateModal = false;
    this.resetUpdateForm();
    this.validationError = null;
  }

  submitUpdate(): void {
    if (!this.updateForm.location?.trim() || !this.updateForm.description?.trim()) {
      this.validationError = 'Lütfen tüm alanları doldurun.';
      return;
    }

    this.validationError = null;
    this.loading = true;
    
    this.cargoServices.updateMovement({
      cargoId: this.updateForm.cargoId,
      newStatus: Number(this.updateForm.newStatus),
      location: this.updateForm.location,
      description: this.updateForm.description
    }).subscribe({
      next: () => {
        this.loading = false;
        this.successMessage = 'Kargo durumu başarıyla güncellendi!';
        
        // 2 saniye sonra modalı kapat ve mesajı temizle
        setTimeout(() => {
          this.closeUpdateModal();
          this.successMessage = null;
          
          // Kargo detaylarını yeniden yükle
          if (this.cargo?.trackingNumber) {
            this.loadCargoDetails(this.cargo.trackingNumber);
          }
        }, 2000);
      },
      error: (err: any) => {
        this.validationError = 'Güncelleme sırasında bir hata oluştu: ' + (err.error?.message || 'Bilinmeyen hata');
        this.loading = false;
        console.error('Hata:', err);
      }
    });
  }

  resetUpdateForm(): void {
    this.updateForm = {
      cargoId: this.cargo?.id || '',
      newStatus: this.cargo?.status || CargoStatus.InTransit,
      location: '',
      description: ''
    };
  }

  getStatusLabel(status: CargoStatus): string {
    const statusObj = this.cargoStatuses.find(s => s.value === status);
    return statusObj?.label || 'Bilinmeyen';
  }
}