import { Injectable } from '@angular/core';
import { Cargo } from '../../shared/Cargo';
import { HttpClient } from '@angular/common/http';
import { Observable, map, BehaviorSubject, tap } from 'rxjs';
import { CargoMovement } from '../../shared/CargoMovement';
import { CargoFilterParams, CargoListResponse } from '../../shared/CargoFilterParams';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class CargoServices {

  private baseUrl: string = `${environment.apiUrl}/Cargo`;

  private cargosSubject = new BehaviorSubject<Cargo[]>([]);
  public cargos$ = this.cargosSubject.asObservable();
  private cacheTimestamp: number = 0;
  private readonly CACHE_DURATION = 60000;

  constructor(private http: HttpClient) { }

  create(cargo: Partial<Cargo>): Observable<Cargo> {
    return this.http.post<Cargo>(`${this.baseUrl}`, cargo);
  }

  getById(id: string): Observable<Cargo> {
    return this.http.get<Cargo>(`${this.baseUrl}/${id}`);
  }

  getAll(filter: Partial<CargoFilterParams> = {}, forceRefresh: boolean = false): Observable<CargoListResponse> {
    const now = Date.now();
    const isBasicList = Object.keys(filter).length === 0;
    const isCacheValid = (now - this.cacheTimestamp) < this.CACHE_DURATION;

    if (isBasicList && !forceRefresh && isCacheValid && this.cargosSubject.value.length > 0) {
      return this.cargos$.pipe(
        map(items => ({
          items,
          totalCount: items.length,
          pageIndex: 1,
          pageSize: items.length,
          totalPages: 1,
          hasMore: false
        }))
      );
    }
    const params: Record<string, string | number | boolean> = {};
    if (filter.status !== undefined) params['status'] = filter.status;
    if (filter.searchTerm) params['searchTerm'] = filter.searchTerm;
    if (filter.trackingNumber) params['trackingNumber'] = filter.trackingNumber;
    if (filter.senderName) params['senderName'] = filter.senderName;
    if (filter.receiverName) params['receiverName'] = filter.receiverName;
    if (filter.receiverEmail) params['receiverEmail'] = filter.receiverEmail;
    if (filter.pageIndex !== undefined) params['pageIndex'] = filter.pageIndex;
    if (filter.pageSize !== undefined) params['pageSize'] = filter.pageSize;
    if (filter.orderBy) params['orderBy'] = filter.orderBy;
    if (filter.isDescending !== undefined) params['isDescending'] = filter.isDescending;
    if (filter.includeTotalCount !== undefined) params['includeTotalCount'] = filter.includeTotalCount;

    return this.http.get<CargoListResponse>(`${this.baseUrl}`, { params }).pipe(
      map(response => {
        const items = response.items ?? (Array.isArray(response) ? response : []);
        const totalCount = response.totalCount ?? items.length;
        const pageIndex = response.pageIndex ?? 1;
        const pageSize = response.pageSize ?? 50;
        const totalPages = response.totalPages ?? (pageSize > 0 ? Math.ceil(totalCount / pageSize) : 1);
        return {
          items,
          totalCount,
          pageIndex,
          pageSize,
          totalPages,
          hasMore: response.hasMore ?? false
        };
      }),
      tap(res => {
        if (isBasicList) {
          this.cargosSubject.next(res.items);
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
    return this.http.get<{ movements: CargoMovement[] }>(`${this.baseUrl}/${cargoId}/movements`).pipe(
      map(res => res?.movements ?? [])
    );
  }

  trackCargo(trackingNumber: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/track/${trackingNumber}`);
  }
}
