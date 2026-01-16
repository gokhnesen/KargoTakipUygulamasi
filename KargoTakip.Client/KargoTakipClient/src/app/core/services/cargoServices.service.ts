import { Injectable } from '@angular/core';
import { Cargo } from '../../shared/Cargo';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CargoMovement } from '../../shared/CargoMovement';
import { CargoStatus } from '../../shared/CargoStatus';

@Injectable({
  providedIn: 'root',
})
export class CargoServices {

  private baseUrl: string = 'https://localhost:7055/api/Cargo';

  constructor(private http: HttpClient) {}

  create(command: Cargo): Observable<any> {
    return this.http.post(`${this.baseUrl}`, command);
  }

  getById(id: string): Observable<Cargo> {
    return this.http.get<Cargo>(`${this.baseUrl}/${id}`);
  }

  getAll(): Observable<Cargo[]> {
    return this.http.get<Cargo[]>(`${this.baseUrl}`);
  }

  createMovement(movement: {
    cargoId: string;
    newStatus: CargoStatus;
    location: string;
    description: string;
  }): Observable<CargoMovement> {
    return this.http.post<CargoMovement>(`${this.baseUrl}/movements`, movement);
  }

  getCargoMovements(cargoId: string): Observable<CargoMovement[]> {
    return this.http.get<CargoMovement[]>(`${this.baseUrl}/${cargoId}/movements`);
  }

  trackCargo(trackingNumber: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/track/${trackingNumber}`);
  }
}
