import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CargoServices } from '../../core/services/cargoServices.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './home.html',
  styleUrls: ['./home.scss']
})
export class Home {
  trackingCode: string = '';
  constructor(private router: Router,
    private cargoService: CargoServices
  ) {}

  searchCargo() { 
    console.log('searchCargo çağrıldı, trackingCode:', this.trackingCode); // Debug log ekle
    
    if (!this.trackingCode.trim()) {
      console.log('trackingCode boş!');
      return;
    }
    
    console.log('Navigate ediliyor...');
    this.router.navigate(['/track'], { 
      queryParams: { tracking: this.trackingCode } 
    });
  }
  
  goToLogin() {
    this.router.navigate(['/login']);
  }
}