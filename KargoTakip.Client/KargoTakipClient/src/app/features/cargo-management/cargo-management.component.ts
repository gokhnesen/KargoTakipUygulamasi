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
  newStatus: CargoStatus = CargoStatus.InTransit;
  newLocation: string = '';
  newDescription: string = '';
  updating: boolean = false;

  CargoStatus = CargoStatus;
  statusOptions = [
    { value: CargoStatus.Created, label: 'Oluşturuldu' },
    { value: CargoStatus.InTransit, label: 'Yolda' },
    { value: CargoStatus.InWarehouse, label: 'Depoda' },
    { value: CargoStatus.OutForDelivery, label: 'Dağıtımda' },
    { value: CargoStatus.Delivered, label: 'Teslim Edildi' },
    { value: CargoStatus.Cancelled, label: 'İptal Edildi' },
    { value: CargoStatus.Returned, label: 'İade Edildi' }
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
      (cargo.receiverName && cargo.receiverName.toLowerCase().includes(term))
    );
  }

  openUpdateModal(cargo: Cargo): void {
    this.selectedCargo = cargo;
    this.newStatus = cargo.status;
    this.newLocation = '';
    this.newDescription = '';
    this.showUpdateModal = true;
  }

  closeUpdateModal(): void {
    this.showUpdateModal = false;
    this.selectedCargo = null;
    this.newStatus = CargoStatus.InTransit;
    this.newLocation = '';
    this.newDescription = '';
  }

  updateCargoStatus(): void {
    if (!this.selectedCargo || !this.newLocation.trim()) {
      alert('Lütfen konum bilgisini giriniz.');
      return;
    }

    this.updating = true;

    const movement = {
      cargoId: this.selectedCargo.id,
      newStatus: this.newStatus,
      location: this.newLocation,
      description: this.newDescription || ''
    };

    this.cargoService.createMovement(movement).subscribe({
      next: () => {
        alert('Kargo durumu başarıyla güncellendi.');
        this.closeUpdateModal();
        this.loadCargos();
        this.updating = false;
      },
      error: (err) => {
        console.error('Güncelleme hatası:', err);
        alert('Güncelleme sırasında bir hata oluştu.');
        this.updating = false;
      }
    });
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

  getStatusClass(status: CargoStatus): string {
    const classMap: Record<CargoStatus, string> = {
      [CargoStatus.Created]: 'status-created',
      [CargoStatus.InTransit]: 'status-transit',
      [CargoStatus.InWarehouse]: 'status-warehouse',
      [CargoStatus.OutForDelivery]: 'status-delivery',
      [CargoStatus.Delivered]: 'status-delivered',
      [CargoStatus.Cancelled]: 'status-cancelled',
      [CargoStatus.Returned]: 'status-returned'
    };
    return classMap[status] || '';
  }

  logout(): void {
    this.loginService.logout();
  }
}
