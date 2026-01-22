import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CargoServices } from '../../core/services/cargoServices.service';
import { LoginServices } from '../../core/services/loginServices.service';
import { Cargo } from '../../shared/Cargo';
import { CargoStatus } from '../../shared/CargoStatus';

@Component({
  selector: 'app-cargo-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './cargo-management.component.html',
  styleUrls: ['./cargo-management.component.scss']
})
export class CargoManagementComponent implements OnInit {
  cargos: Cargo[] = [];
  filteredCargos: Cargo[] = [];
  loading: boolean = false;
  error: string = '';
  searchTerm: string = '';
  
  selectedCargo: Cargo | null = null;
  showUpdateModal: boolean = false;
  showCreateModal: boolean = false;
  newStatus: CargoStatus = CargoStatus.InTransit;
  newLocation: string = '';
  newDescription: string = '';
  updating: boolean = false;
  creating: boolean = false;
  successMessage: string | null = null;
  validationError: string | null = null;

  // Kargo oluşturma formu
  createForm: Partial<Cargo> = {
    senderName: '',
    senderAddress: '',
    senderPhoneNumber: '',
    receiverName: '',
    receiverSurname: '',
    receiverAddress: '',
    receiverPhoneNumber: '',
    receiverEmail: ''
  };

  CargoStatus = CargoStatus;
  statusOptions = [
    { value: CargoStatus.Received, label: 'Teslim Alındı' },
    { value: CargoStatus.Processing, label: 'İşleniyor' },
    { value: CargoStatus.InTransit, label: 'Yolda' },
    { value: CargoStatus.OutForDelivery, label: 'Dağıtımda' },
    { value: CargoStatus.Delivered, label: 'Teslim Edildi' },
    { value: CargoStatus.Undelivered, label: 'Teslim Edilemedi' }
  ];

  constructor(
    private cargoService: CargoServices,
    private loginService: LoginServices,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCargos();
  }

  loadCargos(): void {
    this.loading = true;
    this.error = '';

    this.cargoService.getAll().subscribe({
      next: (response) => {
        this.cargos = response;
        this.filteredCargos = response;
        this.loading = false;
      },
      error: (err) => {
        console.error('Kargolar yüklenirken hata:', err);
        this.error = 'Kargolar yüklenirken bir hata oluştu.';
        this.loading = false;
        
        if (err.status === 401) {
          this.router.navigate(['/login']);
        }
      }
    });
  }

  searchCargo(): void {
    if (!this.searchTerm.trim()) {
      this.filteredCargos = this.cargos;
      return;
    }

    const term = this.searchTerm.toLowerCase();
    this.filteredCargos = this.cargos.filter(cargo => 
      cargo.trackingNumber.toLowerCase().includes(term) ||
      (cargo.senderName && cargo.senderName.toLowerCase().includes(term)) ||
      (cargo.receiverName && cargo.receiverName.toLowerCase().includes(term)) ||
      (cargo.receiverSurname && cargo.receiverSurname.toLowerCase().includes(term))
    );
  }

  openUpdateModal(cargo: Cargo): void {
    this.selectedCargo = cargo;
    this.newStatus = cargo.status;
    this.newLocation = '';
    this.newDescription = '';
    this.successMessage = null;
    this.validationError = null;
    this.showUpdateModal = true;
  }

  closeUpdateModal(): void {
    this.showUpdateModal = false;
    this.selectedCargo = null;
    this.newStatus = CargoStatus.InTransit;
    this.newLocation = '';
    this.newDescription = '';
    this.successMessage = null;
    this.validationError = null;
  }

  openCreateModal(): void {
    this.createForm = {
      senderName: '',
      senderAddress: '',
      senderPhoneNumber: '',
      receiverName: '',
      receiverSurname: '',
      receiverAddress: '',
      receiverPhoneNumber: '',
      receiverEmail: ''
    };
    this.successMessage = null;
    this.validationError = null;
    this.showCreateModal = true;
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
    this.createForm = {
      senderName: '',
      senderAddress: '',
      senderPhoneNumber: '',
      receiverName: '',
      receiverSurname: '',
      receiverAddress: '',
      receiverPhoneNumber: '',
      receiverEmail: ''
    };
    this.successMessage = null;
    this.validationError = null;
  }

  createCargo(): void {
    // Validasyon
    if (!this.createForm.senderName?.trim()) {
      this.validationError = 'Lütfen gönderici adını giriniz.';
      return;
    }

    if (!this.createForm.senderAddress?.trim()) {
      this.validationError = 'Lütfen gönderici adresini giriniz.';
      return;
    }

    if (!this.createForm.senderPhoneNumber?.trim()) {
      this.validationError = 'Lütfen gönderici telefon numarasını giriniz.';
      return;
    }

    if (!this.createForm.receiverName?.trim()) {
      this.validationError = 'Lütfen alıcı adını giriniz.';
      return;
    }

    if (!this.createForm.receiverSurname?.trim()) {
      this.validationError = 'Lütfen alıcı soyadını giriniz.';
      return;
    }

    if (!this.createForm.receiverAddress?.trim()) {
      this.validationError = 'Lütfen alıcı adresini giriniz.';
      return;
    }

    if (!this.createForm.receiverPhoneNumber?.trim()) {
      this.validationError = 'Lütfen alıcı telefon numarasını giriniz.';
      return;
    }

    if (!this.createForm.receiverEmail?.trim()) {
      this.validationError = 'Lütfen alıcı e-posta adresini giriniz.';
      return;
    }

    // Email validasyonu
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.createForm.receiverEmail)) {
      this.validationError = 'Lütfen geçerli bir e-posta adresi giriniz.';
      return;
    }

    this.validationError = null;
    this.creating = true;

    this.cargoService.create(this.createForm).subscribe({
      next: () => {
        this.creating = false;
        this.successMessage = 'Kargo başarıyla oluşturuldu!';
        
        // 2 saniye sonra modalı kapat ve listeyi yenile
        setTimeout(() => {
          this.closeCreateModal();
          this.loadCargos();
        }, 2000);
      },
      error: (err) => {
        console.error('Oluşturma hatası:', err);
        this.validationError = 'Kargo oluşturulurken bir hata oluştu: ' + (err.error?.message || 'Bilinmeyen hata');
        this.creating = false;
      }
    });
  }

  updateCargoStatus(): void {
    // Validasyon
    if (!this.newLocation.trim()) {
      this.validationError = 'Lütfen konum bilgisini giriniz.';
      return;
    }

    if (!this.newDescription.trim()) {
      this.validationError = 'Lütfen açıklama giriniz.';
      return;
    }

    if (!this.selectedCargo) {
      return;
    }

    this.validationError = null;
    this.updating = true;

    this.cargoService.updateMovement({
      cargoId: this.selectedCargo.id,
      newStatus: Number(this.newStatus),
      location: this.newLocation,
      description: this.newDescription
    }).subscribe({
      next: () => {
        this.updating = false;
        this.successMessage = 'Kargo durumu başarıyla güncellendi!';
        
        // 2 saniye sonra modalı kapat ve listeyi yenile
        setTimeout(() => {
          this.closeUpdateModal();
          this.loadCargos();
        }, 2000);
      },
      error: (err) => {
        console.error('Güncelleme hatası:', err);
        this.validationError = 'Güncelleme sırasında bir hata oluştu: ' + (err.error?.message || 'Bilinmeyen hata');
        this.updating = false;
      }
    });
  }

  getStatusText(status: CargoStatus): string {
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

  getStatusClass(status: CargoStatus): string {
    const classMap: Record<CargoStatus, string> = {
      [CargoStatus.Received]: 'status-received',
      [CargoStatus.Processing]: 'status-processing',
      [CargoStatus.InTransit]: 'status-transit',
      [CargoStatus.OutForDelivery]: 'status-delivery',
      [CargoStatus.Delivered]: 'status-delivered',
      [CargoStatus.Undelivered]: 'status-undelivered'
    };
    return classMap[status] || '';
  }

  logout(): void {
    this.loginService.logout();
  }
}
