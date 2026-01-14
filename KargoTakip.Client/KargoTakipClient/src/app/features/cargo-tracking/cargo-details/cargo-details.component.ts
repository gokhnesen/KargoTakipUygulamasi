import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Cargo } from '../../../shared/Cargo';
import { CargoMovement } from '../../../shared/CargoMovement';
import { CargoStatus } from '../../../shared/CargoStatus';

@Component({
  selector: 'app-cargo-details',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cargo-details.component.html',
  styleUrl: './cargo-details.component.scss'
})
export class CargoDetailsComponent implements OnInit, OnChanges {
  @Input() cargo: Cargo | null = null;
  @Input() movements: CargoMovement[] = [];
  @Input() isOpen: boolean = false;
  @Output() closeModal = new EventEmitter<void>();

  CargoStatus = CargoStatus;

  ngOnInit(): void {
    console.log('CargoDetailsComponent - ngOnInit');
    console.log('isOpen:', this.isOpen);
    console.log('cargo:', this.cargo);
  }

  ngOnChanges(changes: SimpleChanges): void {
    console.log('CargoDetailsComponent - ngOnChanges');
    console.log('Changes:', changes);
    
    if (changes['isOpen']) {
      console.log('isOpen değişti:', changes['isOpen'].currentValue);
    }
    if (changes['cargo']) {
      console.log('cargo değişti:', changes['cargo'].currentValue);
    }
  }

  close(): void {
    console.log('Modal close çağrıldı');
    this.closeModal.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    console.log('Backdrop clicked');
    if ((event.target as HTMLElement).classList.contains('modal-backdrop')) {
      this.close();
    }
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
}