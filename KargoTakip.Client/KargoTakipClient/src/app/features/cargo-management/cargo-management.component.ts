import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { CargoServices } from '../../core/services/cargoServices.service';
import { LoginServices } from '../../core/services/loginServices.service';
import { Cargo } from '../../shared/Cargo';
import { CargoMovement } from '../../shared/CargoMovement';
import { CargoStatus } from '../../shared/CargoStatus';
import { CargoFilterParams } from '../../shared/CargoFilterParams';

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
  error: string = '';
  searchTerm: string = '';

  filterParams: CargoFilterParams = {
    pageSize: 50,
    pageIndex: 1,
    includeMovements: true
  };

  private searchSubject = new Subject<string>();

  loading = {
    list: false,
    create: false,
    update: false
  };

  selectedCargo: Cargo | null = null;
  selectedCargoMovements: CargoMovement[] = [];
  showUpdateModal: boolean = false;
  showCreateModal: boolean = false;
  newStatus: CargoStatus = CargoStatus.InTransit;
  newLocation: string = '';
  newDescription: string = '';
  successMessage: string | null = null;
  validationError: string | null = null;
  sidebarOpen: boolean = false;

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
  ) {
    this.searchSubject.pipe(
      debounceTime(500),
      distinctUntilChanged()
    ).subscribe(term => {
      this.filterParams.searchTerm = term;
      this.filterParams.pageIndex = 1;
      this.loadCargos();
    });
  }

  ngOnInit(): void {
    this.loadCargos();
  }

  toggleSidebar(): void {
    this.sidebarOpen = !this.sidebarOpen;
  }

  closeSidebar(): void {
    this.sidebarOpen = false;
  }

  loadCargos(): void {
    this.loading.list = true;
    this.error = '';

    this.cargoService.getAll(this.filterParams).subscribe({
      next: (response) => {
        this.cargos = response;
        this.filteredCargos = response;
        this.loading.list = false;
      },
      error: (err: any) => {
        this.error = err.userMessage || 'Kargolar yüklenirken bir hata oluştu.';
        this.loading.list = false;
      }
    });
  }

  searchCargo(): void {
    this.searchSubject.next(this.searchTerm);
  }

  openUpdateModal(cargo: Cargo): void {
    this.selectedCargo = cargo;
    this.selectedCargoMovements = cargo.movements || [];
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
    this.selectedCargoMovements = [];
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

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.createForm.receiverEmail)) {
      this.validationError = 'Lütfen geçerli bir e-posta adresi giriniz.';
      return;
    }

    this.validationError = null;
    this.loading.create = true;

    // Construct the payload to match API expectations
    const payload = {
      senderFullName: this.createForm.senderName,
      senderAddress: this.createForm.senderAddress,
      senderPhoneNumber: this.createForm.senderPhoneNumber,
      receiverName: this.createForm.receiverName,
      receiverSurname: this.createForm.receiverSurname,
      receiverAddress: this.createForm.receiverAddress,
      receiverPhoneNumber: this.createForm.receiverPhoneNumber,
      receiverEmail: this.createForm.receiverEmail
    };

    this.cargoService.create(payload).subscribe({
      next: () => {
        this.loading.create = false;
        this.successMessage = 'Kargo başarıyla oluşturuldu!';

        setTimeout(() => {
          this.closeCreateModal();
          this.loadCargos();
        }, 2000);
      },
      error: (err: any) => {
        this.validationError = err.userMessage || 'Kargo oluşturulurken bir hata oluştu.';
        this.loading.create = false;
      }
    });
  }

  updateCargoStatus(): void {
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
    this.loading.update = true;

    this.cargoService.updateMovement({
      cargoId: this.selectedCargo.id,
      newStatus: Number(this.newStatus),
      location: this.newLocation,
      description: this.newDescription
    }).subscribe({
      next: () => {
        this.loading.update = false;
        this.successMessage = 'Kargo durumu başarıyla güncellendi!';

        setTimeout(() => {
          this.closeUpdateModal();
          this.loadCargos();
        }, 2000);
      },
      error: (err: any) => {
        this.validationError = err.userMessage || 'Güncelleme sırasında bir hata oluştu.';
        this.loading.update = false;
      }
    });
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

  getStatusClass(status: CargoStatus): string {
    const classMap: Record<CargoStatus, string> = {
      [CargoStatus.Received]: 'status-received',
      [CargoStatus.Processing]: 'status-processing',
      [CargoStatus.InTransit]: 'status-transit',
      [CargoStatus.OutForDelivery]: 'status-delivered',
      [CargoStatus.Delivered]: 'status-delivered',
      [CargoStatus.Undelivered]: 'status-undelivered'
    };
    return classMap[status] || '';
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

  logout(): void {
    this.loginService.logout();
  }
}
