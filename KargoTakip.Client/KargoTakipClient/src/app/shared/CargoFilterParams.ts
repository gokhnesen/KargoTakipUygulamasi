import { CargoStatus } from './CargoStatus';
import { Cargo } from './Cargo';

export interface CargoFilterParams {
    status?: CargoStatus;
    trackingNumber?: string;
    searchTerm?: string;
    senderName?: string;
    receiverName?: string;
    receiverEmail?: string;
    startDate?: Date;
    endDate?: Date;
    orderBy?: string;
    isDescending?: boolean;
    pageIndex?: number;
    pageSize?: number;
    includeTotalCount?: boolean;
}

export interface CargoListResponse {
    items: Cargo[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
    totalPages: number;
    hasMore?: boolean;
}
