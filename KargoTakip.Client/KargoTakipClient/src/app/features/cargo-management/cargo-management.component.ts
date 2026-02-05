import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
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
    pageSize: 10,
    pageIndex: 1,
    includeMovements: true,
    includeTotalCount: true
  };

  totalCount: number = 0;
  totalPages: number = 1;
  selectedStatusFilter: CargoStatus | null = null;

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
      debounceTime(600),
      distinctUntilChanged(),
      switchMap(term => {
        this.filterParams.searchTerm = term || undefined;
        this.filterParams.pageIndex = 1;
        this.loading.list = true;
        this.error = '';
        return this.cargoService.getAll(this.filterParams);
      })
    ).subscribe({
      next: (response) => {
        this.cargos = response.items;
        this.filteredCargos = response.items;
        this.totalCount = response.totalCount >= 0 ? response.totalCount : 0;
        this.totalPages = response.totalPages > 0 ? response.totalPages : 1;
        this.filterParams.pageIndex = response.pageIndex ?? 1;
        this.loading.list = false;
      },
      error: (err) => {
        this.error = err?.message ?? 'Kargolar yüklenirken hata oluştu.';
        this.loading.list = false;
      }
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

  clearStatusFilter(): void {
    this.selectedStatusFilter = null;
    this.filterParams.status = undefined;
    this.filterParams.pageIndex = 1;
    this.loadCargos();
  }

  filterByStatus(status: CargoStatus): void {
    this.selectedStatusFilter = status;
    this.filterParams.status = status;
    this.filterParams.pageIndex = 1;
    this.loadCargos();
  }

  loadCargos(): void {
    this.loading.list = true;
    this.error = '';

    this.cargoService.getAll(this.filterParams).subscribe({
      next: (response) => {
        this.cargos = response.items;
        this.filteredCargos = response.items;
        this.totalCount = response.totalCount >= 0 ? response.totalCount : 0;
        this.totalPages = response.totalPages > 0 ? response.totalPages : 1;
        this.filterParams.pageIndex = response.pageIndex;
        this.loading.list = false;
      },
      error: (err: any) => {
        this.error = err.userMessage || 'Kargolar yüklenirken bir hata oluştu.';
        this.loading.list = false;
      }
    });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.filterParams.pageIndex = page;
    this.loadCargos();
  }

  onPageSizeChange(size: number): void {
    this.filterParams.pageSize = size;
    this.filterParams.pageIndex = 1;
    this.loadCargos();
  }

  get canPrev(): boolean {
    return this.filterParams.pageIndex! > 1;
  }

  get canNext(): boolean {
    return this.filterParams.pageIndex! < this.totalPages;
  }

  get startRecord(): number {
    if (this.totalCount === 0) return 0;
    return (this.filterParams.pageIndex! - 1) * this.filterParams.pageSize! + 1;
  }

  get endRecord(): number {
    const end = this.filterParams.pageIndex! * this.filterParams.pageSize!;
    return end > this.totalCount ? this.totalCount : end;
  }

  get visiblePageNumbers(): (number | string)[] {
    const current = this.filterParams.pageIndex!;
    const total = this.totalPages;
    if (total <= 7) {
      return Array.from({ length: total }, (_, i) => i + 1);
    }
    const pages: (number | string)[] = [];
    if (current <= 4) {
      for (let i = 1; i <= 5; i++) pages.push(i);
      pages.push('...');
      pages.push(total);
    } else if (current >= total - 3) {
      pages.push(1);
      pages.push('...');
      for (let i = total - 4; i <= total; i++) pages.push(i);
    } else {
      pages.push(1);
      pages.push('...');
      for (let i = current - 1; i <= current + 1; i++) pages.push(i);
      pages.push('...');
      pages.push(total);
    }
    return pages;
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
