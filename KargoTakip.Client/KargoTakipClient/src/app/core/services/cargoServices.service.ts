import { Injectable } from '@angular/core';
import { Cargo } from '../../shared/Cargo';
import { HttpClient } from '@angular/common/http';
import { Observable, map, BehaviorSubject, tap } from 'rxjs';
import { CargoMovement } from '../../shared/CargoMovement';
import { CargoStatus } from '../../shared/CargoStatus';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class CargoServices {

  private baseUrl: string = `${environment.apiUrl}/Cargo`;

  private cargosSubject = new BehaviorSubject<Cargo[]>([]);
  public cargos$ = this.cargosSubject.asObservable();
  private cacheTimestamp: number = 0;
  private readonly CACHE_DURATION = 60000; // 1 minute

  constructor(private http: HttpClient) { }

  create(cargo: Partial<Cargo>): Observable<Cargo> {
    return this.http.post<Cargo>(`${this.baseUrl}`, cargo);
  }

  getById(id: string): Observable<Cargo> {
    return this.http.get<Cargo>(`${this.baseUrl}/${id}`);
  }

  getAll(filter: Partial<import('../../shared/CargoFilterParams').CargoFilterParams> = {}, forceRefresh: boolean = false): Observable<Cargo[]> {
    const now = Date.now();
    const isBasicList = Object.keys(filter).length === 0;
    const isCacheValid = (now - this.cacheTimestamp) < this.CACHE_DURATION;

    if (isBasicList && !forceRefresh && isCacheValid && this.cargosSubject.value.length > 0) {
      return this.cargos$;
    }
    let params: any = {};
    if (filter.status !== undefined) params.status = filter.status;
    if (filter.searchTerm) params.searchTerm = filter.searchTerm;
    if (filter.trackingNumber) params.trackingNumber = filter.trackingNumber;
    if (filter.senderName) params.senderName = filter.senderName;
    if (filter.receiverName) params.receiverName = filter.receiverName;
    if (filter.receiverEmail) params.receiverEmail = filter.receiverEmail;
    if (filter.pageIndex) params.pageIndex = filter.pageIndex;
    if (filter.pageSize) params.pageSize = filter.pageSize;
    if (filter.orderBy) params.orderBy = filter.orderBy;
    if (filter.isDescending !== undefined) params.isDescending = filter.isDescending;
    if (filter.includeMovements) params.includeMovements = filter.includeMovements;

    return this.http.get<any>(`${this.baseUrl}`, { params }).pipe(
      map(response => response.items || response),
      tap(cargos => {
        if (isBasicList) {
          this.cargosSubject.next(cargos);
          this.cacheTimestamp = now;
        }
      })
    );
  }

  refreshCache(): void {
    this.cacheTimestamp = 0;
  }

  createMovement(movement: Partial<CargoMovement>): Observable<CargoMovement> {
    return this.http.post<CargoMovement>(`${this.baseUrl}/movements`, movement);
  }

  updateMovement(movement: Partial<CargoMovement>): Observable<CargoMovement> {
    return this.http.put<CargoMovement>(`${this.baseUrl}/movements/update`, movement);
  }

  getCargoMovements(cargoId: string): Observable<CargoMovement[]> {
    return this.http.get<CargoMovement[]>(`${this.baseUrl}/${cargoId}/movements`);
  }

  trackCargo(trackingNumber: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/track/${trackingNumber}`);
  }
}
